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
    public partial class frmInfo : Form
    {
        Uri website = new Uri("http://www.a1z.nl/software/bt-labjack-stream/bt-labjack-streamer-informatie");

        public frmInfo()
        {
            InitializeComponent();
        }

        private void frmInfo_Load(object sender, EventArgs e)
        {
            webBrowser1.Url = website;
        }

        private void frmInfo_VisibleChanged(object sender, EventArgs e)
        {
            webBrowser1.Url = website;
        }

        private void frmInfo_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }


    }
}
