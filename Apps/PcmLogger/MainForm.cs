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
        private DpidsAndMath dpidsAndMath;
        private bool logging;
        private object loggingLock = new object();
        private bool logStopRequested;
        private string profileName;
        private TaskScheduler uiThreadScheduler;
        private static DateTime lastLogTime;
        private uint osid;

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
            this.startStopLogging.Enabled = false;
        }

        protected override void EnableInterfaceSelection()
        {
            this.selectButton.Enabled = true;
        }

        protected override void EnableUserInput()
        {
            this.selectButton.Enabled = true;
            this.startStopLogging.Enabled = true;
            this.startStopLogging.Focus();
        }

        protected override void NoDeviceSelected()
        {
            this.selectButton.Enabled = true;
            this.deviceDescription.Text = "No device selected";
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
                this.startStopLogging.Enabled = true;
                this.parameterGrid.Enabled = true;
            });

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
            await this.HandleSelectButtonClick();
            this.UpdateStartStopButtonState();
        }

        /// <summary>
        /// Select a logging profile.
        /// </summary>
        private async void selectProfile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.AddExtension = true;
            dialog.CheckFileExists = true;
            dialog.AutoUpgradeEnabled = true;
            dialog.CheckPathExists = true;
            dialog.DefaultExt = ".profile";
            dialog.Multiselect = false;
            dialog.ValidateNames = true;
            dialog.Filter = "Logging profiles (*.profile)|*.profile";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                await this.Obsolete_LoadProfile(dialog.FileName);
            }
            else
            {
                this.dpidsAndMath = null;
                this.profileName = null;
            }

            this.UpdateStartStopButtonState();
        }

        /// <summary>
        /// Load the profile from the given path.
        /// </summary>
        private async Task Obsolete_LoadProfile(string path)
        {
            try
            {
                DpidConfiguration profile;
                if (path.EndsWith(".json.profile"))
                {
                    using (Stream stream = File.OpenRead(path))
                    {
                        DpidConfigurationReader reader = new DpidConfigurationReader(stream);
                        profile = await reader.ReadAsync();
                    }

                    string newPath = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path)) + ".xml.profile";
                    using (Stream xml = File.OpenWrite(newPath))
                    {
                        DpidConfigurationXmlWriter writer = new DpidConfigurationXmlWriter(xml);
                        writer.Write(profile);
                    }
                }
                else if (path.EndsWith(".xml.profile"))
                {
                    using (Stream stream = File.OpenRead(path))
                    {
                        DpidConfigurationXmlReader reader = new DpidConfigurationXmlReader(stream);
                        profile = reader.Read();
                    }
                }
                else
                {
                    return;
                }

//                this.profilePath.Text = path;
//                this.profileName = Path.GetFileNameWithoutExtension(this.profilePath.Text);

                this.dpidsAndMath = new DpidsAndMath(profile, loader.Configuration);
                this.logValues.Text = string.Join(Environment.NewLine, this.dpidsAndMath.GetColumnNames());
//                Configuration.Settings.ProfilePath = path;
                Configuration.Settings.Save();
            }
            catch (Exception exception)
            {
                this.logValues.Text = exception.Message;
                this.AddDebugMessage(exception.ToString());
//                this.profilePath.Text = "[no profile loaded]";
                this.profileName = null;
            }
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
        /// Enable or disble the start/stop button.
        /// </summary>
        private void UpdateStartStopButtonState()
        {
            this.startStopLogging.Enabled = this.Vehicle != null && this.dpidsAndMath != null;
        }

        /// <summary>
        /// Start or stop logging.
        /// </summary>
        private void startStopLogging_Click(object sender, EventArgs e)
        {
            if (logging)
            {
                this.StopLogging();
            }
            else
            {
                this.StartLogging();
            }
        }

        /// <summary>
        /// Signal the background thread to stop.
        /// </summary>
        private void StopLogging()
        {
            this.logStopRequested = true;
            this.startStopLogging.Enabled = false;
            this.startStopLogging.Text = "Start &Logging";

            // TODO: Async
            while(this.logging)
            {
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Start the background thread.
        /// </summary>
        private void StartLogging()
        {
            lock (loggingLock)
            {
                if (this.dpidsAndMath == null)
                {
                    this.logValues.Text = "Please select a log profile.";
                    return;
                }

                if (!logging)
                {
                    logging = true;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(LoggingThread), null);
                    this.startStopLogging.Text = "Stop &Logging";
                }
            }
        }

        /// <summary>
        /// Generate a file name for the current log file.
        /// </summary>
        private string GenerateLogFilePath()
        {
            string file = DateTime.Now.ToString("yyyyMMdd_HHmm") +
                "_" +
                this.profileName +
                ".csv";
            return Path.Combine(Configuration.Settings.LogDirectory, file);
        }

        /// <summary>
        /// Create a string that will look reasonable in the UI's main text box.
        /// TODO: Use a grid instead.
        /// </summary>
        private string FormatValuesForTextBox(IEnumerable<string> rowValues)
        {
            StringBuilder builder = new StringBuilder();
            IEnumerator<string> rowValueEnumerator = rowValues.GetEnumerator();
            foreach(ParameterGroup group in this.dpidsAndMath.Profile.ParameterGroups)
            {
                foreach(ProfileParameter parameter in group.Parameters)
                {
                    rowValueEnumerator.MoveNext();
                    builder.Append(rowValueEnumerator.Current);
                    builder.Append('\t');
                    builder.Append(parameter.Conversion.Units);
                    builder.Append('\t');
                    builder.AppendLine(parameter.Parameter.Name);
                }
            }

            foreach(MathValue mathValue in this.dpidsAndMath.MathValueProcessor.GetMathValues())
            {
                rowValueEnumerator.MoveNext();
                builder.Append(rowValueEnumerator.Current);
                builder.Append('\t');
                builder.Append(mathValue.Units);
                builder.Append('\t');
                builder.AppendLine(mathValue.Name);
            }

            DateTime now = DateTime.Now;
            builder.AppendLine((now - lastLogTime).TotalMilliseconds.ToString("0.00") + "\tms\tQuery time");
            lastLogTime = now;

            return builder.ToString();
        }

        private void parameterGrid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
//            this.RegenerateProfile();
        }

        private void parameterGrid_CurrentCellChanged(object sender, EventArgs e)
        {
//            this.RegenerateProfile();
        }
    }
}
