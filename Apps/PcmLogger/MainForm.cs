using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PcmHacking
{
    public partial class MainForm : MainFormBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        public override void AddDebugMessage(string message)
        {
            Debug.WriteLine(message);
        }

        public override void AddUserMessage(string message)
        {
            Debug.WriteLine(message);
        }

        public override void ResetLogs()
        {
        }

        public override string GetAppNameAndVersion()
        {
            return "PCM Logger";
        }

        protected override void DisableUserInput()
        {
            this.startLogging.Enabled = false;
            this.stopLogging.Enabled = false;
        }

        protected override void EnableInterfaceSelection()
        {
            this.selectButton.Enabled = true;
        }

        protected override void EnableUserInput()
        {
            this.startLogging.Enabled = true;
            this.stopLogging.Enabled = true;
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

        /// <summary>
        /// Select which interface device to use. This opens the Device-Picker dialog box.
        /// </summary>
        protected async void selectButton_Click(object sender, EventArgs e)
        {
            await this.HandleSelectButtonClick();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.NoDeviceSelected();
        }

        private bool logging;
        private object loggingLock = new object();
        private bool logStopRequested;

        private void startLogging_Click(object sender, EventArgs e)
        {
            if (!logging)
            {
                lock(loggingLock)
                {
                    if (!logging)
                    {
                        logging = true;
                        ThreadPool.QueueUserWorkItem(new WaitCallback(LoggingThread));
                    }
                }
            }
            
        }

        private void stopLogging_Click(object sender, EventArgs e)
        {
            this.logStopRequested = true;
        }

        private async void LoggingThread(object unused)
        {
            try
            {
                if (!await this.Vehicle.StartLogging())
                {
                    logging = false;
                    return;
                }

                while (!this.logStopRequested)
                {
                    string[] rowValues = await this.Vehicle.ReadLogRow();
                    this.logValues.Text = string.Join(Environment.NewLine, rowValues);
                }
            }
            catch(Exception exception)
            {
                this.AddDebugMessage(exception.ToString());
                this.AddUserMessage("Logging interrupted: " + exception.Message);
            }
            finally
            {
                this.logging = false;
            }
        }
    }
}
