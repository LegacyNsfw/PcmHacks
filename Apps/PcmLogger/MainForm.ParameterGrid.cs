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
            // First, empty the grid.
            this.parameterGrid.Rows.Clear();

            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string appDirectory = Path.GetDirectoryName(appPath);

            this.database = new ParameterDatabase(appDirectory);

            string errorMessage;
            if (!this.database.TryLoad(out errorMessage))
            {
                throw new InvalidDataException("Unable to load parameters from XML: " + errorMessage);
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
                cell.ValueMember = "Units";
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

                        DataGridViewComboBoxCell cell = (DataGridViewComboBoxCell)(row.Cells[2]);
                        Conversion profileConversion = column.Conversion;
                        string profileUnits = column.Conversion.Units;

                        foreach (Conversion conversion in cell.Items)
                        {
                            if ((conversion == profileConversion) || (conversion.Units == profileUnits))
                            {
                                cell.Value = conversion;
                            }
                        }
                    }
                }
            }
            finally
            {
                this.suspendSelectionEvents = false;
            }
        }

        private void parameterGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCheckBoxCell checkBoxCell = this.parameterGrid.CurrentCell as DataGridViewCheckBoxCell;
            if ((checkBoxCell != null) && checkBoxCell.IsInEditMode && this.parameterGrid.IsCurrentCellDirty)
            {
                this.parameterGrid.EndEdit();
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

            this.SetDirtyFlag(true);
        }

        private void CreateProfileFromGrid()
        {
            this.ResetProfile();

            foreach (DataGridViewRow row in this.parameterGrid.Rows)
            {
                if ((bool)row.Cells[0].Value == true)
                {
                    Parameter parameter = (Parameter)row.Cells[1].Value;
                    Conversion conversion = null;

                    DataGridViewComboBoxCell cell = (DataGridViewComboBoxCell)(row.Cells[2]);
                    foreach (Conversion candidate in cell.Items)
                    {
                        // The fact that we have to do both kinds of comparisons here really
                        // seems like a bug in the DataGridViewComboBoxCell code:
                        if ((candidate.Units == cell.Value as string) ||
                            (candidate == cell.Value as Conversion))
                        {
                            conversion = candidate;
                            break;
                        }
                    }

                    LogColumn column = new LogColumn(parameter, conversion);
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
