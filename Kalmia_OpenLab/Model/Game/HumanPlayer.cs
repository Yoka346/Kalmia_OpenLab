using System.Threading;

using Kalmia_OpenLab.Model.Reversi;

namespace Kalmia_OpenLab.Model.Game
{
    internal class HumanPlayer : IPlayer
    {
        public string Name { get; }
        public DiscColor DiscColor { get; }

        Position pos;
        BoardCoordinate humanInput;
        bool waitingForInput = false;
        bool quitFlag = false;
        readonly object LOCK_OBJ = new();

        public HumanPlayer(string name, DiscColor discColor)
        {
            this.Name = name;
            this.DiscColor = discColor;
            this.pos = new Position();
        }

        public void Quit() => this.quitFlag = true;

        public void SetGame(GameInfo game) => this.pos = new Position(game.Position);

        public void UpdateGame(BoardCoordinate move) => this.pos.Update(move);

        public BoardCoordinate GenerateMove()
        {
            if (this.pos.CanPass)
                return BoardCoordinate.Pass;

            this.waitingForInput = true;
            var move = BoardCoordinate.Null;
            while (!this.quitFlag)
            {
                while (!this.quitFlag && this.humanInput == BoardCoordinate.Null)
                    Thread.Yield();

                lock (this.LOCK_OBJ)
                {
                    if (this.pos.IsLegalMove(this.humanInput))
                    {
                        move = this.humanInput;
                        break;
                    }
                    this.humanInput = BoardCoordinate.Null;
                }
            }

            this.waitingForInput = false;
            this.humanInput = BoardCoordinate.Null;
            return move;
        }

        public void SetHumanInput(BoardCoordinate move)
        {
            if (this.waitingForInput)
                lock (this.LOCK_OBJ)
                    this.humanInput = move;
        }
    }
}
