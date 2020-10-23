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
            if (string.IsNullOrWhiteSpace(Configuration.Settings.LogDirectory))
            {
                Configuration.Settings.LogDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                Configuration.Settings.Save();
            }
            logDirectoryTextBox.Text = Configuration.Settings.LogDirectory;

            saveUserLogOnExitCheckBox.Checked = Configuration.Settings.SaveUserLogOnExit;
            saveDebugLogOnExitCheckBox.Checked = Configuration.Settings.SaveDebugLogOnExit;
            applyButton.Enabled = false;
        }

        private void SaveSettings()
        {
            if (Configuration.Settings.LogDirectory != logDirectoryTextBox.Text)
            {
                Configuration.Settings.LogDirectory = logDirectoryTextBox.Text;
            }

            if (Configuration.Settings.SaveUserLogOnExit != saveUserLogOnExitCheckBox.Checked)
            {
                Configuration.Settings.SaveUserLogOnExit = saveUserLogOnExitCheckBox.Checked;
            }

            if (Configuration.Settings.SaveDebugLogOnExit != saveDebugLogOnExitCheckBox.Checked)
            {
                Configuration.Settings.SaveDebugLogOnExit = saveDebugLogOnExitCheckBox.Checked;
            }

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

        private void logDirectoryTextBox_TextChanged(object sender, EventArgs e)
        {
            applyButton.Enabled = true;
        }

        private void logDirectoryButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = Configuration.Settings.LogDirectory;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    logDirectoryTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void saveUserLogOnExitCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            applyButton.Enabled = true;
        }

        private void saveDebugLogOnExitCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            applyButton.Enabled = true;
        }
    }
}
