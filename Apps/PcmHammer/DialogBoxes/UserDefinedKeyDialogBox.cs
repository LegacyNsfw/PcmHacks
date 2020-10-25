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

namespace PcmHacking.DialogBoxes
{
    public partial class UserDefinedKeyDialogBox : Form
    {
        public UInt16 UserDefinedKey { get; set; }

        public UserDefinedKeyDialogBox()
        {
            InitializeComponent();
        }

        private void UserDefinedKeyDialogBox_Load(object sender, EventArgs e)
        {
            this.okButton.Enabled = false;
        }

        private void userDefinedKeyTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;

            if (c != '\b' && !((c <= 0x66 && c >= 0x61) || (c <= 0x46 && c >= 0x41) || (c >= 0x30 && c <= 0x39)))
            {
                e.Handled = true;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (UInt16.TryParse(userDefinedKeyTextBox.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out UInt16 result))
            {
                UserDefinedKey = result;
                DialogResult = DialogResult.OK;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void userDefinedKeyTextBox_TextChanged(object sender, EventArgs e)
        {
            if (this.userDefinedKeyTextBox.TextLength > 0)
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
