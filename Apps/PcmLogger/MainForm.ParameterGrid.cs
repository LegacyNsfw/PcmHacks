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

            if (!this.parameterSearch.Focused)
            {
                this.ShowSearchPrompt();
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

        private bool showSearchPrompt = true;

        private void ShowSearchPrompt()
        {
            this.parameterSearch.Text = "";
            parameterSearch_Leave(this, new EventArgs());
        }

        private void parameterSearch_Enter(object sender, EventArgs e)
        {
            if (this.showSearchPrompt)
            {
                this.parameterSearch.Text = "";
                this.showSearchPrompt = false;
                return;
            }
        }

        private void parameterSearch_Leave(object sender, EventArgs e)
        {
            if (this.parameterSearch.Text.Length == 0)
            {
                this.showSearchPrompt = true;
                this.parameterSearch.Text = "Search...";
                return;
            }
        }

        private void parameterSearch_TextChanged(object sender, EventArgs e)
        {
            if (this.showSearchPrompt)
            {
                return;
            }

            foreach (DataGridViewRow row in this.parameterGrid.Rows)
            {
                Parameter parameter = row.Cells[1].Value as Parameter;
                if (parameter == null)
                {
                    continue;
                }
                
                if (parameter.Name.IndexOf(this.parameterSearch.Text, StringComparison.CurrentCultureIgnoreCase) == -1)
                {
                    row.Visible = false;
                }
                else
                {
                    row.Visible = true;
                }
            }
        }
    }
}
