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
        public string LabelXas = "Aantal metingen";
        private string[] namenVanKanalen = null;
        private bool[] geselecteerdeKanalen = null;
        private readonly Color[] kleurLijst = new Color[aantalKanalen] { Color.Red, Color.Green, Color.Blue, Color.Black, Color.Yellow, Color.DeepPink, Color.GreenYellow, Color.Purple };
        private List<double>[] data = null;
        private int sampleFrequentie = 0;
        private int aantalNieuweMeetPunten = 0;
        Stopwatch stopwatch = null;
        private bool NieuweData = false;
        #endregion

        #region CONSTRUCTOR
        public frmGraph(int aantalGebruikteKanalen, ref int sampleFreq ,ref List<double>[] d, ref int buffer, bool[] isHetKanaalGeselecteerd)
        {
            aantalGeselecteerdeKanalen = aantalGebruikteKanalen;
            namenVanKanalen = new string[aantalKanalen];
            for (int i = 0; i < aantalKanalen; i++)
            {
                if(isHetKanaalGeselecteerd[i])
                    namenVanKanalen[i] = "FIO" + i.ToString();
            }
            data = d;
            sampleFrequentie = sampleFreq;
            InitializeComponent();
            timer1.Interval = buffer;
            geselecteerdeKanalen = isHetKanaalGeselecteerd;
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
            int tempGetalLists = 0;
            for (int i = 0; i < aantalKanalen; i++)
            {
                if (geselecteerdeKanalen[i])
                {
                    myCurves[tempGetalLists] = myPane.AddCurve(namenVanKanalen[i], lists[tempGetalLists], kleurLijst[i], SymbolType.None);
                    tempGetalLists++;
                }
            }

            nudGraphX.Value = sampleFrequentie*10;

            //uiterlijk grafiek
            myPane.Title.Text = TitelGrafiek;
            myPane.XAxis.Title.Text = LabelXas;
            myPane.YAxis.Title.Text = LabelYas;

            myPane.YAxis.Scale.Max = (double)nudGraphY.Value+10;
            myPane.YAxis.Scale.Min = 0;

            myPane.XAxis.Scale.Max = (double)nudGraphX.Value;
            myPane.XAxis.Scale.Min = 0;

            myPane.AxisChange();

            //resetten van stuurvariabelen
            telAantalKeerTekenen = 0;

            //index getal bepalen
            aantalNieuweMeetPunten = sampleFrequentie / (1000 / timer1.Interval); //aantal samples dat er bij is gekomen
            redrawGraph();
        }

        private int aantalMeetPuntenOud = 0;
        private void FillGraph()
        {
            GraphPane myPane = zg1.GraphPane;
            aantalNieuweMeetPunten = data[0].Count - aantalMeetPuntenOud;
            int temp = data[0].Count-(aantalNieuweMeetPunten*2);
            aantalMeetPuntenOud = data[0].Count;
            int tmpGeselecteerdKanaal = 0;


            if (temp > aantalNieuweMeetPunten)
            {
                for (int i = 0; i < aantalKanalen; i++)
                {
                    if (geselecteerdeKanalen[i])
                    {
                        int t = 0;
                        for (int J = 1; J < aantalNieuweMeetPunten + 1; J++)
                        {
                            t = J + temp;
                            lists[tmpGeselecteerdKanaal].Add(t, data[i][t]);
                            if (data[i][t] > (double)nudGraphY.Value) //testen of de waarde wordt weergegeven
                                nudGraphY.Value = (decimal)data[i][t];
                        }
                        tmpGeselecteerdKanaal++;
                    }
                }

                myPane.XAxis.Scale.Max = temp;
                myPane.XAxis.Scale.Min = myPane.XAxis.Scale.Max - (double)nudGraphX.Value;
            }
            
        }

        private void redrawGraph()
        {
            GraphPane myPane = zg1.GraphPane;

            myPane.AxisChange();
            zg1.Invalidate(); // Make sure the Graph gets redrawn
            zg1.Refresh();
        }

        private void frmGraph_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
            Program.frmHoofd.ToolStripMenuItem_Grafiek_aan = false;
        }

        public bool blNieuweData
        {
            set
            {
                NieuweData = value;
            }
        }

        public bool blMagVanMain = true;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.Visible & NieuweData)
                {
                    NieuweData = false;
                    FillGraph();
                    redrawGraph();
                }
        }

        private void frmGraph_Resize(object sender, EventArgs e)
        {         
            gbx_Settings.Location = new Point(12, this.Height - gbx_Settings.Height - 40);
            zg1.Size = new System.Drawing.Size(this.Size.Width - 40, this.Size.Height - (gbx_Settings.Size.Height+80));
        }

        private void frmGraph_VisibleChanged(object sender, EventArgs e)
        {
            if(this.Visible)
                Program.frmHoofd.ToolStripMenuItem_Grafiek_aan = true;
        }

        private void nudGraphY_ValueChanged(object sender, EventArgs e)
        {
            GraphPane myPane = zg1.GraphPane;
            myPane.YAxis.Scale.Max = (double)nudGraphY.Value;
        }

        private void zg1_ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            GraphPane myPane = zg1.GraphPane;
            nudGraphX.Value = (decimal)(myPane.XAxis.Scale.Max-myPane.XAxis.Scale.Min);
        }


    }
}
