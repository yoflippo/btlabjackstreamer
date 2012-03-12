using System;
using System.Threading;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

// Import the UD .NET wrapper object.  The dll referenced is installed by the
// LabJackUD installer.
using LabJack.LabJackUD;

namespace BT_Labjack_Stream
{
    public partial class Form1 : Form
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
        public struct metingInformatie
        {
            public int instellingAnalogeKanalen;
            public int sampleFrequentie;
            public bool blMarker;
            public Int16 aantalGeselecteerdeKanalen;
            public bool[] blIsHetKanaalGeselecteerd;
            public int delayms;
        };
        private metingInformatie metingInfo;

        // Create thread delegate
        delegate void BacklogParameterDelegate(double udBacklog, double commBacklog);
        delegate void ErrorParameterDelegate(LabJackUDException e);
        delegate void ReadingsParameterDelegate(double[] readings);
        #endregion

        public Form1()
        {
            InitializeComponent();
            
            //set INFO struct
            metingInfo.instellingAnalogeKanalen = 3;
            metingInfo.sampleFrequentie = 500;
            metingInfo.blMarker = false;
            metingInfo.aantalGeselecteerdeKanalen = 0;
            metingInfo.blIsHetKanaalGeselecteerd = new bool[aantalKanalen];
            metingInfo.delayms = 1000;

            refreshSettings();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            connectToLabjack();
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            // If we are already streaming, end the stream and thread
            if (streamRunning)
            {
                gbxInstellingen.Enabled = true;
                // Tell the user that we are busy because
                // we have to wait for the thread to close
                btnStartStop.Text = "Please wait...";
                btnStartStop.Enabled = false;
                Update();

                // Stop the stream
                streamRunning = false;
                streamThread.Join(); // Wait for the thread to close
                StopStreaming();

                // Reconfigure start button
                btnStartStop.Text = "Start";
                btnStartStop.Enabled = true;
            }

            // Otherwise, start the stream and thread
            else
            {
                refreshSettings();
                // Set up the stream
                if (u3 != null && StartStreaming())
                {
                    streamRunning = true;
                    gbxInstellingen.Enabled = false;

                    // Start stream thread
                    streamThread = new Thread(new ThreadStart(MakeReadings));
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
        private void StopStreaming()
        {
            //Stop the stream
            try
            {
                if (u3 != null)
                    LJUD.eGet(u3.ljhandle, LJUD.IO.STOP_STREAM, 0, ref dummyDouble, dummyDoubleArray);
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

                //Toevoegen van channels
                LJUD.AddRequest(u3.ljhandle, LJUD.IO.CLEAR_STREAM_CHANNELS, 0, 0, 0, 0);

                for (int i = 0; i < aantalKanalen; i++)
                {
                    if (metingInfo.blIsHetKanaalGeselecteerd[i])
                    {
                        LJUD.AddRequest(u3.ljhandle, LJUD.IO.ADD_STREAM_CHANNEL, i, 0, 0, 0);
                    }
                }

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
                    StopStreaming();
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
                //Schrijf waardes naar form
                int i = 0;
                if (metingInfo.blIsHetKanaalGeselecteerd[0] && i < metingInfo.aantalGeselecteerdeKanalen)
                {
                    tbxFIO0.Text = readings[i].ToString();
                    i++;
                }
                if (metingInfo.blIsHetKanaalGeselecteerd[1] && i < metingInfo.aantalGeselecteerdeKanalen)
                {
                    tbxFIO1.Text = readings[i].ToString();
                    i++;
                }
                if (metingInfo.blIsHetKanaalGeselecteerd[2] && i < metingInfo.aantalGeselecteerdeKanalen)
                {
                    tbxFIO2.Text = readings[i].ToString();
                    i++;
                }
                if (metingInfo.blIsHetKanaalGeselecteerd[3] && i < metingInfo.aantalGeselecteerdeKanalen)
                {
                    tbxFIO3.Text = readings[i].ToString();
                    i++;
                }
                if (metingInfo.blIsHetKanaalGeselecteerd[4] && i < metingInfo.aantalGeselecteerdeKanalen)
                {
                    tbxFIO4.Text = readings[i].ToString();
                    i++;
                }
                if (metingInfo.blIsHetKanaalGeselecteerd[5] && i < metingInfo.aantalGeselecteerdeKanalen)
                {
                    tbxFIO5.Text = readings[i].ToString();
                    i++;
                }
                if (metingInfo.blIsHetKanaalGeselecteerd[6] && i < metingInfo.aantalGeselecteerdeKanalen)
                {
                    tbxFIO6.Text = readings[i].ToString();
                    i++;
                }
                if (metingInfo.blIsHetKanaalGeselecteerd[7] && i < metingInfo.aantalGeselecteerdeKanalen)
                {
                    tbxFIO7.Text = readings[i].ToString();
                    i++;
                }
            }
        }

        private void StreamForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // If we are streaming, we need to end the thread
            if (streamRunning)
            {
                streamRunning = false; // Get the thread to close
                StopStreaming();
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

        private void resetLabjack()
        {
            tsslbl_Error.Text = "";

            if (u3 != null)
            {
                LJUD.ResetLabJack(u3.ljhandle);
                MessageBox.Show("Even geduld...");
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
            }
        }

        private void refreshSettings()
        {
            //instelling te meten kanalen

            metingInfo.blIsHetKanaalGeselecteerd = new bool[aantalKanalen];
            metingInfo.aantalGeselecteerdeKanalen = 0;
            metingInfo.instellingAnalogeKanalen = 0;

            if (cbxFIO0.Checked)
            {
                metingInfo.instellingAnalogeKanalen += 1;
                metingInfo.blIsHetKanaalGeselecteerd[0] = true; //
                metingInfo.aantalGeselecteerdeKanalen++;
            }
            if (cbxFIO1.Checked)
            {
                metingInfo.instellingAnalogeKanalen += 2;
                metingInfo.blIsHetKanaalGeselecteerd[1] = true; //
                metingInfo.aantalGeselecteerdeKanalen++;
            }
            if (cbxFIO2.Checked)
            {
                metingInfo.instellingAnalogeKanalen += 4;
                metingInfo.blIsHetKanaalGeselecteerd[2] = true; //
                metingInfo.aantalGeselecteerdeKanalen++;
            }
            if (cbxFIO3.Checked)
            {
                metingInfo.instellingAnalogeKanalen += 8;
                metingInfo.blIsHetKanaalGeselecteerd[3] = true; //
                metingInfo.aantalGeselecteerdeKanalen++;
            }
            if (cbxFIO4.Checked)
            {
                metingInfo.instellingAnalogeKanalen += 16;
                metingInfo.blIsHetKanaalGeselecteerd[4] = true; //
                metingInfo.aantalGeselecteerdeKanalen++;
            }
            if (cbxFIO5.Checked)
            {
                metingInfo.instellingAnalogeKanalen += 32;
                metingInfo.blIsHetKanaalGeselecteerd[5] = true; //
                metingInfo.aantalGeselecteerdeKanalen++;
            }
            if (cbxFIO6.Checked)
            {
                metingInfo.instellingAnalogeKanalen += 64;
                metingInfo.blIsHetKanaalGeselecteerd[6] = true; //
                metingInfo.aantalGeselecteerdeKanalen++;
            }
            if (cbxFIO7.Checked)
            {
                metingInfo.instellingAnalogeKanalen += 128;
                metingInfo.blIsHetKanaalGeselecteerd[7] = true; //
                metingInfo.aantalGeselecteerdeKanalen++;
            }

            //instellingen
            numScans = (2 * metingInfo.sampleFrequentie * metingInfo.delayms) / 1000;
            metingInfo.sampleFrequentie = Convert.ToInt16(tscbxSampleFrequentie.Text);
            adblData = new double[metingInfo.aantalGeselecteerdeKanalen * (Int16)numScans * 2];
            dataChannel = new List<double>[metingInfo.aantalGeselecteerdeKanalen];
            for (int i = 0; i < metingInfo.aantalGeselecteerdeKanalen; i++) //prepareer juiste datalijsten
            {
                dataChannel[i] = new List<double>(120 * (int)metingInfo.sampleFrequentie * 2); //size 
            }

            //refresh form
            tbxFIO0.Clear();
            tbxFIO1.Clear();
            tbxFIO2.Clear();
            tbxFIO3.Clear();
            tbxFIO4.Clear();
            tbxFIO5.Clear();
            tbxFIO6.Clear();
            tbxFIO7.Clear();
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
                LJUD.Close();
            }
            catch (LabJackUDException ee)
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

        private void GeselecteerdeKanalenOpslaan_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cbxOpslaanFIO0.Checked = cbxFIO0.Checked;
            cbxOpslaanFIO1.Checked = cbxFIO1.Checked;
            cbxOpslaanFIO2.Checked = cbxFIO2.Checked;
            cbxOpslaanFIO3.Checked = cbxFIO3.Checked;
            cbxOpslaanFIO4.Checked = cbxFIO4.Checked;
            cbxOpslaanFIO5.Checked = cbxFIO5.Checked;
            cbxOpslaanFIO6.Checked = cbxFIO6.Checked;
            cbxOpslaanFIO7.Checked = cbxFIO7.Checked;
        }

        private void saveDataInLists(double[] data)
        {
            //Schrijf waardes naar form
            for (int i = 0; i < (metingInfo.aantalGeselecteerdeKanalen * metingInfo.sampleFrequentie); i = i + metingInfo.aantalGeselecteerdeKanalen)
            {
                int j = 0;
                if (metingInfo.blIsHetKanaalGeselecteerd[0] && j < metingInfo.aantalGeselecteerdeKanalen && cbxOpslaanFIO0.Checked)
                {
                    dataChannel[0].Add(data[i]);
                    j++;
                }
                if (metingInfo.blIsHetKanaalGeselecteerd[1] && j < metingInfo.aantalGeselecteerdeKanalen && cbxOpslaanFIO1.Checked)
                {
                    dataChannel[j].Add(data[i + j]);
                    j++;
                }
                if (metingInfo.blIsHetKanaalGeselecteerd[2] && j < metingInfo.aantalGeselecteerdeKanalen && cbxOpslaanFIO2.Checked)
                {
                    dataChannel[j].Add(data[i + j]);
                    j++;
                }
                if (metingInfo.blIsHetKanaalGeselecteerd[3] && j < metingInfo.aantalGeselecteerdeKanalen && cbxOpslaanFIO3.Checked)
                {
                    dataChannel[j].Add(data[i + j]);
                    j++;
                }
                if (metingInfo.blIsHetKanaalGeselecteerd[4] && j < metingInfo.aantalGeselecteerdeKanalen && cbxOpslaanFIO4.Checked)
                {
                    dataChannel[j].Add(data[i + j]);
                    j++;
                }
                if (metingInfo.blIsHetKanaalGeselecteerd[5] && j < metingInfo.aantalGeselecteerdeKanalen && cbxOpslaanFIO5.Checked)
                {
                    dataChannel[j].Add(data[i + j]);
                    j++;
                }
                if (metingInfo.blIsHetKanaalGeselecteerd[6] && j < metingInfo.aantalGeselecteerdeKanalen && cbxOpslaanFIO6.Checked)
                {
                    dataChannel[j].Add(data[i + j]);
                    j++;
                }
                if (metingInfo.blIsHetKanaalGeselecteerd[7] && j < metingInfo.aantalGeselecteerdeKanalen && cbxOpslaanFIO7.Checked)
                {
                    dataChannel[j].Add(data[i + j]);
                    j++;
                }
            }
        }

        private void metingOpslaanAlsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fh == null)
                fh = new ExportData("BT Labjack Streamer");
            fh.OpenSaveFileDialog();

            //geef benodigde data aan ExportData
            fh.DataChannels = dataChannel;
            fh.AantalGeselecteerdeKanalen = metingInfo.aantalGeselecteerdeKanalen;
            fh.GeselecteerdeKanalen = metingInfo.blIsHetKanaalGeselecteerd;
            fh.SampleFrequentie = metingInfo.sampleFrequentie;

            fh.Export();
        }

        #region CHECKBOXES
        private void cbxFIO0_CheckedChanged(object sender, EventArgs e)
        {          
           cbxFIO0Digitaal.Enabled = cbxFIO0.Checked;
           cbxOpslaanFIO0.Enabled = cbxFIO0.Checked; 
        }

        private void cbxFIO1_CheckedChanged(object sender, EventArgs e)
        {
            cbxFIO1Digitaal.Enabled = cbxFIO1.Checked;
            cbxOpslaanFIO1.Enabled = cbxFIO1.Checked; 
        }

        private void cbxFIO2_CheckedChanged(object sender, EventArgs e)
        {
            cbxFIO2Digitaal.Enabled = cbxFIO2.Checked;
            cbxOpslaanFIO2.Enabled = cbxFIO2.Checked; 
        }

        private void cbxFIO3_CheckedChanged(object sender, EventArgs e)
        {
            cbxFIO3Digitaal.Enabled = cbxFIO3.Checked;
            cbxOpslaanFIO3.Enabled = cbxFIO3.Checked; 
        }

        private void cbxFIO4_CheckedChanged(object sender, EventArgs e)
        {
            cbxFIO4Digitaal.Enabled = cbxFIO4.Checked;
            cbxOpslaanFIO4.Enabled = cbxFIO4.Checked; 
        }

        private void cbxFIO5_CheckedChanged(object sender, EventArgs e)
        {
            cbxFIO5Digitaal.Enabled = cbxFIO5.Checked;
            cbxOpslaanFIO5.Enabled = cbxFIO5.Checked; 
        }

        private void cbxFIO6_CheckedChanged(object sender, EventArgs e)
        {
            cbxFIO6Digitaal.Enabled = cbxFIO6.Checked;
            cbxOpslaanFIO6.Enabled = cbxFIO6.Checked; 
        }

        private void cbxFIO7_CheckedChanged(object sender, EventArgs e)
        {
            cbxFIO7Digitaal.Enabled = cbxFIO7.Checked;
            cbxOpslaanFIO7.Enabled = cbxFIO7.Checked; 
        }
        #endregion


        //EINDE KLASSE
    }
}
