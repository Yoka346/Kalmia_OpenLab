using System;
using System.Windows.Forms;

using Kalmia_OpenLab.SDLWrapper;
using Kalmia_OpenLab.View.Controls;

namespace Kalmia_OpenLab.View.Scenes
{
    public partial class TitleScene : UserControl
    {
        bool transitionToNextScene = false;
        MixerMusic bgm;

        public TitleScene()
        {
            this.bgm = MixerMusic.LoadMusic($"{FilePath.BGMDirPath}title.ogg");
            InitializeComponent();
        }

        void TitleScene_Load(object sender,EventArgs e)
        {
            this.fadeIn.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, HEADING_FADE_IN_MS);
            AudioMixer.PlayMusic(this.bgm);
        }

        void TitleScene_Disposed(object sender, EventArgs e) => this.bgm.Dispose();

        void TitleScene_SizeChanged(object sender, EventArgs e) => this.selectMenu.Width = this.Width;

        void FadeIn_OnEndAnimation(object sender, EventArgs e)
            => this.fadeInSubheading.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, SUBHEADING_FADE_IN_MS);

        void SelectMenu_OnSelectedIdxChanged(SelectMenu<string> sender, int selectedIdx)
        {
            if (!this.transitionToNextScene && selectedIdx != -1)
                AudioMixer.PlayChannel(-1, GlobalSE.CursorSE, 0);
        }

        void SelectMenu_OnLeftClickItem(SelectMenu<string> sender, int selectedIdx)
        {
            if (this.transitionToNextScene)
                return;

            AudioMixer.PlayChannel(-1, GlobalSE.ButtonPressSE, 0);

            var selectedItem = sender.SelectedItem;

            UserControl? nextScene;
            if (selectedItem == "Start")
                nextScene = new GameTypeSelectionScene();
            else if (selectedItem == "About Research")
                nextScene = new PosterScene();
            else
                nextScene = null;


            this.fadeOut.OnEndAnimation += (sender, e) =>
            {
                if (this.Parent is not MainForm)
                    return;

                var mainForm = (MainForm)this.Parent;
                mainForm.Invoke(() => mainForm.ChangeUserControl(nextScene));    
            };

            this.transitionToNextScene = true;
            this.fadeOut.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, FADE_OUT_MS);
        }
    }
}
