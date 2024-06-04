using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Kalmia_OpenLab.Model.Reversi;
using Kalmia_OpenLab.SDLWrapper;
using Kalmia_OpenLab.View.Controls;

namespace Kalmia_OpenLab.View.Scenes
{
    internal partial class DiscSelectionScene : UserControl
    {
        bool transitionToNextScene = false;

        Difficulty difficulty;

        public DiscSelectionScene(Difficulty difficulty)
        {
            this.difficulty = difficulty;
            InitializeComponent();
            this.Controls.HideAll();
        }

        void DiscSelectionScene_Load(object sender, EventArgs e)
        {
            this.fadeIn.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 1000);
            this.Controls.ShowAll();
        }

        void SelectMenu_OnSelectedIdxChanged(SelectMenu<DiscColor> sender, int selectedIdx)
        {
            if (this.transitionToNextScene)
                return;

            if (selectedIdx == -1)
            {
                this.descriptionLabel.Text = string.Empty;
                return;
            }

            AudioMixer.PlayChannel(-1, GlobalSE.CursorSE);

            var disc = sender.SelectedItem;
            if (disc == DiscColor.Black)
                this.descriptionLabel.Text = "先手";
            else if (disc == DiscColor.White)
                this.descriptionLabel.Text = "後手";
        }

        void SelectMenu_OnLeftClickItem(SelectMenu<DiscColor> sender, int selectedIdx)
        {
            if (this.transitionToNextScene)
                return;

            AudioMixer.PlayChannel(-1, GlobalSE.ButtonPressSE);

            this.fadeOut.OnEndAnimation += (s, e) =>
            {
                if (this.Parent is MainForm mainForm)
                {
                    var gameSceneConstructor = () => new GameScene(this.difficulty, sender.Items[selectedIdx]);
                    Invoke(() => mainForm.ChangeUserControl(new LoadingScene(gameSceneConstructor)));
                }
            };

            this.transitionToNextScene = true;
            this.fadeOut.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 1000);
        }
    }
}
