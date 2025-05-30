using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

using Kalmia_OpenLab.Model.Reversi;

namespace Kalmia_OpenLab.View.Controls
{
    internal partial class PositionViewer : UserControl
    {
        Position pos;

        public PositionViewer() : this(new Position()) { }

        public PositionViewer(Position pos)
        {
            this.pos = pos;
            InitializeComponent();
        }

        public void SetPosition(Position pos)
        {
            this.pos = new(pos);
            this.posDisplay.Invalidate();
        }

        public bool Update(BoardCoordinate move)
        {
            if (this.pos.Update(move) is not null)
            {
                this.posDisplay.Invalidate();
                return true;
            }
            return false;
        }

        void PositionViewer_Paint(object sender, PaintEventArgs e)
        {
            if (this.backgroundImage is not null)
                e.Graphics.DrawImage(this.backgroundImage, 0, 0);

            AdjustPosDisplay();
            AdjustCoordLabels();
            this.posDisplay.Invalidate();
        }

        void PosDisplay_Paint(object sender, PaintEventArgs e)
        {
            if (this.boardBackgroundImage is not null)
                e.Graphics.DrawImage(this.boardBackgroundImage, 0, 0);
            DrawGrid(e);

            if (!this.showMoveHistory)
            {
                DrawDiscs(e);
                if (this.showLegalMovePointers)
                    DrawLegalMovePointers(e);
            }
            else
                DrawMoveHistory(e);
        }

        void PosDisplay_MouseClick(object sender, MouseEventArgs e)
        {
            var gridSize = (float)this.posDisplay.Width / Position.BOARD_SIZE;
            var x = (int)(e.X / gridSize);
            var y = (int)(e.Y / gridSize);
            this.OnMouseClicked.Invoke(this, (BoardCoordinate)(x + y * Position.BOARD_SIZE));
        }

        void AdjustPosDisplay()
        {
            var margin = this.Width * MARGIN_SIZE_RATIO;
            this.posDisplay.Size = new Size((int)(this.Width - margin * 2.0f), (int)(this.Width - margin * 2.0f));
            this.posDisplay.Location = new Point((int)margin, (int)margin);
        }

        void AdjustCoordLabels()
        {
            var margin = this.Width * MARGIN_SIZE_RATIO;
            var gridSize = (float)this.posDisplay.Width / Position.BOARD_SIZE;
            var startCoord = (gridSize + margin) * 0.5f;
            var labelSize = gridSize * 0.333f;
            var fontSize = gridSize * COORD_SIZE_RATIO_TO_GRID_SIZE;

            for (var i = 0; i < Position.BOARD_SIZE; i++)
            {
                var coord = gridSize * i + startCoord;

                var label = this.XCoordLabels[i];
                label.ForeColor = this.coordLabelTextColor;
                label.Size = new Size((int)margin, (int)margin);
                label.Location = new Point((int)coord, 0);
                label.Font = new Font(this.coordFontFamily, fontSize, GraphicsUnit.Pixel);

                label = this.YCoordLabels[i];
                label.ForeColor = this.coordLabelTextColor;
                label.Size = new Size((int)margin, (int)margin);
                label.Location = new Point(0, (int)coord);
                label.Font = new Font(this.coordFontFamily, fontSize, GraphicsUnit.Pixel);
            }
        }

        void DrawGrid(PaintEventArgs e)
        {
            var graph = e.Graphics;
            graph.SmoothingMode = SmoothingMode.HighQuality;

            var pen = new Pen(this.gridColor);
            var lineLen = (float)this.posDisplay.Width;
            var gridSize = lineLen / Position.BOARD_SIZE;
            for (var i = 1; i < Position.BOARD_SIZE; i++)
            {
                var coord = gridSize * i;
                graph.DrawLine(pen, coord, 0.0f, coord, lineLen);
                graph.DrawLine(pen, 0.0f, coord, lineLen, coord);
            }
        }

