using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PcmHacking.DialogBoxes
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

            if (this.vinBox.Text.Length != 17)
            {
                this.prompt.Text = $"The VIN must be 17 characters long! This is {this.vinBox.Text.Length}.";
                return false;
            }

            if (!VinForm.IsAlphaNumeric(this.vinBox.Text))
            {
                this.prompt.Text = "The VIN must contain only letters and numbers.";
                return false;
            }

            this.vinBox.Text = this.vinBox.Text.ToUpper();

            if (IsVinChecksumOK(this.vinBox.Text))
            {
                this.prompt.Text = "The VIN is valid. Good!";
                return true;
            }

            return false;
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


        private bool IsVinChecksumOK(string vin)
        {
        // Array of VIN character position weight factors:
        ushort[] CharWeight = new ushort[] { 8, 7, 6, 5, 4, 3, 2, 10, 0, 9, 8, 7, 6, 5, 4, 3, 2 };
        ushort checksum = 0;
            for(int i = 0; i < 17; i++)
            {
                ushort digitVal = CharWeight[i];
                if (char.IsDigit(vin[i]))
                {
                    digitVal *= ((ushort)char.GetNumericValue(vin[i]));
                }
                else
                {
                    switch (char.ToUpper(vin[i]))
                    {
                        case 'A':
                        case 'J':
                            digitVal *= 1;
                            break;
                        case 'B':
                        case 'K':
                        case 'S':
                            digitVal *= 2;
                            break;
                        case 'C':
                        case 'L':
                        case 'T':
                            digitVal *= 3;
                            break;
                        case 'D':
                        case 'M':
                        case 'U':
                            digitVal *= 4;
                            break;
                        case 'E':
                        case 'N':
                        case 'V':
                            digitVal *= 5;
                            break;
                        case 'F':
                        case 'W':
                            digitVal *= 6;
                            break;
                        case 'G':
                        case 'P':
                        case 'X':
                            digitVal *= 7;
                            break;
                        case 'H':
                        case 'Y':
                            digitVal *= 8;
                            break;
                        case 'R':
                        case 'Z':
                            digitVal *= 9;
                            break;
                        default:
                            this.prompt.Text = $"The VIN contains invalid character '{vin[i]}' on position {i+1}.";
                            return false;
                    }
                }
                checksum += digitVal;
            }

            checksum %= 11;

            char CheckDigit = 'X';

            if(checksum < 10)
            {
                CheckDigit = checksum.ToString()[0];
            }

            if (vin[8] == CheckDigit)
            {
                return true;
            }
            else
            {
                this.prompt.Text = $"The VIN check digit on position 9 is incorrect!\nCorrect check digit is: {CheckDigit}";
                return false;
            }
        }

    }
}
