using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace PcmHacking
{
    public partial class HWSelector : Form
    {
        public HWSelector()
        {
            InitializeComponent();
        }
        private List<PcmInfoXML> pcminfos;
        public PcmInfoXML Selected
        {
            get
            {
                return pcminfos[HWType.SelectedIndex];
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void HWSelector_Load(object sender, EventArgs e)
        {
            WarningLabel.Text = "WARNING!\n\rUnsupported OSID!\n\rMake sure you select correct HW type!";
            pcminfos = PcmInfo.GetPcmInfos();
            for (int p=0;p<pcminfos.Count;p++)
            {
                PcmInfoXML pcminfo = pcminfos[p];
                string item = pcminfo.HardwareType.ToString() + " [" + pcminfo.Description + "]";
                HWType.Items.Add(item);
            }
        }
    }
}
