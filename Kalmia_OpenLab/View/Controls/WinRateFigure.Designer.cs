using System.Drawing;
using System.Windows.Forms;

namespace Kalmia_OpenLab.View.Controls
{
    partial class WinRateFigure
    {
        private System.ComponentModel.IContainer components = null;

        public int FigureLineWidth { get; set; } = 3;
        public Color FigureColor { get; set; } = Color.Yellow;
        public Color EvenLineColor { get; set; } = Color.White;
        public Color EndBarColor { get; set; } = Color.Red;

        public override Font Font { get => base.Font; set { this.blackLabel.Font = this.whiteLabel.Font = base.Font = value; } }

        TransparentLabel blackLabel;
        TransparentLabel whiteLabel;
        PictureBox figureDisplay;

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

            // Controls
            (var labelWidth, var labelHeight) = (this.Width / 10, this.Height / 2);
            this.blackLabel = new TransparentLabel
            {
                Text = "黒",
                Size = new Size(labelWidth, labelHeight),
                Location = new Point(0, 0),
                Font = this.Font,
                ForeColor = Color.White,
                BackColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(this.blackLabel);

            this.whiteLabel = new TransparentLabel
            {
                Text = "白",
                Size = new Size(labelWidth, labelHeight),
                Location = new Point(0, this.blackLabel.Height),
                Font = this.Font,
                ForeColor = Color.Black,
                BackColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(this.whiteLabel);

            this.figureDisplay = new PictureBox
            {
                Size = new Size(this.Width - labelWidth, this.Height),
                Location = new Point(labelWidth, 0),
            };
            this.Controls.Add(this.figureDisplay);

            // Events
            this.SizeChanged += WinRateGraph_SizeChanged;
            this.figureDisplay.Paint += FigureDisplay_Paint;
        }
    }
}
