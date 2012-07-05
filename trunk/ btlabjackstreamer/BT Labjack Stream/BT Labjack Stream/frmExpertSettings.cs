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
        bool blLabjackHV = false;
        public frmExpertSettings(ToolStripMenuItem t, bool LabjackHV)
        {
            tsmi = t;
            InitializeComponent();
            if (LabjackHV)
            {
                EnableFIO0 = false;
                EnableFIO1 = false;
                EnableFIO2 = false;
                EnableFIO3 = false;
                blLabjackHV = LabjackHV;
            }
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
             if (cbxFIO0_dif.Checked)
             {
                 cbxFIO0_Digitaal.Enabled = false;
                 cbxFIO0_Digitaal.Checked = false;
             }
             else
                 cbxFIO0_Digitaal.Enabled = true;
        }
        private void cbxFIO1_dif_CheckedChanged(object sender, EventArgs e)
        {
            combx_FIO1.Enabled = cbxFIO1_dif.Checked;
            if (cbxFIO1_dif.Checked)
            {
                cbxFIO1_Digitaal.Enabled = false;
                cbxFIO1_Digitaal.Checked = false;
            }
            else
                cbxFIO1_Digitaal.Enabled = true;
        }
        private void cbxFIO2_dif_CheckedChanged(object sender, EventArgs e)
        {
            combx_FIO2.Enabled = cbxFIO2_dif.Checked;
            if (cbxFIO2_dif.Checked)
            {
                cbxFIO2_Digitaal.Enabled = false;
                cbxFIO2_Digitaal.Checked = false;
            }
            else
                cbxFIO2_Digitaal.Enabled = true;
        }
        private void cbxFIO3_dif_CheckedChanged(object sender, EventArgs e)
        {
            combx_FIO3.Enabled = cbxFIO3_dif.Checked;
            if (cbxFIO3_dif.Checked)
            {
                cbxFIO3_Digitaal.Enabled = false;
                cbxFIO3_Digitaal.Checked = false;
            }
            else
                cbxFIO3_Digitaal.Enabled = true;
        }
        private void cbxFIO4_dif_CheckedChanged(object sender, EventArgs e)
        {
            combx_FIO4.Enabled = cbxFIO4_dif.Checked;
            if (cbxFIO4_dif.Checked)
            {
                cbxFIO4_Digitaal.Enabled = false;
                cbxFIO4_Digitaal.Checked = false;
            }
            else
                cbxFIO4_Digitaal.Enabled = true;
        }
        private void cbxFIO5_dif_CheckedChanged(object sender, EventArgs e)
        {
            combx_FIO5.Enabled = cbxFIO5_dif.Checked;
            if (cbxFIO5_dif.Checked)
            {
                cbxFIO5_Digitaal.Enabled = false;
                cbxFIO5_Digitaal.Checked = false;
            }
            else
                cbxFIO5_Digitaal.Enabled = true;
        }
        private void cbxFIO6_dif_CheckedChanged(object sender, EventArgs e)
        {
            combx_FIO6.Enabled = cbxFIO6_dif.Checked;
            if (cbxFIO6_dif.Checked)
            {
                cbxFIO6_Digitaal.Enabled = false;
                cbxFIO6_Digitaal.Checked = false;
            }
            else
                cbxFIO6_Digitaal.Enabled = true;
        }
        private void cbxFIO7_dif_CheckedChanged(object sender, EventArgs e)
        {
            combx_FIO7.Enabled = cbxFIO7_dif.Checked;
            if (cbxFIO7_dif.Checked)
            {
                cbxFIO7_Digitaal.Enabled = false;
                cbxFIO7_Digitaal.Checked = false;
            }
            else
                cbxFIO7_Digitaal.Enabled = true;
        }

        #region DIGITALE CHECKBOXES
        private void cbxFIO0_Digitaal_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxFIO0_Digitaal.Checked)
            {
                cbxFIO0_dif.Checked = false;
                cbxFIO0_dif.Enabled = false;
            }
            else
                cbxFIO0_dif.Enabled = true;
        }
        private void cbxFIO1_Digitaal_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxFIO1_Digitaal.Checked)
            {
                cbxFIO1_dif.Checked = false;
                cbxFIO1_dif.Enabled = false;
            }
            else
                cbxFIO1_dif.Enabled = true;
        }
        private void cbxFIO2_Digitaal_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxFIO2_Digitaal.Checked)
            {
                cbxFIO2_dif.Checked = false;
                cbxFIO2_dif.Enabled = false;
            }
            else
                cbxFIO2_dif.Enabled = true;
        }
        private void cbxFIO3_Digitaal_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxFIO3_Digitaal.Checked)
            {
                cbxFIO3_dif.Checked = false;
                cbxFIO3_dif.Enabled = false;
            }
            else
                cbxFIO3_dif.Enabled = true;
        }
        private void cbxFIO4_Digitaal_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxFIO4_Digitaal.Checked)
            {
                cbxFIO4_dif.Checked = false;
                cbxFIO4_dif.Enabled = false;
            }
            else
                cbxFIO4_dif.Enabled = true;
        }
        private void cbxFIO5_Digitaal_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxFIO5_Digitaal.Checked)
            {
                cbxFIO5_dif.Checked = false;
                cbxFIO5_dif.Enabled = false;
            }
            else
                cbxFIO5_dif.Enabled = true;
        }
        private void cbxFIO6_Digitaal_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxFIO6_Digitaal.Checked)
            {
                cbxFIO6_dif.Checked = false;
                cbxFIO6_dif.Enabled = false;
            }
            else
                cbxFIO6_dif.Enabled = true;
        }
        private void cbxFIO7_Digitaal_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxFIO7_Digitaal.Checked)
            {
                cbxFIO7_dif.Checked = false;
                cbxFIO7_dif.Enabled = false;
            }
            else
                cbxFIO7_dif.Enabled = true;
        }
        #endregion 

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

        //groupbox
        public bool groupBoxExpertSettings
        {
            set
            {
                gbxInstellingen.Enabled = value;
            }
        }

        //Enable/disable Kanalen
        #region ENABLE KANALEN
        public bool EnableFIO0
        {
            set
            {
                if (!blLabjackHV)
                {
                    cbxFIO0_dif.Enabled = value;
                    cbxFIO0_Digitaal.Enabled = value;
                }
            }
        }

        public bool EnableFIO1
        {
            set
            {
                if (!blLabjackHV)
                {
                    cbxFIO1_dif.Enabled = value;
                    cbxFIO1_Digitaal.Enabled = value;
                }
            }
        }

        public bool EnableFIO2
        {
            set
            {
                if (!blLabjackHV)
                {
                    cbxFIO2_dif.Enabled = value;
                    cbxFIO2_Digitaal.Enabled = value;
                }
            }
        }

        public bool EnableFIO3
        {
            set
            {
                if (!blLabjackHV)
                {
                    cbxFIO3_dif.Enabled = value;
                    cbxFIO3_Digitaal.Enabled = value;
                }
            }
        }

        public bool EnableFIO4
        {
            set
            {
                cbxFIO4_dif.Enabled = value;
                cbxFIO4_Digitaal.Enabled = value;
            }
        }

        public bool EnableFIO5
        {
            set
            {
                cbxFIO5_dif.Enabled = value;
                cbxFIO5_Digitaal.Enabled = value;
            }
        }

        public bool EnableFIO6
        {
            set
            {
                cbxFIO6_dif.Enabled = value;
                cbxFIO6_Digitaal.Enabled = value;
            }
        }

        public bool EnableFIO7
        {
            set
            {
                cbxFIO7_dif.Enabled = value;
                cbxFIO7_Digitaal.Enabled = value;
            }
        }
        #endregion

        #endregion 

        //EINDE KLASSE
    }
}
