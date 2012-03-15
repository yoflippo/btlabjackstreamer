using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BT_Labjack_Stream
{
    public partial class frmExpertSettings : Form
    {
        ToolStripMenuItem tsmi = null;
        public frmExpertSettings(ToolStripMenuItem t)
        {
            tsmi = t;
            InitializeComponent();
        }

        private void frmExpertSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
            tsmi.Checked = false;
        }

        #region COMBOBOXES
        private void cbxFIO0_dif_CheckedChanged(object sender, EventArgs e)
        {
             combx_FIO0.Enabled = cbxFIO0_dif.Checked;
             cbxFIO0_Digitaal.Enabled = !cbxFIO0_dif.Checked;
        }

        private void cbxFIO1_dif_CheckedChanged(object sender, EventArgs e)
        {
            combx_FIO1.Enabled = cbxFIO1_dif.Checked;
            cbxFIO1_Digitaal.Enabled = !cbxFIO1_dif.Checked;
        }

        private void cbxFIO2_dif_CheckedChanged(object sender, EventArgs e)
        {
            combx_FIO2.Enabled = cbxFIO2_dif.Checked;
            cbxFIO2_Digitaal.Enabled = !cbxFIO2_dif.Checked;
        }

        private void cbxFIO3_dif_CheckedChanged(object sender, EventArgs e)
        {
            combx_FIO3.Enabled = cbxFIO3_dif.Checked;
            cbxFIO3_Digitaal.Enabled = !cbxFIO3_dif.Checked;
        }

        private void cbxFIO4_dif_CheckedChanged(object sender, EventArgs e)
        {
            combx_FIO4.Enabled = cbxFIO4_dif.Checked;
            cbxFIO4_Digitaal.Enabled = !cbxFIO4_dif.Checked;
        }

        private void cbxFIO5_dif_CheckedChanged(object sender, EventArgs e)
        {
            combx_FIO5.Enabled = cbxFIO5_dif.Checked;
            cbxFIO5_Digitaal.Enabled = !cbxFIO5_dif.Checked;
        }

        private void cbxFIO6_dif_CheckedChanged(object sender, EventArgs e)
        {
            combx_FIO6.Enabled = cbxFIO6_dif.Checked;
            cbxFIO6_Digitaal.Enabled = !cbxFIO6_dif.Checked;
        }

        private void cbxFIO7_dif_CheckedChanged(object sender, EventArgs e)
        {
            combx_FIO7.Enabled = cbxFIO7_dif.Checked;
            cbxFIO7_Digitaal.Enabled = !cbxFIO7_dif.Checked;
        }
        #endregion

        #region ACCESSORS

        //differentieel
        #region DIFF CBXS
        public bool cbx_Differentiaal_FIO0
        {
            get
            { 
                return cbxFIO0_dif.Checked;
            }
        }

        public bool cbx_Differentiaal_FIO1
        {
            get
            {
                return cbxFIO1_dif.Checked;
            }
        }
        public bool cbx_Differentiaal_FIO2
        {
            get
            {
                return cbxFIO2_dif.Checked;
            }
        }
        public bool cbx_Differentiaal_FIO3
        {
            get
            {
                return cbxFIO3_dif.Checked;
            }
        }
        public bool cbx_Differentiaal_FIO4
        {
            get
            {
                return cbxFIO4_dif.Checked;
            }
        }
        public bool cbx_Differentiaal_FIO5
        {
            get
            {
                return cbxFIO5_dif.Checked;
            }
        }
        public bool cbx_Differentiaal_FIO6
        {
            get
            {
                return cbxFIO6_dif.Checked;
            }
        }
        public bool cbx_Differentiaal_FIO7
        {
            get
            {
                return cbxFIO7_dif.Checked;
            }
        }
        #endregion

        //Digitaal
        #region DIGITAL CBXS
        public bool cbx_Digitaal_FIO0
        {
            get
            {
                return cbxFIO0_Digitaal.Checked;
            }
        }

        public bool cbx_Digitaal_FIO1
        {
            get
            {
                return cbxFIO1_Digitaal.Checked;
            }
        }

        public bool cbx_Digitaal_FIO2
        {
            get
            {
                return cbxFIO2_Digitaal.Checked;
            }
        }

        public bool cbx_Digitaal_FIO3
        {
            get
            {
                return cbxFIO3_Digitaal.Checked;
            }
        }

        public bool cbx_Digitaal_FIO4
        {
            get
            {
                return cbxFIO4_Digitaal.Checked;
            }
        }

        public bool cbx_Digitaal_FIO5
        {
            get
            {
                return cbxFIO5_Digitaal.Checked;
            }
        }

        public bool cbx_Digitaal_FIO6
        {
            get
            {
                return cbxFIO6_Digitaal.Checked;
            }
        }

        public bool cbx_Digitaal_FIO7
        {
            get
            {
                return cbxFIO7_Digitaal.Checked;
            }
        }
        #endregion

        //comboboxes
        #region COMBOBOXES
        public string combxFIO0
        {
            get
            {
                return combx_FIO0.SelectedItem.ToString();
            }
        }

        public string combxFIO1
        {
            get
            {
                return combx_FIO1.SelectedItem.ToString();
            }
        }

        public string combxFIO2
        {
            get
            {
                return combx_FIO2.SelectedItem.ToString();
            }
        }

        public string combxFIO3
        {
            get
            {
                return combx_FIO3.SelectedItem.ToString();
            }
        }

        public string combxFIO4
        {
            get
            {
                return combx_FIO4.SelectedItem.ToString();
            }
        }

        public string combxFIO5
        {
            get
            {
                return combx_FIO5.SelectedItem.ToString();
            }
        }

        public string combxFIO6
        {
            get
            {
                return combx_FIO6.SelectedItem.ToString();
            }
        }

        public string combxFIO7
        {
            get
            {
                return combx_FIO7.SelectedItem.ToString();
            }
        }
        #endregion

        //Enable/disable Kanalen
        #region ENABLE KANALEN
        public bool EnableFIO0
        {
            set
            {
                cbxFIO0_dif.Enabled = value;
                if (cbxFIO0_dif.Checked)
                {
                    combx_FIO0.Enabled = value;
                    cbxFIO0_Digitaal.Enabled = false;
                }
            }
        }

        public bool EnableFIO1
        {
            set
            {
                cbxFIO1_dif.Enabled = value;
                if (cbxFIO1_dif.Checked)
                {
                    combx_FIO1.Enabled = value;
                    cbxFIO1_Digitaal.Enabled = false;
                }
            }
        }

        public bool EnableFIO2
        {
            set
            {
                cbxFIO2_dif.Enabled = value;
                if (cbxFIO2_dif.Checked)
                {
                    combx_FIO2.Enabled = value;
                    cbxFIO2_Digitaal.Enabled = false;
                }
            }
        }

        public bool EnableFIO3
        {
            set
            {
                cbxFIO3_dif.Enabled = value;
                if (cbxFIO3_dif.Checked)
                {
                    combx_FIO3.Enabled = value;
                    cbxFIO3_Digitaal.Enabled = false;
                }
            }
        }

        public bool EnableFIO4
        {
            set
            {
                cbxFIO4_dif.Enabled = value;
                if (cbxFIO4_dif.Checked)
                {
                    combx_FIO4.Enabled = value;
                    cbxFIO4_Digitaal.Enabled = value;
                }
            }
        }

        public bool EnableFIO5
        {
            set
            {
                cbxFIO5_dif.Enabled = value;
                if (cbxFIO5_dif.Checked)
                {
                    combx_FIO5.Enabled = value;
                    cbxFIO5_Digitaal.Enabled = value;
                }
            }
        }

        public bool EnableFIO6
        {
            set
            {
                cbxFIO6_dif.Enabled = value;
                if (cbxFIO6_dif.Checked)
                {
                    combx_FIO6.Enabled = value;
                    cbxFIO6_Digitaal.Enabled = value;
                }
            }
        }

        public bool EnableFIO7
        {
            set
            {
                cbxFIO7_dif.Enabled = value;
                if (cbxFIO7_dif.Checked)
                {
                    combx_FIO7.Enabled = value;
                    cbxFIO7_Digitaal.Enabled = value;
                }
            }
        }
        #endregion

        private void cbxFIO3_Digitaal_CheckedChanged(object sender, EventArgs e)
        {

        }


        #endregion 
        //EINDE KLASSE
    }
}
