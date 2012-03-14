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
        frmMain frmMainForm = null;
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

    }
}
