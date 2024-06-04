using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Kalmia_OpenLab.SDLWrapper;

using Kalmia_OpenLab.View.Controls;

namespace Kalmia_OpenLab.View.Scenes
{
    internal class Difficulty
    {
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string WinMessage { get; set; } = string.Empty;
        public string DrawMessage { get; set; } = string.Empty;
        public string LossMessage { get; set; } = string.Empty;
        public int Level { get; set; } = 1;

        public override string ToString() => this.Label;
    }

    public partial class DifficultySelectionScene : UserControl
    {
        bool transitionToNextScene = false;

        public DifficultySelectionScene()
        {
            InitializeComponent();
            this.Controls.HideAll();
        }

        static IEnumerable<Difficulty> LoadDifficulties()
        {
            var count = 0;
            foreach (var files in Directory.GetFiles(FilePath.DifficultyDirPath))
                if (Path.GetFileName(files) == $"{count}.json")
                {
                    var difficulty = JsonSerializer.Deserialize<Difficulty>(File.ReadAllText(files));
                    if (difficulty is not null)
                        yield return difficulty;
                    count++;
                }
        }

        void DifficultySelectionScene_Load(object sender, EventArgs e)
        {
            this.selectMenu.AddItemRange(LoadDifficulties());
            this.fadeIn.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 1000);
            Thread.Sleep(1);
            this.Controls.ShowAll();
        }

        void SelectMenu_OnSelectedIdxChanged(SelectMenu<Difficulty> sender, int selectedIdx)
        {
            if (this.transitionToNextScene)
                return;

            if (selectedIdx != -1)
                AudioMixer.PlayChannel(-1, GlobalSE.CursorSE);
            this.descriptionLabel.Text = sender.SelectedItem?.Description;
        }

        void SelectMenu_OnLeftClickItem(SelectMenu<Difficulty> sender, int selectedIdx)
        {
            if (this.transitionToNextScene)
                return;

            AudioMixer.PlayChannel(-1, GlobalSE.ButtonPressSE);

            this.fadeOut.OnEndAnimation += (s, e) =>
            {
                if (this.Parent is not MainForm)
                    return;

                var mainForm = (MainForm)this.Parent;
                mainForm.Invoke(() => mainForm.ChangeUserControl(new DiscSelectionScene(sender.Items[selectedIdx])));
            };

            this.transitionToNextScene = true;
            this.fadeOut.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 1000);
        }
    }
}
