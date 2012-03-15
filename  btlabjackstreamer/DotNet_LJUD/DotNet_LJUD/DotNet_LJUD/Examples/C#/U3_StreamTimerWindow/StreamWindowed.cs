//---------------------------------------------------------------------------
//
//  U3_StreamWindowed.cs
// 
//	Display of 2-channel stream from the U3 on a GUI window
//  NOTE: This is only to demonstrate the use of timers with
//		  LabJack devices, specifically streaming. It is suggested
//		  that values be fed into files or used for analysis, not
//		  displayed on a GUI. Stream data has high software latency!
//
//  support@labjack.com
//  July 16, 2010
//	Revised December 28, 2010
//----------------------------------------------------------------------
//

using System;
using System.Timers;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

// Import the UD .NET wrapper object.  The dll referenced is installed by the
// LabJackUD installer.
using LabJack.LabJackUD; 

namespace U3_StreamWindowed
{
	/// <summary>
	/// Application's main form
	/// </summary>
	public class StreamForm : System.Windows.Forms.Form
	{
		// our U3 variable
		private U3 u3;

		// The timer
		System.Timers.Timer updateTimer;
		
		// Widgets
		private System.Windows.Forms.Label ain0Label;
		private System.Windows.Forms.Label ain1Label;
		private System.Windows.Forms.Button startStopButton;
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Label versionLabel;
		private System.Windows.Forms.Label scanRateLabel;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label versionDisplay;
		private System.Windows.Forms.Label scanDisplay;
		private System.Windows.Forms.Label sampleDisplay;
		private System.Windows.Forms.Label firmwareLabel;
		private System.Windows.Forms.Label firmwareDisplay;
		private System.Windows.Forms.Label hardwareLabel;
		private System.Windows.Forms.Label hardwareDisplay;
		private System.Windows.Forms.Label commBacklogLabel;
		private System.Windows.Forms.Label commBacklogDisplay;
		private System.Windows.Forms.Label driverBacklogLabel;
		private System.Windows.Forms.Label driverBacklogDisplay;
		private System.Windows.Forms.Label errorLabel;
		private System.Windows.Forms.Label errorDisplay;
		private System.Windows.Forms.ListBox ain0Display;
		private System.Windows.Forms.ListBox ain1Display;

		// Actual stream related variables
		private int delayms = 3000;
		private LJUD.IO ioType=0;
		private LJUD.CHANNEL channel=0;
		private double dblValue=0, dblCommBacklog=0, dblUDBacklog;
		private double scanRate = 500;
		private double numScans = 3000;  //2x the expected # of scans (2*scanRate*delayms/1000)
		private double numScansRequested;
		private double[] adblData = new double[6000];  //Max buffer size (#channels*numScansRequested)

		// Variables to satisfy certain method signatures
		private int dummyInt = 0;
		private double dummyDouble = 0;
		private System.Windows.Forms.Label label1;
		private double[] dummyDoubleArray = {0};

