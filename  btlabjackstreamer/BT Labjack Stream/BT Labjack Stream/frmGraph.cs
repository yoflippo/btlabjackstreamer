using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

using ZedGraph;

namespace BT_Labjack_Stream
{
    public partial class frmGraph : Form
    {
        #region Variabelen
        private const int aantalKanalen = 8;
        int telAantalKeerTekenen = 0;
        PointPairList[] lists = null;
        LineItem[] myCurves = null;
        int aantalGeselecteerdeKanalen = 0;
        public string TitelGrafiek = "BT Labjack Streamer";
        public string LabelYas = "Spanning [Volt]";
        public string LabelXas = "Tijd [seconde]";
        private string[] namenVanKanalen = null;
        private readonly Color[] kleurLijst = new Color[aantalKanalen] { Color.Red, Color.Green, Color.Blue, Color.Black, Color.Yellow, Color.DeepPink, Color.GreenYellow, Color.Purple };
        private List<double>[] data = null;
        private int sampleFrequentie = 0;
        Stopwatch stopwatch = null;
        #endregion

        #region CONSTRUCTOR
        public frmGraph(int aantalGebruikteKanalen, ref int sampleFreq ,ref List<double>[] d)
        {
            aantalGeselecteerdeKanalen = aantalGebruikteKanalen;
            namenVanKanalen = new string[aantalKanalen];
            for (int i = 0; i < aantalGeselecteerdeKanalen; i++)
            {
                namenVanKanalen[i] = "FIO" + i.ToString();
            }
            data = d;
            sampleFrequentie = sampleFreq;
            InitializeComponent();
            Reset();
        }
        #endregion


        public void Reset()
        {
            GraphPane myPane = zg1.GraphPane;
            stopwatch = new Stopwatch(); stopwatch.Start();

            //Maak nieuw Lists aan voor de grafiek
            lists = new PointPairList[aantalGeselecteerdeKanalen];
            for (int i = 0; i < aantalGeselecteerdeKanalen; i++)
            {
                lists[i] = new PointPairList();
            }

            //Maak grafieklijnen aan
            myCurves = new LineItem[aantalGeselecteerdeKanalen];
            for (int i = 0; i < aantalGeselecteerdeKanalen; i++)
            {
                myCurves[i] = myPane.AddCurve(namenVanKanalen[i], lists[i], kleurLijst[i], SymbolType.None);
            }

            nudGraphX.Value = sampleFrequentie;

            myPane.Title.Text = TitelGrafiek;
            myPane.XAxis.Title.Text = LabelXas;
            myPane.YAxis.Title.Text = LabelYas;

            myPane.YAxis.Scale.Max = (double)nudGraphY.Value;
            myPane.YAxis.Scale.Min = 0;

            myPane.XAxis.Scale.Max = (double)nudGraphX.Value;
            myPane.XAxis.Scale.Min = 0;

            myPane.AxisChange();

            redrawGraph();
        }


        private void FillGraph()
        {
            GraphPane myPane = zg1.GraphPane;
            telAantalKeerTekenen++;

            //kies waarde uit list om te tekenen
            //long verstrekenTijd = stopwatch.ElapsedMilliseconds-5000; //de tijd die 1 seconde later is gestart
            //double omrekenGetal = (double)sampleFrequentie / 1000.0;
            //int index = (int)(verstrekenTijd * omrekenGetal);

            int index = data[0].Count - sampleFrequentie/3; //1 seconde vertraging;
            for (int i = 0; i < aantalGeselecteerdeKanalen; i++)
            {
                lists[i].Add(telAantalKeerTekenen, data[i][index]);
            }

            //X-as
            if (telAantalKeerTekenen >= myPane.XAxis.Scale.Max)
            {
                myPane.XAxis.Scale.Max = myPane.XAxis.Scale.Min + (double)nudGraphX.Value;
                myPane.XAxis.Scale.Min++;
            }
            ////Y-as
            //if (knopA > (double)nudGraphY.Value || knopA > (double)nudGraphY.Value)
            //{
            //    int max = 1 + (int)knopA;
            //    nudGraphY.Value = max;
            //}
        }

        private void redrawGraph()
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


        public bool blMagVanMain = true;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (stopwatch.ElapsedMilliseconds > 1000 && blMagVanMain)
            {
                if (this.Visible)
                {
                    FillGraph();
                    redrawGraph();
                }

            }
        }


    }
}
