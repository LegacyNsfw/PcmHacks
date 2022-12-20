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
    /// <summary>
    /// This form reminds the user to pause before attempting a full read.
    /// </summary>
    public partial class DelayDialogBox : Form
    {
        /// <summary>
        /// Timer to drive the countdown text.
        /// </summary>
        private Timer timer;

        /// <summary>
        /// Number of seconds to wait. This value will count down with each timer tick.
        /// </summary>
        private int secondsRemaining = 10;

        /// <summary>
        /// Constructor.
        /// </summary>
        public DelayDialogBox()
        {
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            InitializeComponent();
        }

        /// <summary>
        /// Start the timer when the dialog box loads.
        /// </summary>
        private void DelayDialogBox_Load(object sender, EventArgs e)
        {
            this.Timer_Tick(null, null);
            this.timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        /// <summary>
        /// Decrement the countdown with each timer tick, close the form when we reach zero.
        /// </summary>
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

        /// <summary>
        /// Close the form with an "OK" result.
        /// </summary>
        private void continueButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Close the form with a "Cancel" result.
        /// </summary>
        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
