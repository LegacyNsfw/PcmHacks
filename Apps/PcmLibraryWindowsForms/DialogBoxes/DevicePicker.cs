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

namespace PcmHacking
{
    /// <summary>
    /// This dialog box allows the user to choose the type of device and (for serial devices) the COM port.
    /// </summary>
    public partial class DevicePicker : Form
    {
        /// <summary>
        /// Indicate which category of device the user has chosen.
        /// </summary>
        public string DeviceCategory { get; set; }

        /// <summary>
        /// Indicates the name of the J2534 device that the user has chosen.
        /// </summary>
        public string J2534DeviceType { get; set; }

        /// <summary>
        /// Indicates the serial port (COM port) that the user has chosen. Only relevant for serial devices.
        /// </summary>
        public string SerialPort { get; set; }

        /// <summary>
        /// Indicates which type of serial device the user has chosen.
        /// </summary>
        public string SerialPortDeviceType { get; set; }

        /// <summary>
        /// Enable Disable VPW 4x.
        /// </summary>
        public bool Enable4xReadWrite { get; set; }

        /// <summary>
        /// Prompt to put into drop-down lists to let the user know that they need to make a selection.
        /// </summary>
        private const string prompt = "Select...";

        /// <summary>
        /// This allows the dialog box to add messages to the Results pane and Debug pane.
        /// </summary>
        private ILogger logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        public DevicePicker(ILogger logger)
        {
            this.logger = logger;

            InitializeComponent();
        }

        /// <summary>
        /// Populate controls with the current selections before displaying the form.
        /// </summary>
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

            switch(DeviceConfiguration.Settings.DeviceCategory)
            {
                case DeviceConfiguration.Constants.DeviceCategorySerial:
                    {
                        if (this.serialDeviceList.Items.Count > 0)
                        {
                            this.serialRadioButton.Checked = true;
                        }
                    }
                    break;

                case DeviceConfiguration.Constants.DeviceCategoryJ2534:
                    {
                        if (this.j2534DeviceList.Items.Count > 0)
                        {
                            this.j2534RadioButton.Checked = true;
                        }
                    }
                    break;
            }

            SetDefault(
                this.serialPortList, 
                x => (x as SerialPortInfo)?.PortName,
                DeviceConfiguration.Settings.SerialPort);

            SetDefault(
                this.serialDeviceList,
                x => x.ToString(),
                DeviceConfiguration.Settings.SerialPortDeviceType);

            SetDefault(
                this.j2534DeviceList, 
                x => x.ToString(),
                DeviceConfiguration.Settings.J2534DeviceType);

            this.Enable4xReadWrite = this.enable4xReadWriteCheckBox.Checked = DeviceConfiguration.Settings.Enable4xReadWrite;
        }

        /// <summary>
        /// Set a ComboBox to the given item.
        /// </summary>
        private static void SetDefault(ComboBox list, Func<object, string> getConfigurationValue, string value)
        {
            foreach(object item in list.Items)
            {
                if (getConfigurationValue(item) == value)
                {
                    list.SelectedItem = item;
                    return;
                }
            }
        }

        /// <summary>
        /// Fill the list of serial ports with a list of currently available ports.
        /// </summary>
        private void FillPortList()
        {
            this.serialPortList.Items.Add(prompt);
            this.serialPortList.SelectedIndex = 0;

            foreach (object portInfo in PortDiscovery.GetPorts(this.logger))
            {
                this.serialPortList.Items.Add(portInfo);
            }

            // This is useful for testing without an actual PCM. 
            // You'll need to uncomment a line in FillSerialDeviceList as well as this one.
            // this.serialPortList.Items.Add(MockPort.PortName);
        }

        /// <summary>
        /// Fill the list of serial devices with the device types that the app supports.
        /// </summary>
        private void FillSerialDeviceList()
        {
            this.serialDeviceList.Items.Add(prompt);
            this.serialDeviceList.SelectedIndex = 0;
            this.serialDeviceList.Items.Add(ElmDevice.DeviceType);
            this.serialDeviceList.Items.Add(AvtDevice.DeviceType);
            this.serialDeviceList.Items.Add(OBDXProDevice.DeviceType);

            // This is useful for testing without an actual PCM.
            // You'll need to uncomment a line in FillPortList as well as this one.
            // this.serialDeviceList.Items.Add(MockDevice.DeviceType);
        }

        /// <summary>
        /// Fill the list of J2534 devices with the devices that the J2534 library has discovered.
        /// </summary>
        private void FillJ2534DeviceList()
        {
            this.j2534DeviceList.Items.Add(prompt);
            this.j2534DeviceList.SelectedIndex = 0;

            foreach(J2534.J2534Device device in J2534DeviceFinder.FindInstalledJ2534DLLs(this.logger))
            {
                this.j2534DeviceList.Items.Add(device);
            }
        }

        /// <summary>
        /// In theory we could probably guess correctly 99% of the time...
        /// </summary>
        private void autoDetectButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not yet implemented.");
        }

        /// <summary>
        /// Close the form, let the caller know that the user clicked OK.
        /// </summary>
        private void okButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Close the form, let the caller know that the user clicked Cancel.
        /// </summary>
        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Enable/disable different groups of controls depending of which device type the user has chosen.
        /// </summary>
        private void serialRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (this.serialRadioButton.Checked)
            {
                j2534OptionsGroupBox.Enabled = false;
                J2534DeviceType = string.Empty;
                j2534DeviceList.SelectedIndex = 0;
                serialOptionsGroupBox.Enabled = true;
                this.DeviceCategory = DeviceConfiguration.Constants.DeviceCategorySerial;
            }
        }

        /// <summary>
        /// Enable/disable different groups of controls depending of which device type the user has chosen.
        /// </summary>
        private void j2534RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (this.j2534RadioButton.Checked)
            {
                serialOptionsGroupBox.Enabled = false;
                SerialPortDeviceType = string.Empty;
                serialDeviceList.SelectedIndex = 0;
                serialPortList.SelectedIndex = 0;
                j2534OptionsGroupBox.Enabled = true;
                this.DeviceCategory = DeviceConfiguration.Constants.DeviceCategoryJ2534;
            }
        }

        /// <summary>
        /// Store the name of the newly selected serial port.
        /// </summary>
        private void serialPortList_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SerialPort = (this.serialPortList.SelectedItem as SerialPortInfo)?.PortName;
        }

        /// <summary>
        /// Store the name of the newly selected serial device type.
        /// </summary>
        private void serialDeviceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            string item = this.serialDeviceList.SelectedItem?.ToString();
            if (item == prompt)
            {
                item = null;
            }

            this.SerialPortDeviceType = item;
        }

        /// <summary>
        /// Store the name of the newly selected J2534 device.
        /// </summary>
        private void j2534DeviceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            string item = this.j2534DeviceList.SelectedItem?.ToString();
            if (item == prompt)
            {
                item = null;
            }

            this.J2534DeviceType = item;
        }

        /// <summary>
        /// Enable/disable VPW 4x
        /// </summary>
        private void enable4xReadWriteCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.Enable4xReadWrite = this.enable4xReadWriteCheckBox.Checked;
        }

        /// <summary>
        /// Test the user's selections.
        /// </summary>
        private async void testButton_Click(object sender, EventArgs e)
        {
            Device device;
            if (this.DeviceCategory == DeviceConfiguration.Constants.DeviceCategorySerial)
            {
                device = DeviceFactory.CreateSerialDevice(this.SerialPort, this.SerialPortDeviceType, this.logger);
            }
            else if (this.DeviceCategory == DeviceConfiguration.Constants.DeviceCategoryJ2534)
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
