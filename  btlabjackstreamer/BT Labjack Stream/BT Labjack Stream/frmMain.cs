using System;
using System.Threading;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using LabJack.LabJackUD;

namespace BT_Labjack_Stream
{
    public partial class frmMain : Form
    {
        #region VARIABELEN
        private U3 u3;
        private Thread streamThread;// Thread variable
        private LJUD.IO ioType = 0;
        private LJUD.CHANNEL channel = 0;
        private double dblValue = 0, dblCommBacklog = 0, dblUDBacklog;
        private double numScans = 3000;  //2x the expected # of scans (2*metingInfo.sampleFrequentie*delayms/1000)
        private double numScansRequested;
        private double[] adblData = new double[6000];  //Max buffer size (#channels*numScansRequested)
        private bool streamRunning = false;
        private double[] dummyDoubleArray = { 0 };
        private int dummyInt = 0;
        private double dummyDouble = 0;
        private const int aantalKanalen = 8;
        private List<double>[] dataChannel = null; //plek voor data
        private ExportData fh = null;
        private static frmExpertSettings frmExpert = null;
        private static frmInfo frmInformatie = null;
        private static frmGraph frmGrafiek = null;
        private string[] textBoxesMain = null;
        private const double msec = 1000.0;
        private bool blLabjackHV = true;
        private bool blNieuweData = false;
        #region STRUCTS
        public struct metingInformatie
        {
            public int instellingAnalogeKanalen;
            public int instellingDigitaleKanalen;
            public int sampleFrequentie;
            public bool blMarker;
            public Int16 aantalGeselecteerdeAnalogeKanalen;
            public Int16 aantalGeselecteerdeKanalen;
            public bool[] blIsHetAnalogeKanaalGeselecteerd;
            public Int16 aantalGeselecteerdeDigitaleKanalen;
            public bool[] blIsHetDigitaleKanaalGeselecteerd;
            public bool[] blIsHetKanaalGeselecteerd;
            public int delayms;
            public int aantalKanalenDieMoetenWordenOpgeslagen;
        };
        public struct expertInformatie
        {
            public bool[] cbxDifferentiaal;
            public bool[] cbxDigitaal;
            public int[] diffChannel;
        }
        private expertInformatie expertInfo;
        private metingInformatie metingInfo;
        #endregion

        // Create thread delegate
        delegate void BacklogParameterDelegate(double udBacklog, double commBacklog);
        delegate void ErrorParameterDelegate(LabJackUDException e);
        delegate void ReadingsParameterDelegate(double[] readings);
        #endregion

        public frmMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            connectToLabjack();
            frmExpert = new frmExpertSettings(expertSettingsToolStripMenuItem);
            frmInformatie = new frmInfo();
            refreshSettings();
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            // If we are already streaming, end the stream and thread
            if (streamRunning)
            {
                instellingenToolStripMenuItem.Enabled = true;
                gbxInstellingen.Enabled = true;
                // Tell the user that we are busy because
                // we have to wait for the thread to close
                btnStartStop.Text = "Wachten...";
                btnStartStop.Enabled = false;
                Update();

                // Stop the stream
                streamRunning = false;
                streamThread.Join(); // Wait for the thread to close
                stopStreaming();

                //stop grafiek
                frmGrafiek.blMagVanMain = false;
                frmGrafiek.Reset();

                // Reconfigure start button
                btnStartStop.Text = "Start";
                btnStartStop.Enabled = true;

                //set grafiek tool strip menu 
                 grafiekToolStripMenuItem.Checked = frmGrafiek.Visible;
            }

            // Otherwise, start the stream and thread
            else
            {
                refreshSettings();
                instellingenToolStripMenuItem.Enabled = false;
                //grafiek form opnieuw instellen
                if (frmGrafiek != null)
                    frmGrafiek.Close();
                frmGrafiek = new frmGraph(metingInfo.aantalGeselecteerdeKanalen, ref metingInfo.sampleFrequentie, ref dataChannel, ref metingInfo.delayms,
                    metingInfo.blIsHetKanaalGeselecteerd);
                //grafiek mag worden getekend
                frmGrafiek.blMagVanMain = true;
                //grafiek laten zien
                if (grafiekToolStripMenuItem.Checked)
                    frmGrafiek.Show();

                // Set up the stream
                if (metingInfo.aantalGeselecteerdeKanalen == 0)
                    MessageBox.Show("Geen kanalen geselecteerd.");
                else if (u3 != null  && StartStreaming())
                {
                    streamRunning = true;
                    gbxInstellingen.Enabled = false;

                    // Start stream thread
                    streamThread = new Thread(new ThreadStart(MakeReadings));
                    streamThread.Priority = ThreadPriority.AboveNormal;
                    streamThread.IsBackground = false;
                    streamThread.Start();

                    // Reconfigure start button
                    btnStartStop.Text = "Stop";
                }
            }
        }

