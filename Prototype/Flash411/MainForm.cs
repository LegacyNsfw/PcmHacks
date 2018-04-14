  
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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Flash411
{
    public partial class MainForm : Form, ILogger
    {
        private Vehicle vehicle;

        public MainForm()
        {
            InitializeComponent();
        }

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

        public void AddDebugMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("hh:mm:ss:fff");

            this.debugLog.Invoke(
                (MethodInvoker)delegate ()
                {
                    this.debugLog.AppendText("[" + timestamp + "]  " + message + Environment.NewLine);
                });
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                this.interfaceBox.Enabled = true;
                this.operationsBox.Enabled = true;
                this.startServerButton.Enabled = false;

                await this.ResetDevice();
            }
            catch (Exception exception)
            {
                this.AddUserMessage(exception.Message);
                this.AddDebugMessage(exception.ToString());
            }
        }

        private void DisableUserInput()
        {
            this.interfaceBox.Enabled = false;

            // The operation buttons have to be enabled/disabled individually
            // (rather than via the parent GroupBox) because we sometimes want
            // to enable the re-initialize operation while the others are disabled.
            this.readPropertiesButton.Enabled = false;
            this.readFullContentsButton.Enabled = false;
            this.modifyVinButton.Enabled = false;
            this.writeFullContentsButton.Enabled = false;
            this.reinitializeButton.Enabled = false;
        }

        private void EnableUserInput()
        {
            this.interfaceBox.Enabled = true;

            // The operation buttons have to be enabled/disabled individually
            // (rather than via the parent GroupBox) because we sometimes want
            // to enable the re-initialize operation while the others are disabled.
            this.readPropertiesButton.Enabled = true;
            this.readFullContentsButton.Enabled = true;
            this.modifyVinButton.Enabled = true;
            this.writeFullContentsButton.Enabled = true;
            this.reinitializeButton.Enabled = true;
        }
        
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
                    return;
                }
                this.AddUserMessage("VIN: " + vinResponse.Value);

                var osResponse = await this.vehicle.QueryOperatingSystemId();
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
            catch(Exception exception)
            {
                this.AddUserMessage(exception.Message);
                this.AddDebugMessage(exception.ToString());
            }
            finally
            {
                this.EnableUserInput();
            }
        }

        private async void reinitializeButton_Click(object sender, EventArgs e)
        {
            await this.InitializeCurrentDevice();
        }
        
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


        private async void readFullContentsButton_Click(object sender, EventArgs e)
        {
            this.DisableUserInput();

            try
            {
                if (this.vehicle == null)
                {
                    // This shouldn't be possible - it would mean the buttons 
                    // were enabled when they shouldn't be.
                    return;
                }

                Response<bool> unlockResponse = await this.vehicle.UnlockEcu();
                if (unlockResponse.Status != ResponseStatus.Success)
                {
                    this.AddUserMessage("Unlock was not successful.");
                    return;
                }

                this.AddUserMessage("Unlock succeeded.");

                await this.vehicle.ReadContents();

                /*Response<Stream> readResponse = await this.vehicle.ReadContents();
                if (readResponse.Status != ResponseStatus.Success)
                {
                    this.AddUserMessage("Read failed, " + readResponse.Status.ToString());
                    return;
                }

                string path = this.ShowSaveAsDialog();
                if (path == null)
                {
                    this.AddUserMessage("Save canceled.");
                    return;
                }

                this.AddUserMessage("Saving to " + path);

                try
                {
                    using (Stream output = File.OpenWrite(path))
                    {
                        await readResponse.Value.CopyToAsync(output);
                    }
                }
                catch (IOException exception)
                {
                    this.AddUserMessage(exception.Message);
                }*/
            }
            catch(Exception exception)
            {
                this.AddUserMessage("Read failed: " + exception.ToString());
            }
            finally
            {
                this.EnableUserInput();
            }
        }

        private string ShowSaveAsDialog()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = ".bin";
            dialog.Filter = "Binary Files(*.bin) | *.bin | All Files(*.*) | *.*";
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

        private async void writeFullContentsButton_Click(object sender, EventArgs e)
        {
            if (this.vehicle == null)
            {
                // This shouldn't be possible - it would mean the buttons 
                // were enabled when they shouldn't be.
                return;
            }

            Response<bool> unlockResponse = await this.vehicle.UnlockEcu();
            if (unlockResponse.Status != ResponseStatus.Success)
            {
                this.AddUserMessage("Unlock was not successful.");
                return;
            }

            this.AddUserMessage("Unlock succeeded.");

            string path = this.ShowOpenDialog();
            if (path == null)
            {
                return;
            }

            this.AddUserMessage("Pretending to update PCM with content from " + path);

            try
            {
                using (Stream stream = File.OpenRead(path))
                {
                    await this.vehicle.WriteContents(stream);
                }
            }
            catch (IOException exception)
            {
                this.AddUserMessage(exception.ToString());
            }
        }

        private string ShowOpenDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".bin";
            dialog.Filter = "Binary Files(*.bin) | *.bin | All Files(*.*) | *.*";
            dialog.FilterIndex = 0;
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                return dialog.FileName;
            }

            return null;
        }

        private async void modifyVinButton_Click(object sender, EventArgs e)
        {
            try
            {
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
                    Response<bool> unlocked = await this.vehicle.UnlockEcu();
                    if (unlocked.Value)
                    {
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
                    else
                    {

                    }

                }
            }
            catch (Exception exception)
            {
                this.AddUserMessage("VIN change failed: " + exception.ToString());
            }
        }

        private async void selectButton_Click(object sender, EventArgs e)
        {
            if (this.vehicle != null)
            {
                this.vehicle.Dispose();
                this.vehicle = null;
            }

            DevicePicker picker = new DevicePicker(this);
            DialogResult result = picker.ShowDialog();
            if(result == DialogResult.OK)
            {
                Configuration.DeviceCategory = picker.DeviceCategory;
                Configuration.J2534DeviceType = picker.J2534DeviceType;
                Configuration.SerialPort = picker.SerialPort;
                Configuration.SerialPortDeviceType = picker.SerialPortDeviceType;
            }

            await this.ResetDevice();
        }    
        
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
                return;
            }

            this.deviceDescription.Text = device.ToString();

            this.vehicle = new Vehicle(device, new MessageFactory(), new MessageParser(), this);
            await this.InitializeCurrentDevice();
        }

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
    }
}
 

       

     

       
 