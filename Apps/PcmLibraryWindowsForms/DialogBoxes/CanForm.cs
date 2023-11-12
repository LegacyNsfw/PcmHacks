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
        private ILogger logger;
        private string defaultPort;

        public SerialPortInfo SelectedPort { get; private set; }

        public CanForm(ILogger logger, string defaultPort)
        {
            this.logger = logger;
            this.defaultPort = defaultPort;
            InitializeComponent();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.SelectedPort = this.serialPortList.SelectedItem as SerialPortInfo;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }


        private void CanForm_Load(object sender, EventArgs e)
        {
            this.FillPortList();
        }

        private void FillPortList()
        {            
            foreach (SerialPortInfo portInfo in PortDiscovery.GetPorts(this.logger))
            {
                this.serialPortList.Items.Add(portInfo);
            }

            if (this.serialPortList.Items.Count > 0)
            {
                if (this.defaultPort != null)
                {
                    foreach(SerialPortInfo portInfo in this.serialPortList.Items)
                    {
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
    }
}