        /// <summary>
        /// Actually stops the stream on the LabJack
        /// </summary>
        private void stopStreaming()
        {
            //Stop the stream
            try
            {
                if (u3 != null)
                    LJUD.ePut(u3.ljhandle, LJUD.IO.STOP_STREAM, 0, 0, 0);
                    //LJUD.eGet(u3.ljhandle, LJUD.IO.STOP_STREAM, 0, ref dummyDouble, dummyDoubleArray);
            }
            catch (LabJackUDException e)
            {
                ShowErrorMessage(e);
                return;
            }
        }

        /// <summary>
        /// Configure and start the stream on the LabJack
        /// </summary>
        /// <returns>True if successful and false otherwise</returns>
        private bool StartStreaming()
        {
            try
            {
                //Start by using the pin_configuration_reset IOType so that all
                //pin assignments are in the factory default condition.
                LJUD.ePut(u3.ljhandle, LJUD.IO.PIN_CONFIGURATION_RESET, 0, 0, 0);

                //Configure FIO0 and FIO1 as analog, all else as digital.  That means we
                //will start from channel 0 and update all 16 flexible bits.  We will
                //pass a value of b0000000000000011 or d3.
                LJUD.ePut(u3.ljhandle, LJUD.IO.PUT_ANALOG_ENABLE_PORT, 0, metingInfo.instellingAnalogeKanalen, 16);

                //Configure the stream:
                //Set the scan rate.
                LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.STREAM_SCAN_FREQUENCY, metingInfo.sampleFrequentie, 0, 0);

                //Give the driver a 5 second buffer (metingInfo.sampleFrequentie * 2 channels * 5 seconds).
                LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.STREAM_BUFFER_SIZE, metingInfo.sampleFrequentie * metingInfo.aantalGeselecteerdeKanalen * 5, 0, 0);

