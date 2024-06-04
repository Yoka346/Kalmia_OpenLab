using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Kalmia_OpenLab.Model.Reversi;
using Kalmia_OpenLab.Model.Game;
using System.Collections.ObjectModel;

namespace Kalmia_OpenLab.Model.Engine
{
    internal readonly struct NodeStats
    {
        public ulong NodeCount { get;  init; }
        public int TimeMs { get;  init; }
    }

    internal class SearchInfo
    {
        public float Eval { get; init; }
        public int Depth { get; init; }
        public ReadOnlyCollection<BoardCoordinate> PV => new(this.pv);

        BoardCoordinate[] pv;

        public SearchInfo(IEnumerable<BoardCoordinate> pv) => this.pv = pv.ToArray();
    }

    delegate void NodeStatsEventHandler(object sender, NodeStats nodeStats);
    delegate void SearchInfoEventHandler(object sender, SearchInfo info);

    /// <summary>
    /// NBoardプロトコルに準拠した思考エンジンとやり取りをするクラス.
    /// </summary>
    internal class NBoardEngine
    {
        const int NBOARD_VERSION = 2;
        const int CONNECTION_CHECK_TIMEOUT_MS = 10000;

        /// <summary>
        /// set name コマンドによって受け取った思考エンジンの名前.
        /// </summary>
        public string? Name { get; private set; }

        public string? ProcName => this.process?.Name;

        public bool QuitCommandWasSent => this.quitCommandWasSent;
        public bool IsAlive => (this.process is not null) && !this.process.HasExited;

        public bool IsThinking { get => this.isThinking != 0; }

        /// <summary>
        /// Killメソッドが呼ばれて, プロセスをKillしている最中である.
        /// </summary>
        public bool IsBeingKilled => this.isBeingKilled;

        /// <summary>
        /// Killメソッドが呼ばれて, プロセスがKillされた.
        /// </summary>
        public bool WasKilled => this.wasKilled;

        public event SearchInfoEventHandler OnSearchInfoRecieved = delegate { };
        public event NodeStatsEventHandler OnNodeStatsRecieved = delegate { };
        public event EventHandler ExitedUnexpectedly = delegate { };

        readonly string PATH, ARGS, WORK_DIR_PATH;
        readonly string[] INIT_COMMANDS;
        EngineProcess? process;

        volatile int isThinking = 0;
        volatile bool quitCommandWasSent = false;
        volatile bool isBeingKilled = false;
        volatile bool wasKilled = false;

        int pingCount = 0;

        public NBoardEngine(string path, string args, string workDirPath, IEnumerable<string> initialCommands)
        {
            this.PATH = path;
            this.ARGS = args;
            this.WORK_DIR_PATH = workDirPath;
            this.INIT_COMMANDS = initialCommands.ToArray();
        }

        public bool Run()
        {
            this.process = EngineProcess.Start(this.PATH, this.ARGS, this.WORK_DIR_PATH);
            if (this.process is null)
                return false;

            this.process.Exited += Process_Exited;
            this.process.OnNonResponceTextRecieved += Process_OnNonResponceTextRecieved;

            Thread.Sleep(1000);     // Edaxの場合, 1秒ほど待ってからコマンドを送らないとエラーを出して終了することがある.

            SendCommand($"nboard {NBOARD_VERSION}");

            foreach (var cmd in this.INIT_COMMANDS)
                SendCommand(cmd);

            return true;
        }

        public bool Quit(int timeoutMs)
        {
            if (this.process is null)
                return false;

            this.quitCommandWasSent = true;
            SendCommand("quit");
            this.process.WaitForExit(timeoutMs);
            return !this.IsAlive;
        }

        public bool Kill(int timeoutMs)
        {
            this.isBeingKilled = true;
            this.process?.Kill();
            this.process?.WaitForExit(timeoutMs);
            if (!this.IsAlive)
            {
                this.wasKilled = true;
                this.isBeingKilled = true;
                return true;
            }
            return false;
        }

        public void SetLevel(int level) => SendCommand($"set depth {level}");

        public void SetGameInfo(GameInfo gameInfo) => SendCommand($"set game {gameInfo.ToGGFString()}");

        public void SendMove(BoardCoordinate move) => SendCommand($"move {move}");

