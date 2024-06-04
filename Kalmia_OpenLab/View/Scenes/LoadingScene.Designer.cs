using Kalmia_OpenLab.View.Controls;
using System.Drawing;
using System;
using System.Windows.Forms;

namespace Kalmia_OpenLab.View.Scenes
{
    partial class LoadingScene
    {
        private System.ComponentModel.IContainer components = null;

        TransparentLabel loadingLabel;

        Animator blinkLoadingLabelText;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.BackColor = Color.Black;

            if (GlobalConfig.Instance.FullScreen)
                this.Size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            else
                this.Size = new Size(GlobalConfig.Instance.ScreenWidth, GlobalConfig.Instance.ScreenHeight);

            // loading label
            this.loadingLabel = new TransparentLabel
            {
                Text = "Now Loading",
                Location = new Point(0, 0),
                Size = this.Size,
                ForeColor = Color.White,
                BackColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, this.Width / 30.0f, GraphicsUnit.Pixel)
            };
            this.Controls.Add(this.loadingLabel);

            // Animation
            this.blinkLoadingLabelText = new Animator((frameCount, frameNum) =>
            {
                const double SPEED = 0.1;
                var rate = (Math.Sin(SPEED * frameCount) + 1.0) * 0.5;
                Invoke(() => this.loadingLabel.ForeColor = Color.FromArgb((int)(255.0 * rate), Color.White));
                return this.nextScene is null;
            });

            // Events
            this.Load += LoadingScene_Load;
        }
    }
}
