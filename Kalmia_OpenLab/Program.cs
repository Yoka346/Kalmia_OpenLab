using System;
using System.Windows.Forms;

using Kalmia_OpenLab.View;
using Kalmia_OpenLab.View.Scenes;
using Kalmia_OpenLab.SDLWrapper;
using System.IO;

namespace Kalmia_OpenLab
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            if (GlobalConfig.Instance.WorkDirPath is not null && GlobalConfig.Instance.WorkDirPath != string.Empty)
                Environment.CurrentDirectory = GlobalConfig.Instance.WorkDirPath;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            AudioMixer.Init();
            Application.Run(new MainForm(new TitleScene()));
            AudioMixer.Quit();
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            using var fs = CreateErrorLogFile();
            using var sw = new StreamWriter(fs);
            if (e.ExceptionObject is Exception ex)
                sw.WriteLine(ex.ToString());
            MessageBox.Show($"ゲームが予期せず終了しました。{Environment.NewLine}開発者に報告をお願いします。", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        static FileStream CreateErrorLogFile()
        {
            var i = 0;
            string path;
            do
                path = string.Format(FilePath.ErrorLogPath, i++);
            while (File.Exists(path));
            return new FileStream(path, FileMode.Create, FileAccess.Write);
        }
    }
}