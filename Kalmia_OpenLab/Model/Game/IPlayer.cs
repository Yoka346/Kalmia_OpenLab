using Kalmia_OpenLab.Model.Reversi;

namespace Kalmia_OpenLab.Model.Game
{
    internal interface IPlayer
    {
        public string Name { get; }
        public DiscColor DiscColor { get; }
        public void Quit();
        public void SetGame(GameInfo game);
        public void UpdateGame(BoardCoordinate move);
        public BoardCoordinate GenerateMove();
    }
}
