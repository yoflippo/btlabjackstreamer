using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BT_Labjack_Stream
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            frmHoofd = new frmMain();
            Application.Run(frmHoofd);
        }
        public static frmMain frmHoofd = null;
    }
}
