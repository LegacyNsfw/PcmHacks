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

        private const string fileFilter = "Log Profiles (*.LogProfile)|*.LogProfile|All Files|*.*";

        private void newButton_Click(object sender, EventArgs e)
        {
            if (this.currentProfileIsDirty)
            {
                if (this.SaveIfNecessary() == DialogResult.Cancel)
                {
                    return;
                }
            }

            this.ResetProfile();
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            if (this.currentProfileIsDirty)
            {
                if (this.SaveIfNecessary() == DialogResult.Cancel)
                {
                    return;
                }
            }

            this.ResetProfile();

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = fileFilter;
            dialog.Multiselect = false;
            dialog.Title = "Open Log Profile";
            dialog.ValidateNames = true;

            DialogResult result = dialog.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                LogProfileReader reader = new LogProfileReader(this.database, this);
                currentProfile = reader.Read(dialog.FileName);
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
            this.ShowSaveAs();
        }

        private DialogResult SaveIfNecessary()
        {
            DialogResult result = MessageBox.Show(
                this,
                "Would you like to save the current profile before continuing?",
                "The current profile has changed.",
                MessageBoxButtons.YesNoCancel);

            switch (result)
            {
                case DialogResult.Yes:
                    if (string.IsNullOrEmpty(currentProfilePath))
                    {
                        if (this.ShowSaveAs() == DialogResult.Cancel)
                        {
                            return DialogResult.Cancel;
                        }

                        return DialogResult.OK;
                    }
                    else
                    {
                        this.saveButton_Click(this, new EventArgs());
                        return DialogResult.OK;
                    }

                case DialogResult.Cancel:
                    return DialogResult.Cancel;

                default: // DialogResult.No
                    return DialogResult.OK;
            }
        }

        private DialogResult ShowSaveAs()
        { 
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = fileFilter;
            dialog.OverwritePrompt = true;
            dialog.AddExtension = true;
            DialogResult result = dialog.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                this.currentProfilePath = dialog.FileName;
                LogProfileWriter.Write(this.currentProfile, this.currentProfilePath);
                this.currentProfileIsDirty = false;
            }

            return result;
        }

        private void ResetProfile()
        {
            currentProfile = new LogProfile();
        }
    }
}
