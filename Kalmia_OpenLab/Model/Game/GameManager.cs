using System;
using System.Threading;
using System.Threading.Tasks;

using Kalmia_OpenLab.Model.Reversi;

namespace Kalmia_OpenLab.Model.Game
{
    internal class GameEventArgs : EventArgs
    {
        public BoardCoordinate Coord { get; }
        public GameResult? Result { get; }
        public IPlayer? Winner { get; }

        public GameEventArgs(BoardCoordinate coord, GameResult? result, IPlayer? winner)
        {
            this.Coord = coord;
            this.Result = result;
            this.Winner = winner;
        }
    }

    internal delegate void GameEventHandler(GameManager sender, GameEventArgs e);

    internal enum GameType
    {
        Normal,
        Weakest
    }

    /// <summary>
    /// 対局の管理を行うクラス.
    /// </summary>
    internal class GameManager
    {
        public GameType GameType { get; }
        public IPlayer BlackPlayer { get; }
        public IPlayer WhitePlayer { get; }
        public IPlayer? CurrentPlayer { get; private set; }
        public IPlayer? OpponentPlayer { get; private set; }
        public DiscColor SideToMove { get { return this.pos.SideToMove; } }
        public int BlackDiscCount { get { return this.pos.CountDiscs(DiscColor.Black); } }
        public int WhiteDiscCount { get { return this.pos.CountDiscs(DiscColor.White); } }
        public int MoveCount { get; private set; }
        public bool NowPlaying { get; private set; }
        public bool Paused { get; private set; }

        public event GameEventHandler OnSideToMoveChanged = delegate { };
        public event GameEventHandler OnPlayBlack = delegate { };
        public event GameEventHandler OnPlayWhite = delegate { };
        public event GameEventHandler OnGameIsOver = delegate { };

        Position pos;
        bool suspendFlag = false;

        public GameManager(GameType gameType, IPlayer blackPlayer, IPlayer whitePlayer, int numHandicapDiscs)
        {
            this.GameType = gameType;   
            this.BlackPlayer = blackPlayer;
            this.WhitePlayer = whitePlayer;

            this.pos = new Position();
            if (numHandicapDiscs > 0)
            {
                var handicapCoords = new BoardCoordinate[] { BoardCoordinate.A1, BoardCoordinate.H8, BoardCoordinate.H1, BoardCoordinate.A8 };
                foreach (var coord in handicapCoords[..numHandicapDiscs])
                    pos.PutDisc(DiscColor.Black, coord);
                pos.SideToMove = DiscColor.White;
            }
        }

        ~GameManager() => Dispose();

        public void Dispose()
        {
            this.BlackPlayer.Quit();
            this.WhitePlayer.Quit();

            GC.SuppressFinalize(this);
        }

        public Position GetPosition() => new(this.pos);
        public void Start() => Task.Run(Mainloop);
        public void Pause() => this.Paused = true;
        public void Resume() => this.Paused = false;

        public void Suspend()
        {
            this.suspendFlag = true;
            var startTime = Environment.TickCount;
            while (this.NowPlaying && Environment.TickCount - startTime < 10000)
                Thread.Yield();
        }

        public GameResult GetGameResult()
        {
            var result = this.pos.GetGameResult();
            if (this.GameType == GameType.Weakest && !result.Draw)
                result.Winner = (DiscColor)(-(int)result.Winner);
            return result;
        }

        void Mainloop()
        {
            var gameInfo = new GameInfo
            {
                BlackPlayerName = this.BlackPlayer.Name,
                WhitePlayerName = this.WhitePlayer.Name,
                DateTime = DateTime.Now,
                Position = this.pos
            };

            this.BlackPlayer.SetGame(gameInfo);
            this.WhitePlayer.SetGame(gameInfo);

            this.NowPlaying = true;

            if (pos.SideToMove == DiscColor.Black)
            {
                this.CurrentPlayer = this.BlackPlayer;
                this.OpponentPlayer = this.WhitePlayer;
            }
            else
            {
                this.CurrentPlayer = this.WhitePlayer;
                this.OpponentPlayer = this.BlackPlayer;
            }

            while (!pos.GetGameResult().GameOver)
            {
                if (this.suspendFlag)
                {
                    this.NowPlaying = false;
                    return;
                }
                WaitForResume();

                var move = this.CurrentPlayer.GenerateMove();

                if (this.suspendFlag)
                {
                    this.NowPlaying = false;
                    return;
                }
                WaitForResume();

                if (!this.pos.IsLegalMove(move))
                    throw new IllegalMoveException(this.CurrentPlayer, move);

                this.pos.Update(move);
                this.CurrentPlayer.UpdateGame(move);
                this.OpponentPlayer.UpdateGame(move);

                if (this.CurrentPlayer == this.BlackPlayer)
                    this.OnPlayBlack.Invoke(this, new GameEventArgs(move, null, null));
                else
                    this.OnPlayWhite.Invoke(this, new GameEventArgs(move, null, null));

                (this.CurrentPlayer, this.OpponentPlayer) = (this.OpponentPlayer, this.CurrentPlayer);

                this.OnSideToMoveChanged.Invoke(this, new GameEventArgs(BoardCoordinate.Null, null, null));

                this.MoveCount++;
            }

            this.NowPlaying = false;

            var result = GetGameResult();
            IPlayer? winner;
            if (result.Draw)
                winner = null;
            else
                winner = (result.Winner == DiscColor.Black) ? this.BlackPlayer : this.WhitePlayer;

            this.OnGameIsOver.Invoke(this, new GameEventArgs(BoardCoordinate.Null, result, winner));
        }

        void WaitForResume()
        {
            while (this.Paused)
                Thread.Yield();
        }
    }
}
