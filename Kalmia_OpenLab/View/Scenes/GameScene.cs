using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Kalmia_OpenLab.Model.Game;
using Kalmia_OpenLab.Model.Reversi;
using Kalmia_OpenLab.Model.Engine;
using Kalmia_OpenLab.SDLWrapper;
using Kalmia_OpenLab.View.Controls;

namespace Kalmia_OpenLab.View.Scenes
{
    class GameStatistic
    {
        public int HumanWinCount { get; set; }
        public int HumanDrawCount { get; set; }
        public int HumanLossCount { get; set; }
        public int TotalPlayCount => this.HumanWinCount + this.HumanLossCount + this.HumanDrawCount;
    }

    class GameRecord
    {
        public string Difficulty { get; set; } = string.Empty;
        public string BlackPlayerName { get; set; } = string.Empty;
        public string WhitePlayerName { get; set; } = string.Empty;
        public string WinnerName { get; set; } = string.Empty;
        public BoardCoordinate[] MoveHistroy { get; set; } = Array.Empty<BoardCoordinate>();
    }

    internal partial class GameScene : UserControl
    {
        GameManager gameManager;
        Difficulty difficulty;
        DiscColor humanDiscColor;
        List<GameRecord> gameRecords;
        Dictionary<string, GameStatistic> gameStats;
        NodeStats latestNodeStats;

        MixerChunk putDiscSE;
        MixerChunk gameStartSE;
        MixerChunk gameOverSE;
        MixerChunk cutInSE;

        public GameScene(GameType gameType, Difficulty difficulty, DiscColor discColor)
        {
            this.gameRecords = LoadGameRecords();
            this.gameStats = LoadGameStats();
            this.difficulty = difficulty;
            this.humanDiscColor = discColor;

            this.gameStartSE = MixerChunk.LoadWav($"{FilePath.SEDirPath}game_start.ogg");
            this.gameOverSE = MixerChunk.LoadWav($"{FilePath.SEDirPath}game_over.ogg");
            this.cutInSE = MixerChunk.LoadWav($"{FilePath.SEDirPath}cut_in.ogg");
            this.putDiscSE = MixerChunk.LoadWav($"{FilePath.SEDirPath}put_disc.ogg");

            var players = InitPlayers(gameType, difficulty, this.humanDiscColor);
            this.gameManager = new GameManager(gameType, players.black, players.white);

            InitializeComponent();
            this.Controls.HideAll();
        }

        static List<GameRecord> LoadGameRecords()
        {
            try
            {
                var ret = JsonSerializer.Deserialize<List<GameRecord>>(File.ReadAllText(FilePath.GameRecordFilePath));
                if (ret is null)
                    throw new NullReferenceException();
                return ret;
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is JsonException || ex is NullReferenceException)
            {
                var ret = new List<GameRecord>();
                SaveGameRecords(ret);
                return ret;
            }
        }