        void DrawDiscs(PaintEventArgs e)
        {
            var graph = e.Graphics;
            graph.SmoothingMode = SmoothingMode.HighQuality;

            var blackDiscBrush = new SolidBrush(this.blackDiscColor);
            var whiteDiscBrush = new SolidBrush(this.whiteDiscColor);
            var gridSize = (float)this.posDisplay.Width / Position.BOARD_SIZE;
            var discSize = gridSize * DISC_SIZE_RATIO_TO_GRID_SIZE;
            var margin = (gridSize - discSize) * 0.5f;
            for (var i = 0; i < Position.NUM_SQUARES; i++)
            {
                var disc = this.pos[(BoardCoordinate)i];
                if (disc == DiscColor.Empty)
                    continue;

                var x = (i % Position.BOARD_SIZE) * gridSize + margin;
                var y = (i / Position.BOARD_SIZE) * gridSize + margin;
                if (disc == DiscColor.Black)
                    graph.FillEllipse(blackDiscBrush, x, y, discSize, discSize);
                else
                    graph.FillEllipse(whiteDiscBrush, x, y, discSize, discSize);
            }
        }

        void DrawLegalMovePointers(PaintEventArgs e)
        {
            var graph = e.Graphics;
            graph.SmoothingMode = SmoothingMode.HighQuality;

            var pen = new Pen(this.legalMovePointerColor, this.legalMovePointerThickness);
            var gridSize = (float)this.posDisplay.Width / Position.BOARD_SIZE;
            var discSize = gridSize * DISC_SIZE_RATIO_TO_GRID_SIZE;
            var margin = (gridSize - discSize) * 0.5f;
            foreach (var coord in this.pos.GetNextMoves())
            {
                var x = ((int)coord % Position.BOARD_SIZE) * gridSize + margin;
                var y = ((int)coord / Position.BOARD_SIZE) * gridSize + margin;
                graph.DrawEllipse(pen, x, y, discSize, discSize);
            }
        }

        void DrawMoveHistory(PaintEventArgs e)
        {
            var graph = e.Graphics;
            graph.SmoothingMode = SmoothingMode.HighQuality;
            graph.TextRenderingHint = TextRenderingHint.AntiAlias;

            var blackDiscBrush = new SolidBrush(this.blackDiscColor);
            var whiteDiscBrush = new SolidBrush(this.whiteDiscColor);
            var gridSize = (float)this.posDisplay.Width / Position.BOARD_SIZE;
            var discSize = gridSize * DISC_SIZE_RATIO_TO_GRID_SIZE;
            var margin = (gridSize - discSize) * 0.5f;
            var font = new Font(this.moveNumFontFamily, discSize * MOVE_NUM_RATIO_TO_DISC_SIZE, GraphicsUnit.Pixel);
            var blackMoveNumBrush = new SolidBrush(this.blackMoveNumTextColor);
            var whiteMoveNumBrush = new SolidBrush(this.whiteMoveNumTextColor);
            var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
            };

            var moveNum = 0;
            foreach (var move in this.pos.MoveHistroy)
            {
                if (move.Coord == BoardCoordinate.Pass)
                    continue;

                (var brush, var moveNumBrush) = (move.Player == DiscColor.Black)
                    ? (blackDiscBrush, blackMoveNumBrush)
                    : (whiteDiscBrush, whiteMoveNumBrush);
                var gridX = ((int)move.Coord % Position.BOARD_SIZE) * gridSize;
                var gridY = ((int)move.Coord / Position.BOARD_SIZE) * gridSize;
                var x = gridX + margin;
                var y = gridY + margin;
                graph.FillEllipse(brush, x, y, discSize, discSize);

                var rect = new RectangleF(x, y, discSize, discSize);
                graph.DrawString((++moveNum).ToString(), font, moveNumBrush, rect, format);
            }

            foreach (var loc in Position.CrossCoordinates)
            {
                var brush = (loc.color == DiscColor.Black) ? blackDiscBrush : whiteDiscBrush;
                var x = ((int)loc.coord % Position.BOARD_SIZE) * gridSize + margin;
                var y = ((int)loc.coord / Position.BOARD_SIZE) * gridSize + margin;
                graph.FillEllipse(brush, x, y, discSize, discSize);
            }
        }
    }
}
