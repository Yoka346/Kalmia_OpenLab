using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Kalmia_OpenLab.Model.Engine;
using Kalmia_OpenLab.Model.Reversi;

namespace Kalmia_OpenLab.Model.Game
{
    internal class EnginePlayer : IPlayer
    {
        const int QUIT_TIMEOUT_MS = 10000;

        public string Name { get; }
        public DiscColor DiscColor { get; }

        public event NodeStatsEventHandler OnNodeStatsRecieved = delegate { };

        public event SearchInfoEventHandler OnSearchInfoRecieved = delegate { };

        public event EventHandler OnUnexpectedShutdownOccured = delegate { };

        readonly NBoardEngine engine;

        public EnginePlayer(string enginePath, string engineArgs, string engineWorkDir, int level, IEnumerable<string> initialCommands, DiscColor discColor)
        {
            this.engine = new NBoardEngine(enginePath, engineArgs, engineWorkDir, initialCommands);
            this.engine.OnNodeStatsRecieved += (s, e) => this.OnNodeStatsRecieved.Invoke(this, e);
            this.engine.OnSearchInfoRecieved += (s, e) => this.OnSearchInfoRecieved.Invoke(this, e);
            this.engine.ExitedUnexpectedly += (s, e) => this.OnUnexpectedShutdownOccured.Invoke(this, e);
            this.engine.Run();
            this.Name = this.engine.Name ?? "AI";
            this.DiscColor = discColor;
            this.engine.SetLevel(level);
        }

        public void SetGame(GameInfo game) => this.engine.SetGameInfo(game);

        public void UpdateGame(BoardCoordinate move) 
        {
            if (this.engine.CheckConnection())
                this.engine.SendMove(move);
            else
                throw new Exception("Engine's connection has been lost.");
        }

        public BoardCoordinate GenerateMove()
        {
            if (this.engine.CheckConnection())
                return this.engine.Think();
            
            throw new Exception("Engine's connection has been lost.");
        }

        public void StartAnalyzing()
        {
            if (this.engine.CheckConnection())
                this.engine.StartAnalyzing();
            else
                throw new Exception("Engine's connection has been lost.");
        }

        public void Quit()
        {
            if (!this.engine.Quit(QUIT_TIMEOUT_MS))
            {
                if (!this.engine.Kill(QUIT_TIMEOUT_MS))
                    throw new Exception("Cannot kill an engine process.");
            }
        }
    }
}
