using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Flash411
{
    public partial class MainForm : Form
    {
        //private Interface currentInterface;
        private Vehicle vehicle;

        public MainForm()
        {
            InitializeComponent();
        }
        
        private void MainForm_Load(object sender, EventArgs e)
        {
            this.DisableOperationButtons();
            this.FillPortList();
        }

        private void FillPortList()
        {
            this.interfacePortList.Items.Clear();
            this.interfacePortList.Items.Add("Select...");
            this.interfacePortList.SelectedIndex = 0;

            this.interfacePortList.Items.Add(new MockPort());

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
            this.interfaceTypeList.Items.Clear();

            IPort selectedPort = this.interfacePortList.SelectedItem as IPort;

            // It doesn't count if the user selected the prompt.
            if (selectedPort == null)
            {
                return;
            }

            this.interfaceTypeList.Items.Add("Select...");

            if (selectedPort is MockPort)
            {
                this.interfaceTypeList.Items.Add(new MockDevice(selectedPort));
                this.interfaceTypeList.SelectedIndex = 0;
            }
            else
            {
                // I don't really expect to support all of these. They're just 
                // placeholders until we know which ones we really will support.
                this.interfaceTypeList.Items.Add(new Avt852Device(selectedPort));
                this.interfaceTypeList.Items.Add(new ScanToolMxDevice(selectedPort));
                this.interfaceTypeList.Items.Add(new ThanielDevice(selectedPort));
            }

            this.interfaceTypeList.Enabled = true;
        }

        private async void interfaceTypeList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.vehicle != null)
            {
                this.vehicle.Dispose();
            }

            Device device = this.interfaceTypeList.SelectedItem as Device;

            if (device == null)
            {
                // This shoudn't actually happen, but just in case...
                this.DisableOperationButtons();
                return;
            }

            bool initialized = await device.Initialize();
            if (!initialized)
            {
                this.AppendResults("Unable to initalize device.");
            }

            this.vehicle = new Vehicle(device, new MessageFactory(), new MessageParser());

            this.EnableOperationButtons();
        }

        private void DisableOperationButtons()
        {
            this.readPropertiesButton.Enabled = false;
            this.readFullContentsButton.Enabled = false;
            this.modifyVinButton.Enabled = false;
            this.writeFullContentsButton.Enabled = false;
        }

        private void EnableOperationButtons()
        {
            this.readPropertiesButton.Enabled = true;
            this.readFullContentsButton.Enabled = true;
            this.modifyVinButton.Enabled = true;
            this.writeFullContentsButton.Enabled = true;
        }

        private void AppendResults(string message)
        {
            this.results.Text = this.results.Text + Environment.NewLine + message;
        }

        private async void readPropertiesButton_Click(object sender, EventArgs e)
        {
            if (this.vehicle == null)
            {
                // This shouldn't be possible - it would mean the buttons 
                // were enabled when they shouldn't be.
                return;
            }

            this.Enabled = false;

            var vinResponse = await this.vehicle.QueryVin();
            if (vinResponse.Status != ResponseStatus.Success)
            {
                this.AppendResults("VIN query failed: " + vinResponse.Status.ToString());
                return;
            }

            this.AppendResults("VIN: " + vinResponse.Value);

            var osResponse = await this.vehicle.QueryOperatingSystemId();
            if (osResponse.Status != ResponseStatus.Success)
            {
                this.AppendResults("OS ID query failed: " + osResponse.Status.ToString());
            }

            this.AppendResults("OS: " + osResponse.Value.ToString());

            this.Enabled = true;
        }

        private async void readFullContentsButton_Click(object sender, EventArgs e)
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
                this.AppendResults("Unlock was not successful.");
                return;
            }

            this.AppendResults("Unlock succeeded.");

            Stream contents = await this.vehicle.ReadContents();

            string path = await this.ShowSaveAsDialog();
            if (path == null)
            {
                this.AppendResults("Save canceled.");
                return;
            }

            try
            {
                using (Stream output = File.OpenWrite(path))
                {
                    await contents.CopyToAsync(output);
                }
            }
            catch(IOException exception)
            {
                this.AppendResults(exception.Message);
            }
        }

        private Task<string> ShowSaveAsDialog()
        {
            return Task.FromResult((string)null);
        }
    }
}
