using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace PcmHacking
{
    public partial class MainForm
    {
        private LogProfile currentProfile = new LogProfile();
        private string currentProfilePath = null;
        private bool currentProfileIsDirty = false;

        private const string fileFilter = "*.LogProfile|Log Profiles";

        private void newButton_Click(object sender, EventArgs e)
        {
            currentProfile = new LogProfile();
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = fileFilter;
            dialog.Multiselect = false;
            dialog.Title = "Open Log Profile";
            dialog.ValidateNames = true;
            DialogResult result = dialog.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                currentProfile = LogProfileReader.Read(dialog.FileName);
            }

            this.UpdateGridFromProfile();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (currentProfilePath == null)
            {
                saveAsButton_Click(sender, e);
                return;
            }

            LogProfileWriter.Write(this.currentProfile, this.currentProfilePath);
            this.currentProfileIsDirty = false;
        }

        private void saveAsButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = fileFilter;
            dialog.OverwritePrompt = true;
            DialogResult result = dialog.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                LogProfileWriter.Write(this.currentProfile, this.currentProfilePath);
                this.currentProfileIsDirty = false;
            }
        }

        private void ResetProfile()
        {

        }
    }
}
