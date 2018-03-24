using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using J2534;

namespace Flash411
{
    public partial class DevicePicker : Form
    {
        public string DeviceCategory { get; set; }
        public string J2534DeviceType { get; set; }
        public string SerialPort { get; set; }
        public string SerialPortDeviceType { get; set; }

        private const string prompt = "Select...";

        private ILogger logger;

        public DevicePicker(ILogger logger)
        {
            this.logger = logger;

            InitializeComponent();
        }

        private void DevicePicker_Load(object sender, EventArgs e)
        {
            this.FillPortList();

            this.FillSerialDeviceList();

            this.FillJ2534DeviceList();

            if(this.serialDeviceList.Items.Count > 0)
            {
                this.serialRadioButton.Checked = true;
            }
            else if (this.j2534DeviceList.Items.Count > 0)
            {
                this.j2534RadioButton.Checked = true;
            }
            else
            {
                this.serialRadioButton.Checked = true;
                this.status.Text = "You don't seem to have any serial ports or J2534 devices.";
            }

            SetDefault(this.serialPortList, Configuration.SerialPort);
            SetDefault(this.serialDeviceList, Configuration.SerialPortDeviceType);
            SetDefault(this.j2534DeviceList, Configuration.J2534DeviceType);
        }

        private static void SetDefault(ComboBox list, string value)
        {
            foreach(object item in list.Items)
            {
                if (item.ToString() == value)
                {
                    list.SelectedItem = item;
                    return;
                }
            }
        }

        private void FillPortList()
        {
            this.serialPortList.Items.Add(prompt);
            this.serialPortList.SelectedIndex = 0;

            this.serialPortList.Items.Add(MockPort.PortName);
            this.serialPortList.Items.Add(HttpPort.PortName);
            
            foreach (string name in System.IO.Ports.SerialPort.GetPortNames())
            {
                this.serialPortList.Items.Add(name);
            }
        }

        private void FillSerialDeviceList()
        {
            this.serialDeviceList.Items.Add(prompt);
            this.serialDeviceList.SelectedIndex = 0;

            this.serialDeviceList.Items.Add(AvtDevice.DeviceType);
            this.serialDeviceList.Items.Add(MockDevice.DeviceType);
            this.serialDeviceList.Items.Add(ScanToolDevice.DeviceType);
            this.serialDeviceList.Items.Add(ThanielDevice.DeviceType);
        }

        private void FillJ2534DeviceList()
        {
            foreach(J2534Device device in J2534DeviceFinder.FindInstalledJ2534DLLs(this.logger))
            {
                this.j2534DeviceList.Items.Add(device);
            }
        }

        private void autoDetectButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not yet implemented.");
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void serialRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (this.serialRadioButton.Checked)
            {
                serialOptionsGroupBox.Enabled = true;
                j2534OptionsGroupBox.Enabled = false;
                this.DeviceCategory = Configuration.Constants.DeviceCategorySerial;
            }
        }

        private void j2534DeviceButton_CheckedChanged(object sender, EventArgs e)
        {
            if (this.j2534RadioButton.Checked)
            {
                serialOptionsGroupBox.Enabled = false;
                j2534OptionsGroupBox.Enabled = true;
                this.DeviceCategory = Configuration.Constants.DeviceCategoryJ2534;
            }
        }

        private void serialPortList_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SerialPort = this.serialPortList.SelectedItem?.ToString();
        }

        private void serialDeviceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SerialPortDeviceType = this.serialDeviceList.SelectedItem?.ToString();
        }

        private void j2534DeviceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.J2534DeviceType = this.j2534DeviceList.SelectedItem?.ToString();
        }

        private async void testButton_Click(object sender, EventArgs e)
        {
            Device device;
            if (this.DeviceCategory == Configuration.Constants.DeviceCategorySerial)
            {
                device = DeviceFactory.CreateSerialDevice(this.SerialPort, this.SerialPortDeviceType, this.logger);
            }
            else if (this.DeviceCategory == Configuration.Constants.DeviceCategoryJ2534)
            {
                device = DeviceFactory.CreateJ2534Device(this.J2534DeviceType, this.logger);
            }
            else
            {
                this.status.Text = "No device specified.";
                return;
            }

            if (device == null)
            {
                this.status.Text = "Device not found.";
                return;
            }

            this.status.Text = device.ToString() + " created.";
            
            bool initialized = await device.Initialize();
            if (initialized)
            {
                this.status.Text = device.ToString() + " initialized successfully.";
            }
            else
            {
                this.status.Text = "Unable to initalize " + device.ToString();
            }

            device.Dispose();
        }
    }
}
