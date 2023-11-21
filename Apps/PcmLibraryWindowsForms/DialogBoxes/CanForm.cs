using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PcmHacking
{
    public partial class CanForm : Form
    {
        private const string NoPort = "None";
        private ILogger logger;
        private string defaultPort;

        public SerialPortInfo SelectedPort { get; private set; }

        public CanForm(ILogger logger, string defaultPort)
        {
            this.logger = logger;
            this.defaultPort = defaultPort;
            InitializeComponent();
        }

        private void CanForm_Load(object sender, EventArgs e)
        {
            this.FillPortList();

            // Set the link in the help text.
            const string thisDevice = "this device";
            int start = this.labelRecommendedInterface.Text.IndexOf(thisDevice);
            int length = thisDevice.Length;
            this.labelRecommendedInterface.LinkArea = new LinkArea(start, length);

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            if (this.serialPortList.SelectedItem as string == NoPort)
            {
                this.SelectedPort = null;
            }
            else
            {
                this.SelectedPort = this.serialPortList.SelectedItem as SerialPortInfo;
            }

            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FillPortList()
        {
            this.serialPortList.Items.Add(NoPort);
            foreach (SerialPortInfo portInfo in PortDiscovery.GetPorts(this.logger))
            {
                this.serialPortList.Items.Add(portInfo);
            }

            if (this.serialPortList.Items.Count > 0)
            {
                if (this.defaultPort != null)
                {
                    foreach(object portInfoObject in this.serialPortList.Items)
                    {
                        SerialPortInfo portInfo = portInfoObject as SerialPortInfo;
                        if (portInfo == null)
                        {
                            continue;
                        }

                        if (portInfo.PortName == this.defaultPort)
                        {
                            this.serialPortList.SelectedItem = portInfo;
                            break;
                        }
                    }
                }
                else
                {
                    this.serialPortList.SelectedIndex = 0;
                }
            }
        }

        private void labelRecommendedInterface_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.seeedstudio.com/USB-CAN-Analyzer-p-2888.html");
        }
    }
}
