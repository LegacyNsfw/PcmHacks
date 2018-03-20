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
        //private Interface currentInterface;
        private Vehicle vehicle;

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

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.interfaceBox.Enabled = true;
            this.operationsBox.Enabled = true;
            this.startServerButton.Enabled = false;
            
            this.FillPortList();
        }

        private void FillPortList()
        {
            this.interfacePortList.Items.Clear();
            this.interfacePortList.Items.Add("Select...");
            this.interfacePortList.SelectedIndex = 0;

            this.interfacePortList.Items.Add(new MockPort(this));
            this.interfacePortList.Items.Add(new MockAvt852(this));
            this.interfacePortList.Items.Add(new HttpPort(this));

            if (J2534Port.IsPresent())
            {
                this.interfacePortList.Items.Add(new J2534Port());
            }

            foreach (string name in System.IO.Ports.SerialPort.GetPortNames())
            {
                this.interfacePortList.Items.Add(new StandardPort(name));
            }
        }

        private void interfacePortList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                this.interfaceTypeList.Items.Clear();

                IPort selectedPort = this.interfacePortList.SelectedItem as IPort;

                // It doesn't count if the user selected the prompt.
                if (selectedPort == null)
                {
                    return;
                }

                this.startServerButton.Enabled = true;

                this.interfaceTypeList.Items.Add("Select...");

                if (selectedPort is MockPort)
                {
                    this.interfaceTypeList.Items.Add(new MockDevice(selectedPort, this));
                    this.interfaceTypeList.SelectedIndex = 0;
                }
                else
                {
                    // I don't really expect to support all of these. They're just 
                    // placeholders until we know which ones we really will support.
                    this.interfaceTypeList.Items.Add(new AvtDevice(selectedPort, this));
                    this.interfaceTypeList.Items.Add(new ScanToolDevice(selectedPort, this));
                    this.interfaceTypeList.Items.Add(new ThanielDevice(selectedPort, this));
                }

                this.interfaceTypeList.Enabled = true;
            }
            catch (Exception exception)
            {
                this.AddDebugMessage("Error in interfacePortList_SelectedIndexChanged: " + exception.ToString());
            }
            finally
            {
                // Enabling and disabling controls causes the focus to be stolen.
                this.interfacePortList.Focus();
            }
        }

        private void interfaceTypeList_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.startServerButton.Enabled = false;

            try
            {
                this.reinitializeButton_Click(sender, e);
            }
            finally
            {
                // Enabling and disabling controls causes the focus to be stolen.
                this.interfaceTypeList.Focus();
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

                var serialResponse = await this.vehicle.QuerySerial();
                if (serialResponse.Status != ResponseStatus.Success)
                {
                    this.AddUserMessage("Serial Number query failed: " + serialResponse.Status.ToString());
                }

                this.AddUserMessage("Serial Number: " + serialResponse.Value.ToString());

                var bccResponse = await this.vehicle.QueryBCC();
                if (serialResponse.Status != ResponseStatus.Success)
                {
                    this.AddUserMessage("BCC query failed: " + bccResponse.Status.ToString());
                }

                this.AddUserMessage("Broad Case Code: " + bccResponse.Value.ToString());

                var mecResponse = await this.vehicle.QueryMEC();
                if (serialResponse.Status != ResponseStatus.Success)
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
            catch(IOException exception)
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

        private async void reinitializeButton_Click(object sender, EventArgs e)
        {
            this.DisableUserInput();

            if (this.vehicle != null)
            {
                this.vehicle.Dispose();
                this.vehicle = null;
            }
            
            Device device = this.interfaceTypeList.SelectedItem as Device;

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
            this.DisableUserInput();
            this.startServerButton.Enabled = false;

            IPort selectedPort = this.interfacePortList.SelectedItem as IPort;

            // It doesn't count if the user selected the prompt.
            if (selectedPort == null)
            {
                this.AddUserMessage("You must select an actual port before starting the server.");
                return;
            }

            this.AddUserMessage("There is no way to exit the HTTP server. Just close the app when you're done.");

            HttpServer.StartWebServer(selectedPort, this);
        }
    }
}
