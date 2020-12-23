using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
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

            this.fileName = defaultFileName;
            this.currentProfileIsDirty = false;
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

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = fileFilter;
            dialog.Multiselect = false;
            dialog.Title = "Open Log Profile";
            dialog.ValidateNames = true;

            DialogResult result = dialog.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                this.OpenProfile(dialog.FileName);
            }
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
                this.fileName = Path.GetFileNameWithoutExtension(this.currentProfilePath);

                try
                {
                    LogProfileWriter.Write(this.currentProfile, this.currentProfilePath);
                    this.currentProfileIsDirty = false;
                }
                catch (Exception exception)
                {
                    this.AddDebugMessage(exception.ToString());
                    this.AddUserMessage(exception.Message);
                }

                this.profileList.Items.Remove(this.currentProfilePath);
                this.profileList.Items.Insert(0, new PathDisplayAdapter(this.currentProfilePath));
            }

            return result;
        }

        private void ResetProfile()
        {
            this.currentProfile = new LogProfile();
        }

        private void OpenProfile(string path)
        {
            bool alreadyInList = false;
            foreach (PathDisplayAdapter adapter in this.profileList.Items)
            {
                if (adapter.Path == path)
                {
                    alreadyInList = true;
                    break;
                }
            }

            if (!alreadyInList)
            {
                PathDisplayAdapter newAdapter = new PathDisplayAdapter(path);
                this.profileList.Items.Insert(0, newAdapter);
            }

            this.currentProfilePath = path;
            this.fileName = Path.GetFileNameWithoutExtension(this.currentProfilePath);

            LogProfileReader reader = new LogProfileReader(this.database, this);
            this.currentProfile = reader.Read(this.currentProfilePath);
            this.UpdateGridFromProfile();
            this.currentProfileIsDirty = false;
        }
    }
}
