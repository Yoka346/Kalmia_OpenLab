using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Kalmia_OpenLab.Model.Game;
using Kalmia_OpenLab.Model.Reversi;
using Kalmia_OpenLab.SDLWrapper;
using Kalmia_OpenLab.View.Controls;

namespace Kalmia_OpenLab.View.Scenes
{
    internal partial class GameTypeSelectionScene : UserControl
    {
        bool transitionToNextScene = false;

        public GameTypeSelectionScene()
        {
            InitializeComponent();
            this.Controls.HideAll();
        }

        void DiscSelectionScene_Load(object sender, EventArgs e)
        {
            this.fadeIn.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 1000);
            this.Controls.ShowAll();
        }

        void SelectMenu_OnSelectedIdxChanged(SelectMenu<string> sender, int selectedIdx)
        {
            if (this.transitionToNextScene)
                return;

            if (selectedIdx == -1)
            {
                this.descriptionLabel.Text = string.Empty;
                return;
            }

            AudioMixer.PlayChannel(-1, GlobalSE.CursorSE);

            if (sender.SelectedIdx == 0)
                this.descriptionLabel.Text = "石が多いほうが勝つ通常ルール";
            else if (sender.SelectedIdx == 1)
                this.descriptionLabel.Text = "石が少ないほうが勝つ特殊ルール! 普通のオセロとは異なる戦略が必要";
        }

        void SelectMenu_OnLeftClickItem(SelectMenu<string> sender, int selectedIdx)
        {
            if (this.transitionToNextScene)
                return;

            AudioMixer.PlayChannel(-1, GlobalSE.ButtonPressSE);

            this.fadeOut.OnEndAnimation += (s, e) =>
            {
                if (this.Parent is MainForm mainForm)
                {
                    var gameType = (selectedIdx == 0) ? GameType.Normal : GameType.Weakest;
                    Invoke(() => mainForm.ChangeUserControl(new DifficultySelectionScene(gameType)));
                }
            };

            this.transitionToNextScene = true;
            this.fadeOut.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 1000);
        }
    }
}