        public BoardCoordinate Think()
        {
            if (process is null)
                throw new NullReferenceException("Execute Run method at first.");

            if (Interlocked.Exchange(ref this.isThinking, 1) == 1)
                throw new InvalidOperationException("Cannnot execute multiple thinking.");

            var responce = SendCommand("go", "^\\s*===");

            while (!responce.HasResult && this.IsThinking)
                Thread.Yield();

            if (!this.IsThinking)
                return BoardCoordinate.Null;

            Interlocked.Exchange(ref this.isThinking, 0);

            var tokenizer = new Tokenizer(responce.Result);
            tokenizer.Read();  // "==="の読み飛ばし.
            var moveStr = tokenizer.Read();
            var move = Position.ParseBoardCoordinate(new string(moveStr));
            if (move == BoardCoordinate.Null)
                throw new NBoardProtocolException($"Recieved move string \"{moveStr}\" was invalid.");

            return move;
        }

        public void StartAnalyzing() 
        {
            if (process is null)
                throw new NullReferenceException("Execute Run method at first.");

            if (Interlocked.Exchange(ref this.isThinking, 1) == 1)
                throw new InvalidOperationException("Cannnot execute multiple thinking.");

            SendCommand("hint 1");
        }

        public bool CheckConnection(int timeoutMs = CONNECTION_CHECK_TIMEOUT_MS)
        {
            if (this.process is null)
                throw new InvalidOperationException("Engine process is not created.");

            var pingID = this.pingCount++;
            var responce = this.process.SendCommand($"ping {pingID}", $"^\\s*pong\\s+{pingID}");
            var res = responce.Wait(timeoutMs);

            if (res)
                Interlocked.Exchange(ref this.isThinking, 0);

            return res;
        }

        EngineProcess.Responce SendCommand(string cmd, string? regex = null)
        {
            if (this.process is null)
                throw new InvalidOperationException("Engine process is not running.");
            return this.process.SendCommand(cmd, regex);
        }

        void Process_OnNonResponceTextRecieved(object? sender, string e)
        {
            var tokenizer = new Tokenizer(e);
            var head = tokenizer.Read();

            if (head.CompareTo("set", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (tokenizer.Read().CompareTo("myname", StringComparison.OrdinalIgnoreCase) == 0 && tokenizer.Peek() != -1)
                    this.Name = tokenizer.Read().ToString();
            }
            else if (head.CompareTo("nodestats", StringComparison.OrdinalIgnoreCase) == 0 && tokenizer.Peek() != -1)
            {
                if (!ulong.TryParse(tokenizer.Read(), out ulong nodeCount))
                    return;

                var time = 0.0f;
                if (tokenizer.Peek() != -1)
                    float.TryParse(tokenizer.Read(), out time);

                this.OnNodeStatsRecieved.Invoke(this, new NodeStats { NodeCount = nodeCount, TimeMs = (int)(time * 1.0e+3f) });
            }
            else if (head.CompareTo("search", StringComparison.OrdinalIgnoreCase) == 0 && tokenizer.Peek() != -1)
            {
                var pvStr = new string(tokenizer.Read());
                var eval = 0.0f;
                if (tokenizer.Peek() != -1)
                    float.TryParse(tokenizer.Read(), out eval);

                if (tokenizer.Peek() != -1)
                    tokenizer.Read();   // 0の読み飛ばし

                var depth = 0;
                if (tokenizer.Peek() != -1)
                    int.TryParse(tokenizer.Read(), out depth);

                var searchInfo = new SearchInfo(ParsePV(pvStr)) { Eval = eval, Depth = depth };
                this.OnSearchInfoRecieved.Invoke(this, searchInfo);
            }
        }

        void Process_Exited(object? sender, EventArgs e)
        {
            if (this.quitCommandWasSent || this.isBeingKilled || this.wasKilled)
                return;

            this.ExitedUnexpectedly.Invoke(this, EventArgs.Empty);
            Interlocked.Exchange(ref this.isThinking, 0);
        }

        static IEnumerable<BoardCoordinate> ParsePV(string pvStr)
        {
            for(var i = 0; i < pvStr.Length - 1;)
            {
                var coord = Position.ParseBoardCoordinate(pvStr[i..(i + 2)]);
                if (coord != BoardCoordinate.Null)
                {
                    yield return coord;
                    i += 2;
                }
                else
                    i++;
            }
        }
    }
}
