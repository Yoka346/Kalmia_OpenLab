using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Kalmia_OpenLab.SDLWrapper;
using Kalmia_OpenLab.View.Controls;

namespace Kalmia_OpenLab.View.Scenes
{
    public partial class PosterScene : UserControl
    {
        Image backgroundImage;
        MixerChunk posterSE;

        public PosterScene()
        {
            InitializeComponent();
            this.backgroundImage = Image.FromFile($"{FilePath.GraphDirPath}poster.bmp");
            this.posterSE = MixerChunk.LoadWav($"{FilePath.SEDirPath}poster.ogg");
        }

        ~PosterScene() => this.posterSE.Dispose();

        void PosterScene_Load(object sender, EventArgs e)
        {
            this.blinkBackToTitleLabel.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, int.MaxValue);
            AudioMixer.PlayChannel(-1, this.posterSE);
        }

        void PosterBox_Paint(object sender, PaintEventArgs e)
        {
            var ratio = (float)this.posterBox.Height / this.backgroundImage.Height;
            var width = this.backgroundImage.Width * ratio;
            var height = this.backgroundImage.Height * ratio;   
            var margin = (this.posterBox.Width - width) * 0.5f;

            e.Graphics.DrawImage(this.backgroundImage, (int)margin, 0, (int)width, (int)height);
        }

        void BackToTitleLabel_MouseClick(object sender, MouseEventArgs e)
        {
            this.blinkBackToTitleLabel.Stop();
            AudioMixer.PlayChannel(-1, this.posterSE);

            if (this.Parent is MainForm mainForm)
                mainForm.ChangeUserControl(new TitleScene());
        }

        void BackToTitleLabel_MouseLeave(object sender, EventArgs e)
            => this.blinkBackToTitleLabel.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, int.MaxValue);

        void BackToTitleLabel_MouseEnter(object sender, EventArgs e)
            => this.blinkBackToTitleLabel.Stop();

        void BlinkBackToTitleLabel_OnEndAnimation(object sender, EventArgs e) 
            => this.backToTitleLabel.ForeColor = Color.FromArgb(255, this.backToTitleLabel.ForeColor);
    }
}