        static void SaveGameRecords(List<GameRecord> records)
        {
            using var fs = File.Create(FilePath.GameRecordFilePath);
            JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true });
            using var sw = new StreamWriter(fs);
            sw.WriteLine(JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true }));
        }

        static Dictionary<string, GameStatistic> LoadGameStats()
        {
            try
            {
                var ret = JsonSerializer.Deserialize<Dictionary<string, GameStatistic>>(File.ReadAllText(FilePath.GameStatisticFilePath));
                if (ret is null)
                    throw new NullReferenceException();
                return ret;
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is JsonException || ex is NullReferenceException)
            {
                var ret = new Dictionary<string, GameStatistic>();
                SaveGameStats(ret);
                return ret;
            }
        }

        static void SaveGameStats(Dictionary<string, GameStatistic> stats)
        {
            using var fs = File.Create(FilePath.GameStatisticFilePath);
            using var sw = new StreamWriter(fs);
            sw.WriteLine(JsonSerializer.Serialize(stats, new JsonSerializerOptions { WriteIndented = true }));
        }

        (IPlayer black, IPlayer white) InitPlayers(GameType gameType, Difficulty difficulty, DiscColor humanDiscColor)
        {
            string enginePath, workDir;
            if(gameType == GameType.Normal)
            {
                enginePath = $"{FilePath.EngineDirPath}/Kalmia.exe";
                workDir = FilePath.EngineDirPath;
            }
            else
            {
                enginePath = $"{FilePath.EngineDirPath}/Weakest/Kalmia.exe";
                workDir = $"{FilePath.EngineDirPath}/Weakest";
            }

            var human = new HumanPlayer("You", humanDiscColor);
            var engine = new EnginePlayer(enginePath, "", workDir, difficulty.Level, [$"set option tt_size_mib {difficulty.TTSize}"], (DiscColor)((int)humanDiscColor));
            engine.OnNodeStatsRecieved += Engine_OnNodeStatsRecieved;
            engine.OnSearchInfoRecieved += Engine_OnSearchInfoRecieved;
            engine.OnUnexpectedShutdownOccured += Engine_OnUnexpectedShutdownOccured;
            return (humanDiscColor == DiscColor.Black) ? (human, engine) : (engine, human);
        }

        void GameScene_Load(object sender, EventArgs e)
        {
            if (this.gameManager.BlackPlayer is HumanPlayer)
                this.posViewer.ShowLegalMovePointers = true;
            this.messageLabel.Text = "対局開始!!";
            this.blackPlayerNameLabel.Text = this.gameManager.BlackPlayer.Name;
            this.whitePlayerNameLabel.Text = this.gameManager.WhitePlayer.Name;

            BindGameEvents();
            InitLabelsText();

            this.showGameScene.OnEndAnimation += ShowGameScene_OnEndAnimation;
            this.showGameScene.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 500);
            Thread.Sleep(1);
            this.Controls.ShowAll();
        }

        void GameScene_Disposed(object sender, EventArgs e)
        {
            if (this.gameManager.NowPlaying)
                this.gameManager.Suspend();

            this.gameManager.Dispose();

            this.gameStartSE.Dispose();
            this.gameOverSE.Dispose();
            this.cutInSE.Dispose();
        }

        void BindGameEvents()
        {
            this.gameManager.OnSideToMoveChanged += GameManager_OnSideToMoveChanged;
            this.gameManager.OnPlayBlack += GameManager_OnPlayBlack;
            this.gameManager.OnPlayWhite += GameManager_OnPlayWhite;
            this.gameManager.OnGameIsOver += GameManager_OnGameOver;
        }

        void ShowGameScene_OnEndAnimation(object? sender, EventArgs e)
        {
            Invoke(() => this.cutInLabel.Text = "対局開始");
            this.cutOut.OnEndAnimation += StartGame;
            StartCutInOutAnimation();
            AudioMixer.PlayChannel(-1, this.gameStartSE);
        }

        void HideGameScene_OnEndAnimation(object? sender, EventArgs e) => Invoke(() =>
        {
            if (this.Parent is MainForm mainForm)
            {
                var result = this.gameManager.GetGameResult();
                string message;
                string bgmPath;
                if (result.Draw)
                {
                    message = this.difficulty.DrawMessage;
                    bgmPath = $"{FilePath.BGMDirPath}draw.ogg";
                }
                else if (result.Winner == this.humanDiscColor)
                {
                    message = this.difficulty.WinMessage;
                    bgmPath = $"{FilePath.BGMDirPath}win.ogg";
                }
                else
                {
                    message = this.difficulty.LossMessage;
                    bgmPath = $"{FilePath.BGMDirPath}loss.ogg";
                }
                mainForm.ChangeUserControl(new ResultScene(message, this.gameManager.GetPosition(), bgmPath));   
            }
        });

        void BlinkNextSceneLabel_OnEndAnimation(object sender, EventArgs e)
            => this.nextSceneLabel.ForeColor = Color.FromArgb(255, this.nextSceneLabel.ForeColor);

        void StartGame(object? sender, EventArgs e)
        {
            this.gameManager.Start();
            Invoke(InitLabelsText);
            this.cutOut.OnEndAnimation -= StartGame;
        }

        void GameManager_OnSideToMoveChanged(GameManager sender, GameEventArgs e)
        {
            Invoke(() =>
            {
                lock (sender)
                {
                    this.posViewer.ShowLegalMovePointers = sender.CurrentPlayer is HumanPlayer;

                    //if (sender.NowPlaying && sender.OpponentPlayer is EnginePlayer engine)
                    //    engine.StartAnalyzing();

                    var winRates = this.winRateFigure.BlackWinRates;
                    this.winRateFigure.AddBlackWinRate((winRates.Count > 1) ? winRates[^1] : 50.0f);
                    InitLabelsText();
                }
            });
        }

        void GameManager_OnPlayBlack(GameManager sender, GameEventArgs e) => Invoke(() =>
        {
            if (e.Coord == BoardCoordinate.Pass)
            {
                this.messageLabel.Text = $"{sender.BlackPlayer.Name}: パス";
                this.winRateFigure.CurrentBlackWinRate = this.winRateFigure.BlackWinRates[^2];
                this.cutInLabel.Text = "パス";
                this.cutOut.OnEndAnimation += (_, _) => this.gameManager.Resume();
                this.gameManager.Pause();
                StartCutInOutAnimation();
                AudioMixer.PlayChannel(-1, this.cutInSE);
            }
            else
            {
                AudioMixer.PlayChannel(-1, this.putDiscSE);
                this.messageLabel.Text = $"{sender.BlackPlayer.Name}: {e.Coord}";
            }

            lock (this.posViewer)
                this.posViewer.Update(e.Coord);
        });

        void GameManager_OnPlayWhite(GameManager sender, GameEventArgs e) => Invoke(() =>
        {
            if (e.Coord == BoardCoordinate.Pass)
            {
                this.messageLabel.Text = $"{sender.WhitePlayer.Name}: パス";
                this.winRateFigure.CurrentBlackWinRate = this.winRateFigure.BlackWinRates[^2];
                this.cutInLabel.Text = "パス";
                this.cutOut.OnEndAnimation += (_, _) => this.gameManager.Resume();
                this.gameManager.Pause();
                StartCutInOutAnimation();
                AudioMixer.PlayChannel(-1, this.cutInSE);
            }
            else
            {
                AudioMixer.PlayChannel(-1, this.putDiscSE);
                this.messageLabel.Text = $"{sender.WhitePlayer.Name}: {e.Coord}";
            }

            lock (this.posViewer)
                this.posViewer.Update(e.Coord);
        });

        void GameManager_OnGameOver(GameManager sender, GameEventArgs e) => Invoke(() =>
        {
            Debug.Assert(e.Result is not null);

            this.sideToMoveLabel.Text = "終局";

            var record = new GameRecord
            {
                Difficulty = this.difficulty.Label,
                BlackPlayerName = this.gameManager.BlackPlayer.Name,
                WhitePlayerName = this.gameManager.WhitePlayer.Name
            };

            if (!this.gameStats.TryGetValue(this.difficulty.Label, out GameStatistic? gameStat))
                this.gameStats[this.difficulty.Label] = gameStat = new GameStatistic();

            if (e.Result.Draw)
            {
                this.messageLabel.Text = "Draw";
                record.WinnerName = "NULL";
                gameStat.HumanDrawCount++;
            }
            else
            {
                Debug.Assert(e.Winner is not null);

                record.WinnerName = e.Winner.Name;
                if (e.Winner is HumanPlayer)
                {
                    this.messageLabel.Text = "You Win!!";
                    gameStat.HumanWinCount++;
                }
                else
                {
                    this.messageLabel.Text = "You Lose...";
                    gameStat.HumanLossCount++;
                }
            }
            record.MoveHistroy = this.gameManager.GetPosition().MoveHistroy.Select(e => e.Coord).ToArray();
            this.gameRecords.Add(record);
            SaveGameRecords(this.gameRecords);
            SaveGameStats(this.gameStats);

            this.cutInLabel.Text = this.messageLabel.Text;
            this.cutOut.OnEndAnimation += (_, _) =>
            {
                this.nextSceneLabel.MouseEnter += NextSceneLabel_MouseEnter;
                this.nextSceneLabel.MouseLeave += NextSceneLabel_MouseLeave;
                this.nextSceneLabel.MouseClick += NextSceneLabel_MouseClick;
                this.blinkNextSceneLabel.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, int.MaxValue);
                Invoke(this.nextSceneLabel.Show);
            };
            StartCutInOutAnimation();
            AudioMixer.PlayChannel(-1, this.gameOverSE);
        });

        void Engine_OnSearchInfoRecieved(object sender, SearchInfo searchInfo)
        {
            var nodeCount = this.latestNodeStats.NodeCount;

            this.searchInfoLabel.Invoke(() => this.searchInfoLabel.Text = $"読み: {NodeCountToText(nodeCount)} {searchInfo.Depth}手先");

            if (sender is not EnginePlayer player)
                return;

            var winRate = (int)(searchInfo.Eval + 50.0f);
            if (player != this.gameManager.CurrentPlayer)
                winRate = 100 - winRate;

            if (player == this.gameManager.BlackPlayer)
            {
                this.winRateBar.Invoke(() => this.winRateBar.BlackWinRate = winRate);
                this.winRateFigure.Invoke(() => this.winRateFigure.SetBlackWinRate(this.winRateFigure.BlackWinRates.Count - 1, winRate));
                this.situationLabel.Invoke(() => this.situationLabel.Text = WinRateToSituationText(DiscColor.Black, winRate));
            }
            else
            {
                this.winRateBar.Invoke(() => this.winRateBar.WhiteWinRate = winRate);
                this.winRateFigure.Invoke(() => this.winRateFigure.SetWhiteWinRate(this.winRateFigure.BlackWinRates.Count - 1, winRate));
                this.situationLabel.Invoke(() => this.situationLabel.Text = WinRateToSituationText(DiscColor.White, winRate));
            }
        }

        void Engine_OnNodeStatsRecieved(object sender, NodeStats nodeStats) => this.latestNodeStats = nodeStats;

        void PosViewer_OnMouseClicked(PositionViewer sender, BoardCoordinate coord)
        {
            if (this.gameManager.CurrentPlayer is not HumanPlayer)
                return;
            ((HumanPlayer)this.gameManager.CurrentPlayer).SetHumanInput(coord);
        }

        void NextSceneLabel_MouseEnter(object? sender, EventArgs e) => this.blinkNextSceneLabel.Stop();

        void NextSceneLabel_MouseLeave(object? sender, EventArgs e)
            => this.blinkNextSceneLabel.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, int.MaxValue);

        void NextSceneLabel_MouseClick(object? sender, MouseEventArgs e)
        {
            this.blinkNextSceneLabel.Stop();
            this.nextSceneLabel.MouseLeave -= NextSceneLabel_MouseLeave;
            this.nextSceneLabel.MouseEnter -= NextSceneLabel_MouseEnter;
            this.hideGameScene.OnEndAnimation += HideGameScene_OnEndAnimation;
            this.hideGameScene.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 500);
        }

        void Engine_OnUnexpectedShutdownOccured(object? sender, EventArgs e)
        {
            MessageBox.Show($"対局用の思考エンジンが予期せず終了しました。開発者へ報告をお願いします。", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            MessageBox.Show($"アプリケーションを終了します。", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (this.Parent is MainForm)
                this.Invoke(() => ((MainForm)this.Parent).ChangeUserControl(null));
        }

        string NodeCountToText(ulong nodeCount)
        {
            if (nodeCount < 9999)
                return $"{nodeCount}局面";

            if (nodeCount < 9999999)
                return $"{nodeCount / 10000}万局面";

            if (nodeCount < 99999999)
                return $"{nodeCount / (int)1.0e+7}千万局面";

            return $"{nodeCount / (int)1.0e+8}億局面";
        }

        string WinRateToSituationText(DiscColor color, double winRate)
        {
            DiscColor player;
            if (winRate < 50.0)
            {
                player = (DiscColor)(-(int)color);
                winRate = 100.0 - winRate;
            }
            else
                player = color;

            if (50.0 <= winRate && winRate <= 55.0)
                return "互角";

            var sb = new StringBuilder((player == DiscColor.Black) ? "先手(黒)" : "後手(白)").Append('が');
            if (winRate > 90.0)
                sb.Append("勝勢");
            else if (winRate > 75.0)
                sb.Append("優勢");
            else if (winRate > 65.0)
                sb.Append("有利");
            else
                sb.Append("やや有利");

            return sb.ToString();
        }

        void InitLabelsText()
        {
            if (this.gameManager is not null)
            {
                this.discCountLabel.Text = $"{this.gameManager.BlackDiscCount}-{this.gameManager.WhiteDiscCount}";
                this.sideToMoveLabel.Text = $"Turn: {this.gameManager.CurrentPlayer?.Name}";
                this.gameTypeLabel.Text = $"GameType: {this.gameManager.GameType}";
                this.difficultyLabel.Text = $"Difficulty: {this.difficulty.Label}";
            }
        }

        void StartCutInOutAnimation()
        {
            Task.Run(() =>
            {
                var endAnimation = false;
                EventHandler onEndAnimation = (_, _) => endAnimation = true;
                this.cutIn.OnEndAnimation += onEndAnimation;
                this.cutIn.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 500);

                while (!endAnimation)
                    Thread.Yield();
                this.cutIn.OnEndAnimation -= onEndAnimation;

                Thread.Sleep(2000);

                this.cutOut.OnEndAnimation += onEndAnimation;
                this.cutOut.AnimateForDuration(GlobalConfig.Instance.AnimationFrameIntervalMs, 500);

                while (!endAnimation)
                    Thread.Yield();
                this.cutOut.OnEndAnimation -= onEndAnimation;
            });
        }
    }
}
