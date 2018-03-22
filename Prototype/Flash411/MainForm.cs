  
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
        private Device device;
        private Vehicle vehicle;
        List<J2534Device> InstalledDLLs;

        public MainForm()
        {
            InitializeComponent();
        }
        
        public void AddUserMessage(string message)
        {
            this.userLog.Invoke(
                (MethodInvoker)delegate()
                {
                    this.userLog.AppendText("[" + DateTime.Now.ToString("hh:mm:ss:ms") +  "]  " + message + Environment.NewLine);

                    // User messages are added to the debug log as well, so that the debug log has everything.
                    this.debugLog.AppendText("[" + DateTime.Now.ToString("hh:mm:ss:ms") + "]  " + message + Environment.NewLine);

                });
        }

        public void AddDebugMessage(string message)
        {
            this.debugLog.Invoke(
                (MethodInvoker)delegate ()
                {
                    this.debugLog.AppendText(message + Environment.NewLine);
                });
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            this.interfaceBox.Enabled = true;
            this.operationsBox.Enabled = true;
            this.startServerButton.Enabled = false;

            //this.FillPortList();

            this.device = await DeviceFactory.CreateDeviceFromConfigurationSettings(this);
            if (this.device != null)
            {
                this.deviceDescription.Text = this.device.ToString();
            }
            else
            {
                this.deviceDescription.Text = "None selected.";
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

                this.AddUserMessage("OS: " + osResponse.Value.ToString());
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
            this.DisableUserInput();

            if (this.vehicle != null)
            {
                this.vehicle.Dispose();
                this.vehicle = null;
            }

            if (device == null)
            {
                // The user selected the mock device. Let them continue;
                this.interfaceBox.Enabled = true;
                return;
            }

            this.debugLog.Clear();
            this.userLog.Clear();

            try
            {
                // TODO: this should not return a boolean, it should just throw 
                // an exception if it is not able to initialize the device.
                bool initialized = await device.Initialize();
                if (!initialized)
                {
                    this.AddUserMessage("Unable to initialize " + device.ToString());
                    this.interfaceBox.Enabled = true;
                    this.reinitializeButton.Enabled = true;
                    return;
                }
            }
            catch (Exception exception)
            {
                this.AddUserMessage("Unable to initialize " + device.ToString());
                this.AddDebugMessage(exception.ToString());
                this.interfaceBox.Enabled = true;
                this.reinitializeButton.Enabled = true;
                return;
            }

            this.vehicle = new Vehicle(device, new MessageFactory(), new MessageParser(), this);

            this.EnableUserInput();
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

                Response<Stream> readResponse = await this.vehicle.ReadContents();
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
                }
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

        private void selectButton_Click(object sender, EventArgs e)
        {

        }


        /// <summary>
        /// Find all installed J2534 DLLs
        /// </summary>
        private const string PASSTHRU_REGISTRY_PATH = "Software\\PassThruSupport.04.04";
        private const string PASSTHRU_REGISTRY_PATH_6432 = "Software\\Wow6432Node\\PassThruSupport.04.04";
        public bool FindInstalledJ2534DLLs()
        {
            try
            {

               InstalledDLLs = new List<J2534Device>();
                RegistryKey myKey = Registry.LocalMachine.OpenSubKey(PASSTHRU_REGISTRY_PATH, false);
                if ((myKey == null))
                {
                    myKey = Registry.LocalMachine.OpenSubKey(PASSTHRU_REGISTRY_PATH_6432, false);
                    if ((myKey == null))
                    {
                        return false;
                    }

                }

                string[] devices = myKey.GetSubKeyNames();
                foreach (string device in devices)
                {
                    J2534Device tempDevice = new J2534Device();
                    RegistryKey deviceKey = myKey.OpenSubKey(device);
                    if ((deviceKey == null))
                    {
                        continue; //Skip device... its empty
                    }

                    tempDevice.Vendor = (string)deviceKey.GetValue("Vendor", "");
                    tempDevice.Name = (string)deviceKey.GetValue("Name", "");
                    tempDevice.ConfigApplication = (string)deviceKey.GetValue("ConfigApplication", "");
                    tempDevice.FunctionLibrary = (string)deviceKey.GetValue("FunctionLibrary", "");
                    tempDevice.CAN = (int)(deviceKey.GetValue("CAN", 0));
                    tempDevice.ISO14230 = (int)(deviceKey.GetValue("ISO14230", 0));
                    tempDevice.ISO15765 = (int)(deviceKey.GetValue("ISO15765", 0));
                    tempDevice.ISO9141 = (int)(deviceKey.GetValue("ISO9141", 0));
                    tempDevice.J1850PWM = (int)(deviceKey.GetValue("J1850PWM", 0));
                    tempDevice.J1850VPW = (int)(deviceKey.GetValue("J1850VPW", 0));
                    tempDevice.SCI_A_ENGINE = (int)(deviceKey.GetValue("SCI_A_ENGINE", 0));
                    tempDevice.SCI_A_TRANS = (int)(deviceKey.GetValue("SCI_A_TRANS", 0));
                    tempDevice.SCI_B_ENGINE = (int)(deviceKey.GetValue("SCI_B_ENGINE", 0));
                    tempDevice.SCI_B_TRANS = (int)(deviceKey.GetValue("SCI_B_TRANS", 0));
                    InstalledDLLs.Add(tempDevice);
                }
                return true;
            }
            catch (Exception exception)
            {
                this.AddDebugMessage("Error occured while finding installed J2534 devices");
                this.AddDebugMessage(exception.ToString());
                //do something with errors here for now return false
                return false;
            }

                case DialogResult.Cancel:
                    break;
            }
        }

        
    }
}
 

       

     

       
 