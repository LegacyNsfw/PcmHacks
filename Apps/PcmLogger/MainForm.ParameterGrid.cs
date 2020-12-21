using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PcmHacking
{
    public partial class MainForm
    {
        private Dictionary<string, DataGridViewRow> parameterIdsToRows;
        private ParameterDatabase database;
        private bool suspendSelectionEvents = false;
        private MathValueConfigurationLoader loader;

        private void FillParameterGrid()
        {
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string appDirectory = Path.GetDirectoryName(appPath);

            this.database = new ParameterDatabase(appDirectory);

            string errorMessage;
            if (!this.database.TryLoad(out errorMessage))
            {
                MessageBox.Show(this, errorMessage, "Unable to load parameters from XML.");
            }

            this.parameterIdsToRows = new Dictionary<string, DataGridViewRow>();

            foreach (Parameter parameter in this.database.Parameters)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(this.parameterGrid);
                row.Cells[0].Value = false; // enabled
                row.Cells[1].Value = parameter;

                DataGridViewComboBoxCell cell = (DataGridViewComboBoxCell)row.Cells[2];
                cell.DisplayMember = "Units";
                foreach (Conversion conversion in parameter.Conversions)
                {
                    cell.Items.Add(conversion);
                }
                row.Cells[2].Value = parameter.Conversions.First();

                this.parameterIdsToRows[parameter.Id] = row;
                this.parameterGrid.Rows.Add(row);
            }
        }

        private void UpdateGridFromProfile()
        {
            try
            {
                this.suspendSelectionEvents = true;

                foreach (DataGridViewRow row in this.parameterGrid.Rows)
                {
                    row.Cells[0].Value = false;
                }

                foreach (ProfileParameter logParameter in this.currentProfile.Parameters)
                {
                    DataGridViewRow row;
                    if (this.parameterIdsToRows.TryGetValue(logParameter.Parameter.Id, out row))
                    {
                        row.Cells[0].Value = true;
                    }
                }
            }
            finally
            {
                this.suspendSelectionEvents = false;
            }
        }

        private void parameterGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            this.RegenerateProfile();
        }

        private void RegenerateProfile()
        { 
            if (this.suspendSelectionEvents)
            {
                return;
            }

            bool wasLogging = this.logging;

            if (wasLogging)
            {
                this.StopLogging();
            }

            this.ResetProfile();

            this.CreateProfileFromGrid();

            this.CreateDpidConfiguration();

            this.currentProfileIsDirty = true;

            if (wasLogging)
            {
                this.StartLogging();
            }
            
        }

        private void CreateProfileFromGrid()
        {
            this.ResetProfile();

            foreach (DataGridViewRow row in this.parameterGrid.Rows)
            {
                if ((bool)row.Cells[0].Value == true)
                {
                    Conversion conversion = (Conversion)row.Cells[2].Value;
                    ProfileParameter parameter = new ProfileParameter((Parameter)row.Cells[1].Value, conversion);
                    this.currentProfile.AddParameter(parameter);
                }
            }
        }

        private void CreateDpidConfiguration()
        {
            DpidConfiguration dpids = new DpidConfiguration();

            List<ProfileParameter> singleByteParameters = new List<ProfileParameter>();

            byte groupId = 0xFE;
            ParameterGroup group = new ParameterGroup(groupId);
            foreach(ProfileParameter parameter in this.currentProfile.Parameters)
            {
                PcmParameter pcmParameter = parameter.Parameter as PcmParameter;
                if (pcmParameter == null)
                {
                    continue;
                }

                if (pcmParameter.ByteCount == 1)
                {
                    singleByteParameters.Add(parameter);
                    continue;
                }

                group.TryAddParameter(parameter);
                if (group.TotalBytes == ParameterGroup.MaxBytes)
                {
                    dpids.ParameterGroups.Add(group);
                    groupId--;
                    group = new ParameterGroup(groupId);
                }
            }

            foreach (ProfileParameter parameter in singleByteParameters)
            {
                group.TryAddParameter(parameter);
                if (group.TotalBytes == ParameterGroup.MaxBytes)
                {
                    dpids.ParameterGroups.Add(group);
                    groupId--;
                    group = new ParameterGroup(groupId);
                }
            }

            if (group.Parameters.Count > 0)
            {
                dpids.ParameterGroups.Add(group);
                group = null;
            }

            if (this.loader == null)
            {
                // TODO: Move this into ParameterDatabase
                this.loader = new MathValueConfigurationLoader(this);
                this.loader.Initialize();
            }

            this.dpidsAndMath = new DpidsAndMath(dpids, loader.Configuration);
        }
    }
}
