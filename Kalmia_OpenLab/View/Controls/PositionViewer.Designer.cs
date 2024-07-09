using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Kalmia_OpenLab.Model.Reversi;

namespace Kalmia_OpenLab.View.Controls
{
    internal delegate void PositionViewerEventHandler(PositionViewer sender, BoardCoordinate coord);

    partial class PositionViewer
    {
        const float MARGIN_SIZE_RATIO = 0.05f;
        const float DISC_SIZE_RATIO_TO_GRID_SIZE = 0.8f;
        const float MOVE_NUM_RATIO_TO_DISC_SIZE = 0.6f;
        const float COORD_SIZE_RATIO_TO_GRID_SIZE = 0.3333f;

        public Color GridColor { get => this.gridColor; set { this.gridColor = value; Invalidate(); } }
        public Color BlackDiscColor { get => this.blackDiscColor; set { this.blackDiscColor = value; Invalidate(); } }
        public Color WhiteDiscColor { get => this.whiteDiscColor; set { this.whiteDiscColor = value; Invalidate(); } }
        public Color LegalMovePointerColor { get => this.legalMovePointerColor; set { this.legalMovePointerColor = value; Invalidate(); } }
        public Color CoordLabelTextColor { get => this.coordLabelTextColor; set { this.coordLabelTextColor = value; Invalidate(); } }
        public string CoordFontFamily { get => this.coordFontFamily; set { this.coordFontFamily = value; Invalidate(); } }
        public Color BlackMoveNumTextColor { get => this.blackMoveNumTextColor; set { this.blackMoveNumTextColor = value; Invalidate(); } }
        public Color WhiteMoveNumTextColor { get => this.whiteMoveNumTextColor; set { this.whiteMoveNumTextColor = value; Invalidate(); } }
        public string MoveNumFontFamily { get => this.moveNumFontFamily; set { this.moveNumFontFamily = value; Invalidate(); } }
        public Color BoardBackColor { get => this.posDisplay.BackColor; set => this.posDisplay.BackColor = value; }
        public Image BoardBackgroundImage { get => this.boardBackgroundImage; set { this.boardBackgroundImage = value; this.posDisplay.Invalidate(); } }
        public bool ShowLegalMovePointers { get => showLegalMovePointers; set { this.showLegalMovePointers = value; this.posDisplay.Invalidate(); } }
        public bool ShowMoveHistory { get => showMoveHistory; set { this.showMoveHistory = value; this.posDisplay.Invalidate(); } }

        public override Image BackgroundImage { get => this.backgroundImage; set { this.backgroundImage = value; Invalidate(); } }

        public event PositionViewerEventHandler OnMouseClicked = delegate { };

        private System.ComponentModel.IContainer components = null;

        Image backgroundImage;
        Image boardBackgroundImage;
        Color gridColor = Color.Black;
        Color blackDiscColor = Color.Black;
        Color whiteDiscColor = Color.White;
        Color legalMovePointerColor = Color.Red;
        Color coordLabelTextColor = Color.White;
        string coordFontFamily = Control.DefaultFont.FontFamily.Name;
        Color blackMoveNumTextColor = Color.White;
        Color whiteMoveNumTextColor = Color.Black;
        string moveNumFontFamily = Control.DefaultFont.FontFamily.Name;
        bool showLegalMovePointers = true;
        bool showMoveHistory = false;
        int legalMovePointerThickness = 3;

        PictureBox posDisplay;
        TransparentLabel[] XCoordLabels;
        TransparentLabel[] YCoordLabels;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.DoubleBuffered = true;
            this.BackColor = Color.Brown;

            // Controls

            // Position display
            var margin = this.Width * MARGIN_SIZE_RATIO;
            this.posDisplay = new PictureBox
            {
                Size = new Size((int)(this.Width - margin * 2.0f), (int)(this.Width - margin * 2.0f)),
                Location = new Point((int)margin, (int)margin),
                BackColor = Color.Green,
                SizeMode = PictureBoxSizeMode.Normal,
                Anchor = AnchorStyles.None,
            };
            AdjustPosDisplay();
            this.Controls.Add(this.posDisplay);

            // Coordinate labels
            this.XCoordLabels = (from i in Enumerable.Range(0, Position.BOARD_SIZE)
                                 select new TransparentLabel
                                 {
                                     Text = ((char)('A' + i)).ToString(),
                                     TextAlign = ContentAlignment.MiddleCenter,
                                     BackColor = Color.Transparent
                                 }).ToArray();

            this.YCoordLabels = (from i in Enumerable.Range(0, Position.BOARD_SIZE)
                                 select new TransparentLabel
                                 {
                                     Text = (i + 1).ToString(),
                                     TextAlign = ContentAlignment.MiddleCenter,
                                     BackColor = Color.Transparent
                                 }).ToArray();
            AdjustCoordLabels();
            this.Controls.AddRange(this.XCoordLabels);
            this.Controls.AddRange(this.YCoordLabels);

            // Events
            this.SizeChanged += (s, e) => { this.Height = this.Width; Invalidate(); };
            this.Paint += PositionViewer_Paint;
            this.posDisplay.MouseClick += PosDisplay_MouseClick;

            this.posDisplay.Paint += PosDisplay_Paint;
        }
    }
}