		public StreamForm()
		{
			// Required for Windows Form Designer support
			InitializeComponent();

			// Create timer but do not start it
			updateTimer = new System.Timers.Timer();
			updateTimer.Interval = delayms;
			updateTimer.Elapsed += new ElapsedEventHandler( MakeReadings );
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ain0Label = new System.Windows.Forms.Label();
			this.ain0Display = new System.Windows.Forms.ListBox();
			this.ain1Label = new System.Windows.Forms.Label();
			this.ain1Display = new System.Windows.Forms.ListBox();
			this.startStopButton = new System.Windows.Forms.Button();
			this.versionLabel = new System.Windows.Forms.Label();
			this.scanRateLabel = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.versionDisplay = new System.Windows.Forms.Label();
			this.scanDisplay = new System.Windows.Forms.Label();
			this.sampleDisplay = new System.Windows.Forms.Label();
			this.firmwareLabel = new System.Windows.Forms.Label();
			this.firmwareDisplay = new System.Windows.Forms.Label();
			this.hardwareLabel = new System.Windows.Forms.Label();
			this.hardwareDisplay = new System.Windows.Forms.Label();
			this.commBacklogLabel = new System.Windows.Forms.Label();
			this.commBacklogDisplay = new System.Windows.Forms.Label();
			this.driverBacklogLabel = new System.Windows.Forms.Label();
			this.driverBacklogDisplay = new System.Windows.Forms.Label();
			this.errorLabel = new System.Windows.Forms.Label();
			this.errorDisplay = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// ain0Label
			// 
			this.ain0Label.Location = new System.Drawing.Point(16, 152);
			this.ain0Label.Name = "ain0Label";
			this.ain0Label.Size = new System.Drawing.Size(112, 16);
			this.ain0Label.TabIndex = 0;
			this.ain0Label.Text = "AIN/FIO0:";
			// 
			// ain0Display
			// 
			this.ain0Display.Location = new System.Drawing.Point(136, 144);
			this.ain0Display.Name = "ain0Display";
			this.ain0Display.Size = new System.Drawing.Size(168, 121);
			this.ain0Display.TabIndex = 1;
			// 
			// ain1Label
			// 
			this.ain1Label.Location = new System.Drawing.Point(312, 152);
			this.ain1Label.Name = "ain1Label";
			this.ain1Label.Size = new System.Drawing.Size(112, 16);
			this.ain1Label.TabIndex = 2;
			this.ain1Label.Text = "AIN/FIO1:";
			// 
			// ain1Display
			// 
			this.ain1Display.Location = new System.Drawing.Point(432, 144);
			this.ain1Display.Name = "ain1Display";
			this.ain1Display.Size = new System.Drawing.Size(168, 121);
			this.ain1Display.TabIndex = 3;
			// 
			// startStopButton
			// 
			this.startStopButton.Location = new System.Drawing.Point(16, 296);
			this.startStopButton.Name = "startStopButton";
			this.startStopButton.Size = new System.Drawing.Size(584, 24);
			this.startStopButton.TabIndex = 4;
			this.startStopButton.Text = "Start";
			this.startStopButton.Click += new System.EventHandler(this.startStopButton_Click);
			// 
			// versionLabel
			// 
			this.versionLabel.Location = new System.Drawing.Point(16, 16);
			this.versionLabel.Name = "versionLabel";
			this.versionLabel.Size = new System.Drawing.Size(112, 16);
			this.versionLabel.TabIndex = 5;
			this.versionLabel.Text = "UD Driver Version:";
			// 
			// scanRateLabel
			// 
			this.scanRateLabel.Location = new System.Drawing.Point(312, 16);
			this.scanRateLabel.Name = "scanRateLabel";
			this.scanRateLabel.Size = new System.Drawing.Size(112, 16);
			this.scanRateLabel.TabIndex = 6;
			this.scanRateLabel.Text = "Actual Scan Rate:";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(312, 40);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(112, 16);
			this.label3.TabIndex = 7;
			this.label3.Text = "Actual Sample Rate:";
			// 
			// versionDisplay
			// 
			this.versionDisplay.Location = new System.Drawing.Point(136, 16);
			this.versionDisplay.Name = "versionDisplay";
			this.versionDisplay.Size = new System.Drawing.Size(168, 16);
			this.versionDisplay.TabIndex = 8;
			// 
			// scanDisplay
			// 
			this.scanDisplay.Location = new System.Drawing.Point(432, 16);
			this.scanDisplay.Name = "scanDisplay";
			this.scanDisplay.Size = new System.Drawing.Size(168, 16);
			this.scanDisplay.TabIndex = 9;
			// 
			// sampleDisplay
			// 
			this.sampleDisplay.Location = new System.Drawing.Point(432, 40);
			this.sampleDisplay.Name = "sampleDisplay";
			this.sampleDisplay.Size = new System.Drawing.Size(168, 16);
			this.sampleDisplay.TabIndex = 10;
			// 
			// firmwareLabel
			// 
			this.firmwareLabel.Location = new System.Drawing.Point(16, 64);
			this.firmwareLabel.Name = "firmwareLabel";
			this.firmwareLabel.Size = new System.Drawing.Size(112, 16);
			this.firmwareLabel.TabIndex = 11;
			this.firmwareLabel.Text = "Firmware Version:";
			// 
			// firmwareDisplay
			// 
			this.firmwareDisplay.Location = new System.Drawing.Point(136, 64);
			this.firmwareDisplay.Name = "firmwareDisplay";
			this.firmwareDisplay.Size = new System.Drawing.Size(168, 16);
			this.firmwareDisplay.TabIndex = 12;
			// 
			// hardwareLabel
			// 
			this.hardwareLabel.Location = new System.Drawing.Point(16, 40);
			this.hardwareLabel.Name = "hardwareLabel";
			this.hardwareLabel.Size = new System.Drawing.Size(112, 16);
			this.hardwareLabel.TabIndex = 13;
			this.hardwareLabel.Text = "Hardware Version:";
			// 
			// hardwareDisplay
			// 
			this.hardwareDisplay.Location = new System.Drawing.Point(136, 40);
			this.hardwareDisplay.Name = "hardwareDisplay";
			this.hardwareDisplay.Size = new System.Drawing.Size(168, 16);
			this.hardwareDisplay.TabIndex = 14;
			// 
			// commBacklogLabel
			// 
			this.commBacklogLabel.Location = new System.Drawing.Point(312, 64);
			this.commBacklogLabel.Name = "commBacklogLabel";
			this.commBacklogLabel.Size = new System.Drawing.Size(112, 16);
			this.commBacklogLabel.TabIndex = 15;
			this.commBacklogLabel.Text = "Comm Backlog:";
			// 
			// commBacklogDisplay
			// 
			this.commBacklogDisplay.Location = new System.Drawing.Point(432, 64);
			this.commBacklogDisplay.Name = "commBacklogDisplay";
			this.commBacklogDisplay.Size = new System.Drawing.Size(168, 16);
			this.commBacklogDisplay.TabIndex = 16;
			// 
			// driverBacklogLabel
			// 
			this.driverBacklogLabel.Location = new System.Drawing.Point(312, 88);
			this.driverBacklogLabel.Name = "driverBacklogLabel";
			this.driverBacklogLabel.Size = new System.Drawing.Size(112, 16);
			this.driverBacklogLabel.TabIndex = 17;
			this.driverBacklogLabel.Text = "Driver Backlog:";
			// 
			// driverBacklogDisplay
			// 
			this.driverBacklogDisplay.Location = new System.Drawing.Point(432, 88);
			this.driverBacklogDisplay.Name = "driverBacklogDisplay";
			this.driverBacklogDisplay.Size = new System.Drawing.Size(168, 16);
			this.driverBacklogDisplay.TabIndex = 18;
			// 
			// errorLabel
			// 
			this.errorLabel.Location = new System.Drawing.Point(16, 88);
			this.errorLabel.Name = "errorLabel";
			this.errorLabel.Size = new System.Drawing.Size(112, 16);
			this.errorLabel.TabIndex = 19;
			this.errorLabel.Text = "Error:";
			// 
			// errorDisplay
			// 
			this.errorDisplay.Location = new System.Drawing.Point(136, 88);
			this.errorDisplay.Name = "errorDisplay";
			this.errorDisplay.Size = new System.Drawing.Size(168, 48);
			this.errorDisplay.TabIndex = 20;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(312, 112);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(264, 32);
			this.label1.TabIndex = 21;
			this.label1.Text = "Note: The software timer can be slighly off step so there may be a small backlog";
			// 
			// StreamForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(616, 342);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.errorDisplay);
			this.Controls.Add(this.errorLabel);
			this.Controls.Add(this.driverBacklogDisplay);
			this.Controls.Add(this.driverBacklogLabel);
			this.Controls.Add(this.commBacklogDisplay);
			this.Controls.Add(this.commBacklogLabel);
			this.Controls.Add(this.hardwareDisplay);
			this.Controls.Add(this.hardwareLabel);
			this.Controls.Add(this.firmwareDisplay);
			this.Controls.Add(this.firmwareLabel);
			this.Controls.Add(this.sampleDisplay);
			this.Controls.Add(this.scanDisplay);
			this.Controls.Add(this.versionDisplay);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.scanRateLabel);
			this.Controls.Add(this.versionLabel);
			this.Controls.Add(this.startStopButton);
			this.Controls.Add(this.ain1Display);
			this.Controls.Add(this.ain1Label);
			this.Controls.Add(this.ain0Display);
			this.Controls.Add(this.ain0Label);
			this.Name = "StreamForm";
			this.Text = "StreamWindowed";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.StreamForm_Closing);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new StreamForm());
		}

		/// <summary>
		/// Set up stream and then start the stream timer or,
		/// if the stream has already started, stop the stream and
		/// timer.
		/// </summary>
		private void startStopButton_Click(object sender, System.EventArgs e)
		{
			// Reset the error display
			errorDisplay.Text = "";

			// If we are already streaming, end the stream and timer
			if(updateTimer.Enabled)
			{
				// Stop the stream
				StopStreaming();
				updateTimer.Stop();

				// Reconfigure start button
				startStopButton.Text = "Start";
				startStopButton.Enabled = true;
			}

				// Otherwise, start the stream and timer
			else
			{
				// Set up the stream
				if (StartStreaming())
				{
					updateTimer.Start();

					// Reconfigure start button
					startStopButton.Text = "Stop";
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
				if(u3 != null)
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
			//Read and display the UD version.
			dblValue = LJUD.GetDriverVersion();
			versionDisplay.Text = String.Format("{0:0.000}",dblValue);

			try 
			{
				//Open the first found LabJack U3.
				u3 = new U3(LJUD.CONNECTION.USB, "0", true); // Connection through USB

				//Read and display the hardware version of this U3.
				LJUD.eGet (u3.ljhandle, LJUD.IO.GET_CONFIG, LJUD.CHANNEL.HARDWARE_VERSION, ref dblValue, 0);
				hardwareDisplay.Text = String.Format("{0:0.000}",dblValue);

				//Read and display the firmware version of this U3.
				LJUD.eGet (u3.ljhandle, LJUD.IO.GET_CONFIG, LJUD.CHANNEL.FIRMWARE_VERSION, ref dblValue, 0);
				firmwareDisplay.Text = String.Format("{0:0.000}",dblValue);

				//Start by using the pin_configuration_reset IOType so that all
				//pin assignments are in the factory default condition.
				LJUD.ePut (u3.ljhandle, LJUD.IO.PIN_CONFIGURATION_RESET, 0, 0, 0);

				//Configure FIO0 and FIO1 as analog, all else as digital.  That means we
				//will start from channel 0 and update all 16 flexible bits.  We will
				//pass a value of b0000000000000011 or d3.
				LJUD.ePut (u3.ljhandle, LJUD.IO.PUT_ANALOG_ENABLE_PORT, 0, 3, 16);

				//Configure the stream:
				//Set the scan rate.
				LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.STREAM_SCAN_FREQUENCY, scanRate, 0, 0);
				
				//Give the driver a 5 second buffer (scanRate * 2 channels * 5 seconds).
				LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.STREAM_BUFFER_SIZE, scanRate*2*5, 0, 0);
				
				//Configure reads to retrieve whatever data is available without waiting (wait mode LJUD.STREAMWAITMODES.NONE).
				//See comments below to change this program to use LJUD.STREAMWAITMODES.SLEEP mode.
				LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.STREAM_WAIT_MODE, (double) LJUD.STREAMWAITMODES.NONE, 0, 0);
				
				//Define the scan list as AIN0 then AIN1.
				LJUD.AddRequest(u3.ljhandle, LJUD.IO.CLEAR_STREAM_CHANNELS, 0, 0, 0, 0);
				LJUD.AddRequest(u3.ljhandle, LJUD.IO.ADD_STREAM_CHANNEL, 0, 0, 0, 0);
				LJUD.AddRequest(u3.ljhandle, LJUD.IO.ADD_STREAM_CHANNEL_DIFF, 1, 0, 32, 0);
    
				//Execute the list of requests.
				LJUD.GoOne(u3.ljhandle);
			}
			catch (LabJackUDException e) 
			{
				ShowErrorMessage(e);
				return false;
			}
    
			//Get all the results just to check for errors.
			try {LJUD.GetFirstResult(u3.ljhandle, ref ioType, ref channel, ref dblValue, ref dummyInt, ref dummyDouble);}
			catch (LabJackUDException e) { ShowErrorMessage(e); }
			bool finished = false;
			while(!finished)
			{
				try { LJUD.GetNextResult(u3.ljhandle, ref ioType, ref channel, ref dblValue, ref dummyInt, ref dummyDouble); }
				catch (LabJackUDException e) 
				{
					// If we get an error, report it.  If the error is NO_MORE_DATA_AVAILABLE we are done
					if(e.LJUDError == U3.LJUDERROR.NO_MORE_DATA_AVAILABLE)
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
			scanDisplay.Text = String.Format("{0:0.000}",dblValue);
			sampleDisplay.Text = String.Format("{0:0.000}",2*dblValue); // # channels * scan rate

			// The stream started successfully
			return true;
		}

		/// <summary>
		/// performs read of stream data on LabJack device
		/// </summary>
		/// <param name="source">The object that called the event</param>
		/// <param name="e">Event details</param>
		private void MakeReadings(object source, ElapsedEventArgs e)
		{
			//init array so we can easily tell if it has changed
			for(int i=0;i<numScans*2;i++)
			{
				adblData[i] = 9999.0;
			}

			try
			{
				//Read the data.  We will request twice the number we expect, to
				//make sure we get everything that is available.
				//Note that the array we pass must be sized to hold enough SAMPLES, and
				//the Value we pass specifies the number of SCANS to read.
				numScansRequested=numScans;
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
			catch (LabJackUDException exc) 
			{ 
				ShowErrorMessage(exc); 
				return;
			}
		}

		/// <summary>
		/// Display the driver and comm backlog on the GUI
		/// </summary>
		/// <param name="udBacklog">The backlog reported by the UD driver</param>
		/// <param name="commBacklog">The backlog reported by the device driver</param>
		private void DisplayBacklog(double udBacklog, double commBacklog)
		{
			driverBacklogDisplay.Text = String.Format("{0:0.000}",udBacklog);
			commBacklogDisplay.Text = String.Format("{0:0.000}",commBacklog);
		}

		/// <summary>
		/// Displays the error on the GUI in a label
		/// </summary>
		/// <param name="e">The exception encountered</param>
		private void ShowErrorMessage(LabJackUDException e)
		{
			errorDisplay.Text = e.ToString();

			// Stop streaming if we are still streaming
			if (updateTimer.Enabled)
			{
				updateTimer.Enabled = false;
				StopStreaming();
			}
		}
		

		/// <summary>
		/// Show the readings for AIN0 and AIN1
		/// </summary>
		/// <param name="readings">The array of readings returned from the driver</param>
		private void ShowReadings(double[] readings)
		{
			// Clear current values
			ain0Display.Items.Clear();
			ain1Display.Items.Clear();

			// Put in values. 
			// The second half should be 9999 because 
			// of the padding set up earlier in the program.
			for(int i=0; i<readings.Length; i+=2)
			{
				ain0Display.Items.Add(readings[i]);
				ain1Display.Items.Add(readings[i+1]);
			}
		}

		private void StreamForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// If we are streaming
			if (updateTimer.Enabled)
				StopStreaming();
		}
	}
}
