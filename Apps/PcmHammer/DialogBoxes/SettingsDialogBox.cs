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

            if (!string.IsNullOrWhiteSpace(Configuration.Settings.BinDirectory))
            {
                binDirectoryTextBox.Text = Configuration.Settings.BinDirectory;
            }

            saveUserLogOnExitCheckBox.Checked = Configuration.Settings.SaveUserLogOnExit;
            saveDebugLogOnExitCheckBox.Checked = Configuration.Settings.SaveDebugLogOnExit;
            mainWindowPersistenceCheckBox.Checked = Configuration.Settings.MainWindowPersistence;
            DarkModeCheckBox.Checked = Configuration.Settings.DarkModeCheckBox;
            useLogSaveAsDialogCheckBox.Checked = Configuration.Settings.UseLogSaveAsDialog;
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

            if (Configuration.Settings.MainWindowPersistence != mainWindowPersistenceCheckBox.Checked)
            {
                Configuration.Settings.MainWindowPersistence = mainWindowPersistenceCheckBox.Checked;
            }

            if (Configuration.Settings.BinDirectory != binDirectoryTextBox.Text)
            {
                Configuration.Settings.BinDirectory = binDirectoryTextBox.Text;
            }

            if (Configuration.Settings.UseLogSaveAsDialog != useLogSaveAsDialogCheckBox.Checked)
            {
                Configuration.Settings.UseLogSaveAsDialog = useLogSaveAsDialogCheckBox.Checked;
            }
            if (Configuration.Settings.DarkModeCheckBox != DarkModeCheckBox.Checked)
            {
                Configuration.Settings.DarkModeCheckBox = DarkModeCheckBox.Checked;
            }

            Configuration.Settings.Save();
            applyButton.Enabled = false;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (applyButton.Enabled)
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

        private void mainWindowPersistenceCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            applyButton.Enabled = true;
        }

        private void binDirectoryTextBox_TextChanged(object sender, EventArgs e)
        {
            applyButton.Enabled = true;
        }

        private void binDirectoryButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = Configuration.Settings.BinDirectory;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    binDirectoryTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void useLogSaveAsDialogCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            applyButton.Enabled = true;
        }


        //Enabling DarkMode 

        private void SetDarkMode(Form form, bool enabled)
        {

            foreach (Control control in form.Controls)

                if (enabled)
                {

                    generalTabPage.BackColor = Color.DarkGray;

                    control.BackColor = SystemColors.ControlDark;
                    form.BackColor = SystemColors.ControlDark;
                    applyButton.Enabled = true;
                }
                else
                {

                    form.BackColor = SystemColors.Control;
                    generalTabPage.BackColor = Color.White;
                    control.BackColor = Color.White;
                    applyButton.Enabled = true;
                }
        }

        private bool darkModeEnabled = false;

        private void DarkModeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            darkModeEnabled = DarkModeCheckBox.Checked;
            SetDarkMode(this, DarkModeCheckBox.Checked);
           
            Form mainForm = Application.OpenForms["MainForm"];
            //Form vinForm = Application.


            if (DarkModeCheckBox.Checked)
            {

                EnabledDarkMode(mainForm);
               // EnabledDarkMode(vinForm);
                this.BackColor = Color.DarkGray;
                generalTabPage.BackColor = Color.DarkGray;
                applyButton.Enabled = true;
                cancelButton.Enabled = true;
            }
            else
            {
                DisabledDarkMode(mainForm);
                this.BackColor = Color.White;
                generalTabPage.BackColor = Color.White;
                applyButton.Enabled = true;
                cancelButton.Enabled = true;
            }
        }
        private void EnabledDarkMode(Form form)
        {

            
            form.BackColor = SystemColors.ControlDark;

            foreach (Control control in form.Controls)
            {
                control.BackColor = SystemColors.ControlDark;

            }
        }

        private void DisabledDarkMode(Form form)
        {
            form.BackColor = Color.White; 

            foreach (Control control in form.Controls)
            {
                control.BackColor = Color.White;

            }
        }

        private void DevicePicker_Load(object sender, EventArgs e)
        {
            SetDarkMode(this, DarkModeCheckBox.Checked);
        }


        private void DelayDialogBox_Load(object sender, EventArgs e)
        {
            SetDarkMode(this, DarkModeCheckBox.Checked);
        }
    }
}





   

