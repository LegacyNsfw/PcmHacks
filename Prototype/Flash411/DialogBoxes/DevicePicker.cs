using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Flash411
{
    public partial class DevicePicker : Form
    {
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
            // todo
        }

        private void autoDetectButton_Click(object sender, EventArgs e)
        {

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
    }
}
