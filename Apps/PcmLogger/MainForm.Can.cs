using PcmHacking;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PcmHacking
{
    public partial class MainForm
    {
        private void selectCanButton_Click(object sender, EventArgs e)
        {
            string canPort = DeviceConfiguration.Settings.CanPort;
            CanForm canForm = new CanForm(this, canPort);

            switch (canForm.ShowDialog())
            {
                case DialogResult.OK:
                    if (canForm.SelectedPort == null)
                    {
                        this.canPortName = null;
                        DeviceConfiguration.Settings.CanPort = null;
                    }
                    else
                    {
                        this.canPortName = canForm.SelectedPort.PortName;
                        DeviceConfiguration.Settings.CanPort = this.canPortName;
                    }

                    DeviceConfiguration.Settings.Save();
                    this.canDeviceDescription.Text = this.canPortName;

                    // Re-create the logger, so it starts using the new port.
                    this.ResetProfile();
                    this.CreateProfileFromGrid();
                    break;

                case DialogResult.Cancel:
                    break;
            }
        }

        private void canDeviceDescription_Click(object sender, EventArgs e)
        {

        }

        private void enableCanLogging_CheckedChanged(object sender, EventArgs e)
        {
            this.enableCanControls(true, true);
        }

        private void disableCanLogging_CheckedChanged(object sender, EventArgs e)
        {
            this.enableCanControls(false, true);
        }

        private void enableCanControls(bool enabled, bool reset)
        {
            this.selectCanButton.Enabled = enabled;
            this.canDeviceDescription.Enabled = enabled;
            this.canParameterGrid.Enabled = enabled;
            this.canParameterGrid.ReadOnly = !enabled;

            if (enabled)
            {
                this.canDeviceDescription.Text = this.canPortName;
            }
            else
            {
                this.canDeviceDescription.Text = string.Empty;
            }

            DeviceConfiguration.Settings.CanPort = this.canDeviceDescription.Text;
            DeviceConfiguration.Settings.Save();

            if (reset)
            {
                this.ResetProfile();
                this.CreateProfileFromGrid();
            }
        }

        private string GetSettingsKey(CanParameter parameter)
        {
            return "CanParameter_" + parameter.Name + "_Units";
        }

        private void FillCanParameterGrid()
        {
            foreach (CanParameter parameter in this.database.CanParameters)
            {
                int row = this.canParameterGrid.Rows.Add(parameter);
                DataGridViewCell cell = this.canParameterGrid.Rows[row].Cells[1];
                DataGridViewComboBoxCell combo = cell as DataGridViewComboBoxCell;
                if (combo == null)
                {
                    this.AddDebugMessage("Unexpected cell type in CAN parameter grid: " + cell.GetType().Name);
                    continue;
                }

                combo.Items.AddRange(parameter.Conversions.ToArray());

                Conversion selectedConversion = null;
                try
                {
                    string selectedUnits = Configuration.Settings[this.GetSettingsKey(parameter)] as string;
                    selectedConversion = parameter.Conversions.Where(x => x.Units == selectedUnits).FirstOrDefault();
                }
                catch (SettingsPropertyNotFoundException)
                {
                    // this space intentionally left blank
                }

                if (selectedConversion == null)
                {
                    selectedConversion = parameter.Conversions.First();
                }

                combo.Value = selectedConversion;
                parameter.SelectedConversion = selectedConversion;
            }
        }
    }
}
