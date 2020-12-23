//#define Vpw4x

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PcmHacking
{
    public partial class MainForm : MainFormBase
    {
        private bool saving;
        private object loggingLock = new object();
        private bool logStopRequested;
        private TaskScheduler uiThreadScheduler;
        private uint osid;

        private const string defaultFileName = "Data";
        private string fileName = defaultFileName;


        /// <summary>
        /// Constructor
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Not used.
        /// </summary>
        /// <param name="message"></param>
        public override void AddUserMessage(string message)
        {
        }

        /// <summary>
        /// Add a message to the debug pane of the main window.
        /// </summary>
        public override void AddDebugMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("hh:mm:ss:fff");

            Task foreground = Task.Factory.StartNew(
                delegate ()
                {
                    try
                    {
                        this.debugLog.AppendText("[" + timestamp + "]  " + message + Environment.NewLine);
                    }
                    catch (ObjectDisposedException)
                    {
                        // This will happen if the window is closing. Just ignore it.
                    }
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                uiThreadScheduler);
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
            this.startStopSaving.Enabled = false;
        }

        protected override void EnableInterfaceSelection()
        {
            this.selectButton.Enabled = true;
        }

        protected override void EnableUserInput()
        {
            this.selectButton.Enabled = true;
            this.startStopSaving.Enabled = true;
            this.startStopSaving.Focus();
        }

        protected override void NoDeviceSelected()
        {
            this.selectButton.Enabled = true;
            this.deviceDescription.Text = "No device selected";
        }

        protected override void SetSelectedDeviceText(string message)
        {
            this.deviceDescription.Text = message;
        }

        protected override async Task ValidDeviceSelectedAsync(string deviceName)
        {
            this.AddDebugMessage("ValidDeviceSelectedAsync started.");
            Response<uint> response = await this.Vehicle.QueryOperatingSystemId(new CancellationToken());
            if (response.Status != ResponseStatus.Success)
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    this.deviceDescription.Text = deviceName + " is unable to connect to the PCM";
                });

                return;
            }

            this.osid = response.Value;
            
            this.Invoke((MethodInvoker)delegate ()
            {
                this.deviceDescription.Text = deviceName + " " + osid.ToString();
                this.startStopSaving.Enabled = true;
                this.parameterGrid.Enabled = true;
            });

            // Start pulling data from the PCM
            ThreadPool.QueueUserWorkItem(new WaitCallback(LoggingThread), null);

            this.AddDebugMessage("ValidDeviceSelectedAsync ended.");
        }

        /// <summary>
        /// Open the last device, if possible.
        /// </summary>
        private void MainForm_Load(object sender, EventArgs e)
        {
            // Order matters - the scheduler must be set before adding messages.
            this.uiThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.AddDebugMessage("MainForm_Load started.");
                        
            string logDirectory = Configuration.Settings.LogDirectory;
            if (string.IsNullOrWhiteSpace(logDirectory))
            {
                logDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                Configuration.Settings.LogDirectory = logDirectory;
                Configuration.Settings.Save();
            }

            ThreadPool.QueueUserWorkItem(BackgroundInitialization);

            this.logFilePath.Text = logDirectory;
            
            this.AddDebugMessage("MainForm_Load ended.");
        }

        private async void BackgroundInitialization(object unused)
        {
            this.FillParameterGrid();
            this.parameterGrid.Enabled = true;

            this.AddDebugMessage("Device reset started.");
            await this.ResetDevice();
            this.AddDebugMessage("Device reset completed.");
        }

        /// <summary>
        /// Select which interface device to use. This opens the Device-Picker dialog box.
        /// </summary>
        protected async void selectButton_Click(object sender, EventArgs e)
        {
            await base.HandleSelectButtonClick();
        }

        /// <summary>
        /// Choose which directory to create log files in.
        /// </summary>
        private void setDirectory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = Configuration.Settings.LogDirectory;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Configuration.Settings.LogDirectory = dialog.SelectedPath;
                Configuration.Settings.Save();
                this.logFilePath.Text = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// Open a File Explorer window in the log directory.
        /// </summary>
        private void openDirectory_Click(object sender, EventArgs e)
        {
            Process.Start(Configuration.Settings.LogDirectory);
        }

        /// <summary>
        /// Start or stop logging.
        /// </summary>
        private void startStopSaving_Click(object sender, EventArgs e)
        {
            if (saving)
            {
                this.saving = false;
                this.startStopSaving.Text = "Start &Logging";
                this.loggerProgress.MarqueeAnimationSpeed = 0;
                this.loggerProgress.Visible = false;
                this.logState = LogState.StopSaving;
            }
            else
            {
                this.saving = true;
                this.startStopSaving.Text = "Stop &Logging";
                this.loggerProgress.MarqueeAnimationSpeed = 100;
                this.loggerProgress.Visible = true;
                this.logState = LogState.StartSaving;
            }
        }

        /// <summary>
        /// Generate a file name for the current log file.
        /// </summary>
        private string GenerateLogFilePath()
        {
            string file = DateTime.Now.ToString("yyyyMMdd_HHmm") +
                "_" +
                this.fileName +
                ".csv";
            return Path.Combine(Configuration.Settings.LogDirectory, file);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.logStopRequested = true;

            // It turns out that WaitAll is not supported on an STA thread.
            // WaitHandle.WaitAll(new WaitHandle[] { loggerThreadEnded, writerThreadEnded });
            loggerThreadEnded.WaitOne(1000);
            writerThreadEnded.WaitOne(1000);
        }
    }
}
