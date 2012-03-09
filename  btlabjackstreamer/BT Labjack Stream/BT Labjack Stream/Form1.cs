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
        private int delayms = 1000;
        private LJUD.IO ioType = 0;
        private LJUD.CHANNEL channel = 0;
        private double dblValue = 0, dblCommBacklog = 0, dblUDBacklog;
        private double scanRate = 500;
        private double numScans = 3000;  //2x the expected # of scans (2*scanRate*delayms/1000)
        private double numScansRequested;
        private double[] adblData = new double[6000];  //Max buffer size (#channels*numScansRequested)
        private bool streamRunning = false;
        private double[] dummyDoubleArray = { 0 };
        private int dummyInt = 0;
        private double dummyDouble = 0;
        private int instellingAnalogeKanalen = 3;
        private const int aantalKanalen = 8;
        private Int16 aantalGeselecteerdeKanalen = 0;
        private bool[] blIsHetKanaalGeselecteerd = new bool[aantalKanalen];
        private List<float>[] dataChannel = null; //plek voor data

        // Create thread delegate
        delegate void BacklogParameterDelegate(double udBacklog, double commBacklog);
        delegate void ErrorParameterDelegate(LabJackUDException e);
        delegate void ReadingsParameterDelegate(double[] readings);
        #endregion

        public Form1()
        {
            InitializeComponent();
            refreshSettings();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            connectToLabjack();
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            refreshSettings();

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
                LJUD.ePut(u3.ljhandle, LJUD.IO.PUT_ANALOG_ENABLE_PORT, 0, instellingAnalogeKanalen, 16);

                //Configure the stream:
                //Set the scan rate.
                LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.STREAM_SCAN_FREQUENCY, scanRate, 0, 0);

                //Give the driver a 5 second buffer (scanRate * 2 channels * 5 seconds).
                LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.STREAM_BUFFER_SIZE, scanRate * aantalGeselecteerdeKanalen * 5, 0, 0);

                //Configure reads to retrieve whatever data is available without waiting (wait mode LJUD.STREAMWAITMODES.NONE).
                //See comments below to change this program to use LJUD.STREAMWAITMODES.SLEEP mode.
                LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.STREAM_WAIT_MODE, (double)LJUD.STREAMWAITMODES.NONE, 0, 0);

                //Toevoegen van channels
                LJUD.AddRequest(u3.ljhandle, LJUD.IO.CLEAR_STREAM_CHANNELS, 0, 0, 0, 0);

                for (int i = 0; i < aantalKanalen; i++ )
                {
                    if (blIsHetKanaalGeselecteerd[i])
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

                Thread.Sleep(delayms);	//Remove if using LJUD.STREAMWAITMODES.SLEEP.

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
                // Put in values. 
                // The second half should be 9999 because of the padding set up earlier in the program.
                int i = 0;
                if (blIsHetKanaalGeselecteerd[0] && i < aantalGeselecteerdeKanalen)
                {
                    tbxFIO0.Text = readings[0].ToString();
                    i++;
                }
                if (blIsHetKanaalGeselecteerd[1] && i < aantalGeselecteerdeKanalen)
                {
                    tbxFIO1.Text = readings[1].ToString();
                    i++;
                }
                if (blIsHetKanaalGeselecteerd[2] && i < aantalGeselecteerdeKanalen)
                {
                    tbxFIO2.Text = readings[2].ToString();
                    i++;
                }
                if (blIsHetKanaalGeselecteerd[3] && i < aantalGeselecteerdeKanalen)
                {
                    tbxFIO3.Text = readings[3].ToString();
                    i++;
                }
                if (blIsHetKanaalGeselecteerd[4] && i < aantalGeselecteerdeKanalen)
                {
                    tbxFIO4.Text = readings[4].ToString();
                    i++;
                }
                if (blIsHetKanaalGeselecteerd[5] && i < aantalGeselecteerdeKanalen)
                {
                    tbxFIO5.Text = readings[5].ToString();
                    i++;
                }
                if (blIsHetKanaalGeselecteerd[6] && i < aantalGeselecteerdeKanalen)
                {
                    tbxFIO6.Text = readings[6].ToString();
                    i++;
                }
                if (blIsHetKanaalGeselecteerd[7] && i < aantalGeselecteerdeKanalen)
                {
                    tbxFIO7.Text = readings[7].ToString();
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
                u3 = new U3(LJUD.CONNECTION.USB, "0", true); // Connection through USB
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
            instellingAnalogeKanalen = 0;
            blIsHetKanaalGeselecteerd = new bool[aantalKanalen];
            aantalGeselecteerdeKanalen = 0;

            if (cbxFIO0.Checked)
            {
                instellingAnalogeKanalen += 1;
                blIsHetKanaalGeselecteerd[0] = true; //
                aantalGeselecteerdeKanalen++;
            }
            if (cbxFIO1.Checked)
            {
                instellingAnalogeKanalen += 2;
                blIsHetKanaalGeselecteerd[1] = true; //
                aantalGeselecteerdeKanalen++;
            }
            if (cbxFIO2.Checked)
            {
                instellingAnalogeKanalen += 4;
                blIsHetKanaalGeselecteerd[2] = true; //
                aantalGeselecteerdeKanalen++;
            }
            if (cbxFIO3.Checked)
            {
                instellingAnalogeKanalen += 8;
                blIsHetKanaalGeselecteerd[3] = true; //
                aantalGeselecteerdeKanalen++;
            }
            if (cbxFIO4.Checked)
            {
                instellingAnalogeKanalen += 16;
                blIsHetKanaalGeselecteerd[4] = true; //
                aantalGeselecteerdeKanalen++;
            }
            if (cbxFIO5.Checked)
            {
                instellingAnalogeKanalen += 32;
                blIsHetKanaalGeselecteerd[5] = true; //
                aantalGeselecteerdeKanalen++;
            }
            if (cbxFIO6.Checked)
            {
                instellingAnalogeKanalen += 64;
                blIsHetKanaalGeselecteerd[6] = true; //
                aantalGeselecteerdeKanalen++;
            }
            if (cbxFIO7.Checked)
            {
                instellingAnalogeKanalen += 128;
                blIsHetKanaalGeselecteerd[7] = true; //
                aantalGeselecteerdeKanalen++;
            }

            //instellingen
            numScans = (2 * scanRate * delayms) / 1000;
            adblData = new double[aantalGeselecteerdeKanalen * (Int16)numScans * 2];
            dataChannel = new List<float>[aantalGeselecteerdeKanalen];
            for (int i = 0; i < aantalGeselecteerdeKanalen; i++) //prepareer juiste datalijsten
            {
                if (blIsHetKanaalGeselecteerd[i])
                    dataChannel[i] = new List<float>(120 * (int)scanRate * 2);
            }
        }

        private void instellingenAanuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            instellingenAanuitToolStripMenuItem.Checked = !instellingenAanuitToolStripMenuItem.Checked;
            gbxInstellingen.Visible = instellingenAanuitToolStripMenuItem.Checked;
        }

        private void tscbxSampleFrequentie_TextChanged(object sender, EventArgs e)
        {
            scanRate = Convert.ToInt16(tscbxSampleFrequentie.Text);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                LJUD.Close();
            }
            catch (Exception)
            {
            }
        }


        //EINDE KLASSE
    }
}
