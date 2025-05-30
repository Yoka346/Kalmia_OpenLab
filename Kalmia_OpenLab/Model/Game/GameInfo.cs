using System;
using System.Reflection;
using System.Text;

using Kalmia_OpenLab.Model.Reversi;

namespace Kalmia_OpenLab.Model.Game
{
    internal struct GameTime
    {
        /// <summary>
        /// 持ち時間. 
        /// </summary>
        public int MainTimeMs { get; set; }

        /// <summary>
        /// 1手ごとに持ち時間に加算される時間.
        /// </summary>
        public int IncrementTimeMs { get; set; }

        /// <summary>
        /// 秒読み. 持ち時間を使い切ったら1手ByoyomiTimeMs[ms]以内に着手しなければならない.
        /// </summary>
        public int ByoYomiMs { get; set; }

        public override readonly string ToString()
        {
            var ts = TimeSpan.FromMilliseconds(MainTimeMs);
            return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }
    }

    internal class GameInfo
    {
        public string BlackPlayerName { get; set; } = string.Empty;
        public string WhitePlayerName { get; set; } = string.Empty;
        public GameTime BlackThinkingTime { get; set; }
        public GameTime WhiteThinkingTime { get; set; }
        public Position Position { get; set; } = new();
        public DateTime DateTime { get; set; } = DateTime.Now;

        public string ToGGFString()
        {
            var sb = new StringBuilder("(;GM[Othello]PC[");
            sb.Append(Assembly.GetExecutingAssembly().GetName().Name).Append("]DT[");
            sb.Append(this.DateTime.ToString()).Append("]PB[");
            sb.Append(this.BlackPlayerName).Append("]PW[");
            sb.Append(this.WhitePlayerName).Append("]RE[?]BT[");
            sb.Append(this.BlackThinkingTime.ToString()).Append("]WT[");
            sb.Append(this.WhiteThinkingTime.ToString()).Append("]TY[");
            sb.Append(Position.BOARD_SIZE).Append("]BO[").Append(Position.BOARD_SIZE).Append(' ');

            // 盤面情報の構成.
            Span<char> discs = ['O', '-', '*'];
            for (var coord = BoardCoordinate.A1; coord <= BoardCoordinate.H8; coord++)
                sb.Append(discs[(int)this.Position[coord] + 1]);
            sb.Append(' ').Append(discs[(int)this.Position.SideToMove + 1]).Append(']');

            // 着手情報の構成.
            Span<char> colors = ['B', 'W', '?'];
            foreach (var move in this.Position.MoveHistroy)
                sb.Append(colors[(int)move.Player]).Append('[').Append(move.Coord.ToString()).Append(']');

            return sb.Append(";)").ToString();
        }
    }
}
