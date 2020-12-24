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
        private bool suspendSelectionEvents = true;

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
                if (!parameter.IsSupported(osid))
                {
                    continue;
                }

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

            this.suspendSelectionEvents = false;
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

                foreach (LogColumn column in this.currentProfile.Columns)
                {
                    DataGridViewRow row;
                    if (this.parameterIdsToRows.TryGetValue(column.Parameter.Id, out row))
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
            this.LogProfileChanged();
        }

        private void LogProfileChanged()
        { 
            if (this.suspendSelectionEvents)
            {
                return;
            }
 
            this.ResetProfile();

            this.CreateProfileFromGrid();

            this.currentProfileIsDirty = true;
        }

        private void CreateProfileFromGrid()
        {
            this.ResetProfile();

            foreach (DataGridViewRow row in this.parameterGrid.Rows)
            {
                if ((bool)row.Cells[0].Value == true)
                {
                    Conversion conversion = (Conversion)row.Cells[2].Value;
                    LogColumn column = new LogColumn((Parameter)row.Cells[1].Value, conversion);
                    this.currentProfile.AddColumn(column);
                }
            }
        }
    }
}
