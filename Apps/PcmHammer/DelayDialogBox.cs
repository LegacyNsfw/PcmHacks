using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PcmHacking
{
    public partial class DelayDialogBox : Form
    {
        private Timer timer;
        private int secondsRemaining = 15;

        public DelayDialogBox()
        {
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            InitializeComponent();
        }

        private void DelayDialogBox_Load(object sender, EventArgs e)
        {
            this.Timer_Tick(null, null);
            this.timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (secondsRemaining > 0)
            {
                this.countdown.Text = secondsRemaining + " seconds remaining...";
                this.secondsRemaining--;
            }
            else
            {
                this.countdown.Text = "Continuing...";
                this.continueButton_Click(this, e);
            }
        }

        private void continueButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
