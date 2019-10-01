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
        private LogProfile profile;
        private bool logging;
        private object loggingLock = new object();
        private bool logStopRequested;
        private string profileName;

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

            this.debugLog.Invoke(
                (MethodInvoker)delegate ()
                {
                    this.debugLog.AppendText("[" + timestamp + "]  " + message + Environment.NewLine);
                });
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
            this.startStopLogging.Enabled = false;
            this.startStopLogging.Enabled = false;
        }

        protected override void EnableInterfaceSelection()
        {
            this.selectButton.Enabled = true;
        }

        protected override void EnableUserInput()
        {
            this.startStopLogging.Enabled = true;
            this.startStopLogging.Enabled = true;
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
        /// Open the last device, if possible.
        /// </summary>
        private async void MainForm_Load(object sender, EventArgs e)
        {
            await this.ResetDevice();
        }

        /// <summary>
        /// Select which interface device to use. This opens the Device-Picker dialog box.
        /// </summary>
        protected async void selectButton_Click(object sender, EventArgs e)
        {
            await this.HandleSelectButtonClick();
        }

        /// <summary>
        /// Start or stop logging.
        /// </summary>
        private void startStopLogging_Click(object sender, EventArgs e)
        {
            if (logging)
            {
                this.logStopRequested = true;
                this.startStopLogging.Enabled = false;
                this.startStopLogging.Text = "Start &Logging";
            }
            else
            {
                lock (loggingLock)
                {
                    if (!logging)
                    {
                        logging = true;
                        ThreadPool.QueueUserWorkItem(new WaitCallback(LoggingThread), TaskScheduler.FromCurrentSynchronizationContext());
                        this.startStopLogging.Text = "Stop &Logging";
                    }
                }
            }

        }
        
        /// <summary>
        /// The loop that reads data from the PCM.
        /// </summary>
        private async void LoggingThread(object unused)
        {
            TaskScheduler uiThread = (TaskScheduler)unused;
            
            try
            {
                if (this.profile == null)
                {
                    this.logValues.Text = "Please select a log profile.";
                    return;
                }

                Logger logger = new Logger(this.Vehicle, this.profile);
                if (!await logger.StartLogging())
                {
                    this.AddUserMessage("Unable to start logging.");
                    return;
                }

                string logFilePath = GenerateLogFilePath();
                using (StreamWriter streamWriter = new StreamWriter(logFilePath))
                {
                    LogFileWriter writer = new LogFileWriter(streamWriter);
                    await writer.WriteHeader(this.profile);

                    while (!this.logStopRequested)
                    {
                        this.AddDebugMessage("Reading row...");
                        string[] rowValues = await logger.GetNextRow();
                        if (rowValues == null)
                        {
                            break;
                        }

                        // Write the data to disk on a background thread.
                        Task background = Task.Factory.StartNew(
                            delegate ()
                            {
                                writer.WriteLine(rowValues);
                            });
                        
                        // Display the data using a foreground thread.
                        Task foreground = Task.Factory.StartNew(
                            delegate ()
                            {
                                string formattedValues = FormatValuesForTextBox(rowValues);
                                this.logValues.Text = string.Join(Environment.NewLine, formattedValues);
                            },
                            CancellationToken.None,
                            TaskCreationOptions.None,
                            uiThread);
                    }
                }
            }
            catch (Exception exception)
            {
                this.AddDebugMessage(exception.ToString());
                this.AddUserMessage("Logging interrupted. " + exception.Message);
                this.logValues.Invoke(
                    (MethodInvoker)
                    delegate ()
                    {
                        this.logValues.Text = "Logging interrupted. " + exception.Message;
                    });
            }
            finally
            {
                this.logStopRequested = false;
                this.logging = false;
                this.startStopLogging.Invoke(
                    (MethodInvoker)
                    delegate ()
                    {
                        this.startStopLogging.Enabled = true;
                        this.startStopLogging.Text = "Start &Logging";
                    });
            }
        }

        /// <summary>
        /// Generate a file name for the current log file.
        /// </summary>
        private string GenerateLogFilePath()
        {
            string directory = Environment.GetEnvironmentVariable("USERPROFILE");
            string file = DateTime.Now.ToString("yyyyMMdd_HHmm") +
                "_" +
                this.profileName +
                ".csv";
            return Path.Combine(directory, file);
        }

        /// <summary>
        /// Create a string that will look reasonable in the UI's main text box.
        /// TODO: Use a grid instead.
        /// </summary>
        private string FormatValuesForTextBox(string[] rowValues)
        {
            StringBuilder builder = new StringBuilder();
            int index = 0;
            foreach(ParameterGroup group in this.profile.ParameterGroups)
            {
                foreach(ProfileParameter parameter in group.Parameters)
                {
                    builder.Append(rowValues[index++]);
                    builder.Append('\t');
                    builder.Append(parameter.Conversion.Name);
                    builder.Append('\t');
                    builder.AppendLine(parameter.Name);
                }
            }

            return builder.ToString();
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
                try
                {
                    using (Stream stream = File.OpenRead(dialog.FileName))
                    {
                        LogProfileReader reader = new LogProfileReader(stream);
                        this.profile = await reader.ReadAsync();
                    }

                    this.profilePath.Text = dialog.FileName;
                    this.profileName = Path.GetFileNameWithoutExtension(this.profilePath.Text);
                    this.logValues.Text = this.profile.GetParameterNames(Environment.NewLine);
                }
                catch(Exception exception)
                {
                    this.logValues.Text = exception.ToString();
                    this.profileName = null;
                }
            }
            else
            {
                this.profile = null;
                this.profileName = null;
            }
        }

        /// <summary>
        /// Create a logging profile for testing (this was also use for testing the JSON serialization / deserialization).
        /// </summary>
        private async void GenerateTestProfile()
        {
            LogProfile test = new LogProfile();
            ParameterGroup group = new ParameterGroup();
            group.Dpid = 0xFE;
            group.TryAddParameter(
                new ProfileParameter
                {
                    Name = "Engine Speed",
                    DefineBy = DefineBy.Pid,
                    ByteCount = 2,
                    Address = 0x000c,
                    Conversion = new Conversion { Name = "RPM", Expression = "x*.25" },
                });
            group.TryAddParameter(
                new ProfileParameter
                {
                    Name = "Mass Air Flow",
                    DefineBy = DefineBy.Pid,
                    ByteCount = 2,
                    Address = 0x0010,
                    Conversion = new Conversion { Name = "g/s", Expression = "x/100" },
                });
            group.TryAddParameter(
                new ProfileParameter
                {
                    Name = "Manifold Absolute Pressure",
                    DefineBy = DefineBy.Pid,
                    ByteCount = 1,
                    Address = 0x000B,
                    Conversion = new Conversion { Name = "kpa", Expression = "x" },
                });
            group.TryAddParameter(
                new ProfileParameter
                {
                    Name = "Throttle Position Sensor",
                    DefineBy = DefineBy.Pid,
                    ByteCount = 1,
                    Address = 0x0011,
                    Conversion = new Conversion { Name = "%", Expression = "x/2.56" },
                });

            test.TryAddGroup(group);

            group = new ParameterGroup();
            group.Dpid = 0xFD;
            group.TryAddParameter(
                new ProfileParameter
                {
                    Name = "Intake Air Temperature",
                    DefineBy = DefineBy.Pid,
                    ByteCount = 1,
                    Address = 0x000F,
                    Conversion = new Conversion { Name = "C", Expression = "x-40" },
                });
            group.TryAddParameter(
                new ProfileParameter
                {
                    Name = "Engine Coolant Temperature",
                    DefineBy = DefineBy.Pid,
                    ByteCount = 1,
                    Address = 0x000c,
                    Conversion = new Conversion { Name = "C", Expression = "x-40" },
                });
            group.TryAddParameter(
                new ProfileParameter
                {
                    Name = "Left Long Term Fuel Trim",
                    DefineBy = DefineBy.Pid,
                    ByteCount = 1,
                    Address = 0x0007,
                    Conversion = new Conversion { Name = "%", Expression = "(x-128)/1.28" },
                });
            group.TryAddParameter(
                new ProfileParameter
                {
                    Name = "Right Long Term Fuel Trim",
                    DefineBy = DefineBy.Pid,
                    ByteCount = 1,
                    Address = 0x0009,
                    Conversion = new Conversion { Name = "%", Expression = "(x-128)/1.28" },
                });
            group.TryAddParameter(
                new ProfileParameter
                {
                    Name = "Knock Retard",
                    DefineBy = DefineBy.Pid,
                    ByteCount = 1,
                    Address = 0x11A6,
                    Conversion = new Conversion { Name = "Degrees", Expression = "(x*256)/22.5" },
                });
            group.TryAddParameter(
                new ProfileParameter
                {
                    Name = "Target AFR",
                    DefineBy = DefineBy.Pid,
                    ByteCount = 1,
                    Address = 0x119E,
                    Conversion = new Conversion { Name = "AFR", Expression = "x*10" },
                });
            test.TryAddGroup(group);

            using (Stream outputStream = File.OpenWrite(@"C:\temp\test.profile"))
            {
                LogProfileWriter writer = new LogProfileWriter(outputStream);
                await writer.WriteAsync(test);
            }
        }
    }
}