                //Configure reads to retrieve whatever data is available without waiting (wait mode LJUD.STREAMWAITMODES.NONE).
                //See comments below to change this program to use LJUD.STREAMWAITMODES.SLEEP mode.
                LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.STREAM_WAIT_MODE, (double)LJUD.STREAMWAITMODES.NONE, 0, 0);

                ///Toevoegen van channels
                LJUD.AddRequest(u3.ljhandle, LJUD.IO.CLEAR_STREAM_CHANNELS, 0, 0, 0, 0);

                #region KANALEN_INSTELLEN
                for (int i = 0; i < aantalKanalen; i++)
                {
                    //De analoge kanalen instellen
                    if (metingInfo.blIsHetAnalogeKanaalGeselecteerd[i]
                        && !expertInfo.cbxDigitaal[i] && !expertInfo.cbxDifferentiaal[i]) //alleen analoog als niet (digitaal & diff)
                    {
                        LJUD.AddRequest(u3.ljhandle, LJUD.IO.ADD_STREAM_CHANNEL, i, 0, 0, 0);
                    }

                    if (expertSettingsToolStripMenuItem.Checked)
                    {
                        //differentieel kanalen
                        if (expertInfo.cbxDifferentiaal[i])
                            LJUD.AddRequest(u3.ljhandle, LJUD.IO.ADD_STREAM_CHANNEL_DIFF, i, 0, expertInfo.diffChannel[i], 0);

                        ////digitaal kanalen instellen (sowieso als expert settings is ingesteld)
                        //LJUD.AddRequest(u3.ljhandle, LJUD.IO.ADD_STREAM_CHANNEL, 193, 0, 0, 0); //193, digitaal 
                    }
                }
                //De digitale kanalen instellen
                LJUD.AddRequest(u3.ljhandle, LJUD.IO.ADD_STREAM_CHANNEL, 193, 0, 0, 0);

                #endregion

                //Execute the list of requests.
                LJUD.GoOne(u3.ljhandle);
            }
            catch (LabJackUDException e)
            {
                ShowErrorMessage(e);
                return false;
            }

            //Get all the results just to check for errors.
            try { LJUD.GetFirstResult(u3.ljhandle, ref ioType, ref channel, ref dblValue, ref dummyInt, ref dummyDouble); }
            catch (LabJackUDException e) { ShowErrorMessage(e); }
            bool finished = false;
            while (!finished)
            {
                try { LJUD.GetNextResult(u3.ljhandle, ref ioType, ref channel, ref dblValue, ref dummyInt, ref dummyDouble); }
                catch (LabJackUDException e)
                {
                    // If we get an error, report it.  If the error is NO_MORE_DATA_AVAILABLE we are done
                    if (e.LJUDError == U3.LJUDERROR.NO_MORE_DATA_AVAILABLE)
                        finished = true;
                    else
                        ShowErrorMessage(e);
                }
            }

            //Start the stream.
            try { LJUD.eGet(u3.ljhandle, LJUD.IO.START_STREAM, 0, ref dblValue, 0); }
            catch (LabJackUDException e)
            {
                ShowErrorMessage(e);
                return false;
            }

            //The actual scan rate is dependent on how the desired scan rate divides into
            //the LabJack clock.  The actual scan rate is returned in the value parameter
            //from the start stream command.
            //tstbSampleFrequentie.Text = String.Format("{0:0.000}", dblValue);
            ///sampleDisplay.Text = String.Format("{0:0.000}", 2 * dblValue); // # channels * scan rate

            // The stream started successfully
            return true;
        }

        /// <summary>
        /// Read from the stream
        /// </summary>
        private void MakeReadings()
        {
            while (streamRunning)
            {
                //Since we are using wait mode LJUD.STREAMWAITMODES.NONE, we will wait a little, then
                //read however much data is available.  Thus this delay will control how
                //fast the program loops and how much data is read each loop.  An
                //alternative common method is to use wait mode LJUD.STREAMWAITMODES.SLEEP where the
                //stream read waits for a certain number of scans.  In such a case
                //you would not have a delay here, since the stream read will actually
                //control how fast the program loops.
                //
                //To change this program to use sleep mode,
                //	-change numScans to the actual number of scans desired per read,
                //	-change wait mode addrequest value to LJUD.STREAMWAITMODES.SLEEP,
                //	-comment out the following Thread.Sleep command.

                Thread.Sleep(metingInfo.delayms);	//Remove if using LJUD.STREAMWAITMODES.SLEEP.

                //init array so we can easily tell if it has changed
                for (int i = 0; i < numScans * 2; i++)
                {
                    adblData[i] = 9999.0;
                }

                try
                {
                    //Read the data.  We will request twice the number we expect, to
                    //make sure we get everything that is available.
                    //Note that the array we pass must be sized to hold enough SAMPLES, and
                    //the Value we pass specifies the number of SCANS to read.
                    numScansRequested = numScans;
                    LJUD.eGet(u3.ljhandle, LJUD.IO.GET_STREAM_DATA, LJUD.CHANNEL.ALL_CHANNELS, ref numScansRequested, adblData);
                    ShowReadings(adblData);
                    saveDataInLists(adblData);
                    frmGrafiek.blNieuweData = true;
                    //Retrieve the current backlog.  The UD driver retrieves stream data from
                    //the U3 in the background, but if the computer is too slow for some reason
                    //the driver might not be able to read the data as fast as the U3 is
                    //acquiring it, and thus there will be data left over in the U3 buffer.
                    //Likewise, if this program can not retrieve data quickly enough, the UD driver's
                    //own backlog will fill.
                    LJUD.eGet(u3.ljhandle, LJUD.IO.GET_CONFIG, LJUD.CHANNEL.STREAM_BACKLOG_COMM, ref dblCommBacklog, 0);
                    LJUD.eGet(u3.ljhandle, LJUD.IO.GET_CONFIG, LJUD.CHANNEL.STREAM_BACKLOG_UD, ref dblUDBacklog, 0);
                    DisplayBacklog(dblUDBacklog, dblCommBacklog);
                }
                catch (LabJackUDException e)
                {
                    ShowErrorMessage(e);
                    return;
                }
            }
        }

        /// <summary>
        /// Display the driver and comm backlog on the GUI in a thread safe way
        /// </summary>
        /// <param name="udBacklog">The backlog reported by the UD driver</param>
        /// <param name="commBacklog">The backlog reported by the device driver</param>
        private void DisplayBacklog(double udBacklog, double commBacklog)
        {
            //// If we are not in the GUI thread, switch to GUI thread and run again
            //if (InvokeRequired)
            //    BeginInvoke(new BacklogParameterDelegate(DisplayBacklog), new object[] { udBacklog, commBacklog });
            //else
            //{
            //    driverBacklogDisplay.Text = String.Format("{0:0.000}", udBacklog);
            //    commBacklogDisplay.Text = String.Format("{0:0.000}", commBacklog);
            //}
        }

        /// <summary>
        /// Displays the error on the GUI in a label in a thread safe way
        /// </summary>
        /// <param name="e">The exception encountered</param>
        private void ShowErrorMessage(LabJackUDException e)
        {
            // If we are not in the GUI thread, switch to GUI thread and run again
            if (InvokeRequired)
                BeginInvoke(new ErrorParameterDelegate(ShowErrorMessage), new object[] { e });
            else
            {
                MessageBox.Show(e.ToString());

                // Stop streaming if we are still streaming
                if (streamRunning)
                {
                    streamRunning = false;
                    stopStreaming();
                    btnStartStop.Text = "Start";
                    gbxInstellingen.Enabled = true;
                    frmExpert.groupBoxExpertSettings = true;
                }
            }
        }

        /// <summary>
        /// Show the readings for AIN0 and AIN1 in a thread safe way
        /// </summary>
        /// <param name="readings">The array of readings returned from the driver</param>
        private void ShowReadings(double[] readings)
        {
            // If we are not in the GUI thread, switch to GUI thread and run again
            if (InvokeRequired)
                BeginInvoke(new ReadingsParameterDelegate(ShowReadings), new object[] { readings });
            else
            {
                //stop alle waardes/tekst in textBoxesMain en schrijf onderaan alles naar textBoxes
                textBoxesMain = new string[aantalKanalen];

                int tellerReading = 0;              
                for (int i = 0; i < aantalKanalen; i++)
                {
                    //analoge kanalen en differentiele kanalen
                    if (metingInfo.blIsHetAnalogeKanaalGeselecteerd[i] && !metingInfo.blIsHetDigitaleKanaalGeselecteerd[i])
                    {
                        textBoxesMain[i] = readings[tellerReading].ToString();
                        tellerReading++;
                    }
                }

                //digitale kanalen verwerken
                int[] digKanTemp = verkrijgBits(readings[tellerReading]);
                for (int i = 0; i < aantalKanalen; i++)
                {
                    if (metingInfo.blIsHetDigitaleKanaalGeselecteerd[i])
                        textBoxesMain[i] = digKanTemp[i].ToString();
                }

                //update tekst
                tbxFIO0.Text = textBoxesMain[0];
                tbxFIO1.Text = textBoxesMain[1];
                tbxFIO2.Text = textBoxesMain[2];
                tbxFIO3.Text = textBoxesMain[3];
                tbxFIO4.Text = textBoxesMain[4];
                tbxFIO5.Text = textBoxesMain[5];
                tbxFIO6.Text = textBoxesMain[6];
                tbxFIO7.Text = textBoxesMain[7];

                ////grafiek
                //if (grafiekToolStripMenuItem.Checked)
                //{
                //    frmGrafiek.FillGraph(readings[0], readings[0]);
                //}
            }
        }

        private void saveDataInLists(double[] data)
        {
            //Schrijf waardes in Lists
            for (int i = 0; i < (metingInfo.aantalGeselecteerdeAnalogeKanalen * metingInfo.sampleFrequentie
                * (metingInfo.delayms / msec)); i = i + metingInfo.aantalGeselecteerdeAnalogeKanalen+1) //+1 voor digitale kanalen
            {
                //analoog en differentieel opslaan
                int tellerAnalogeKanalen = 0;
                for (int k = 0; k < aantalKanalen; k++) //door kanalen lopen
                {
                    if (metingInfo.blIsHetAnalogeKanaalGeselecteerd[k] && !metingInfo.blIsHetDigitaleKanaalGeselecteerd[k]) //alleen analoge of differntiele kanalen opslaan
                    {
                        dataChannel[k].Add(data[i + tellerAnalogeKanalen]);
                        tellerAnalogeKanalen++;
                    }
                }

                //Daarna digitale kanalen verwerken
                int[] digKanTemp = verkrijgBits(data[i + tellerAnalogeKanalen]);
                for (int k = 0; k < aantalKanalen; k++)
                {
                    if (metingInfo.blIsHetDigitaleKanaalGeselecteerd[k])
                        dataChannel[k].Add(digKanTemp[k]);
                }
                
            }
        }

        private void resetLabjack()
        {
            tsslbl_Error.Text = "";

            if (u3 != null)
            {
                LJUD.ResetLabJack(u3.ljhandle);
                MessageBox.Show("Wacht a.u.b. op het bericht:" + " Verbonden met de Labjack ");
                Thread.Sleep(5000);

                u3 = null;
                btnStartStop.Text = "Start";
            }
        }

        private void connectToLabjack()
        {
            int teller = 0;

            //Maak verbinding met de Labjack
            tsslbl_Status.Text = " Verbonden met de Labjack ";
            try
            {
                while (u3 == null && teller++ < 1000000)
                {
                    u3 = new U3(LJUD.CONNECTION.USB, "0", true); //Probeer direct weer te verbinden
                }
            }
            catch (LabJackUDException e) //Reset en probeer opnieuw
            {
                ShowErrorMessage(e);
                try
                {
                    resetLabjack();
                    while (u3 == null && teller++ < 1000000)
                    {
                        u3 = new U3(LJUD.CONNECTION.USB, "0", true); //Probeer direct weer te verbinden
                    }
                }
                catch (Exception)
                {
                    tsslbl_Status.Text = "Geen Labjack aangesloten ";
                }
            }

            if (u3 != null) //test voor labjack aanwezigheid
            {
                //Read and display the hardware version of this U3.
                LJUD.eGet(u3.ljhandle, LJUD.IO.GET_CONFIG, LJUD.CHANNEL.SERIAL_NUMBER, ref dblValue, 0);
                serienummerToolStripMenuItem1.Text = "Serienummer: " + String.Format("{0:0}", dblValue);

                //Read and display the hardware version of this U3.
                LJUD.eGet(u3.ljhandle, LJUD.IO.GET_CONFIG, LJUD.CHANNEL.HARDWARE_VERSION, ref dblValue, 0);
                hardwareVersieToolStripMenuItem.Text = "Hardware versie: " + String.Format("{0:0.000}", dblValue);

                //Read and display the firmware version of this U3.
                LJUD.eGet(u3.ljhandle, LJUD.IO.GET_CONFIG, LJUD.CHANNEL.FIRMWARE_VERSION, ref dblValue, 0);
                firmwareVersieToolStripMenuItem.Text = "Firmware versie:" + String.Format("{0:0.000}", dblValue);

                //Hv or Lv
                LJUD.eGet(u3.ljhandle, LJUD.IO.GET_CONFIG, LJUD.CHANNEL.U3HV, ref dblValue, 0);
                if (dblValue >= 1.0)
                    tsmi_LabjackHV.Text = "U3-HV (High Voltage, 0-10 V)";
                else
                {
                    tsmi_LabjackHV.Text = "U3-LV (Low Voltage)";
                    blLabjackHV = false;
                }
            }
        }

        private void refreshSettings()
        {
            if (streamRunning)
            {
                instellingenToolStripMenuItem.Enabled = false;
                return;
            }
            instellingenToolStripMenuItem.Enabled = true;

            //RESET instelling te meten kanalen
           // metingInfo = new metingInformatie(); //mag niet dan werkt het niet meer
            metingInfo.blIsHetAnalogeKanaalGeselecteerd = new bool[aantalKanalen];
            metingInfo.blIsHetDigitaleKanaalGeselecteerd = new bool[aantalKanalen];
            metingInfo.blIsHetKanaalGeselecteerd = new bool[aantalKanalen];
            metingInfo.aantalGeselecteerdeAnalogeKanalen = 0;
            metingInfo.aantalGeselecteerdeDigitaleKanalen = 0;
            metingInfo.aantalGeselecteerdeKanalen = 0;
            metingInfo.instellingAnalogeKanalen = 0;
            metingInfo.instellingDigitaleKanalen = 0;
            metingInfo.aantalKanalenDieMoetenWordenOpgeslagen = 0;

            //instellen Expert settings
            if (expertSettingsToolStripMenuItem.Checked)
            {
                frmExpert.EnableFIO0 = cbxFIO0.Checked;
                frmExpert.EnableFIO1 = cbxFIO1.Checked;
                frmExpert.EnableFIO2 = cbxFIO2.Checked;
                frmExpert.EnableFIO3 = cbxFIO3.Checked;
                frmExpert.EnableFIO4 = cbxFIO4.Checked;
                frmExpert.EnableFIO5 = cbxFIO5.Checked;
                frmExpert.EnableFIO6 = cbxFIO6.Checked;
                frmExpert.EnableFIO7 = cbxFIO7.Checked;
            }

            //set INFO meting struct
            expertInfo.cbxDifferentiaal = new bool[aantalKanalen];
            expertInfo.cbxDigitaal = new bool[aantalKanalen];
            expertInfo.diffChannel = new int[aantalKanalen];

            //verkrijg instellinge expert form
            expertInfo.cbxDifferentiaal[0] = frmExpert.cbx_Differentiaal_FIO0;  //diff
            expertInfo.cbxDifferentiaal[1] = frmExpert.cbx_Differentiaal_FIO1;
            expertInfo.cbxDifferentiaal[2] = frmExpert.cbx_Differentiaal_FIO2;
            expertInfo.cbxDifferentiaal[3] = frmExpert.cbx_Differentiaal_FIO3;
            expertInfo.cbxDifferentiaal[4] = frmExpert.cbx_Differentiaal_FIO4;
            expertInfo.cbxDifferentiaal[5] = frmExpert.cbx_Differentiaal_FIO5;
            expertInfo.cbxDifferentiaal[6] = frmExpert.cbx_Differentiaal_FIO6;
            expertInfo.cbxDifferentiaal[7] = frmExpert.cbx_Differentiaal_FIO7;
            expertInfo.cbxDigitaal[0] = frmExpert.cbx_Digitaal_FIO0;            //digitaal, eerste vier kanalen kunnen niet digitaal worden gebruikt
            expertInfo.cbxDigitaal[1] = frmExpert.cbx_Digitaal_FIO1;
            expertInfo.cbxDigitaal[2] = frmExpert.cbx_Digitaal_FIO2;
            expertInfo.cbxDigitaal[3] = frmExpert.cbx_Digitaal_FIO3;
            expertInfo.cbxDigitaal[4] = frmExpert.cbx_Digitaal_FIO4;
            expertInfo.cbxDigitaal[5] = frmExpert.cbx_Digitaal_FIO5;
            expertInfo.cbxDigitaal[6] = frmExpert.cbx_Digitaal_FIO6;
            expertInfo.cbxDigitaal[7] = frmExpert.cbx_Digitaal_FIO7;
            expertInfo.diffChannel[0] = Convert.ToInt16(frmExpert.combxFIO0);   //kanaal
            expertInfo.diffChannel[1] = Convert.ToInt16(frmExpert.combxFIO1);
            expertInfo.diffChannel[2] = Convert.ToInt16(frmExpert.combxFIO2);
            expertInfo.diffChannel[3] = Convert.ToInt16(frmExpert.combxFIO3);
            expertInfo.diffChannel[4] = Convert.ToInt16(frmExpert.combxFIO4);
            expertInfo.diffChannel[5] = Convert.ToInt16(frmExpert.combxFIO5);
            expertInfo.diffChannel[6] = Convert.ToInt16(frmExpert.combxFIO6);
            expertInfo.diffChannel[7] = Convert.ToInt16(frmExpert.combxFIO7);

            //Loop alle instellingen door en pas daarbij de later benodigde variabelen aan
            #region INSTELLINGEN
            if (cbxFIO0.Checked)
            {
                if (expertSettingsToolStripMenuItem.Checked && frmExpert.cbx_Digitaal_FIO0) //digitaal
                {
                    metingInfo.instellingDigitaleKanalen += 1;
                    metingInfo.blIsHetDigitaleKanaalGeselecteerd[0] = true; //
                    metingInfo.aantalGeselecteerdeDigitaleKanalen++;
                }
                else //analoog
                {
                    metingInfo.instellingAnalogeKanalen += 1;
                    metingInfo.blIsHetAnalogeKanaalGeselecteerd[0] = true; //
                    metingInfo.aantalGeselecteerdeAnalogeKanalen++;
                }
                metingInfo.blIsHetKanaalGeselecteerd[0] = true;
                metingInfo.aantalGeselecteerdeKanalen++;
                            }

            if (cbxFIO1.Checked)
            {
                if (expertSettingsToolStripMenuItem.Checked && frmExpert.cbx_Digitaal_FIO1) //digitaal
                {
                    metingInfo.instellingDigitaleKanalen += 2;
                    metingInfo.blIsHetDigitaleKanaalGeselecteerd[1] = true; //
                    metingInfo.aantalGeselecteerdeDigitaleKanalen++;
                }
                else //analoog
                {
                    metingInfo.instellingAnalogeKanalen += 2;
                    metingInfo.blIsHetAnalogeKanaalGeselecteerd[1] = true; //
                    metingInfo.aantalGeselecteerdeAnalogeKanalen++;
                }
                metingInfo.blIsHetKanaalGeselecteerd[1] = true;
                metingInfo.aantalGeselecteerdeKanalen++;
            }

            if (cbxFIO2.Checked)
            {
                if (expertSettingsToolStripMenuItem.Checked && frmExpert.cbx_Digitaal_FIO2) //digitaal
                {
                    metingInfo.instellingDigitaleKanalen += 4;
                    metingInfo.blIsHetDigitaleKanaalGeselecteerd[2] = true; //
                    metingInfo.aantalGeselecteerdeDigitaleKanalen++;
                }
                else //analoog
                {
                    metingInfo.instellingAnalogeKanalen += 4;
                    metingInfo.blIsHetAnalogeKanaalGeselecteerd[2] = true; //
                    metingInfo.aantalGeselecteerdeAnalogeKanalen++;
                }
                metingInfo.blIsHetKanaalGeselecteerd[2] = true;
                metingInfo.aantalGeselecteerdeKanalen++;
            }

            if (cbxFIO3.Checked)
            {
                if (expertSettingsToolStripMenuItem.Checked && frmExpert.cbx_Digitaal_FIO3) //digitaal
                {
                    metingInfo.instellingDigitaleKanalen += 8;
                    metingInfo.blIsHetDigitaleKanaalGeselecteerd[3] = true; //
                    metingInfo.aantalGeselecteerdeDigitaleKanalen++;
                }
                else //analoog
                {
                    metingInfo.instellingAnalogeKanalen += 8;
                    metingInfo.blIsHetAnalogeKanaalGeselecteerd[3] = true; //
                    metingInfo.aantalGeselecteerdeAnalogeKanalen++;
                }
                metingInfo.blIsHetKanaalGeselecteerd[3] = true;
                metingInfo.aantalGeselecteerdeKanalen++;
            }

            if (cbxFIO4.Checked)
            {
                if (expertSettingsToolStripMenuItem.Checked && frmExpert.cbx_Digitaal_FIO4) //digitaal
                {
                    metingInfo.instellingDigitaleKanalen += 16;
                    metingInfo.blIsHetDigitaleKanaalGeselecteerd[4] = true; //
                    metingInfo.aantalGeselecteerdeDigitaleKanalen++;
                }
                else //analoog
                {
                    metingInfo.instellingAnalogeKanalen += 16;
                    metingInfo.blIsHetAnalogeKanaalGeselecteerd[4] = true; //
                    metingInfo.aantalGeselecteerdeAnalogeKanalen++;
                }
                metingInfo.blIsHetKanaalGeselecteerd[4] = true;
                metingInfo.aantalGeselecteerdeKanalen++;
            }

            if (cbxFIO5.Checked)
            {
                if (expertSettingsToolStripMenuItem.Checked && frmExpert.cbx_Digitaal_FIO5) //digitaal
                {
                    metingInfo.instellingDigitaleKanalen += 32;
                    metingInfo.blIsHetDigitaleKanaalGeselecteerd[5] = true; //
                    metingInfo.aantalGeselecteerdeDigitaleKanalen++;
                }
                else //analoog
                {
                    metingInfo.instellingAnalogeKanalen += 32;
                    metingInfo.blIsHetAnalogeKanaalGeselecteerd[5] = true; //
                    metingInfo.aantalGeselecteerdeAnalogeKanalen++;
                }
                metingInfo.blIsHetKanaalGeselecteerd[5] = true;
                metingInfo.aantalGeselecteerdeKanalen++;
            }

            if (cbxFIO6.Checked)
            {
                if (expertSettingsToolStripMenuItem.Checked && frmExpert.cbx_Digitaal_FIO6) //digitaal
                {
                    metingInfo.instellingDigitaleKanalen += 64;
                    metingInfo.blIsHetDigitaleKanaalGeselecteerd[6] = true; //
                    metingInfo.aantalGeselecteerdeDigitaleKanalen++;
                }
                else //analoog
                {
                    metingInfo.instellingAnalogeKanalen += 64;
                    metingInfo.blIsHetAnalogeKanaalGeselecteerd[6] = true; //
                    metingInfo.aantalGeselecteerdeAnalogeKanalen++;
                }
                metingInfo.blIsHetKanaalGeselecteerd[6] = true;
                metingInfo.aantalGeselecteerdeKanalen++;
            }

            if (cbxFIO7.Checked)
            {
                if (expertSettingsToolStripMenuItem.Checked && frmExpert.cbx_Digitaal_FIO7) //digitaal
                {
                    metingInfo.instellingDigitaleKanalen += 128;
                    metingInfo.blIsHetDigitaleKanaalGeselecteerd[7] = true; //
                    metingInfo.aantalGeselecteerdeDigitaleKanalen++;
                }
                else //analoog
                {
                    metingInfo.instellingAnalogeKanalen += 128;
                    metingInfo.blIsHetAnalogeKanaalGeselecteerd[7] = true; //
                    metingInfo.aantalGeselecteerdeAnalogeKanalen++;
                }
                metingInfo.blIsHetKanaalGeselecteerd[7] = true;
                metingInfo.aantalGeselecteerdeKanalen++;
            }
            #endregion

            //instellingen meting
            numScans = (2 * metingInfo.sampleFrequentie * metingInfo.delayms) / msec;
            metingInfo.sampleFrequentie = Convert.ToInt16(tscbxSampleFrequentie.Text);
            adblData = new double[metingInfo.aantalGeselecteerdeKanalen * (Int16)numScans * 2];
            dataChannel = new List<double>[aantalKanalen];

            for (int i = 0; i < aantalKanalen; i++) //prepareer juiste datalijsten
            {
                dataChannel[i] = new List<double>(120 * (int)metingInfo.sampleFrequentie * 2); //size 
            }

            //clear texboxes
            tbxFIO0.Clear();
            tbxFIO1.Clear();
            tbxFIO2.Clear();
            tbxFIO3.Clear();
            tbxFIO4.Clear();
            tbxFIO5.Clear();
            tbxFIO6.Clear();
            tbxFIO7.Clear();

            //set buffer grootte
            metingInfo.delayms = Convert.ToInt16(tscb_BufferGrootte.Text);

            //tekst groupboxes
            if (!blLabjackHV)
                gbxFIO0_3.Text = "0-2,4 Volt";
            else
                gbxFIO0_3.Text = "0-10 Volt";
        }

        #region CHECKBOXES
        private void cbxFIO0_CheckedChanged(object sender, EventArgs e)
        {
            frmExpert.EnableFIO0 = cbxFIO0.Checked;
        }

        private void cbxFIO1_CheckedChanged(object sender, EventArgs e)
        {
            frmExpert.EnableFIO1 = cbxFIO1.Checked;
        }

        private void cbxFIO2_CheckedChanged(object sender, EventArgs e)
        {
            frmExpert.EnableFIO2 = cbxFIO2.Checked;
        }

        private void cbxFIO3_CheckedChanged(object sender, EventArgs e)
        {
            frmExpert.EnableFIO3 = cbxFIO3.Checked;
        }

        private void cbxFIO4_CheckedChanged(object sender, EventArgs e)
        {
            frmExpert.EnableFIO4 = cbxFIO4.Checked;
        }

        private void cbxFIO5_CheckedChanged(object sender, EventArgs e)
        {
            frmExpert.EnableFIO5 = cbxFIO5.Checked;
        }

        private void cbxFIO6_CheckedChanged(object sender, EventArgs e)
        {
            frmExpert.EnableFIO6 = cbxFIO6.Checked;
        }

        private void cbxFIO7_CheckedChanged(object sender, EventArgs e)
        {
            frmExpert.EnableFIO7 = cbxFIO7.Checked;
        }
        #endregion

        private void expertSettingsToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            expertSettingsToolStripMenuItem.Checked = !expertSettingsToolStripMenuItem.Checked;
            if (expertSettingsToolStripMenuItem.Checked) //AAN
            {
                frmExpert.Show();
                refreshSettings();
            }
            else //UIT
            {
                frmExpert.Hide();
            }
        }

        private void expertSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toolStripComboBox1_TextChanged(object sender, EventArgs e)
        {
            refreshSettings();
        }

        private void metingOpslaanAlsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fh = new ExportData("BT Labjack Streamer");
            fh.SaveFile();

            ////geef benodigde data aan ExportData
            fh.AantalGeselecteerdeKanalen = metingInfo.aantalGeselecteerdeKanalen;
            fh.IsHetKanaalGeselecteerd = metingInfo.blIsHetKanaalGeselecteerd;
            fh.SampleFrequentie = metingInfo.sampleFrequentie;
            fh.LabjackID = serienummerToolStripMenuItem1.Text;

            fh.Export(dataChannel);
            fh = null;
        }

        private void creditzToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox ab = new AboutBox();
            ab.Show();
        }

        private void StreamForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // If we are streaming, we need to end the thread
            if (streamRunning)
            {
                streamRunning = false; // Get the thread to close
                stopStreaming();
                Visible = false; // Hide the window
                streamThread.Join(); // Wait for the thread to close
            }
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tsslbl_Status.Text = "Geen Labjack aangesloten ";
            resetLabjack();
            connectToLabjack();
            gbxInstellingen.Enabled = true;
        }

        private void instellingenAanuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            instellingenAanuitToolStripMenuItem.Checked = !instellingenAanuitToolStripMenuItem.Checked;
            gbxInstellingen.Visible = instellingenAanuitToolStripMenuItem.Checked;
        }

        private void tscbxSampleFrequentie_TextChanged(object sender, EventArgs e)
        {
            metingInfo.sampleFrequentie = Convert.ToInt16(tscbxSampleFrequentie.Text);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                LJUD.ePut(u3.ljhandle, LJUD.IO.STOP_STREAM, 0, 0, 0);
                LJUD.Close();
            }
            catch (LabJackUDException)
            {
            }
        }

        private void AlleKanalenAanUit_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AlleKanalenAanUit_ToolStripMenuItem.Checked = !AlleKanalenAanUit_ToolStripMenuItem.Checked;
            cbxFIO0.Checked = AlleKanalenAanUit_ToolStripMenuItem.Checked;
            cbxFIO1.Checked = AlleKanalenAanUit_ToolStripMenuItem.Checked;
            cbxFIO2.Checked = AlleKanalenAanUit_ToolStripMenuItem.Checked;
            cbxFIO3.Checked = AlleKanalenAanUit_ToolStripMenuItem.Checked;
            cbxFIO4.Checked = AlleKanalenAanUit_ToolStripMenuItem.Checked;
            cbxFIO5.Checked = AlleKanalenAanUit_ToolStripMenuItem.Checked;
            cbxFIO6.Checked = AlleKanalenAanUit_ToolStripMenuItem.Checked;
            cbxFIO7.Checked = AlleKanalenAanUit_ToolStripMenuItem.Checked;
        }

        private int[] verkrijgBits(double waarde)
        {
            double w = waarde - 65280; //eerste byte hebben we niets aan
            int[] b = new int[aantalKanalen];
            for (int i = 7; 0 < i ; i--)
            {
                double temp = Math.Pow(2,i);
                if (w >= temp)  //1
                {
                    b[i] = 1;
                    w -= temp;
                }
                else            //0
                    b[i] = 0;
            }
            return b;
        }

        private void tsmi_MeerInformatie_Click(object sender, EventArgs e)
        {
            frmInformatie.Show();
        }

        private void grafiekToolStripMenuItem_Click(object sender, EventArgs e)
        {
            grafiekToolStripMenuItem.Checked = !grafiekToolStripMenuItem.Checked;
            if (grafiekToolStripMenuItem.Checked)
                frmGrafiek.Show();
            else
                frmGrafiek.Hide();
        }

        public bool ToolStripMenuItem_Grafiek_aan
        {
            set { grafiekToolStripMenuItem.Checked = value; }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If we are already streaming, end the stream and thread
            if (streamRunning)
            {
                gbxInstellingen.Enabled = true;
                // Tell the user that we are busy because
                // we have to wait for the thread to close
                btnStartStop.Text = "Wachten...";
                btnStartStop.Enabled = false;
                Update();

                // Stop the stream
                streamRunning = false;
                streamThread.Join(); // Wait for the thread to close
                stopStreaming();

                //stop grafiek
                frmGrafiek.blMagVanMain = false;
                frmGrafiek.Reset();

                // Reconfigure start button
                btnStartStop.Text = "Start";
                btnStartStop.Enabled = true;

                //set grafiek tool strip menu 
                grafiekToolStripMenuItem.Checked = frmGrafiek.Visible;
            }
        }
        //EINDE KLASSE
    }
}
