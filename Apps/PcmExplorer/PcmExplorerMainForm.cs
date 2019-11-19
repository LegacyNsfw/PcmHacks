using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PcmHacking
{
    public partial class PcmExplorerMainForm : MainFormBase
    {
        private TaskScheduler uiThreadScheduler;

        public PcmExplorerMainForm()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Not used.
        /// </summary>
        /// <param name="message"></param>
        public override void AddUserMessage(string message)
        {
            Task foreground = Task.Factory.StartNew(
                delegate ()
                {
                    this.debugLog.AppendText("[" + GetTimestamp() + "]  " + message + Environment.NewLine);
                    this.userLog.AppendText("[" + GetTimestamp() + "]  " + message + Environment.NewLine);
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                uiThreadScheduler);
        }

        /// <summary>
        /// Add a message to the debug pane of the main window.
        /// </summary>
        public override void AddDebugMessage(string message)
        {
            Task foreground = Task.Factory.StartNew(
                delegate ()
                {
                    this.debugLog.AppendText("[" + GetTimestamp() + "]  " + message + Environment.NewLine);
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                uiThreadScheduler);
        }

        private string GetTimestamp()
        {
            return DateTime.Now.ToString("hh:mm:ss:fff");
        }

        public override void ResetLogs()
        {
            this.debugLog.Clear();
        }

        public override string GetAppNameAndVersion()
        {
            return "PCM Logger";
        }

        protected override void DisableUserInput()
        {
            this.selectButton.Enabled = false;
        }

        protected override void EnableInterfaceSelection()
        {
            this.selectButton.Enabled = true;
        }

        protected override void EnableUserInput()
        {
            this.selectButton.Enabled = true;
        }

        protected override void NoDeviceSelected()
        {
            this.selectButton.Enabled = true;
            this.deviceDescription.Text = "No device selected";
        }

        protected override void ValidDeviceSelected(string deviceName)
        {
            this.deviceDescription.Text = deviceName;
        }

        private void PcmExplorerMainForm_Load(object sender, EventArgs e)
        {
            this.uiThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        private async void testPid_Click(object sender, EventArgs e)
        {
            uint pid;
            if (!UInt32.TryParse(this.pid.Text, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out pid))
            {
                this.AddUserMessage("Unable to parse PID.");
                return;
            }

            try
            {
                Response<int> response = await this.Vehicle.GetPid(pid);
                if (response.Status == ResponseStatus.Success)
                {
                    this.AddUserMessage(string.Format("{0:X4} = {1}", pid, response.Value));
                }
                else
                {
                    this.AddUserMessage(string.Format("{0:X4} = {1}", pid, response.Status));
                }
            }
            catch(Exception exception)
            {
                this.AddUserMessage(exception.ToString());
            }
        }

        private async void selectButton_Click(object sender, EventArgs e)
        {
            await this.HandleSelectButtonClick();
            //this.UpdateStartStopButtonState();
        }
    }
}
