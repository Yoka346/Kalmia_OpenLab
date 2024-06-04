using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Kalmia_OpenLab.View.Controls;

namespace Kalmia_OpenLab.View.Scenes
{
    partial class TitleScene
    {
        const int TEXT_ALPHA = 255;
        const int NOT_SELECTED_TEXT_ALPHA = 128;

        const int HEADING_FADE_IN_MS = 1500;
        const int SUBHEADING_FADE_IN_MS = 1000;
        const int FADE_OUT_MS = 2000;

        private IContainer components = null;

        TransparentLabel headingLabel;
        SelectMenu<string> selectMenu;
        TransparentLabel subheadingLabel;

        Animator fadeIn;
        Animator fadeOut;
        Animator fadeInSubheading;

        void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = Color.Black;

            if(GlobalConfig.Instance.FullScreen)
                this.Size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            else
                this.Size = new Size(GlobalConfig.Instance.ScreenWidth, GlobalConfig.Instance.ScreenHeight);

            // Controls

            // Heading
            this.headingLabel = new TransparentLabel
            {
                Text = "リバーシAI",
                ForeColor = Color.White,
                Location = new Point(0, this.Height / 10),
                Size = new Size(this.Width, this.Height / 5),
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, this.Height / 10, GraphicsUnit.Pixel),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.Controls.Add(this.headingLabel);

            // Subheading
            this.subheadingLabel = new TransparentLabel
            {
                Text = "~リバーシにおける人間の知識を用いない特徴量の自動設計~",
                ForeColor = Color.FromArgb(0, Color.White),
                Size = new Size(this.Width, this.Height / 8),
                Location = new Point(0, this.headingLabel.Location.Y + this.headingLabel.Height),
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, this.Height / 16, FontStyle.Italic, GraphicsUnit.Pixel),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.Controls.Add(this.subheadingLabel);

            // SelectMenu
            this.selectMenu = new SelectMenu<string>(this.Width / 3, this.Height / 5, this.Width / 3, (int)(3.5f * this.Height / 5.0f));
            this.selectMenu.AddItemRange(new string[] { "Start", "About Research", "Quit" });
            this.Controls.Add(selectMenu);

            // Animations

            this.fadeIn = new Animator((frameCount, numFrames) =>
            {
                var rate = (double)frameCount / (numFrames - 1);
                this.headingLabel.ForeColor = Color.FromArgb((int)(TEXT_ALPHA * rate), this.headingLabel.ForeColor);

                var color = Color.FromArgb((int)(NOT_SELECTED_TEXT_ALPHA * rate), this.selectMenu.NotSelectedTextColor);
                this.selectMenu.NotSelectedTextColor = color;

                color = Color.FromArgb((int)(TEXT_ALPHA * rate), this.selectMenu.SelectedTextColor);
                this.selectMenu.SelectedTextColor = color;
                return true;
            });

            this.fadeInSubheading = new Animator((frameCount, frameNum) =>
            {
                var rate = (double)frameCount / (frameNum - 1);
                this.subheadingLabel.ForeColor
                = Color.FromArgb((int)(TEXT_ALPHA * rate), this.subheadingLabel.ForeColor);
                return true;
            });

            this.fadeOut = new Animator((frameCount, numFrames) =>
            {
                var rate = 1.0 - (double)frameCount / (numFrames - 1);
                this.headingLabel.ForeColor = Color.FromArgb((int)(TEXT_ALPHA * rate), this.headingLabel.ForeColor);

                var color = Color.FromArgb((int)(NOT_SELECTED_TEXT_ALPHA * rate), this.selectMenu.NotSelectedTextColor);
                this.selectMenu.NotSelectedTextColor = color;

                color = Color.FromArgb((int)(TEXT_ALPHA * rate), this.selectMenu.SelectedTextColor);
                this.selectMenu.SelectedTextColor = color;

                color = Color.FromArgb((int)(TEXT_ALPHA * rate), this.subheadingLabel.ForeColor);
                this.subheadingLabel.ForeColor = color;
                return true;
            });

            // Events
            this.Load += TitleScene_Load;
            this.Disposed += TitleScene_Disposed;
            this.SizeChanged += TitleScene_SizeChanged;
            this.selectMenu.OnLeftClickItem += SelectMenu_OnLeftClickItem;
            this.selectMenu.OnSelectedIdxChanged += SelectMenu_OnSelectedIdxChanged;
            this.fadeIn.OnEndAnimation += FadeIn_OnEndAnimation;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }
    }
}
