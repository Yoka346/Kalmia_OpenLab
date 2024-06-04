using System.Windows.Forms;

namespace Kalmia_OpenLab.View.Controls
{
    public partial class WinRateBar : UserControl
    {
        public int? BlackWinRate
        {
            get => this.winRateBar.Value;

            set
            {
                if (value is null)
                {
                    this.winRateBar.Value = 50;
                    this.blackWinRateLabel.Text = "--%";
                    this.whiteWinRateLabel.Text = "--%";
                }
                else
                {
                    this.winRateBar.Value = value.Value;
                    this.blackWinRateLabel.Text = $"{value}%";
                    this.whiteWinRateLabel.Text = $"{(100 - value)}%";
                }
            }
        }

        public int? WhiteWinRate
        {
            get => 100 - this.winRateBar.Value;

            set
            {
                if (value is null)
                {
                    this.winRateBar.Value = 50;
                    this.blackWinRateLabel.Text = "--%";
                    this.whiteWinRateLabel.Text = "--%";
                }
                else
                {
                    this.winRateBar.Value = 100 - value.Value;
                    this.blackWinRateLabel.Text = $"{(100 - value)}%";
                    this.whiteWinRateLabel.Text = $"{value}%";
                }
            }
        }

        public WinRateBar() => InitializeComponent();
    }
}
