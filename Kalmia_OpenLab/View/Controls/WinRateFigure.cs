using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Kalmia_OpenLab.View.Controls
{
    public partial class WinRateFigure : UserControl
    {
        public float CurrentBlackWinRate
        {
            get => this.blackWinRates[^1];
            set { this.blackWinRates[^1] = value; this.figureDisplay.Invalidate(); }
        }

        public float CurrentWhiteWinRate
        {
            get => 100.0f - this.blackWinRates[^1];
            set { this.blackWinRates[^1] = 100.0f - value; this.figureDisplay.Invalidate(); }
        }

        public ReadOnlyCollection<float> BlackWinRates => new(this.blackWinRates);

        readonly List<float> blackWinRates = new();

        public WinRateFigure() => InitializeComponent();

        public void AddBlackWinRate(float blackWinRate)
        {
            this.blackWinRates.Add(blackWinRate);
            this.figureDisplay.Invalidate();
        }

        public void AddWhiteWinRate(float whiteWinRate)
        {
            this.blackWinRates.Add(100.0f - whiteWinRate);
            this.figureDisplay.Invalidate();
        }

        public void SetBlackWinRate(int idx, float blackWinRate)
        {
            this.blackWinRates[idx] = blackWinRate;
            this.figureDisplay.Invalidate();
        }

        public void SetWhiteWinRate(int idx, float whiteWinRate)
        {
            this.blackWinRates[idx] = 100.0f - whiteWinRate;
            this.figureDisplay.Invalidate();
        }

        void WinRateGraph_SizeChanged(object sender, EventArgs e)
        {
            (var labelWidth, var labelHeight) = (this.Width / 10, this.Height / 2);
            this.blackLabel.Size = this.whiteLabel.Size = new Size(labelWidth, labelHeight);
            this.blackLabel.Location = new Point(0, 0);
            this.whiteLabel.Location = new Point(0, this.blackLabel.Height);
            this.figureDisplay.Size = new Size(this.Width - labelWidth, this.Height);
            this.figureDisplay.Location = new Point(labelWidth, 0);
        }

        void FigureDisplay_Paint(object sender, PaintEventArgs e)
        {
            (var width, var height) = (this.figureDisplay.Width, this.figureDisplay.Height);
            var graph = e.Graphics;
            graph.SmoothingMode = SmoothingMode.HighQuality;
            graph.Clear(this.BackColor);
            graph.DrawLine(new Pen(this.EvenLineColor), 0.0f, height * 0.5f, width, height * 0.5f);

            if (this.blackWinRates.Count < 2)
                return;

            var plotWidth = width * 0.0125f;
            var points = new List<PointF>();
            for (var i = 0; i < this.blackWinRates.Count; i++)
                points.Add(new PointF(plotWidth * i, height * (100.0f - this.blackWinRates[i]) * 0.01f));

            graph.DrawLines(new Pen(this.FigureColor, this.FigureLineWidth), points.ToArray());
            graph.DrawLine(new Pen(this.EndBarColor), points[^1].X, 0.0f, points[^1].X, height);
        }
    }
}
