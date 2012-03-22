using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO.Ports;
using System.IO;
using ZedGraph;
using System.Threading;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        #region Variabelen
        const int aantalMetingen = 10000000;
        int maxYScale = 1;                                          //variabele testen maximale waarde voor grafiek
        int max_line_cnt = 2;
        public const int LINE_BREAK = 160;
        public const int MAX_BYTES = 320;
        int max_bytes_cnt = 0;
        int telAantalKeerTekenen = 0;
        SerialPort comPort = new SerialPort();
        String indata;
        Boolean sync = false;
        int sync_cnt = 0;
        PointPairList listA = new PointPairList();
        PointPairList listB = new PointPairList();
        bool startMonitoring = false;
        int lstBoxCounter = 0;                                      //vertragen van verversing lstBox
        //List<string> lstRawData = new List<string>(aantalMetingen);
        List<string> lstDataToTxtFile = new List<string>(aantalMetingen);
        LineItem myCurveA = null;
        LineItem myCurveB = null;
        bool blDataIsJuist = false;
        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetReset();
        }

        private void SetReset()
        {
            SerialPort comPort = new SerialPort("COM6");

            connectButton.Enabled = false;
            comPortsList.Items.Clear();
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();

            foreach (string port in ports)
            {
                comPortsList.Items.Add(port);
                connectButton.Enabled = true;
            }

            progressBar.Maximum = max_line_cnt;
            UpDown.Text = Convert.ToString(max_line_cnt);
            max_line_cnt = Convert.ToInt16(UpDown.Value);

            resetGraph();
        }

        private void resetGraph()
        {
            GraphPane myPane = zg1.GraphPane;
            string titel = "Reactie capacitieve knoppen";
            if (cbxGrafiekAanUit.Checked)//(cbxGrafiekAanUit.Checked && myPane.Title.Text != titel)
            {
                if (myCurveA == null)
                {
                    myCurveA = myPane.AddCurve("Knop A",
                                    listA, Color.Red, SymbolType.None);//, SymbolType.Diamond);

                    myCurveB = myPane.AddCurve("Knop B",
                                    listB, Color.Blue, SymbolType.None);//, SymbolType.Diamond);
                }

                listBox.Items.Clear();
                progressBar.Value = 0;

                max_bytes_cnt = 0;
                listA.Clear();
                listB.Clear();

                //LineItem myCurveA_avg = myPane.AddCurve("Knop A avg",
                //                listA_avg, Color.Black, SymbolType.None);//, SymbolType.Diamond);

                //LineItem myCurveB_avg = myPane.AddCurve("Knop B avg",
                //                listB_avg, Color.Orange, SymbolType.None);//, SymbolType.Diamond);

                // Set the titles and axis labels
                myPane.Title.Text = titel;
                myPane.XAxis.Title.Text = "Time, Seconds";
                myPane.YAxis.Title.Text = "Parameter A";

                myPane.YAxis.Scale.Max = (double)nudGrafiekYmax.Value;
                myPane.YAxis.Scale.Min = 0;

                myPane.XAxis.Scale.Max = (double)nudGrafiekXmax.Value;
                myPane.XAxis.Scale.Min = 0;

                myPane.AxisChange();

                //ThreadStart job = new ThreadStart(ThreadJob);
                //Thread thread = new Thread(job);
                //thread.Start();
            }
            redrawGraph();
        }

        private void connect_Click(object sender, EventArgs e)
        {
            tryConnectToCOM();
        }

        private void comPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            int[] knoppen = null;
            try
            {
                indata = comPort.ReadExisting(); 

                //test indata op correctheid
                if (indata.Length > 5)
                {
                    indata = indata.Substring(0, 5);                    //pak eerste 5 karakters
                    if (tstlbl_waarschuwing.Text == "Er mist data!!")
                    {
                        tstlbl_waarschuwing.Text = " Er mist data!!";
                    }
                    else
                        tstlbl_waarschuwing.Text = "Er mist data!!";
                }
                else
                {
                    tstlbl_waarschuwing.Text = "";
                }
                if(indata!=string.Empty)
                    blDataIsJuist = true;                               //aangeven dat data correct/gecorrigeerd is
            }
            catch (Exception)
            {
            }

            if (startMonitoring && blDataIsJuist)
            {
                blDataIsJuist = false;
                this.BeginInvoke(new EventHandler(delegate
                {
                    //lstRawData.Add(indata);                                         //put raw data in list
                    knoppen = calculateValueFromClockAndPutInLists(indata);         //calculate rawdata to int

                    if (lstBoxCounter > nudVertraging.Value)                        //delay form
                    {
                        if (cbxGrafiekAanUit.Checked)                               //tekenen
                            FillGraph(knoppen[0], knoppen[1]);                      //draw/fill graph

                        listBox.Items.Add(knoppen[0].ToString() + " " + knoppen[1].ToString());
                        listBox.Refresh();
                        lstBoxCounter = 0;
                        //laatste waardes laten zien
                        listBox.SelectedIndex = listBox.Items.Count - 1;
                        listBox.SelectedIndex = -1;
                    }
                    else
                    {
                        lstBoxCounter++;
                    }
                }));

                Application.DoEvents();
            }
        }

        private int[] calculateValueFromClockAndPutInLists(string strText)
        {
            int knopA = 0;              //tijdelijke opslagplaats waarde van Knop A
            bool knopAByteLow = true;   //test voor High of LowByte
            int knopB = 0;              //tijdelijke opslagplaats waarde van Knop A
            bool knopBByteLow = true;   //test voor High of LowByte
            string dataToTxtFile = null;
            string tab = "\t";

            foreach (char c in strText)
            {
                int tmp = c;
                if (sync == false) //testen op sync teken
                {
                    if (tmp == 0xA5)
                    {
                        sync = true;
                        sync_cnt = 0;
                    }
                    dataToTxtFile = max_bytes_cnt.ToString() + tab; //index meegeven in txt-file
                }
                else
                {
                    sync_cnt++;

                    // KNOP A, afhandelen
                    if (sync_cnt < 3)           //volgorde: sync, knopAByteHigh, knopAByteLow, knopBByteHigh, knopBByteLow
                    {
                        if (knopAByteLow)
                        {
                            knopA = tmp * 256;  //keer 8 bits, opschuiven naar links
                            knopAByteLow = false;
                        }
                        else
                        {
                            knopA += tmp;
                            dataToTxtFile += knopA.ToString() + tab; //data in string zetten voor txt-file
                        }
                    }

                    // KNOP B, afhandelen
                    else if (sync_cnt < 5)
                    {
                        if (knopBByteLow)
                        {
                            knopB = tmp * 256;  //keer 8 bits, opschuiven naar links
                            knopBByteLow = false;
                        }
                        else
                        {
                            knopB += tmp;

                            dataToTxtFile += knopB.ToString() + tab;    //data in string zetten voor txt-fil
                            lstDataToTxtFile.Add(dataToTxtFile);        //data in list zetten.

                            max_bytes_cnt++;                            //index bijhouden binnengekomen waardes

                            //opnieuw instellen voor de volgende ronde
                            sync = false;
                            sync_cnt = 0;
                        }
                    }
                }
            }
            if (knopA == 0)
            {
                int i = knopA; //test of knopX 0 is
            }
            return new int[] { knopA, knopB };
        }

        private void FillGraph(int knopA, int knopB)
        {
            GraphPane myPane = zg1.GraphPane;
            telAantalKeerTekenen++;
            listA.Add(telAantalKeerTekenen, knopA);
            listB.Add(telAantalKeerTekenen, knopB);

            //X-as
            if (telAantalKeerTekenen >= myPane.XAxis.Scale.Max && cbxGrafiekAanUit.Checked)
            {
                myPane.XAxis.Scale.Max++;
                myPane.XAxis.Scale.Min++;
            }
            //Y-as
            if (knopB > maxYScale || knopA > maxYScale)
            {
                int max = Math.Max(knopA, knopB);
                myPane.YAxis.Scale.Max = max;
                myPane.Y2Axis.Scale.Max = max;
                nudGrafiekYmax.Value = max;
                maxYScale = max;
            }
            redrawGraph();
        }

        private void comPortsList_SelectedValueChanged(object sender, EventArgs e)
        {
            if (comPortsList.SelectedIndex != -1)
            {
                connectButton.Enabled = true;
            }
        }

        private void redrawGraph()
        {
            GraphPane myPane = zg1.GraphPane;

            myPane.YAxis.Scale.Max = (double)nudGrafiekYmax.Value;
            myPane.YAxis.Scale.Min = 0;

            //myPane.XAxis.Scale.Max = (double)nudGrafiekXmax.Value;
            //myPane.XAxis.Scale.Min = 0;

            myPane.AxisChange();
            zg1.Invalidate(); // Make sure the Graph gets redrawn
            zg1.Refresh();
        }

        private void UpDown_ValueChanged(object sender, EventArgs e)
        {
            GraphPane myPane = zg1.GraphPane;
            max_line_cnt = Convert.ToInt16(UpDown.Value);
            progressBar.Maximum = max_line_cnt;
            ///myPane.XAxis.Scale.Max = max_line_cnt;
            myPane.AxisChange();
        }

        private void save_Click(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            foreach (string item in lstDataToTxtFile)
            {
                sb.AppendLine(item);
            }
            string data = sb.ToString();

            //sb = new StringBuilder();
            //foreach (string item in lstRawData)
            //{
            //    sb.AppendLine(item);
            //}
            //string dataRaw = sb.ToString();

            if (saveFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                StreamWriter sr = File.CreateText(@saveFileDialog.FileName);
                sr.Write(data);
                sr.Close();
            }
            //if (saveFileDialog.ShowDialog() != DialogResult.Cancel)
            //{
            //    StreamWriter sr = File.CreateText(@saveFileDialog.FileName);
            //   sr.Write(dataRaw);
            //    sr.Close();
            //}
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            startStopMonitoring();
        }

        private void startStopMonitoring()
        {
            if (startMonitoring == false)
            {
                startMonitoring = true;
                startButton.Text = "Stop";
            }
            else
            {
                startMonitoring = false;
                startButton.Text = "Start";
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            lstDataToTxtFile.Clear();
            //lstRawData.Clear();
            try
            {
                comPort.Close();
            }
            catch (Exception)
            {
            }

            comPort = new SerialPort();
            telAantalKeerTekenen = 0; //voor grafiek, weer op nul zetten
            SetReset();
            redrawGraph();
            tryConnectToCOM();

        }

        private void tryConnectToCOM()
        {
            if (connectButton.Text == "Connect")
            {
                try
                {
                    connectButton.Text = "Disconnect";
                    comPort.BaudRate = 230400;// 115200;// 19200;// 460800;// 115200;
                    tstlb_BaudRate.Text = "Baudrate: " + comPort.BaudRate.ToString();
                    if (comPortsList.SelectedItems.Count == 0)
                    {
                        comPortsList.SetSelected(0, true);    //selecteer eerste item              
                    }
                    comPort.PortName = comPortsList.SelectedItem.ToString();
                    comPort.Encoding = Encoding.GetEncoding(28591);// Encoding.UTF8;              
                    comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
                    comPort.Open();

                    //succesvol, gelijk starten
                    SetReset();
                    startStopMonitoring();
                }
                catch (Exception)
                {
                    MessageBox.Show("Geen verbinding met COM-poort kunnen maken");
                }
            }
            else
            {
                connectButton.Text = "Connect";
                comPort.Close();
            }
        }
    }
}
