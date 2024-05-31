using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;


namespace Kalmia_OpenLab
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent(UserControl initialControl)
        {
            this.ClientSize = new Size(GlobalConfig.Instance.ScreenWidth, GlobalConfig.Instance.ScreenHeight);
            this.MinimumSize = this.MaximumSize = this.ClientSize;
            this.MinimizeBox = this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.BackColor = Color.Black;
            initialControl.AutoSize = false;
            initialControl.Size = this.ClientSize;
            initialControl.Location = new Point(0, 0);
            this.Controls.Add(initialControl);
            this.Load += MainForm_Load;
            this.FormClosed += MainForm_FormClosed;
        }

        #endregion
    }
}
