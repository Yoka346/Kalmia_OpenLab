using System.Drawing;
using System.Security.Policy;
using System.Windows.Forms;

using Kalmia_OpenLab.View.Controls;

namespace Kalmia_OpenLab.View.Scenes
{
    partial class HandicapSelectionScene
    {
        const int TEXT_ALPHA = 255;
        const int SELECT_MENU_NOT_SELECTED_ALPHA = 128;

        System.ComponentModel.IContainer components = null;

        TransparentLabel requestLabel;
        SelectMenu<string> selectMenu;
        PositionViewer posViewer;
        TransparentLabel descriptionLabel;

        Animator fadeIn;
        Animator fadeOut;
        
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
            this.components = new System.ComponentModel.Container();
            this.BackColor = Color.Black;

            if (GlobalConfig.Instance.FullScreen)
                this.Size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            else
                this.Size = new Size(GlobalConfig.Instance.ScreenWidth, GlobalConfig.Instance.ScreenHeight);

            // Request label
            this.requestLabel = new TransparentLabel
            {
                ForeColor = Color.White,
                Text = "ハンデの有無を選択してください",
                Location = new Point(0, this.Height / 20),
                Size = new Size(this.Width, this.Height / 20),
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, this.Height / 30, GraphicsUnit.Pixel),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            this.Controls.Add(this.requestLabel);

            // Position viewer
            var size = (int)(this.Width * (1.0f / 3.5f));
            this.posViewer = new PositionViewer()
            {
                Size = new Size(size, size),
                Location = new Point((this.Width - size) / 2, (this.Height - size) / 2),
                CoordFontFamily = GlobalConfig.Instance.DefaultFontFamily,
                BoardBackColor = Color.Green,
                ShowLegalMovePointers = false,
                BackgroundImage = Image.FromFile($"{FilePath.GraphDirPath}board_viewer_bg.bmp"),
                BoardBackgroundImage = Image.FromFile($"{FilePath.GraphDirPath}board_bg.bmp")
            };
            this.Controls.Add(this.posViewer);

            // Select menu
            this.selectMenu = new SelectMenu<string>(this.Width / 3, this.Height / 4, 0, (int)(this.Height - this.Height * 0.6)) { BackColor = Color.Transparent };
            this.selectMenu.AddItemRange(["ハンデなし", "1子局", "2子局", "3子局", "4子局"]);
            this.Controls.Add(this.selectMenu);

            // Description label
            this.descriptionLabel = new TransparentLabel
            {
                ForeColor = Color.White,
                Location = new Point(0, (int)(this.Height - this.Height * 0.2)),
                Size = new Size(this.Width, (int)(this.Height * 0.05)),
                Font = new Font(GlobalConfig.Instance.DefaultFontFamily, (int)(this.Height * 0.04), GraphicsUnit.Pixel),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(this.descriptionLabel);

            // Animation
            this.fadeIn = new Animator((frameCount, frameNum) =>
            {
                setAlphaRate((double)frameCount / (frameNum - 1));
                return true;
            });

            this.fadeOut = new Animator((frameCount, frameNum) =>
            {
                setAlphaRate(1.0 - (double)frameCount / (frameNum - 1));
                return true;
            });

            void setAlphaRate(double rate)
            {
                this.requestLabel.ForeColor = Color.FromArgb((int)(TEXT_ALPHA * rate), this.requestLabel.ForeColor);

                var color = Color.FromArgb((int)(SELECT_MENU_NOT_SELECTED_ALPHA * rate), this.selectMenu.NotSelectedTextColor);
                this.selectMenu.NotSelectedTextColor = color;

                color = Color.FromArgb((int)(TEXT_ALPHA * rate), this.selectMenu.SelectedTextColor);
                this.selectMenu.SelectedTextColor = color;

                color = Color.FromArgb((int)(TEXT_ALPHA * rate), this.descriptionLabel.ForeColor);
                this.descriptionLabel.ForeColor = color;
            }

            // Events
            Load += HandicapSelectionScene_Load;
            this.selectMenu.OnSelectedIdxChanged += SelectMenu_OnSelectedIdxChanged;
            this.selectMenu.OnLeftClickItem += SelectMenu_OnLeftClickItem;
        }
    }
}
