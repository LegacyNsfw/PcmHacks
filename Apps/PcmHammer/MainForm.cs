using CommandLine;
using J2534;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PcmHacking
{
    public partial class MainForm : MainFormBase, ILogger
    {
        /// <summary>
        /// This warning will be shown to users who have not yet verified their connectivity.
        /// </summary>
        /// <remarks>
        /// Users can verify connectivity by completing a successful test write, real write, or read.
        /// The message just encourages users to do a full read, because that's the best test.
        /// </remarks>
        private static readonly string UnverifiedConnectionWarning =
            "{0}" +
            Environment.NewLine + Environment.NewLine +
            "If this doesn't work, your vehicle will not be driveable." +
            Environment.NewLine + Environment.NewLine +
            "You should read the contents of your PCM before you try this. " +
            "A successful read will prove that you have a good " +
            "connection to your PCM, and will determine whether " +
            "or not there are any other modules in the vehicle " +
            "that will cause the write process to fail." +
            Environment.NewLine + Environment.NewLine +
            "A successful read will also give you a file that you can use to replace your current PCM with a new one if something goes wrong." +
            Environment.NewLine + Environment.NewLine +
            "It is dangerous to attempt to modify the PCM's flash memory before completing a successful read of the PCM.";

        /// <summary>
        /// Title for the unverified-connect warning prompt.
        /// </summary>
        private static readonly string UnverifiedConnectionWarningTitle = "Are you sure you want to do this?";

        /// <summary>
        /// Remind the user how to verify their connection.
        /// </summary>
        private static readonly string WiseChoice = "You have made a wise choice. Try a full read first.";

        /// <summary>
        /// Simple prompt for users who have already verified their connection.
        /// </summary>
        private static readonly string ClickOkToContinue = "Click OK to continue.";

        /// <summary>
        /// This will become the first half of the Window caption, and will 
        /// be printed to the user and debug logs each time a device is 
        /// initialized.
        /// </summary>
        private const string AppName = "PCM Hammer";

        /// <summary>
        /// This becomes the second half of the window caption, is printed
        /// when devices are initialized, and is used to create links to the
        /// help.html and start.txt files.
        /// 
        /// If null, the build timestamp will be used.
        /// 
        /// If not null, use a number like "004" that matches a release branch.
        /// </summary>
        private const string AppVersion = null;

        /// <summary>
        /// We had to move some operations to a background thread for the J2534 code as the DLL functions do not have an awaiter.
        /// </summary>
        private System.Threading.Thread BackgroundWorker = new System.Threading.Thread(delegate () { return; });

        /// <summary>
        /// This flag will initialized when a long-running operation begins. 
        /// It will be toggled if the user clicks the cancel button.
        /// Long-running operations can abort when this flag changes.
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Indicates what type of write, if any, is in progress.
        /// </summary>
        private WriteType currentWriteType = WriteType.None;

        /// <summary>
        /// Initializes a new instance of the main window.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Add a message to the main window.
        /// </summary>
        public override void AddUserMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("hh:mm:ss:fff");

            this.userLog.Invoke(
                (MethodInvoker)delegate ()
                {
                    this.userLog.AppendText("[" + timestamp + "]  " + message + Environment.NewLine);

                    // User messages are added to the debug log as well, so that the debug log has everything.
                    this.debugLog.AppendText("[" + timestamp + "]  " + message + Environment.NewLine);
                });
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

        public override void StatusUpdateActivity(string activity)
        {
            this.statusStatusStrip.Invoke(
                (MethodInvoker)delegate ()
                {
                    this.activityToolStripStatusLabel.Text = activity;
                });
        }

        public override void StatusUpdateTimeRemaining(string remaining)
        {
            this.statusStatusStrip.Invoke(
                (MethodInvoker)delegate ()
                {
                    this.timeRemainingToolStripStatusLabel.Text = remaining;
                });
        }

        public override void StatusUpdatePercentDone(string percent)
        {
            this.statusStatusStrip.Invoke(
                (MethodInvoker)delegate ()
                {
                    this.percentDoneToolStripStatusLabel.Text = percent;
                });
        }

        public override void StatusUpdateRetryCount(string retries)
        {
            this.statusStatusStrip.Invoke(
                (MethodInvoker)delegate ()
                {
                    this.retryCountToolStripStatusLabel.Text = retries;
                });
        }

        public override void StatusUpdateProgressBar(double completed, bool visible)
        {
            this.statusStatusStrip.Invoke(
                (MethodInvoker)delegate ()
                {
                    if (visible)
                    {
                        this.progressBarToolStripProgressBar.Visible = true;
                    }
                    else
                    {
                        this.progressBarToolStripProgressBar.Visible = false;
                    }

                    this.progressBarToolStripProgressBar.Value = (int)(completed * 100);
                });
        }

        public override void StatusUpdateKbps(string Kbps)
        {
            this.statusStatusStrip.Invoke(
                (MethodInvoker)delegate ()
                {
                    this.kbpsToolStripStatusLabel.Text = Kbps;
                });
        }

        public override void StatusUpdateReset()
        {
            this.StatusUpdateActivity(string.Empty);
            this.StatusUpdateTimeRemaining(string.Empty);
            this.StatusUpdatePercentDone(string.Empty);
            this.StatusUpdateRetryCount(string.Empty);
            this.StatusUpdateProgressBar(0, false);
            this.StatusUpdateKbps(string.Empty);
        }

        /// <summary>
        /// Reset the user and debug logs.
        /// </summary>
        public override void ResetLogs()
        {
            this.userLog.Invoke(
                (MethodInvoker)delegate ()
                {
                    this.userLog.Text = string.Empty;
                    this.debugLog.Text = string.Empty;
                });
        }

        /// <summary>
        /// Invoked when a device is selected but NOT successfully initalized.
        /// </summary>
        protected override void NoDeviceSelected()
        {
            this.deviceDescription.Text = "No device selected.";
        }

        /// <summary>
        /// Invoked when a device is selected and successfully initialized.
        /// </summary>
        protected override Task ValidDeviceSelectedAsync(string deviceName)
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                this.deviceDescription.Text = deviceName;
            });

            return Task.CompletedTask;
        }

        protected override void SetSelectedDeviceText(string message)
        {
            this.deviceDescription.Text = message;
        }

        /// <summary>
        /// Show the save-as dialog box (after a full read has completed).
        /// </summary>
        private string ShowSaveAsDialog()
        {
            string fileName = null;

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.DefaultExt = ".bin";
                dialog.Filter = "Binary Files (*.bin)|*.bin|All Files (*.*)|*.*";
                dialog.FilterIndex = 1;
                dialog.OverwritePrompt = true;
                dialog.ValidateNames = true;
                dialog.RestoreDirectory = true;

                if (!string.IsNullOrWhiteSpace(Configuration.Settings.BinDirectory))
                {
                    dialog.InitialDirectory = Configuration.Settings.BinDirectory;
                }

                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    fileName = dialog.FileName;
                }
            }
            return fileName;
        }

        /// <summary>
        /// Show the file-open dialog box, so the user can choose the file to write to the flash.
        /// </summary>
        private string ShowOpenDialog()
        {
            string fileName = null;

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.DefaultExt = ".bin";
                dialog.Filter = "Binary Files (*.bin)|*.bin|All Files (*.*)|*.*";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;

                if (!string.IsNullOrWhiteSpace(Configuration.Settings.BinDirectory))
                {
                    dialog.InitialDirectory = Configuration.Settings.BinDirectory;
                }

                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    fileName = dialog.FileName;
                }
            }
            return fileName;
        }

        /// <summary>
        /// Generate a filename based on Log Name and Timestamp.
        /// </summary>
        /// <remarks>
        /// i.e. userLog.Name or debugLog.Name
        /// </remarks>
        private string GetLogFilename(string logName)
        {
            string fileName =
                "PcmHammer_"
                + logName
                + "_"
                + DateTime.Now.ToString("yyyyMMdd@HHmmss")
                + ".txt";
            return fileName;
        }

        /// <summary>
        /// Show a save-as dialog box for saving log files.
        /// </summary>
        /// <remarks>
        /// i.e. userLog.Name or debugLog.Name
        /// </remarks>
        private string ShowLogSaveAsDialog(string logName)
        {
            string fileName = string.Empty;

            if (!Configuration.Settings.UseLogSaveAsDialog)
            {
                return Configuration.Settings.LogDirectory + "\\" + GetLogFilename(logName);
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;
                dialog.InitialDirectory = Configuration.Settings.LogDirectory;
                dialog.FileName = GetLogFilename(logName);

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = dialog.FileName;
                }
            }
            return fileName;
        }

        /// <summary>
        /// Gets a string to use in the window caption and at the top of each log.
        /// </summary>
        public override string GetAppNameAndVersion()
        {
            string versionString = AppVersion;
            if (versionString == null)
            {
                DateTime localTime = new DateTime(Generated.BuildTime).ToLocalTime();
                versionString = String.Format(
                    "({0}, {1})",
                    localTime.ToShortDateString(),
                    localTime.ToShortTimeString());
            }

            return AppName + " " + versionString;
        }

        /// <summary>
        /// Save the selected log
        /// </summary>
        protected void SaveLog(TextBox logBox, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(logBox.Text))
            {
                return;
            }

            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName))
                {
                    file.WriteLine(logBox.Text);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(this, fileName,
                    "ERROR file NOT Saved." + Environment.NewLine + e.Message,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }


        /// <summary>
        /// Called when the main window is being created.
        /// </summary>
        private async void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text = GetAppNameAndVersion();
                this.interfaceBox.Enabled = true;
                this.operationsBox.Enabled = true;

                // This will be enabled during full reads (but not writes)
                this.cancelButton.Enabled = false;

                // Load the dynamic content asynchronously.
                ThreadPool.QueueUserWorkItem(new WaitCallback(LoadStartMessage));
                ThreadPool.QueueUserWorkItem(new WaitCallback(LoadHelp));
                ThreadPool.QueueUserWorkItem(new WaitCallback(LoadCredits));

                this.MinimumSize = new Size(800, 600);

                if (string.IsNullOrWhiteSpace(Configuration.Settings.LogDirectory) || !Directory.Exists(Configuration.Settings.LogDirectory))
                {
                    Configuration.Settings.LogDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    Configuration.Settings.Save();
                }

                if (Configuration.Settings.MainWindowPersistence)
                {
                    if (Configuration.Settings.MainWindowSize.Width > 0 || Configuration.Settings.MainWindowSize.Height > 0)
                    {
                        this.WindowState = Configuration.Settings.MainWindowState;
                        if (this.WindowState == FormWindowState.Minimized)
                        {
                            this.WindowState = FormWindowState.Normal;
                        }
                        this.Location = Configuration.Settings.MainWindowLocation;
                        this.Size = Configuration.Settings.MainWindowSize;
                    }
                }

                this.StatusUpdateReset();

                ProcessCommandLine();

                await this.ResetDevice();
            }
            catch (Exception exception)
            {
                this.AddUserMessage(exception.Message);
                this.AddDebugMessage(exception.ToString());
            }
        }

        /// <summary>
        /// Parse cmdline parameters
        /// </summary>
        private void ProcessCommandLine()
        {
            string[] args = Environment.GetCommandLineArgs();
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed<CommandLineOptions>(o =>
                {
                    if (o.BinFilePath != null)
                    {
                        WriteCalibration(o.BinFilePath);
                    }

                    if (o.ShowVersion)
                    {
                        Console.WriteLine(GetAppNameAndVersion());
                    }

                    if (o.ResetDeviceConfiguration)
                    {
                        ResetDeviceConfiguration();
                    }
                });
        }

        /// <summary>
        /// Reset Device Configuration
        /// </summary>
        /// <remarks>
        /// Used by command line argument --resetdeviceconfig to reset device configuration
        /// </remarks>
        private void ResetDeviceConfiguration()
        {
            DeviceConfiguration.Settings.DeviceCategory = string.Empty;
            DeviceConfiguration.Settings.SerialPortDeviceType = string.Empty;
            DeviceConfiguration.Settings.J2534DeviceType = string.Empty;
            DeviceConfiguration.Settings.SerialPort = string.Empty;
            DeviceConfiguration.Settings.Save();
        }

        /// <summary>
        /// Write calibration automatically after program start, if cmdline parameter 
        /// "writecalibration" with filename is detected
        /// </summary>
        private async void WriteCalibration(string BinFilePath)
        {
            if (!writeCalibrationButton.Enabled)
            {
                await HandleSelectButtonClick();
            }
            if (writeCalibrationButton.Enabled)
            {
                BackgroundWorker = new System.Threading.Thread(() => write_BackgroundThread(WriteType.Calibration, BinFilePath));
                BackgroundWorker.IsBackground = true;
                BackgroundWorker.Start();
            }
            else
            {
                this.AddUserMessage("No device configured");
            }
        }

        /// <summary>
        /// The startup message is loaded after the window appears, so that it doesn't slow down app initialization.
        /// </summary>
        private async void LoadStartMessage(object unused)
        {
            ContentLoader loader = new ContentLoader("start.txt", AppVersion, Assembly.GetExecutingAssembly(), this);
            using (Stream content = await loader.GetContentStream())
            {
                try
                {
                    StreamReader reader = new StreamReader(content);
                    string message = reader.ReadToEnd();
                    this.AddUserMessage(message);
                }
                catch (Exception exception)
                {
                    this.AddDebugMessage("Unable to display startup message: " + exception.ToString());
                }
            }
        }

        /// <summary>
        /// The Help page is loaded after the window appears, so that it doesn't slow down app initialization.
        /// </summary>
        private async void LoadHelp(object unused)
        {
            ContentLoader loader = new ContentLoader("help.html", AppVersion, Assembly.GetExecutingAssembly(), this);
            Stream content = await loader.GetContentStream();
            this.helpWebBrowser.Invoke(
                (MethodInvoker)delegate ()
                {
                    try
                    {
                        this.helpWebBrowser.DocumentStream = content;
                    }
                    catch (Exception exception)
                    {
                        this.AddDebugMessage("Unable to load help content: " + exception.ToString());
                    }
                });
        }

        /// <summary>
        /// The credits page is loaded after the window appears, so that it doesn't slow down app initialization.
        /// </summary>
        private async void LoadCredits(object unused)
        {
            ContentLoader loader = new ContentLoader("credits.html", AppVersion, Assembly.GetExecutingAssembly(), this);
            Stream content = await loader.GetContentStream();
            this.helpWebBrowser.Invoke(
                (MethodInvoker)delegate ()
                {
                    try
                    {
                        this.creditsWebBrowser.DocumentStream = content;
                    }
                    catch (Exception exception)
                    {
                        this.AddDebugMessage("Unable load content for Credits tab: " + exception.ToString());
                    }
                });
        }

        /// <summary>
        /// Discourage users from closing the app during a write.
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            switch (this.currentWriteType)
            {
                case WriteType.None:
                case WriteType.TestWrite:
                    break;

                default:
                    DialogResult choice = MessageBox.Show(
                        this,
                        "Closing PCM Hammer now could make your PCM unusable." + Environment.NewLine +
                        "Are you sure you want to take that risk?",
                        "PCM Hammer",
                        MessageBoxButtons.YesNo);

                    if (choice == DialogResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
                    break;
            }

            if (Configuration.Settings.MainWindowPersistence)
            {
                Configuration.Settings.MainWindowState = this.WindowState;
                if (this.WindowState == FormWindowState.Normal)
                {
                    Configuration.Settings.MainWindowLocation = this.Location;
                    Configuration.Settings.MainWindowSize = this.Size;
                }
                else
                {
                    Configuration.Settings.MainWindowLocation = this.RestoreBounds.Location;
                    Configuration.Settings.MainWindowSize = this.RestoreBounds.Size;
                }
                Configuration.Settings.Save();
            }

            if (Configuration.Settings.SaveUserLogOnExit)
            {
                string fileName = Configuration.Settings.LogDirectory + "\\" + GetLogFilename(userLog.Name);
                SaveLog(this.userLog, fileName);
            }

            if (Configuration.Settings.SaveDebugLogOnExit)
            {
                string fileName = Configuration.Settings.LogDirectory + "\\" + GetLogFilename(debugLog.Name);
                SaveLog(this.debugLog, fileName);
            }
        }

        /// <summary>
        /// Disable buttons during a long-running operation (like reading or writing the flash).
        /// </summary>
        protected override void DisableUserInput()
        {
            this.interfaceBox.Enabled = false;

            // The operation buttons have to be enabled/disabled individually
            // (rather than via the parent GroupBox) because we sometimes want
            // to enable the re-initialize operation while the others are disabled.
            this.readEntirePCMToolStripMenuItem.Enabled = false;
            this.verifyEntirePCMToolStripMenuItem.Enabled = false;
            this.modifyVINToolStripMenuItem.Enabled = false;
            this.writeParmetersCloneToolStripMenuItem.Enabled = false;
            this.writeOSCalibrationBootToolStripMenuItem.Enabled = false;
            this.writeFullToolStripMenuItem.Enabled = false;
            this.settingsToolStripMenuItem.Enabled = false;
            this.saveToolStripMenuItem.Enabled = false;
            this.exitApplicationToolStripMenuItem.Enabled = false;
            this.userDefinedKeyToolStripMenuItem.Enabled = false;

            this.readPropertiesButton.Enabled = false;

            this.testWriteButton.Enabled = false;
            this.writeCalibrationButton.Enabled = false;
            this.exitKernelButton.Enabled = false;
            this.reinitializeButton.Enabled = false;
        }

        /// <summary>
        /// Enable the buttons when a long-running operation completes.
        /// </summary>
        protected override void EnableUserInput()
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                this.interfaceBox.Enabled = true;

                // The operation buttons have to be enabled/disabled individually
                // (rather than via the parent GroupBox) because we sometimes want
                // to enable the re-initialize operation while the others are disabled.
                this.readEntirePCMToolStripMenuItem.Enabled = true;
                this.verifyEntirePCMToolStripMenuItem.Enabled = true;
                this.modifyVINToolStripMenuItem.Enabled = true;
                this.writeParmetersCloneToolStripMenuItem.Enabled = true;
                this.writeOSCalibrationBootToolStripMenuItem.Enabled = true;
                this.writeFullToolStripMenuItem.Enabled = true;
                this.settingsToolStripMenuItem.Enabled = true;
                this.saveToolStripMenuItem.Enabled = true;
                this.exitApplicationToolStripMenuItem.Enabled = true;
                this.userDefinedKeyToolStripMenuItem.Enabled = true;

                this.readPropertiesButton.Enabled = true;

                this.testWriteButton.Enabled = true;
                this.writeCalibrationButton.Enabled = true;
                this.exitKernelButton.Enabled = true;
                this.reinitializeButton.Enabled = true;
            });
        }

        protected override void EnableInterfaceSelection()
        {
            this.interfaceBox.Enabled = true;
            this.settingsToolStripMenuItem.Enabled = true;
            this.exitApplicationToolStripMenuItem.Enabled = true;
        }

        /// <summary>
        /// Save Debug Log
        /// </summary>
        private void saveDebugLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = ShowLogSaveAsDialog(debugLog.Name);
            SaveLog(debugLog, fileName);
        }

        /// <summary>
        /// Save Results Log
        /// </summary>
        private void saveResultsLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = ShowLogSaveAsDialog(userLog.Name);
            SaveLog(userLog, fileName);
        }

        /// <summary>
        /// Exit Application
        /// </summary>
        private void exitApplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// Settings Dialog
        /// </summary>
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (DialogBoxes.SettingsDialogBox settingsDialog = new DialogBoxes.SettingsDialogBox())
            {
                DialogResult dialogResult = settingsDialog.ShowDialog();
            }
        }

        /// <summary>
        /// User Defined Key
        /// </summary>
        private void userDefinedKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (userDefinedKeyToolStripMenuItem.Checked)
            {
                using (DialogBoxes.UserDefinedKeyDialogBox keyDialog = new DialogBoxes.UserDefinedKeyDialogBox())
                {
                    DialogResult dialogResult = keyDialog.ShowDialog();
                    if (dialogResult == DialogResult.OK)
                    {
                        this.Vehicle.UserDefinedKey = keyDialog.UserDefinedKey;
                    }
                    else
                    {
                        this.userDefinedKeyToolStripMenuItem.Checked = false;
                    }
                }
            }
            else
            {
                this.Vehicle.UserDefinedKey = -1;
            }
        }

        /// <summary>
        /// Select which interface device to use. This opens the Device-Picker dialog box.
        /// </summary>
        private async void selectButton_Click(object sender, EventArgs e)
        {
            await this.HandleSelectButtonClick();
        }

        /// <summary>
        /// Reset the current interface device.
        /// </summary>
        private async void reinitializeButton_Click(object sender, EventArgs e)
        {
            await this.InitializeCurrentDevice();
        }
        
        /// <summary>
        /// Read the VIN, OS, etc.
        /// </summary>
        private async void readPropertiesButton_Click(object sender, EventArgs e)
        {
            if (this.Vehicle == null)
            {
                // This shouldn't be possible - it would mean the buttons 
                // were enabled when they shouldn't be.
                return;
            }

            try
            {
                PcmInfo pcmInfo = null;

                this.DisableUserInput();

                var vinResponse = await this.Vehicle.QueryVin();
                if (vinResponse.Status != ResponseStatus.Success)
                {
                    this.AddUserMessage("VIN query failed: " + vinResponse.Status.ToString());
                    await this.Vehicle.ExitKernel();
                    return;
                }
                this.AddUserMessage("VIN: " + vinResponse.Value);

                var osResponse = await this.Vehicle.QueryOperatingSystemId(CancellationToken.None);
                if (osResponse.Status == ResponseStatus.Success)
                {
                    this.AddUserMessage("OS ID: " + osResponse.Value.ToString());
                    pcmInfo = new PcmInfo(osResponse.Value);
                    this.AddUserMessage("Hardware Type: " + pcmInfo.HardwareType.ToString());
                    if (pcmInfo.HardwareType == PcmType.P04)
                    {
                        this.AddUserMessage("**********************************************");
                        this.AddUserMessage("WARNING: P04 Support is still in development.");
                        this.AddUserMessage("It may or may not read your P04 correctly.");
                        this.AddUserMessage("There is currently no ETA for P04 Write.");
                        this.AddUserMessage("**********************************************");
                    }
                }
                else
                {
                    this.AddUserMessage("OS ID query failed: " + osResponse.Status.ToString());
                }

                var calResponse = await this.Vehicle.QueryCalibrationId();
                if (calResponse.Status == ResponseStatus.Success)
                {
                    this.AddUserMessage("Calibration ID: " + calResponse.Value.ToString());
                }
                else
                {
                    this.AddUserMessage("Calibration ID query failed: " + calResponse.Status.ToString());
                }

                // Disable HardwareID lookup for the P10, P12 and E54.
                if (pcmInfo != null && pcmInfo.HardwareType != PcmType.P10 && pcmInfo.HardwareType != PcmType.P12 && pcmInfo.HardwareType != PcmType.E54)
                {
                    var hardwareResponse = await this.Vehicle.QueryHardwareId();
                    if (hardwareResponse.Status == ResponseStatus.Success)
                    {
                        this.AddUserMessage("Hardware ID: " + hardwareResponse.Value.ToString());
                    }
                    else
                    {
                        this.AddUserMessage("Hardware ID query failed: " + hardwareResponse.Status.ToString());
                    }
                }

                var serialResponse = await this.Vehicle.QuerySerial();
                if (serialResponse.Status == ResponseStatus.Success)
                {
                    this.AddUserMessage("Serial Number: " + serialResponse.Value.ToString());
                }
                else
                {
                    this.AddUserMessage("Serial Number query failed: " + serialResponse.Status.ToString());
                }

                // Disable BCC lookup for the P04
                if (pcmInfo != null && pcmInfo.HardwareType != PcmType.P04 && pcmInfo.HardwareType != PcmType.P08)
                {
                    var bccResponse = await this.Vehicle.QueryBCC();
                    if (bccResponse.Status == ResponseStatus.Success)
                    {
                        this.AddUserMessage("Broad Cast Code: " + bccResponse.Value.ToString());
                    }
                    else
                    {
                        this.AddUserMessage("BCC query failed: " + bccResponse.Status.ToString());
                    }
                }

                var mecResponse = await this.Vehicle.QueryMEC();
                if (mecResponse.Status == ResponseStatus.Success)
                {
                    this.AddUserMessage("MEC: " + mecResponse.Value.ToString());
                }
                else
                {
                    this.AddUserMessage("MEC query failed: " + mecResponse.Status.ToString());
                }
            }
            catch (Exception exception)
            {
                this.AddUserMessage(exception.Message);
                this.AddDebugMessage(exception.ToString());
            }
            finally
            {
                this.EnableUserInput();
            }
        }

        /// <summary>
        /// Update the VIN.
        /// </summary>
        private async void modifyVinButton_Click(object sender, EventArgs e)
        {
            try
            {
                Response<uint> osidResponse = await this.Vehicle.QueryOperatingSystemId(CancellationToken.None);
                if (osidResponse.Status != ResponseStatus.Success)
                {
                    this.AddUserMessage("Operating system query failed: " + osidResponse.Status);
                    return;
                }

                PcmInfo info = new PcmInfo(osidResponse.Value);

                var vinResponse = await this.Vehicle.QueryVin();
                if (vinResponse.Status != ResponseStatus.Success)
                {
                    this.AddUserMessage("VIN query failed: " + vinResponse.Status.ToString());
                    return;
                }

                DialogBoxes.VinForm vinForm = new DialogBoxes.VinForm();
                vinForm.Vin = vinResponse.Value;
                DialogResult dialogResult = vinForm.ShowDialog();

                if (dialogResult == DialogResult.OK)
                {
                    bool unlocked = await this.Vehicle.UnlockEcu(info.KeyAlgorithm);
                    if (!unlocked)
                    {
                        this.AddUserMessage("Unable to unlock PCM.");
                        return;
                    }

                    Response<bool> vinmodified = await this.Vehicle.UpdateVin(vinForm.Vin.Trim());
                    if (vinmodified.Value)
                    {
                        this.AddUserMessage("VIN successfully updated to " + vinForm.Vin);
                        MessageBox.Show("VIN updated to " + vinForm.Vin + " successfully.", "Good news.", MessageBoxButtons.OK);
                    }
                    else
                    {
                        MessageBox.Show("Unable to change the VIN to " + vinForm.Vin + ". Error: " + vinmodified.Status, "Bad news.", MessageBoxButtons.OK);
                    }
                }
            }
            catch (Exception exception)
            {
                this.AddUserMessage("VIN change failed: " + exception.ToString());
            }
        }

        /// <summary>
        /// Read the entire contents of the flash.
        /// </summary>
        private void readFullContentsButton_Click(object sender, EventArgs e)
        {
            if (!BackgroundWorker.IsAlive)
            {
                BackgroundWorker = new System.Threading.Thread(() => readFullContents_BackgroundThread());
                BackgroundWorker.IsBackground = true;
                BackgroundWorker.Start();
            }
        }

        /// <summary>
        /// Prompt the user before a write operation.
        /// </summary>
        /// <param name="description">The first line of text in the dialog box.</param>
        /// <returns>True if the user wants to proceed, false if not.</returns>
        private bool ConfirmBeforeWrite(string description)
        {
            DialogResult result;
            if (Configuration.Settings.ConnectionVerified)
            {
                result = MessageBox.Show(
                    description,
                    MainForm.ClickOkToContinue,
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            }
            else
            {
                result = MessageBox.Show(
                    string.Format(
                        MainForm.UnverifiedConnectionWarning,
                        description),
                    MainForm.UnverifiedConnectionWarningTitle,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1); ;
            }

            switch(result)
            {
                case DialogResult.OK:
                case DialogResult.Yes:
                    return true;

                case DialogResult.No:
                    this.AddUserMessage(MainForm.WiseChoice);
                    return false;

                case DialogResult.Cancel:
                default:
                    return false;
            }
        }

        /// <summary>
        /// Write Calibration.
        /// </summary>
        private void writeCalibrationButton_Click(object sender, EventArgs e)
        {
            if (!BackgroundWorker.IsAlive)
            {
                if (ConfirmBeforeWrite("This will update the calibration on your PCM."))
                { 
                    BackgroundWorker = new System.Threading.Thread(() => write_BackgroundThread(WriteType.Calibration));
                    BackgroundWorker.IsBackground = true;
                    BackgroundWorker.Start();
                }
            }
        }

        /// <summary>
        /// Write the parameter blocks (VIN, problem history, etc)
        /// </summary>
        private void writeParametersButton_Click(object sender, EventArgs e)
        {
            if (!BackgroundWorker.IsAlive)
            {
                if (ConfirmBeforeWrite("This will update the parameter block on your PCM."))
                {
                    BackgroundWorker = new System.Threading.Thread(() => write_BackgroundThread(WriteType.Parameters));
                    BackgroundWorker.IsBackground = true;
                    BackgroundWorker.Start();
                }
            }
        }

        /// <summary>
        /// Write Os, Calibration and Boot.
        /// </summary>
        private void writeOSCalibrationBootToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!BackgroundWorker.IsAlive)
            {
                if (ConfirmBeforeWrite("This will replace the operating system and calibration on your PCM."))
                {
                    BackgroundWorker = new System.Threading.Thread(() => write_BackgroundThread(WriteType.OsPlusCalibrationPlusBoot));
                    BackgroundWorker.IsBackground = true;
                    BackgroundWorker.Start();
                }
            }
        }

        /// <summary>
        /// Write Full flash (Clone)
        /// </summary>
        private void writeFullToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!BackgroundWorker.IsAlive)
            {
                if (ConfirmBeforeWrite("This will replace the contents of the flash memory on your PCM."))
                { 
                    BackgroundWorker = new System.Threading.Thread(() => write_BackgroundThread(WriteType.Full));
                    BackgroundWorker.IsBackground = true;
                    BackgroundWorker.Start();
                }
            }
        }

        /// <summary>
        /// Compare block CRCs of a file and the PCM.
        /// </summary>
        private void quickComparisonButton_Click(object sender, EventArgs e)
        {
            if (!BackgroundWorker.IsAlive)
            {
                BackgroundWorker = new System.Threading.Thread(() => write_BackgroundThread(WriteType.Compare));
                BackgroundWorker.IsBackground = true;
                BackgroundWorker.Start();
            }
        }

        private void testWriteButton_Click(object sender, EventArgs e)
        {
            if (!BackgroundWorker.IsAlive)
            {
                BackgroundWorker = new System.Threading.Thread(() => write_BackgroundThread(WriteType.TestWrite));
                BackgroundWorker.IsBackground = true;
                BackgroundWorker.Start();
            }
        }

        /// <summary>
        /// Test something in a kernel.
        /// </summary>
        private void testKernelButton_Click(object sender, EventArgs e)
        {
            if (!BackgroundWorker.IsAlive)
            {
                BackgroundWorker = new System.Threading.Thread(() => exitKernel_BackgroundThread());
                BackgroundWorker.IsBackground = true;
                BackgroundWorker.Start();
            }
        }

        /// <summary>
        /// Set the cancelOperation flag, so that an ongoing operation can be aborted.
        /// </summary>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            if ((this.currentWriteType != WriteType.None) && (this.currentWriteType != WriteType.TestWrite))
            {
                var choice = MessageBox.Show(
                    this,
                    "Canceling now could make your PCM unusable." + Environment.NewLine +
                    "Are you sure you want to take that risk?",
                    "PCM Hammer",
                    MessageBoxButtons.YesNo);

                if (choice == DialogResult.No)
                {
                    return;
                }
            }

            this.AddUserMessage("Cancel button clicked.");
            this.cancellationTokenSource?.Cancel();
        }
        
        /// <summary>
        /// Read the entire contents of the flash.
        /// </summary>
        private async void readFullContents_BackgroundThread()
        {
            using (new AwayMode())
            {
                try
                {
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        this.DisableUserInput();
                        this.cancelButton.Enabled = true;
                    });

                    if (this.Vehicle == null)
                    {
                        // This shouldn't be possible - it would mean the buttons 
                        // were enabled when they shouldn't be.
                        return;
                    }

                    // Get the path to save the image to.
                    string path = "";
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        path = this.ShowSaveAsDialog();

                        if (path == null)
                        {
                            return;
                        }

                        this.AddUserMessage("Will save to " + path);

                        DelayDialogBox dialogBox = new DelayDialogBox();
                        DialogResult dialogResult = dialogBox.ShowDialog(this);
                        if (dialogResult == DialogResult.Cancel)
                        {
                            path = null;
                            return;
                        }
                    });

                    if (path == null)
                    {
                        this.AddUserMessage("Read canceled.");
                        return;
                    }

                    this.cancellationTokenSource = new CancellationTokenSource();

                    this.AddUserMessage("Querying operating system of current PCM.");
                    Response<uint> osidResponse = await this.Vehicle.QueryOperatingSystemId(this.cancellationTokenSource.Token);
                    if (osidResponse.Status != ResponseStatus.Success)
                    {
                        this.AddUserMessage("Operating system query failed, will retry: " + osidResponse.Status);
                        await this.Vehicle.ExitKernel();

                        osidResponse = await this.Vehicle.QueryOperatingSystemId(this.cancellationTokenSource.Token);
                        if (osidResponse.Status != ResponseStatus.Success)
                        {
                            this.AddUserMessage("Operating system query failed: " + osidResponse.Status);
                        }
                    }

                    PcmInfo pcmInfo;
                    if (osidResponse.Status == ResponseStatus.Success)
                    {
                        // Look up the information about this PCM, based on the OSID;
                        this.AddUserMessage("OSID: " + osidResponse.Value);
                        pcmInfo = new PcmInfo(osidResponse.Value);
                    }
                    else
                    {
                        this.AddUserMessage("Unable to get operating system ID. Will assume this can be unlocked with the default seed/key algorithm.");

                        UInt32 OperatingSystemId = 0;

                        await Vehicle.ForceSendToolPresentNotification();
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            OperatingSystemIDDialogBox osDialog = new OperatingSystemIDDialogBox();
                            DialogResult dialogResult = osDialog.ShowDialog();
                            if (dialogResult == DialogResult.OK)
                            {
                                OperatingSystemId = osDialog.OperatingSystemId;
                            }
                        });
                        await Vehicle.ForceSendToolPresentNotification();

                        pcmInfo = new PcmInfo(OperatingSystemId); // osid

                        AddUserMessage($"Using OsID: {pcmInfo.OSID}");
                    }

                    if (pcmInfo.HardwareType == PcmType.P04)
                    {
                        this.AddUserMessage("WARNING: P04 Support i still in development.");
                        this.AddUserMessage("It may or may not read your P04 correctly.");
                        DialogResult dialogResult = MessageBox.Show("WARNING: P04 Read is still in development.\nIt may or may not read your P04 correctly.\n", "Continue?", MessageBoxButtons.YesNo);
                        if (dialogResult == DialogResult.No)
                        {
                            this.AddUserMessage("User chose not to proceed");
                            return;
                        }
                    }

                    await this.Vehicle.SuppressChatter();

                    bool unlocked = await this.Vehicle.UnlockEcu(pcmInfo.KeyAlgorithm);
                    if (!unlocked)
                    {
                        this.AddUserMessage("Unlock was not successful.");
                        return;
                    }

                    this.AddUserMessage("Unlock succeeded.");

                    if (cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    // Do the actual reading.
                    DateTime start = DateTime.Now;

                    CKernelReader reader = new CKernelReader(
                        this.Vehicle,
                        pcmInfo,
                        this);

                    Response<Stream> readResponse = await reader.ReadContents(cancellationTokenSource.Token);

                    this.AddUserMessage("Elapsed time " + DateTime.Now.Subtract(start));
                    if (readResponse.Status != ResponseStatus.Success)
                    {
                        this.AddUserMessage("Read failed, " + readResponse.Status.ToString());
                        return;
                    }

                    // This will suppress the scary warnings prior to writing.
                    Configuration.Settings.ConnectionVerified = true;

                    // Save the contents to the path that the user provided.
                    bool success = false;
                    do
                    {
                        try
                        {
                            this.AddUserMessage("Saving contents to " + path);

                            readResponse.Value.Position = 0;

                            using (Stream output = File.Open(path, FileMode.Create))
                            {
                                await readResponse.Value.CopyToAsync(output);
                            }

                            success = true;
                        }
                        catch (IOException exception)
                        {
                            this.AddUserMessage("Unable to save file: " + exception.Message);
                            this.AddDebugMessage(exception.ToString());

                            this.Invoke((MethodInvoker)delegate () { path = this.ShowSaveAsDialog(); });
                            if (path == null)
                            {
                                this.AddUserMessage("Save canceled.");
                                return;
                            }
                        }
                    } while (!success);
                }
                catch (Exception exception)
                {
                    this.AddUserMessage("Read failed: " + exception.ToString());
                }
                finally
                {
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        this.EnableUserInput();
                        this.cancelButton.Enabled = false;
                    });

                    // The token / token-source can only be cancelled once, so we need to make sure they won't be re-used.
                    this.cancellationTokenSource = null;
                }
            }
        }

        /// <summary>
        /// Write changes to the PCM's flash memory.
        /// </summary>
        private async void write_BackgroundThread(WriteType writeType, string path = null)
        {
            using (new AwayMode())
            {
                try
                {
                    this.currentWriteType = writeType;

                    if (this.Vehicle == null)
                    {
                        // This shouldn't be possible - it would mean the buttons 
                        // were enabled when they shouldn't be.
                        return;
                    }

                    this.cancellationTokenSource = new CancellationTokenSource();
                    
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        this.DisableUserInput();
                        this.cancelButton.Enabled = true;

                        if (string.IsNullOrWhiteSpace(path))
                        {
                            path = this.ShowOpenDialog();
                        }
                        if (string.IsNullOrWhiteSpace(path))
                        {
                            return;
                        }

                        DelayDialogBox dialogBox = new DelayDialogBox();
                        DialogResult dialogResult = dialogBox.ShowDialog(this);
                        if (dialogResult == DialogResult.Cancel)
                        {
                            path = null;
                            return;
                        }
                    });

                    if (path == null)
                    {
                        this.AddUserMessage(
                            writeType == WriteType.TestWrite ?
                                "Test write canceled." :
                                "Write canceled.");
                        return;
                    }

                    this.AddUserMessage(path);

                    byte[] image;
                    using (Stream stream = File.OpenRead(path))
                    {
                        image = new byte[stream.Length];
                        int bytesRead = await stream.ReadAsync(image, 0, (int)stream.Length);
                        if (bytesRead != stream.Length)
                        {
                            // If this happens too much, we should try looping rather than reading the whole file in one shot.
                            this.AddUserMessage("Unable to load file.");
                            return;
                        }
                    }

                    // Sanity checks. 
                    FileValidator validator = new FileValidator(image, this);
                    if (!validator.IsValid())
                    {
                        this.AddUserMessage("This file is corrupt or its format is unknown to PCMHammer. It would render your PCM unusable.");
                        return;
                    }

                    UInt32 kernelVersion = 0;
                    bool needUnlock;
                    int keyAlgorithm = 1;
                    bool shouldHalt;
                    PcmInfo pcmInfo = null;
                    bool needToCheckOperatingSystem =
                        (writeType != WriteType.OsPlusCalibrationPlusBoot) &&
                        (writeType != WriteType.Full) &&
                        (writeType != WriteType.TestWrite);

                    this.AddUserMessage("Requesting operating system ID...");
                    Response<uint> osidResponse = await this.Vehicle.QueryOperatingSystemId(this.cancellationTokenSource.Token);
                    if (osidResponse.Status == ResponseStatus.Success)
                    {
                        pcmInfo = new PcmInfo(osidResponse.Value);
                        keyAlgorithm = pcmInfo.KeyAlgorithm;
                        needUnlock = true;

                        if (!validator.IsSameHardware(osidResponse.Value))
                        {
                            return;
                        }

                        if (!validator.IsSameOperatingSystem(osidResponse.Value))
                        {
                            Utility.ReportOperatingSystems(validator.GetOsidFromImage(), osidResponse.Value, writeType, this, out shouldHalt);
                            if (shouldHalt)
                            {
                                return;
                            }
                        }

                        needToCheckOperatingSystem = false;
                    }
                    else
                    {
                        if (this.cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            return;
                        }

                        this.AddUserMessage("Operating system request failed, checking for a live kernel...");

                        kernelVersion = await this.Vehicle.GetKernelVersion();
                        if (kernelVersion == 0)
                        {
                            this.AddUserMessage("Checking for recovery mode...");
                            bool recoveryMode = await this.Vehicle.IsInRecoveryMode();

                            if (recoveryMode)
                            {
                                this.AddUserMessage("PCM is in recovery mode.");
                                needUnlock = true;
                            }
                            else
                            {
                                this.AddUserMessage("PCM is not responding to OSID, kernel version, or recovery mode checks.");
                                this.AddUserMessage("Unlock may not work, but we'll try...");
                                needUnlock = true;
                            }
                            pcmInfo = new PcmInfo(validator.GetOsidFromImage()); // Prevent Null Reference Exceptions from breaking Recovery Mode
                        }
                        else
                        {
                            needUnlock = false;

                            this.AddUserMessage("Kernel version: " + kernelVersion.ToString("X8"));

                            this.AddUserMessage("Asking kernel for the PCM's operating system ID...");

                            if (needToCheckOperatingSystem)
                            {
                                osidResponse = await this.Vehicle.QueryOperatingSystemIdFromKernel(this.cancellationTokenSource.Token);
                                if (osidResponse.Status != ResponseStatus.Success)
                                {
                                    // The kernel seems broken. This shouldn't happen, but if it does, halt.
                                    this.AddUserMessage("The kernel did not respond to operating system ID query.");
                                    return;
                                }

                                Utility.ReportOperatingSystems(validator.GetOsidFromImage(), osidResponse.Value, writeType, this, out shouldHalt);
                                if (shouldHalt)
                                {
                                    return;
                                }

                                pcmInfo = new PcmInfo(osidResponse.Value);
                            }

                            needToCheckOperatingSystem = false;
                        }
                    }

                    if (writeType != WriteType.Compare && pcmInfo.HardwareType == PcmType.P04 && pcmInfo.HardwareType == PcmType.E54)
                    {
                        string msg = $"PCMHammer currently does not support writing to the {pcmInfo.HardwareType.ToString()}";
                        this.AddUserMessage(msg);
                        MessageBox.Show(msg);
                        return;
                    }

                    await this.Vehicle.SuppressChatter();

                    if (needUnlock)
                    {

                        bool unlocked = await this.Vehicle.UnlockEcu(keyAlgorithm);
                        if (!unlocked)
                        {
                            this.AddUserMessage("Unlock was not successful.");
                            return;
                        }

                        this.AddUserMessage("Unlock succeeded.");
                    }

                    DateTime start = DateTime.Now;

                    CKernelWriter writer = new CKernelWriter(
                        this.Vehicle,
                        pcmInfo,
                        new Protocol(),
                        writeType,
                        this);

                    await writer.Write(
                        image,
                        kernelVersion,
                        validator,
                        needToCheckOperatingSystem,
                        this.cancellationTokenSource.Token);

                    this.AddUserMessage("Elapsed time " + DateTime.Now.Subtract(start));

                    // This will suppress the scary warnings prior to writing.
                    Configuration.Settings.ConnectionVerified = true;
                }
                catch (IOException exception)
                {
                    this.AddUserMessage(exception.ToString());
                }
                finally
                {
                    this.currentWriteType = WriteType.None;

                    this.Invoke((MethodInvoker)delegate ()
                    {
                        this.EnableUserInput();
                        this.cancelButton.Enabled = false;
                    });

                    // The token / token-source can only be cancelled once, so we need to make sure they won't be re-used.
                    this.cancellationTokenSource = null;
                }
            }
        }

        /// <summary>
        /// From the user's perspective, this is for exiting the kernel, in 
        /// case it remains running after an aborted operation.
        /// 
        /// From the developer's perspective, this is for testing, debugging,
        /// and investigating kernel features that are development.
        /// </summary>
        private async void exitKernel_BackgroundThread()
        {
            try
            {
                if (this.Vehicle == null)
                {
                    // This shouldn't be possible - it would mean the buttons 
                    // were enabled when they shouldn't be.
                    return;
                }

                this.Invoke((MethodInvoker)delegate ()
                {
                    this.DisableUserInput();
                    this.cancelButton.Enabled = true;
                });

                this.cancellationTokenSource = new CancellationTokenSource();

                try
                {
                    await this.Vehicle.ExitKernel(true, false, this.cancellationTokenSource.Token, null);
                }
                catch (IOException exception)
                {
                    this.AddUserMessage(exception.ToString());
                }
            }
            finally
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    this.EnableUserInput();
                    this.cancelButton.Enabled = false;
                });

                // The token / token-source can only be cancelled once, so we need to make sure they won't be re-used.
                this.cancellationTokenSource = null;
            }
        }

        private async void testFileChecksumsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = this.ShowOpenDialog();
            if (path == null)
            {
                return;
            }

            this.AddUserMessage("Examining " + path);

            byte[] image;
            using (Stream stream = File.OpenRead(path))
            {
                image = new byte[stream.Length];
                int bytesRead = await stream.ReadAsync(image, 0, (int)stream.Length);
                if (bytesRead != stream.Length)
                {
                    // If this happens too much, we should try looping rather than reading the whole file in one shot.
                    this.AddUserMessage("Unable to load file.");
                    return;
                }
            }

            // Sanity checks. 
            FileValidator validator = new FileValidator(image, this);
            if (validator.IsValid())
            {
                this.AddUserMessage("All checksums are valid.");
            }
            else
            {
                this.AddUserMessage("This file is corrupt or its format is unknown to PCMHammer. It would render your PCM unusable.");
            }
        }
    }
}
