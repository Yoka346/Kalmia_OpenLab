using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Kalmia_OpenLab.View.Controls;
using Kalmia_OpenLab.SDLWrapper;
using Kalmia_OpenLab.Model.Game;
using Kalmia_OpenLab.Model.Reversi;

namespace Kalmia_OpenLab.View.Scenes
{
    public partial class HandicapSelectionScene : UserControl
    {
        bool transitionToNextScene = false;
        int numHandicapDiscs = 0;
        Position[] examplePositions = new Position[5];

        public HandicapSelectionScene()
        {
            InitializeComponent();
            this.Controls.HideAll();

            var handicapCoords = new BoardCoordinate[] { BoardCoordinate.A1, BoardCoordinate.H8, BoardCoordinate.H1, BoardCoordinate.A8 };
            for (var numHandi = 0; numHandi <= 4; numHandi++) 
            {
                var pos = new Position();
                foreach (var coord in handicapCoords[..numHandi])
                    pos.PutDisc(DiscColor.Black, coord);
                examplePositions[numHandi] = pos;
            }
        }

        void HandicapSelectionScene_Load(object sender, EventArgs e)
        {
            this.fadeIn.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 2000);
            this.Controls.ShowAll();
        }

        void SelectMenu_OnSelectedIdxChanged(SelectMenu<string> sender, int selectedIdx)
        {
            if (this.transitionToNextScene)
                return;

            if(selectedIdx == -1)
            {
                this.posViewer.SetPosition(this.examplePositions[0]);
                this.descriptionLabel.Text = string.Empty;
                return;
            }

            AudioMixer.PlayChannel(-1, GlobalSE.CursorSE);

            if(selectedIdx == 0)
            {
                this.posViewer.SetPosition(this.examplePositions[selectedIdx]);
                this.numHandicapDiscs = 0;
                this.descriptionLabel.Text = "通常のオセロ（黒が先手）";
            }
            else
            {
                this.posViewer.SetPosition(this.examplePositions[selectedIdx]);
                this.numHandicapDiscs = selectedIdx;
                this.descriptionLabel.Text = $"隅に石を{selectedIdx}つ置いた状態から開始（白が先手）";
            }
        }

        void SelectMenu_OnLeftClickItem(SelectMenu<string> sender, int selectedIdx)
        {
            if (this.transitionToNextScene)
                return;

            AudioMixer.PlayChannel(-1, GlobalSE.ButtonPressSE);

            this.fadeOut.OnEndAnimation += (s, e) =>
            {
                if(this.Parent is MainForm mainForm) 
                {
                    if(this.numHandicapDiscs == 0)
                        Invoke(() => mainForm.ChangeUserControl(new DifficultySelectionScene(GameType.Normal)));
                    else 
                    {
                        var path = $"{FilePath.DifficultyDirPath}handicap_{this.numHandicapDiscs}.json";
                        var difficulty = JsonSerializer.Deserialize<Difficulty>(File.ReadAllText(path));
                        var gameSceneConstructor = () => new GameScene(GameType.Normal, difficulty!, DiscColor.Black, this.numHandicapDiscs);
                        Invoke(() => mainForm.ChangeUserControl(new LoadingScene(gameSceneConstructor)));
                    }
                }
            };

            this.transitionToNextScene = true;
            this.fadeOut.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 1000);
        }
    }
}
