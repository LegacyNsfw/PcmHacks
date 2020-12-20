using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PcmHacking
{
    public partial class MainForm
    {
        private void FillParameterGrid()
        {
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string appDirectory = Path.GetDirectoryName(appPath);
            string parametersPath = Path.Combine(appDirectory, "Parameters.Standard.xml");

            string errorMessage;
            if (!ParameterDatabase.TryLoad(parametersPath, out errorMessage))
            {
                MessageBox.Show(this, errorMessage, "Unable to load parameters from XML.");
            }

            foreach (Parameter parameter in ParameterDatabase.Parameters)
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
                this.parameterGrid.Rows.Add(row);
            }
        }

        private void UpdateGridFromProfile()
        {
            // TODO
        }
    }
}
