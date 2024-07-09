using System;
using System.Windows.Forms;

using Kalmia_OpenLab.Model.Reversi;
using Kalmia_OpenLab.SDLWrapper;

namespace Kalmia_OpenLab.View.Scenes
{
    internal partial class ResultScene : UserControl
    {
        string message;
        Position endGamePos;

        MixerMusic bgm;

        public ResultScene(string message, Position endGamePos, string bgmPath)
        {
            this.message = message;
            this.endGamePos = endGamePos;
            this.bgm = MixerMusic.LoadMusic(bgmPath);
            InitializeComponent();
        }

        void ResultScene_Load(object sender, EventArgs e)
        {
            this.showScene.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 500);
            this.blinkBackToTitleLabel.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, int.MaxValue);
            AudioMixer.PlayMusic(this.bgm);
        }

        void ResultScene_Disposed(object sender, EventArgs e) => this.bgm.Dispose();

        void ResultScene_Click(object sender, EventArgs e)
        {
            this.blinkBackToTitleLabel.Stop();
            this.backToTitleLabel.Hide();
            this.hideScene.OnEndAnimation += HideScene_OnEndAnimation;
            this.hideScene.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 1000);
            AudioMixer.FadeOutMusic(1000);
        }

        void HideScene_OnEndAnimation(object? sender, EventArgs e) => Invoke(() =>
        {
            if (this.Parent is MainForm mainForm)
                mainForm.ChangeUserControl(new TitleScene());
        });
    }
}
