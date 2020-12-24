using System;
using System.IO;
using System.Collections.Specialized;
using System.Windows.Forms;

namespace PcmHacking
{
    partial class MainForm
    {
        private void LoadProfileHistory()
        {
            if (Configuration.Settings.RecentProfiles != null)
            {
                foreach (string path in Configuration.Settings.RecentProfiles)
                {
                    if (File.Exists(path) && !this.profileList.Items.Contains(path))
                    {
                        PathDisplayAdapter adapter = new PathDisplayAdapter(path);
                        this.profileList.Items.Add(adapter);
                    }
                }
            }
        }

        private void SaveProfileHistory()
        {
            StringCollection paths = Configuration.Settings.RecentProfiles;
            if (paths == null)
            {
                paths = new StringCollection();
            }

            paths.Clear();

            foreach(PathDisplayAdapter adapter in this.profileList.Items)
            {
                paths.Add(adapter.Path);
            }

            Configuration.Settings.RecentProfiles = paths;
            Configuration.Settings.Save();
        }

        private void profileList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.profileList.SelectedIndex == -1)
            {
                this.removeProfileButton.Enabled = false;
                return;
            }
            else
            {
                this.removeProfileButton.Enabled = true;
            }

            if (this.currentProfileIsDirty)
            {
                if (this.SaveIfNecessary() == DialogResult.Cancel)
                {
                    this.profileList.ClearSelected();
                    return;
                }
            }

            PathDisplayAdapter adapter = (PathDisplayAdapter)this.profileList.SelectedItem;
            if (adapter == null)
            {
                return;
            }

            this.OpenProfile(adapter.Path);
        }

        private void removeProfileButton_Click(object sender, EventArgs e)
        {
            int index = this.profileList.SelectedIndex;
            this.profileList.Items.RemoveAt(this.profileList.SelectedIndex);

            if (index >= this.profileList.Items.Count)
            {
                index = this.profileList.Items.Count - 1;
            }

            this.profileList.SelectedIndex = index;
            this.profileList.Focus();
        }

        private void MoveProfileToTop(string path)
        {
            foreach (PathDisplayAdapter adapter in this.profileList.Items)
            {
                if (adapter.Path == path)
                {
                    this.profileList.Items.Remove(adapter);
                    break;
                }
            }

            PathDisplayAdapter newAdapter = new PathDisplayAdapter(path);
            this.profileList.Items.Insert(0, newAdapter);
        }
    }
}
