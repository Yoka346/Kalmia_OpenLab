using System;
using System.Drawing;
using System.Windows.Forms;

using Kalmia_OpenLab.SDLWrapper;

namespace Kalmia_OpenLab
{
    public partial class MainForm : Form
    {
        public MainForm(UserControl initialControl) => InitializeComponent(initialControl);

        void MainForm_Load(object? sender, EventArgs e) => AudioMixer.Init();

        void MainForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            GlobalConfig.Save();
            GlobalSE.DisposeAll();
        }

        public void ChangeUserControl(UserControl? control)
        {
            this.Controls[^1].Hide();
            this.Controls[^1].Dispose();    // ‚È‚ñ‚ÅDispose?

            if(control is null)
            {
                Close();
                return;
            }

            control.AutoSize = false;
            control.Size = this.ClientSize;
            control.Location = new Point(0, 0);

            this.Controls.Add(control);
            control.Show();
        }
    }
}
