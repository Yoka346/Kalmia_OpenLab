using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kalmia_OpenLab.View.Scenes
{
    public partial class LoadingScene : UserControl
    {
        Func<UserControl> Loading;
        UserControl? nextScene;

        public LoadingScene(Func<UserControl> nextSceneConstructor)
        {
            this.Loading = nextSceneConstructor;
            InitializeComponent();
        }

        async void LoadingScene_Load(object sender, EventArgs e)
        {
            this.blinkLoadingLabelText.OnEndAnimation += BlinkLoadingLabelText_OnEndAnimation;
            this.blinkLoadingLabelText.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, int.MaxValue);
            await Task.Run(() => this.nextScene = this.Loading.Invoke());
        }

        void BlinkLoadingLabelText_OnEndAnimation(object? sender, EventArgs e) => Invoke(() =>
        {
            if (this.Parent is MainForm form)
                form.ChangeUserControl(this.nextScene);
        });
    }
}
