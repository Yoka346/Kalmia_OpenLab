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

using Kalmia_OpenLab.Model.Game;
using Kalmia_OpenLab.SDLWrapper;

using Kalmia_OpenLab.View.Controls;

namespace Kalmia_OpenLab.View.Scenes
{
    internal class Difficulty
    {
        public string Label { get; set; } = string.Empty;
        public string[] Descriptions { get; set; } = [string.Empty, string.Empty];
        public string[] WinMessages { get; set; } = [string.Empty, string.Empty];
        public string[] DrawMessages { get; set; } = [string.Empty, string.Empty];
        public string[] LossMessages { get; set; } = [string.Empty, string.Empty];
        public int Level { get; set; } = 1;
        public int TTSize { get; set; } = 1024;

        public override string ToString() => this.Label;
    }

    internal partial class DifficultySelectionScene : UserControl
    {
        bool transitionToNextScene = false;

        GameType gameType;

        public DifficultySelectionScene(GameType gameType)
        {
            this.gameType = gameType;   
            InitializeComponent();
            this.Controls.HideAll();
        }

        static IEnumerable<Difficulty> LoadDifficulties()
        {
            var count = 0;
            foreach (var file in Directory.GetFiles(FilePath.DifficultyDirPath))
            {
                if (Path.GetFileName(file) == $"{count}.json")
                {
                    var difficulty = JsonSerializer.Deserialize<Difficulty>(File.ReadAllText(file));
                    if (difficulty is not null)
                        yield return difficulty;
                    count++;
                }
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
            this.descriptionLabel.Text = sender.SelectedItem?.Descriptions[(this.gameType == GameType.Normal) ? 0 : 1];
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
                mainForm.Invoke(() => mainForm.ChangeUserControl(new DiscSelectionScene(this.gameType, sender.Items[selectedIdx])));
            };

            this.transitionToNextScene = true;
            this.fadeOut.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 1000);
        }
    }
}
