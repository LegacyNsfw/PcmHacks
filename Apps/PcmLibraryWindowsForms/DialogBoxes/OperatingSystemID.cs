using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

namespace PcmHacking
{
    public partial class OperatingSystemIDDialogBox : Form
    {
        public uint OperatingSystemId { get; set; }

        public OperatingSystemIDDialogBox()
        {
            InitializeComponent();
        }

        private void operatingSystemIdDialogBox_Load(object sender, EventArgs e)
        {
            this.okButton.Enabled = false;
        }

        private void operatingSystemIdTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;

            if (c != '\b' && !(c >= 0x30 && c <= 0x39))
            {
                e.Handled = true;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(operatingSystemIdTextBox.Text))
            {
                if (uint.TryParse(operatingSystemIdTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out uint result))
                {
                    OperatingSystemId = result;
                    DialogResult = DialogResult.OK;
                }
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void operatingSystemIdTextBox_TextChanged(object sender, EventArgs e)
        {
            if (this.operatingSystemIdTextBox.TextLength > 0)
            {
                this.okButton.Enabled = true;
            }
            else
            {
                this.okButton.Enabled = false;
            }
        }
    }
}
