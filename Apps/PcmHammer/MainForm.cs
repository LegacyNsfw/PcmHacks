  
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
    public partial class MainForm : MainFormBase, ILogger
    {
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

            // Wide enough for the CRC comparison table
            this.Width = 1000;

            // Golden ratio
            this.Height = 618;
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
        protected override void ValidDeviceSelected(string deviceName)
        {
            this.deviceDescription.Text = deviceName;
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
        public override string GetAppNameAndVersion()
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

                // Load the dynamic content asynchronously.
                ThreadPool.QueueUserWorkItem(new WaitCallback(LoadStartMessage));
                ThreadPool.QueueUserWorkItem(new WaitCallback(LoadHelp));
                ThreadPool.QueueUserWorkItem(new WaitCallback(LoadCredits));

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
        /// Get the URL to a file on github, using the release branch or the develop branch.
        /// </summary>
        private string GetFileUrl(string path)
        {
            string urlBase = "https://raw.githubusercontent.com/LegacyNsfw/PcmHacks/";
            string branch = AppVersion == null ? "develop" : "Release/" + AppVersion;
            string result = urlBase + branch + path;
            return result;
        }

        /// <summary>
        /// The Help content is loaded after the window appears, so that it doesn't slow down app initialization.
        /// </summary>
        private async void LoadStartMessage(object unused)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Get,
                    GetFileUrl("/Apps/PcmHammer/start.txt"));

                request.Headers.Add("Cache-Control", "no-cache");
                HttpClient client = new HttpClient();
                var response = await client.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string message = await response.Content.ReadAsStringAsync();
                    this.AddUserMessage(message);                    
                }
            }
            catch (Exception exception)
            {
                this.AddDebugMessage("Unable to fetch the startup message: " + exception.Message);
            }
        }

        /// <summary>
        /// The Help content is loaded after the window appears, so that it doesn't slow down app initialization.
        /// </summary>
        private async void LoadHelp(object unused)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Get,
                    GetFileUrl("/Apps/PcmHammer/help.html"));

                request.Headers.Add("Cache-Control", "no-cache");
                HttpClient client = new HttpClient();
                var response = await client.SendAsync(request);

                Stream stream;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    stream = await response.Content.ReadAsStreamAsync();
                }
                else
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var resourceName = "PcmHacking.help.html";
                    stream = assembly.GetManifestResourceStream(resourceName);
                }

                this.helpWebBrowser.Invoke((MethodInvoker)delegate ()
                {
                    this.helpWebBrowser.DocumentStream = stream;
                });
            }
            catch (Exception exception)
            {
                this.AddDebugMessage("Unable to fetch updated help content.");
                this.AddDebugMessage("This exception can safely be ignored.");
                this.AddDebugMessage(exception.ToString());
            }
        }

        /// <summary>
        /// The Help content is loaded after the window appears, so that it doesn't slow down app initialization.
        /// </summary>
        private void LoadCredits(object unused)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "PcmHacking.credits.html";
                var stream = assembly.GetManifestResourceStream(resourceName);

                this.helpWebBrowser.Invoke((MethodInvoker)delegate ()
                {
                    this.creditsWebBrowser.DocumentStream = stream;
                });
            }
            catch (Exception exception)
            {
                this.AddDebugMessage("Unable load content for Credits tab.");
                this.AddDebugMessage("This exception can safely be ignored.");
                this.AddDebugMessage(exception.ToString());
            }
        }

        /// <summary>
        /// Discourage users from closing the app during a write.
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.currentWriteType == WriteType.None)
            {
                return;
            }

            if (this.currentWriteType == WriteType.TestWrite)
            {
                return;
            }

            var choice = MessageBox.Show(
                this,
                "Closing PCM Hammer now could make your PCM unusable." + Environment.NewLine +
                "Are you sure you want to take that risk?",
                "PCM Hammer",
                MessageBoxButtons.YesNo);

            if (choice == DialogResult.No)
            {
                e.Cancel = true;
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
            this.readPropertiesButton.Enabled = false;
            this.readFullContentsButton.Enabled = false;
            this.modifyVinButton.Enabled = false;
            this.quickComparisonButton.Enabled = false;
            this.testWriteButton.Enabled = false;
            this.writeCalibrationButton.Enabled = false;
            this.writeParametersButton.Enabled = false;
            this.writeFullContentsButton.Enabled = false;
            this.exitKernelButton.Enabled = false;
            this.reinitializeButton.Enabled = false;
        }

        /// <summary>
        /// Enable the buttons when a long-running operation completes.
        /// </summary>
        protected override void EnableUserInput()
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
            this.writeFullContentsButton.Invoke((MethodInvoker)delegate () { this.writeFullContentsButton.Enabled = true; });
            this.exitKernelButton.Invoke((MethodInvoker)delegate () { this.exitKernelButton.Enabled = true; });
            this.reinitializeButton.Invoke((MethodInvoker)delegate () { this.reinitializeButton.Enabled = true; });
        }

        protected override void EnableInterfaceSelection()
        {
            this.interfaceBox.Enabled = true;
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

                var hardwareResponse = await this.Vehicle.QueryHardwareId();
                if (hardwareResponse.Status == ResponseStatus.Success)
                {
                    this.AddUserMessage("Hardware ID: " + hardwareResponse.Value.ToString());
                }
                else
                {
                    this.AddUserMessage("Hardware ID query failed: " + hardwareResponse.Status.ToString());
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

                var bccResponse = await this.Vehicle.QueryBCC();
                if (bccResponse.Status == ResponseStatus.Success)
                {
                    this.AddUserMessage("Broad Cast Code: " + bccResponse.Value.ToString());
                }
                else
                {
                    this.AddUserMessage("BCC query failed: " + bccResponse.Status.ToString());
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
                    this.Invoke((MethodInvoker)delegate () { path = this.ShowSaveAsDialog(); });
                    if (path == null)
                    {
                        this.AddUserMessage("Save canceled.");
                        return;
                    }

                    this.AddUserMessage("Will save to " + path);

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

                    await this.Vehicle.SuppressChatter();

                    bool unlocked = await this.Vehicle.UnlockEcu(info.KeyAlgorithm);
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
                        this);

                    Response<Stream> readResponse = await reader.ReadContents(
                        info,
                        cancellationTokenSource.Token);

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
        }

        /// <summary>
        /// Write changes to the PCM's flash memory.
        /// </summary>
        private async void write_BackgroundThread(WriteType writeType)
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
                    Response<uint> osidResponse = await this.Vehicle.QueryOperatingSystemId(this.cancellationTokenSource.Token);
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
                        }
                        else
                        {
                            needUnlock = false;

                            this.AddUserMessage("Kernel version: " + kernelVersion.ToString("X8"));

                            this.AddUserMessage("Asking kernel for the PCM's operating system ID...");
                            if (needToCheckOperatingSystem && !await this.Vehicle.IsSameOperatingSystemAccordingToKernel(validator, this.cancellationTokenSource.Token))
                            {
                                this.AddUserMessage("Flashing this file could render your PCM unusable.");
                                return;
                            }

                            needToCheckOperatingSystem = false;
                        }
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
                        new Protocol(),
                        this);

                    await writer.Write(
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
    }
}
 

       

     

       
 
