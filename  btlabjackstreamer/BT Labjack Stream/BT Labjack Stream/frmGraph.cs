using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using ZedGraph;

namespace BT_Labjack_Stream
{
    public partial class frmGraph : Form
    {
        #region Variabelen
        int telAantalKeerTekenen = 0;
        PointPairList listA = new PointPairList();
        PointPairList listB = new PointPairList();
        bool startMonitoring = false;
        LineItem myCurveA = null;
        LineItem myCurveB = null;
        bool blDataIsJuist = false;
        int maxYScale = 0;
        int maxXScale = 0;
        #endregion


        public frmGraph()
        {
            InitializeComponent();
            resetGraph();
        }


        private void resetGraph()
        {
            GraphPane myPane = zg1.GraphPane;
            string titel = "Reactie capacitieve knoppen";

                if (myCurveA == null)
                {
                    myCurveA = myPane.AddCurve("Knop A",
                                    listA, Color.Red, SymbolType.None);//, SymbolType.Diamond);
                    myCurveB = myPane.AddCurve("Knop B",
                                    listB, Color.Blue, SymbolType.None);//, SymbolType.Diamond);
                }

                listA.Clear();
                listB.Clear();

                myPane.Title.Text = titel;
                myPane.XAxis.Title.Text = "Tijd [sec]";
                myPane.YAxis.Title.Text = "Spanning [Volt]";

                myPane.YAxis.Scale.Max = (double)nudGraphY.Value;
                myPane.YAxis.Scale.Min = 0;

                myPane.XAxis.Scale.Max = (double)nudGraphX.Value;
                myPane.XAxis.Scale.Min = 0;

                myPane.AxisChange();
                redrawGraph();
        }


        public void FillGraph(double knopA, double knopB)
        {
            GraphPane myPane = zg1.GraphPane;
            telAantalKeerTekenen++;
            listA.Add(telAantalKeerTekenen, knopA);
            listB.Add(telAantalKeerTekenen, knopB);

            //X-as
            if (telAantalKeerTekenen >= myPane.XAxis.Scale.Max)
            {
                myPane.XAxis.Scale.Max = myPane.XAxis.Scale.Min + (double)nudGraphX.Value;
                myPane.XAxis.Scale.Min++;
            }
            //Y-as
            if (knopB > (double)nudGraphY.Value || knopA > (double)nudGraphY.Value)
            {
                int max = 1+(int)knopA;
                nudGraphY.Value = max;
                maxYScale = max;
            }
            redrawGraph();
        }

        public void redrawGraph()
        {
            GraphPane myPane = zg1.GraphPane;

            myPane.YAxis.Scale.Max = (double)nudGraphY.Value;
            myPane.YAxis.Scale.Min = 0;

            myPane.AxisChange();
            zg1.Invalidate(); // Make sure the Graph gets redrawn
            zg1.Refresh();
        }

        private void frmGraph_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }


    }
}
