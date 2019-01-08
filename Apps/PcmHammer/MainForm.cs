  
﻿using J2534;
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
    public partial class MainForm : Form, ILogger
    {
        /// <summary>
        /// This will become the first half of the Window caption, and will 
        /// be printed to the user and debug logs each time a device is 
        /// initialized.
        /// </summary>
        private const string AppName = "PCM Hammer";

        /// <summary>
        /// This becomes the second half of the window caption, and is printed
        /// when devices are initialized. 
        /// 
        /// If null, the build timestamp will be used.
        /// </summary>
        private const string AppVersion = null;

        /// <summary>
        /// The Vehicle object is our interface to the car. It has the device, the message generator, and the message parser.
        /// </summary>
        private Vehicle vehicle;

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
        /// Initializes a new instance of the main window.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Add a message to the main window.
        /// </summary>
        public void AddUserMessage(string message)
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
        public void AddDebugMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("hh:mm:ss:fff");

            this.debugLog.Invoke(
                (MethodInvoker)delegate ()
                {
                    this.debugLog.AppendText("[" + timestamp + "]  " + message + Environment.NewLine);
                });
        }

        /// <summary>
        /// Show the save-as dialog box (after a full read has completed).
        /// </summary>
        private string ShowSaveAsDialog()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = ".bin";
            dialog.Filter = "Binary Files (*.bin)|*.bin|All Files (*.*)|*.*";
            dialog.FilterIndex = 0;
            dialog.OverwritePrompt = true;
            dialog.ValidateNames = true;
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                return dialog.FileName;
            }

            return null;
        }

        /// <summary>
        /// Show the file-open dialog box, so the user can choose the file to write to the flash.
        /// </summary>
        private string ShowOpenDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".bin";
            dialog.Filter = "Binary Files (*.bin)|*.bin|All Files (*.*)|*.*";
            dialog.FilterIndex = 0;
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                return dialog.FileName;
            }

            return null;
        }

        /// <summary>
        /// Gets a string to use in the window caption and at the top of each log.
        /// </summary>
        private static string GetAppNameAndVersion()
        {
            string versionString = AppVersion;
            if (versionString == null)
            {
                DateTime localTime = Generated.BuildTime.ToLocalTime();
                versionString = String.Format(
                    "({0}, {1})",
                    localTime.ToShortDateString(),
                    localTime.ToShortTimeString());
            }

            return AppName + " " + versionString;
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

                // Load the Help content asynchronously.
                ThreadPool.QueueUserWorkItem(new WaitCallback(LoadHelp));

                await this.ResetDevice();

                this.MinimumSize = new Size(800, 600);
            }
            catch (Exception exception)
            {
                this.AddUserMessage(exception.Message);
                this.AddDebugMessage(exception.ToString());
            }
        }

        /// <summary>
        /// The Help content is loaded after the window appears, so that it doesn't slow down app initialization.
        /// </summary>
        private void LoadHelp(object unused)
        {
            this.helpWebBrowser.Invoke((MethodInvoker)async delegate ()
            {
                try
                {
                    HttpRequestMessage request = new HttpRequestMessage(
                        HttpMethod.Get,
                        "https://raw.githubusercontent.com/LegacyNsfw/PcmHacks/Release/003/Apps/PcmHammer/help.html");
                    request.Headers.Add("Cache-Control", "no-cache");
                    HttpClient client = new HttpClient();
                    var response = await client.SendAsync(request);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        this.helpWebBrowser.DocumentStream = await response.Content.ReadAsStreamAsync();
                    }
                    else
                    {
                        var assembly = Assembly.GetExecutingAssembly();
                        var resourceName = "PcmHacking.help.html";

                        // This will leak the stream, but it will only be invoked once.
                        this.helpWebBrowser.DocumentStream = assembly.GetManifestResourceStream(resourceName);
                    }
                }
                catch (Exception exception)
                {
                    this.AddUserMessage("Unable to load help content: " + exception.Message);
                    this.AddDebugMessage(exception.ToString());
                }
            });
        }

        /// <summary>
        /// Disable buttons during a long-running operation (like reading or writing the flash).
        /// </summary>
        private void DisableUserInput()
        {
            this.interfaceBox.Enabled = false;

            // The operation buttons have to be enabled/disabled individually
            // (rather than via the parent GroupBox) because we sometimes want
            // to enable the re-initialize operation while the others are disabled.
            this.readPropertiesButton.Enabled = false;
            this.readFullContentsButton.Enabled = false;
            this.modifyVinButton.Enabled = false;
            this.quickComparisonButton.Enabled = false;
            this.testWriteButton.Enabled = false;
            this.writeCalibrationButton.Enabled = false;
            this.writeParametersButton.Enabled = false;
            this.writeOsAndCalibration.Enabled = false;
            this.writeFullContentsButton.Enabled = false;
            this.exitKernelButton.Enabled = false;
            this.reinitializeButton.Enabled = false;
        }

        /// <summary>
        /// Enable the buttons when a long-running operation completes.
        /// </summary>
        private void EnableUserInput()
        {
            this.interfaceBox.Invoke((MethodInvoker)delegate () { this.interfaceBox.Enabled = true; });

            // The operation buttons have to be enabled/disabled individually
            // (rather than via the parent GroupBox) because we sometimes want
            // to enable the re-initialize operation while the others are disabled.
            this.readPropertiesButton.Invoke((MethodInvoker)delegate () { this.readPropertiesButton.Enabled = true; });
            this.readFullContentsButton.Invoke((MethodInvoker)delegate () { this.readFullContentsButton.Enabled = true; });
            this.modifyVinButton.Invoke((MethodInvoker)delegate () { this.modifyVinButton.Enabled = true; });
            this.quickComparisonButton.Invoke((MethodInvoker)delegate () { this.quickComparisonButton.Enabled = true; });
            this.testWriteButton.Invoke((MethodInvoker)delegate () { this.testWriteButton.Enabled = true; });
            this.writeCalibrationButton.Invoke((MethodInvoker)delegate () { this.writeCalibrationButton.Enabled = true; });
            this.writeParametersButton.Invoke((MethodInvoker)delegate () { this.writeParametersButton.Enabled = true; });
            this.writeOsAndCalibration.Invoke((MethodInvoker)delegate () { this.writeOsAndCalibration.Enabled = true; });
            this.writeFullContentsButton.Invoke((MethodInvoker)delegate () { this.writeFullContentsButton.Enabled = true; });
            this.exitKernelButton.Invoke((MethodInvoker)delegate () { this.exitKernelButton.Enabled = true; });
            this.reinitializeButton.Invoke((MethodInvoker)delegate () { this.reinitializeButton.Enabled = true; });
        }

        /// <summary>
        /// Select which interface device to use. This opens the Device-Picker dialog box.
        /// </summary>
        private async void selectButton_Click(object sender, EventArgs e)
        {
            if (this.vehicle != null)
            {
                this.vehicle.Dispose();
                this.vehicle = null;
            }

            DevicePicker picker = new DevicePicker(this);
            DialogResult result = picker.ShowDialog();
            if (result == DialogResult.OK)
            {
                Configuration.DeviceCategory = picker.DeviceCategory;
                Configuration.J2534DeviceType = picker.J2534DeviceType;
                Configuration.SerialPort = picker.SerialPort;
                Configuration.SerialPortDeviceType = picker.SerialPortDeviceType;
            }

            await this.ResetDevice();
        }

        /// <summary>
        /// Reset the current interface device.
        /// </summary>
        private async void reinitializeButton_Click(object sender, EventArgs e)
        {
            await this.InitializeCurrentDevice();
        }
        
        /// <summary>
        /// Close the old interface device and open a new one.
        /// </summary>
        private async Task ResetDevice()
        {
            if (this.vehicle != null)
            {
                this.vehicle.Dispose();
                this.vehicle = null;
            }

            Device device = DeviceFactory.CreateDeviceFromConfigurationSettings(this);
            if (device == null)
            {
                this.deviceDescription.Text = "None selected.";
                this.DisableUserInput();
                this.interfaceBox.Enabled = true;
                return;
            }

            this.deviceDescription.Text = device.ToString();

            Protocol protocol = new Protocol();
            this.vehicle = new Vehicle(
                device, 
                protocol, 
                this,
                new ToolPresentNotifier(device, protocol, this));
            await this.InitializeCurrentDevice();
        }

        /// <summary>
        /// Initialize the current device.
        /// </summary>
        private async Task<bool> InitializeCurrentDevice()
        {
            this.DisableUserInput();

            if (this.vehicle == null)
            {
                this.interfaceBox.Enabled = true;
                return false;
            }

            this.debugLog.Clear();
            this.userLog.Clear();
            this.AddUserMessage(GetAppNameAndVersion());

            try
            {
                // TODO: this should not return a boolean, it should just throw 
                // an exception if it is not able to initialize the device.
                bool initialized = await this.vehicle.ResetConnection();
                if (!initialized)
                {
                    this.AddUserMessage("Unable to initialize " + this.vehicle.DeviceDescription);
                    this.interfaceBox.Enabled = true;
                    this.reinitializeButton.Enabled = true;
                    return false;
                }
            }
            catch (Exception exception)
            {
                this.AddUserMessage("Unable to initialize " + this.vehicle.DeviceDescription);
                this.AddDebugMessage(exception.ToString());
                this.interfaceBox.Enabled = true;
                this.reinitializeButton.Enabled = true;
                return false;
            }

            this.EnableUserInput();
            return true;
        }

        /// <summary>
        /// Set the HTTP server. This hasn't worked for a while, might just remove it rather than fixing it...
        /// </summary>
        private void startServerButton_Click(object sender, EventArgs e)
        {
            /*
            this.DisableUserInput();
            this.startServerButton.Enabled = false;

            // It doesn't count if the user selected the prompt.
            if (selectedPort == null)
            {
                this.AddUserMessage("You must select an actual port before starting the server.");
                return;
            }

            this.AddUserMessage("There is no way to exit the HTTP server. Just close the app when you're done.");

            HttpServer.StartWebServer(selectedPort, this);
            */
        }

        /// <summary>
        /// Read the VIN, OS, etc.
        /// </summary>
        private async void readPropertiesButton_Click(object sender, EventArgs e)
        {
            if (this.vehicle == null)
            {
                // This shouldn't be possible - it would mean the buttons 
                // were enabled when they shouldn't be.
                return;
            }

            try
            {
                this.DisableUserInput();

                var vinResponse = await this.vehicle.QueryVin();
                if (vinResponse.Status != ResponseStatus.Success)
                {
                    this.AddUserMessage("VIN query failed: " + vinResponse.Status.ToString());
                    await this.vehicle.ExitKernel();
                    return;
                }
                this.AddUserMessage("VIN: " + vinResponse.Value);

                var osResponse = await this.vehicle.QueryOperatingSystemId(CancellationToken.None);
                if (osResponse.Status != ResponseStatus.Success)
                {
                    this.AddUserMessage("OS ID query failed: " + osResponse.Status.ToString());
                }
                this.AddUserMessage("OS ID: " + osResponse.Value.ToString());

                var calResponse = await this.vehicle.QueryCalibrationId();
                if (calResponse.Status != ResponseStatus.Success)
                {
                    this.AddUserMessage("Calibration ID query failed: " + calResponse.Status.ToString());
                }
                this.AddUserMessage("Calibration ID: " + calResponse.Value.ToString());

                var hardwareResponse = await this.vehicle.QueryHardwareId();
                if (hardwareResponse.Status != ResponseStatus.Success)
                {
                    this.AddUserMessage("Hardware ID query failed: " + hardwareResponse.Status.ToString());
                }

                this.AddUserMessage("Hardware ID: " + hardwareResponse.Value.ToString());

                var serialResponse = await this.vehicle.QuerySerial();
                if (serialResponse.Status != ResponseStatus.Success)
                {
                    this.AddUserMessage("Serial Number query failed: " + serialResponse.Status.ToString());
                }
                this.AddUserMessage("Serial Number: " + serialResponse.Value.ToString());

                var bccResponse = await this.vehicle.QueryBCC();
                if (bccResponse.Status != ResponseStatus.Success)
                {
                    this.AddUserMessage("BCC query failed: " + bccResponse.Status.ToString());
                }
                this.AddUserMessage("Broad Cast Code: " + bccResponse.Value.ToString());

                var mecResponse = await this.vehicle.QueryMEC();
                if (mecResponse.Status != ResponseStatus.Success)
                {
                    this.AddUserMessage("MEC query failed: " + mecResponse.Status.ToString());
                }
                this.AddUserMessage("MEC: " + mecResponse.Value.ToString());
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
                Response<uint> osidResponse = await this.vehicle.QueryOperatingSystemId(CancellationToken.None);
                if (osidResponse.Status != ResponseStatus.Success)
                {
                    this.AddUserMessage("Operating system query failed: " + osidResponse.Status);
                    return;
                }

                PcmInfo info = new PcmInfo(osidResponse.Value);

                var vinResponse = await this.vehicle.QueryVin();
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
                    bool unlocked = await this.vehicle.UnlockEcu(info.KeyAlgorithm);
                    if (!unlocked)
                    {
                        this.AddUserMessage("Unable to unlock PCM.");
                        return;
                    }

                    Response<bool> vinmodified = await this.vehicle.UpdateVin(vinForm.Vin.Trim());
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
            DelayDialogBox dialogBox = new DelayDialogBox();
            DialogResult dialogResult = dialogBox.ShowDialog();
            if (dialogResult == DialogResult.Cancel)
            {
                return;
            }

            if (!BackgroundWorker.IsAlive)
            {
                BackgroundWorker = new System.Threading.Thread(() => readFullContents_BackgroundThread());
                BackgroundWorker.IsBackground = true;
                BackgroundWorker.Start();
            }
        }

        /// <summary>
        /// Write the contents of the flash.
        /// </summary>
        private void writeCalibrationButton_Click(object sender, EventArgs e)
        {
            DelayDialogBox dialogBox = new DelayDialogBox();
            DialogResult dialogResult = dialogBox.ShowDialog();
            if (dialogResult == DialogResult.Cancel)
            {
                return;
            }

            if (!BackgroundWorker.IsAlive)
            {
                DialogResult result = MessageBox.Show(
                    "This software is still new, and it is not as reliable as commercial software." + Environment.NewLine +
                    "The PCM can be rendered unusuable, and special tools may be needed to make the PCM work again." + Environment.NewLine +
                    "If your PCM stops working, will that make your life difficult?",
                    "Answer carefully...",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);

                if (result == DialogResult.Yes)
                {
                    this.AddUserMessage("Please try again with a less important PCM.");
                }
                else
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
            DelayDialogBox dialogBox = new DelayDialogBox();
            DialogResult dialogResult = dialogBox.ShowDialog();
            if (dialogResult == DialogResult.Cancel)
            {
                return;
            }

            if (!BackgroundWorker.IsAlive)
            {
                DialogResult result = MessageBox.Show(
                    "This software is still new, and it is not as reliable as commercial software." + Environment.NewLine +
                    "The PCM can be rendered unusuable, and special tools may be needed to make the PCM work again." + Environment.NewLine +
                    "If your PCM stops working, will that make your life difficult?",
                    "Answer carefully...",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);

                if (result == DialogResult.Yes)
                {
                    this.AddUserMessage("Please try again with a less important PCM.");
                }
                else
                {
                    BackgroundWorker = new System.Threading.Thread(() => write_BackgroundThread(WriteType.Parameters));
                    BackgroundWorker.IsBackground = true;
                    BackgroundWorker.Start();
                }
            }
        }

        /// <summary>
        /// Write the operating system and calibration.
        /// </summary>
        private void writeOsAndCalibration_Click(object sender, EventArgs e)
        {
            DelayDialogBox dialogBox = new DelayDialogBox();
            DialogResult dialogResult = dialogBox.ShowDialog();
            if (dialogResult == DialogResult.Cancel)
            {
                return;
            }

            if (!BackgroundWorker.IsAlive)
            {
                DialogResult result = MessageBox.Show(
                    "Changing the operating system can render the PCM unusable." + Environment.NewLine +
                    "Special tools may be needed to make the PCM work again." + Environment.NewLine +
                    "Are you sure you really want to take that risk?",
                    "This is dangerous.",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (result == DialogResult.No)
                {
                    this.AddUserMessage("You have made a wise choice.");
                }
                else
                {
                    BackgroundWorker = new System.Threading.Thread(() => write_BackgroundThread(WriteType.OsAndCalibration));
                    BackgroundWorker.IsBackground = true;
                    BackgroundWorker.Start();
                }
            }
        }

        /// <summary>
        /// Write the entire flash.
        /// </summary>
        private void writeFullContentsButton_Click(object sender, EventArgs e)
        {
            DelayDialogBox dialogBox = new DelayDialogBox();
            DialogResult dialogResult = dialogBox.ShowDialog();
            if (dialogResult == DialogResult.Cancel)
            {
                return;
            }

            if (!BackgroundWorker.IsAlive)
            {
                DialogResult result = MessageBox.Show(
                    "Changing the operating system can render the PCM unusable." + Environment.NewLine +
                    "Special tools may be needed to make the PCM work again." + Environment.NewLine +
                    "Are you sure you really want to take that risk?",
                    "This is dangerous.",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (result == DialogResult.No)
                {
                    this.AddUserMessage("You have made a wise choice.");
                }
                else
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
            DelayDialogBox dialogBox = new DelayDialogBox();
            DialogResult dialogResult = dialogBox.ShowDialog();
            if (dialogResult == DialogResult.Cancel)
            {
                return;
            }

            if (!BackgroundWorker.IsAlive)
            {
                BackgroundWorker = new System.Threading.Thread(() => write_BackgroundThread(WriteType.Compare));
                BackgroundWorker.IsBackground = true;
                BackgroundWorker.Start();
            }
        }

        private void testWriteButton_Click(object sender, EventArgs e)
        {
            DelayDialogBox dialogBox = new DelayDialogBox();
            DialogResult dialogResult = dialogBox.ShowDialog();
            if (dialogResult == DialogResult.Cancel)
            {
                return;
            }

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
            this.AddUserMessage("Cancel button clicked.");
            this.cancellationTokenSource?.Cancel();
        }
        
        /// <summary>
        /// Read the entire contents of the flash.
        /// </summary>
        private async void readFullContents_BackgroundThread()
        {
            try
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    this.DisableUserInput();
                    this.cancelButton.Enabled = true;
                });

                if (this.vehicle == null)
                {
                    // This shouldn't be possible - it would mean the buttons 
                    // were enabled when they shouldn't be.
                    return;
                }

                // Get the path to save the image to.
                string path = "";
                this.Invoke((MethodInvoker)delegate () { path = this.ShowSaveAsDialog(); });
                if (path == null)
                {
                    this.AddUserMessage("Save canceled.");
                    return;
                }

                this.AddUserMessage("Will save to " + path);

                this.cancellationTokenSource = new CancellationTokenSource();

                this.AddUserMessage("Querying operating system of current PCM.");
                Response<uint> osidResponse = await this.vehicle.QueryOperatingSystemId(this.cancellationTokenSource.Token);
                if (osidResponse.Status != ResponseStatus.Success)
                {
                    this.AddUserMessage("Operating system query failed, will retry: " + osidResponse.Status);
                    await this.vehicle.ExitKernel();

                    osidResponse = await this.vehicle.QueryOperatingSystemId(this.cancellationTokenSource.Token);
                    if (osidResponse.Status != ResponseStatus.Success)
                    {
                        this.AddUserMessage("Operating system query failed: " + osidResponse.Status);
                    }
                }

                PcmInfo info;
                if (osidResponse.Status == ResponseStatus.Success)
                {
                    // Look up the information about this PCM, based on the OSID;
                    this.AddUserMessage("OSID: " + osidResponse.Value);
                    info = new PcmInfo(osidResponse.Value);
                }
                else
                {
                    // TODO: prompt the user - 512kb or 1mb?
                    this.AddUserMessage("Will assume this is a 512kb PCM in recovery mode.");
                    info = new PcmInfo(0);
                }

                await this.vehicle.SuppressChatter();

                bool unlocked = await this.vehicle.UnlockEcu(info.KeyAlgorithm);
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
                Response<Stream> readResponse = await this.vehicle.ReadContents(info, this.cancellationTokenSource.Token);
                this.AddUserMessage("Elapsed time " + DateTime.Now.Subtract(start));
                if (readResponse.Status != ResponseStatus.Success)
                {
                    this.AddUserMessage("Read failed, " + readResponse.Status.ToString());
                    return;
                }

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

        /// <summary>
        /// Write changes to the PCM's flash memory.
        /// </summary>
        private async void write_BackgroundThread(WriteType writeType)
        {
            try
            {
                if (this.vehicle == null)
                {
                    // This shouldn't be possible - it would mean the buttons 
                    // were enabled when they shouldn't be.
                    return;
                }

                this.cancellationTokenSource = new CancellationTokenSource();

                string path = null;
                this.Invoke((MethodInvoker)delegate ()
                {
                    this.DisableUserInput();
                    this.cancelButton.Enabled = true;

                    path = this.ShowOpenDialog();
                });

                if (path == null)
                {
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
                    this.AddUserMessage("This file is corrupt. It would render your PCM unusable.");
                    return;
                }

                UInt32 kernelVersion = 0;
                bool needUnlock;
                int keyAlgorithm = 1;
                UInt32 pcmOperatingSystemId = 0;
                bool needToCheckOperatingSystem = writeType != WriteType.Full;

                this.AddUserMessage("Requesting operating system ID...");
                Response<uint> osidResponse = await this.vehicle.QueryOperatingSystemId(this.cancellationTokenSource.Token);
                if (osidResponse.Status == ResponseStatus.Success)
                {
                    pcmOperatingSystemId = osidResponse.Value;
                    PcmInfo info = new PcmInfo(pcmOperatingSystemId);
                    keyAlgorithm = info.KeyAlgorithm;
                    needUnlock = true;

                    if (needToCheckOperatingSystem && !validator.IsSameOperatingSystem(pcmOperatingSystemId))
                    {
                        this.AddUserMessage("Flashing this file could render your PCM unusable.");
                        return;
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

                    kernelVersion = await vehicle.GetKernelVersion();
                    if (kernelVersion == 0)
                    {
                        this.AddUserMessage("Checking for recovery mode...");
                        bool recoveryMode = await this.vehicle.IsInRecoveryMode();

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
                    }
                    else
                    {
                        needUnlock = false;

                        this.AddUserMessage("Kernel version: " + kernelVersion.ToString("X8"));

                        this.AddUserMessage("Asking kernel for the PCM's operating system ID...");
                        if (needToCheckOperatingSystem && !await vehicle.IsSameOperatingSystemAccordingToKernel(validator, this.cancellationTokenSource.Token))
                        {
                            this.AddUserMessage("Flashing this file could render your PCM unusable.");
                            return;
                        }

                        needToCheckOperatingSystem = false;
                    }
                }

                await this.vehicle.SuppressChatter();

                if (needUnlock)
                {

                    bool unlocked = await this.vehicle.UnlockEcu(keyAlgorithm);
                    if (!unlocked)
                    {
                        this.AddUserMessage("Unlock was not successful.");
                        return;
                    }

                    this.AddUserMessage("Unlock succeeded.");
                }

                DateTime start = DateTime.Now;
                await this.vehicle.Write(
                    image,
                    writeType,
                    kernelVersion,
                    validator,
                    needToCheckOperatingSystem,
                    this.cancellationTokenSource.Token);
                this.AddUserMessage("Elapsed time " + DateTime.Now.Subtract(start));
            }
            catch (IOException exception)
            {
                this.AddUserMessage(exception.ToString());
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
                if (this.vehicle == null)
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
                    await this.vehicle.ExitKernel(true, false, this.cancellationTokenSource.Token, null);
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
    }
}
 

       

     

       
 
