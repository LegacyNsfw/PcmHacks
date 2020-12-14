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
using System.Xml.Linq;

namespace PcmHacking
{
    public partial class MainForm : MainFormBase
    {
        private LogProfileAndMath profileAndMath;
        private bool logging;
        private object loggingLock = new object();
        private bool logStopRequested;
        private string profileName;
        private TaskScheduler uiThreadScheduler;
        private static DateTime lastLogTime;

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
//            this.selectProfileButton.Enabled = false;
            this.startStopLogging.Enabled = false;
        }

        protected override void EnableInterfaceSelection()
        {
            this.selectButton.Enabled = true;
        }

        protected override void EnableUserInput()
        {
            this.selectButton.Enabled = true;
//            this.selectProfileButton.Enabled = true;
            this.startStopLogging.Enabled = true;
            this.startStopLogging.Focus();
        }

        protected override void NoDeviceSelected()
        {
            this.selectButton.Enabled = true;
            this.deviceDescription.Text = "No device selected";
        }

        protected override void ValidDeviceSelected(string deviceName)
        {
            this.deviceDescription.Text = deviceName;

            // TODO: Do this asynchronously
            // this.AddExtendedParameters();
        }

        /// <summary>
        /// Open the last device, if possible.
        /// </summary>
        private async void MainForm_Load(object sender, EventArgs e)
        {
            this.uiThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            await this.ResetDevice();
//            string profilePath = Configuration.Settings.ProfilePath;
  //          if (!string.IsNullOrEmpty(profilePath))
    //        {
      //          await this.LoadProfile(profilePath);
         //   }

            string logDirectory = Configuration.Settings.LogDirectory;
            if (string.IsNullOrWhiteSpace(logDirectory))
            {
                logDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                Configuration.Settings.LogDirectory = logDirectory;
                Configuration.Settings.Save();
            }


            this.logFilePath.Text = logDirectory;

            // TODO: do this async
            this.FillParameterList();
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
                await this.LoadProfile(dialog.FileName);
            }
            else
            {
                this.profileAndMath = null;
                this.profileName = null;
            }

