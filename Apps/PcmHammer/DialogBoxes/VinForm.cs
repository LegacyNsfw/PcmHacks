using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Flash411.DialogBoxes
{
    /// <summary>
    /// Prompt the user to enter a valid VIN.
    /// </summary>
    public partial class VinForm : Form
    {
        /// <summary>
        /// This will be copied into the text box when the dialog box appears.
        /// When the dialog closes, if the user provided a valid VIN it will
        /// be returned via this property. If they didn't, this will be null.
        /// </summary>
        public string Vin { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public VinForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Load event handler.
        /// </summary>
        private void VinForm_Load(object sender, EventArgs e)
        {
            this.vinBox.Text = this.Vin;
            this.vinBox_TextChanged(null, null);
        }

        /// <summary>
        /// OK button click handler.
        /// </summary>
        private void okButton_Click(object sender, EventArgs e)
        {
            if (!this.IsLegal())
            {
                return;
            }

            this.Vin = this.vinBox.Text;

            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Cancel button click handler.
        /// </summary>
        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Vin = null;

            this.DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Validate the new VIN every time it changes.
        /// </summary>
        private void vinBox_TextChanged(object sender, EventArgs e)
        {
            this.okButton.Enabled = this.IsLegal();
        }

        /// <summary>
        /// Validate the VIN.
        /// </summary>
        private bool IsLegal()
        {
            bool isLegal = true;

            if (this.vinBox.Text.Length == 17)
            {
                this.prompt.Text = "The VIN is 17 characters long. Good!";
            }
            else
            {
                this.prompt.Text = $"The VIN must be 17 characters long. This is {this.vinBox.Text.Length}.";
                isLegal = false;
            }

            if (VinForm.IsAlphaNumeric(this.vinBox.Text))
            {
                this.prompt2.Text = "The VIN contains only letters and numbers. Good!";
            }
            else
            {
                this.prompt2.Text = "The VIN must contain only letters and numbers.";
                isLegal = false;
            }

            return isLegal;
        }

        /// <summary>
        /// Find out if the whole VIN string is alphanumeric.
        /// </summary>
        private static bool IsAlphaNumeric(string vin)
        {
            foreach(char c in vin)
            {
                if(!char.IsLetterOrDigit(c))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
