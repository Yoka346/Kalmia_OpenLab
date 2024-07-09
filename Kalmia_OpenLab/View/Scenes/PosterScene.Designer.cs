using System;
using System.Drawing;
using System.Windows.Forms;

using Kalmia_OpenLab.SDLWrapper;
using Kalmia_OpenLab.View.Controls;

namespace Kalmia_OpenLab.View.Scenes
{
    partial class PosterScene
    {
        const float BLINK_SPEED = 0.1f;

        /// <summary> 
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        PictureBox posterBox;
        TransparentLabel backToTitleLabel;

        Animator blinkBackToTitleLabel;

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

            this.posterBox = new PictureBox
            {
                Size = new Size(this.Width, 9 * this.Height / 10)
            };

            this.backToTitleLabel = new TransparentLabel
            {
                Text = "ここをクリックしてタイトルへ戻る",
                ForeColor = Color.White,
                Size = new Size(this.Width, this.Height / 25),
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, this.Height / 25, GraphicsUnit.Pixel),
                Location = new Point(0, this.posterBox.Height),
                TextAlign = ContentAlignment.TopCenter
            };
            this.backToTitleLabel.MouseEnter += BackToTitleLabel_MouseEnter;
            this.backToTitleLabel.MouseLeave += BackToTitleLabel_MouseLeave;
            this.backToTitleLabel.MouseClick += BackToTitleLabel_MouseClick;

            this.Controls.Add(this.posterBox);
            this.Controls.Add(this.backToTitleLabel);

            // Animation
            this.blinkBackToTitleLabel = new Animator((frameCount, frameNum) =>
            {
                var rate = (MathF.Sin(BLINK_SPEED * frameCount) + 1.0f) * 0.5f;
                if (!this.Disposing && !this.IsDisposed)
                {
                    Invoke(() => this.backToTitleLabel.ForeColor = Color.FromArgb((int)(255.0f * rate), this.backToTitleLabel.ForeColor));
                    return true;
                }
                return false;
            });
            this.blinkBackToTitleLabel.OnEndAnimation += BlinkBackToTitleLabel_OnEndAnimation;

            // Events
            this.Load += PosterScene_Load;
            this.posterBox.Paint += PosterBox_Paint;
        }
    }
}