            this.UpdateStartStopButtonState();
        }

        /// <summary>
        /// Load the profile from the given path.
        /// </summary>
        private async Task LoadProfile(string path)
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

                MathValueConfigurationLoader loader = new MathValueConfigurationLoader(this);
                loader.Initialize();
                this.profileAndMath = new LogProfileAndMath(profile, loader.Configuration);
                this.logValues.Text = string.Join(Environment.NewLine, this.profileAndMath.GetColumnNames());
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
            this.startStopLogging.Enabled = this.Vehicle != null && this.profileAndMath != null;
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
                    if (this.profileAndMath == null)
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
        }

        /// <summary>
        /// The loop that reads data from the PCM.
        /// </summary>
        private async void LoggingThread(object threadContext)
        {
            using (AwayMode lockScreenSuppressor = new AwayMode())
            {
                try
                {
                    string logFilePath = GenerateLogFilePath();

                    this.loggerProgress.Invoke(
                    (MethodInvoker)
                    delegate ()
                    {
                        this.loggerProgress.Value = 0;
                        this.loggerProgress.Visible = true;
                        this.logFilePath.Text = logFilePath;
                        this.setDirectory.Enabled = false;
                        this.startStopLogging.Focus();
                    });

                    MathValueConfigurationLoader loader = new MathValueConfigurationLoader(this);
                    loader.Initialize();
                    Logger logger = new Logger(this.Vehicle, this.profileAndMath, loader.Configuration);
                    if (!await logger.StartLogging())
                    {
                        this.AddUserMessage("Unable to start logging.");
                        return;
                    }

#if Vpw4x
                if (!await this.Vehicle.VehicleSetVPW4x(VpwSpeed.FourX))
                {
                    this.AddUserMessage("Unable to switch to 4x.");
                    return;
                }
#endif
                    using (StreamWriter streamWriter = new StreamWriter(logFilePath))
                    {
                        LogFileWriter writer = new LogFileWriter(streamWriter);
                        IEnumerable<string> columnNames = this.profileAndMath.GetColumnNames();
                        await writer.WriteHeader(columnNames);

                        lastLogTime = DateTime.Now;

                        this.loggerProgress.Invoke(
                            (MethodInvoker)
                            delegate ()
                            {
                                this.loggerProgress.MarqueeAnimationSpeed = 150;
                                this.selectButton.Enabled = false;
//                                this.selectProfileButton.Enabled = false;
                            });

                        while (!this.logStopRequested)
                        {
                            this.AddDebugMessage("Requesting row...");
                            IEnumerable<string> rowValues = await logger.GetNextRow();
                            if (rowValues == null)
                            {
                                continue;
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
                                uiThreadScheduler);
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
                            this.startStopLogging.Focus();
                        });
                }
                finally
                {
#if Vpw4x
                if (!await this.Vehicle.VehicleSetVPW4x(VpwSpeed.Standard))
                {
                    // Try twice...
                    await this.Vehicle.VehicleSetVPW4x(VpwSpeed.Standard);
                }
#endif
                    this.logStopRequested = false;
                    this.logging = false;
                    this.startStopLogging.Invoke(
                        (MethodInvoker)
                        delegate ()
                        {
                            this.loggerProgress.MarqueeAnimationSpeed = 0;
                            this.loggerProgress.Visible = false;
                            this.startStopLogging.Enabled = true;
                            this.startStopLogging.Text = "Start &Logging";
                            this.logFilePath.Text = Configuration.Settings.LogDirectory;
                            this.setDirectory.Enabled = true;

                            this.selectButton.Enabled = true;
//                            this.selectProfileButton.Enabled = true;
                            this.startStopLogging.Focus();
                        });
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
            foreach(ParameterGroup group in this.profileAndMath.Profile.ParameterGroups)
            {
                foreach(ProfileParameter parameter in group.Parameters)
                {
                    rowValueEnumerator.MoveNext();
                    builder.Append(rowValueEnumerator.Current);
                    builder.Append('\t');
                    builder.Append(parameter.Conversion.Name);
                    builder.Append('\t');
                    builder.AppendLine(parameter.Name);
                }
            }

            foreach(MathValue mathValue in this.profileAndMath.MathValueProcessor.GetMathValues())
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

        private void newButton_Click(object sender, EventArgs e)
        {

        }

        private void openButton_Click(object sender, EventArgs e)
        {

        }

        private void saveButton_Click(object sender, EventArgs e)
        {

        }

        private void saveAsButton_Click(object sender, EventArgs e)
        {

        }

        private void FillParameterList()
        {
            try
            {
                string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string appDirectory = Path.GetDirectoryName(appPath);
                string parametersPath = Path.Combine(appDirectory, "Parameters.Standard.xml");
                XDocument xml = XDocument.Load(parametersPath);

                List<Parameter> parameters = new List<Parameter>();
                foreach (XElement parameter in xml.Root.Elements("Parameter"))
                {
                    List<Conversion> conversions = new List<Conversion>();
                    foreach (XElement conversion in parameter.Elements("Conversion"))
                    {
                        conversions.Add(
                            new Conversion(
                                conversion.Attribute("units").Value,
                                conversion.Attribute("formula").Value));
                    }

                    parameters.Add(
                        new Parameter(
                            parameter.Attribute("id").Value,
                            parameter.Attribute("name").Value,
                            parameter.Attribute("description").Value,
                            (ParameterType)Enum.Parse(typeof(ParameterType), parameter.Attribute("type").Value, true),
                            int.Parse(parameter.Attribute("size").Value),
                            bool.Parse(parameter.Attribute("bitMapped").Value),
                            conversions));
                }

                foreach (Parameter parameter in parameters)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    row.CreateCells(this.parameterGrid);
                    row.Cells[0].Value = false; // enabled
                    row.Cells[1].Value = parameter;

                    DataGridViewComboBoxCell cell = (DataGridViewComboBoxCell)row.Cells[2];
                    cell.DisplayMember = "Units";
                    foreach (Conversion conversion in parameter.Conversions)
                    {
                        cell.Items.Add(conversion);
                    }
                    row.Cells[2].Value = parameter.Conversions.First();
                    this.parameterGrid.Rows.Add(row);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    this,
                    exception.ToString(),
                    "Unable to load the parameter list.");
            }
        }
    }
}
