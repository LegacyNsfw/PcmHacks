using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PcmHacking.DialogBoxes
{
    public partial class SettingsDialogBox : Form
    {
        public SettingsDialogBox()
        {
            InitializeComponent();
        }

        private void SettingsDialogBox_Load(object sender, EventArgs e)
        {
            applyButton.Enabled = false;
        }

        private void SaveSettings()
        {
            Configuration.Settings.Save();
            applyButton.Enabled = false;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if(applyButton.Enabled)
            {
                SaveSettings();
            }
            DialogResult = DialogResult.OK;
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            SaveSettings();
            applyButton.Enabled = false;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
