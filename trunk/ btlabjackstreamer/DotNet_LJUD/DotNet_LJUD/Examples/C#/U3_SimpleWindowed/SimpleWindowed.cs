//---------------------------------------------------------------------------
//
//  U3_SimpleWindowed.cs
// 
//  Basic command/response U3 example using the UD driver and
//	a C# GUI window. Please note that most of this code
//  has been copied directly from the U3_Simple example. 
//  This is because, in most cirumstances, the LabJack related
//  code is the same as it would be without a GUI
//
//  support@labjack.com
//  July 13, 2010
//	Revised December 28, 2010
//----------------------------------------------------------------------
//

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

// Import the UD .NET wrapper object.  The dll referenced is installed by the
// LabJackUD installer.
using LabJack.LabJackUD; 

namespace U3_SimpleWindowed
{
	/// <summary>
	/// The application's basic form
	/// </summary>
	public class SimpleWindow : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label versionLabel;
		private System.Windows.Forms.Label versionDisplay;
		private System.Windows.Forms.Label ain0Label;
		private System.Windows.Forms.Label ain0Display;
		private System.Windows.Forms.Label ain1Label;
		private System.Windows.Forms.Label ain1Display;
		private System.Windows.Forms.Label ain2Label;
		private System.Windows.Forms.Label ain2Display;
		private System.Windows.Forms.Label fio5Label;
		private System.Windows.Forms.Label fio5Display;
		private System.Windows.Forms.Label fio6Label;
		private System.Windows.Forms.Label fio6Display;
		private System.Windows.Forms.Label counter1Label;
		private System.Windows.Forms.Label counter1Display;
		private System.Windows.Forms.Button goButton;
		private System.Windows.Forms.Label errorLabel;
		private System.Windows.Forms.Label errorDisplay;
		private System.ComponentModel.Container components = null;
		
		// our U3 variable
		private U3 u3;

		public SimpleWindow()
		{
			// Required for Windows Form Designer support
			InitializeComponent();
		}

		// If error occured print a message indicating which one occurred
		public void ShowErrorMessage(LabJackUDException exc)
		{
			errorDisplay.Text = exc.ToString();
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
			this.versionLabel = new System.Windows.Forms.Label();
			this.versionDisplay = new System.Windows.Forms.Label();
			this.ain0Label = new System.Windows.Forms.Label();
			this.ain0Display = new System.Windows.Forms.Label();
			this.ain1Label = new System.Windows.Forms.Label();
			this.ain1Display = new System.Windows.Forms.Label();
			this.ain2Label = new System.Windows.Forms.Label();
			this.ain2Display = new System.Windows.Forms.Label();
			this.fio5Label = new System.Windows.Forms.Label();
			this.fio5Display = new System.Windows.Forms.Label();
			this.fio6Label = new System.Windows.Forms.Label();
			this.fio6Display = new System.Windows.Forms.Label();
			this.counter1Label = new System.Windows.Forms.Label();
			this.counter1Display = new System.Windows.Forms.Label();
			this.goButton = new System.Windows.Forms.Button();
			this.errorLabel = new System.Windows.Forms.Label();
			this.errorDisplay = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// versionLabel
			// 
			this.versionLabel.Location = new System.Drawing.Point(8, 8);
			this.versionLabel.Name = "versionLabel";
			this.versionLabel.Size = new System.Drawing.Size(136, 16);
			this.versionLabel.TabIndex = 0;
			this.versionLabel.Text = "UD Driver Version:";
			// 
			// versionDisplay
			// 
			this.versionDisplay.Location = new System.Drawing.Point(152, 8);
			this.versionDisplay.Name = "versionDisplay";
			this.versionDisplay.Size = new System.Drawing.Size(136, 16);
			this.versionDisplay.TabIndex = 1;
			// 
			// ain0Label
			// 
			this.ain0Label.Location = new System.Drawing.Point(8, 32);
			this.ain0Label.Name = "ain0Label";
			this.ain0Label.Size = new System.Drawing.Size(136, 16);
			this.ain0Label.TabIndex = 2;
			this.ain0Label.Text = "AIN0:";
			// 
			// ain0Display
			// 
			this.ain0Display.Location = new System.Drawing.Point(152, 32);
			this.ain0Display.Name = "ain0Display";
			this.ain0Display.Size = new System.Drawing.Size(136, 16);
			this.ain0Display.TabIndex = 3;
			// 
			// ain1Label
			// 
			this.ain1Label.Location = new System.Drawing.Point(8, 56);
			this.ain1Label.Name = "ain1Label";
			this.ain1Label.Size = new System.Drawing.Size(136, 16);
			this.ain1Label.TabIndex = 4;
			this.ain1Label.Text = "AIN1:";
			// 
			// ain1Display
			// 
			this.ain1Display.Location = new System.Drawing.Point(152, 56);
			this.ain1Display.Name = "ain1Display";
			this.ain1Display.Size = new System.Drawing.Size(136, 16);
			this.ain1Display.TabIndex = 5;
			// 
			// ain2Label
			// 
			this.ain2Label.Location = new System.Drawing.Point(8, 80);
			this.ain2Label.Name = "ain2Label";
			this.ain2Label.Size = new System.Drawing.Size(136, 16);
			this.ain2Label.TabIndex = 6;
			this.ain2Label.Text = "AIN2:";
			// 
			// ain2Display
			// 
			this.ain2Display.Location = new System.Drawing.Point(152, 80);
			this.ain2Display.Name = "ain2Display";
			this.ain2Display.Size = new System.Drawing.Size(136, 16);
			this.ain2Display.TabIndex = 7;
			// 
			// fio5Label
			// 
			this.fio5Label.Location = new System.Drawing.Point(8, 104);
			this.fio5Label.Name = "fio5Label";
			this.fio5Label.Size = new System.Drawing.Size(136, 16);
			this.fio5Label.TabIndex = 8;
			this.fio5Label.Text = "FIO5:";
			// 
			// fio5Display
			// 
			this.fio5Display.Location = new System.Drawing.Point(152, 104);
			this.fio5Display.Name = "fio5Display";
			this.fio5Display.Size = new System.Drawing.Size(136, 16);
			this.fio5Display.TabIndex = 9;
			// 
			// fio6Label
			// 
			this.fio6Label.Location = new System.Drawing.Point(8, 128);
			this.fio6Label.Name = "fio6Label";
			this.fio6Label.Size = new System.Drawing.Size(136, 16);
			this.fio6Label.TabIndex = 10;
			this.fio6Label.Text = "FIO5-FIO6:";
			// 
			// fio6Display
			// 
			this.fio6Display.Location = new System.Drawing.Point(152, 128);
			this.fio6Display.Name = "fio6Display";
			this.fio6Display.Size = new System.Drawing.Size(136, 16);
			this.fio6Display.TabIndex = 11;
			// 
			// counter1Label
			// 
			this.counter1Label.Location = new System.Drawing.Point(8, 152);
			this.counter1Label.Name = "counter1Label";
			this.counter1Label.Size = new System.Drawing.Size(136, 16);
			this.counter1Label.TabIndex = 12;
			this.counter1Label.Text = "Counter1 (FIO7):";
			// 
			// counter1Display
			// 
			this.counter1Display.Location = new System.Drawing.Point(152, 152);
			this.counter1Display.Name = "counter1Display";
			this.counter1Display.Size = new System.Drawing.Size(136, 16);
			this.counter1Display.TabIndex = 13;
			// 
			// goButton
			// 
			this.goButton.Location = new System.Drawing.Point(8, 232);
			this.goButton.Name = "goButton";
			this.goButton.Size = new System.Drawing.Size(280, 24);
			this.goButton.TabIndex = 14;
			this.goButton.Text = "Go";
			this.goButton.Click += new System.EventHandler(this.goButton_Click);
			// 
			// errorLabel
			// 
			this.errorLabel.Location = new System.Drawing.Point(8, 176);
			this.errorLabel.Name = "errorLabel";
			this.errorLabel.Size = new System.Drawing.Size(136, 16);
			this.errorLabel.TabIndex = 15;
			this.errorLabel.Text = "Error:";
			// 
			// errorDisplay
			// 
			this.errorDisplay.Location = new System.Drawing.Point(152, 176);
			this.errorDisplay.Name = "errorDisplay";
			this.errorDisplay.Size = new System.Drawing.Size(136, 48);
			this.errorDisplay.TabIndex = 16;
			// 
			// SimpleWindow
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 262);
			this.Controls.Add(this.errorDisplay);
			this.Controls.Add(this.errorLabel);
			this.Controls.Add(this.goButton);
			this.Controls.Add(this.counter1Display);
			this.Controls.Add(this.counter1Label);
			this.Controls.Add(this.fio6Display);
			this.Controls.Add(this.fio6Label);
			this.Controls.Add(this.fio5Display);
			this.Controls.Add(this.fio5Label);
			this.Controls.Add(this.ain2Display);
			this.Controls.Add(this.ain2Label);
			this.Controls.Add(this.ain1Display);
			this.Controls.Add(this.ain1Label);
			this.Controls.Add(this.ain0Display);
			this.Controls.Add(this.ain0Label);
			this.Controls.Add(this.versionDisplay);
			this.Controls.Add(this.versionLabel);
			this.Name = "SimpleWindow";
			this.Text = "SimpleWindow";
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new SimpleWindow());
		}

		/// <summary>
		/// Actually performs actions on the U3 and updates the displaye
		/// </summary>
		/// <param name="sender">The object that executed this method</param>
		/// <param name="e">Event parameters</param>
		private void goButton_Click(object sender, System.EventArgs e)
		{
			double dblDriverVersion;
			LJUD.IO ioType=0;
			LJUD.CHANNEL channel=0;
			double dblValue=0;
			double Value0=9999,Value1=9999,Value2=9999;
			double ValueDIBit=9999,ValueDIPort=9999,ValueCounter=9999;

			// Variables to satisfy certain method signatures
			int dummyInt = 0;
			double dummyDouble = 0;

			//Read and display the UD version.
			dblDriverVersion = LJUD.GetDriverVersion();
			versionDisplay.Text = String.Format("{0:0.000}",dblDriverVersion);

			try 
			{
				//Open the first found LabJack U3.
				u3 = new U3(LJUD.CONNECTION.USB, "0", true); // Connection through USB

				//Start by using the pin_configuration_reset IOType so that all
				//pin assignments are in the factory default condition.
				LJUD.ePut (u3.ljhandle, LJUD.IO.PIN_CONFIGURATION_RESET, 0, 0, 0);

				//First some configuration commands.  These will be done with the ePut
				//function which combines the add/go/get into a single call.

				//Configure FIO0-FIO3 as analog, all else as digital.  That means we
				//will start from channel 0 and update all 16 flexible bits.  We will
				//pass a value of b0000000000001111 or d15.
				LJUD.ePut (u3.ljhandle, LJUD.IO.PUT_ANALOG_ENABLE_PORT, 0, 15, 16);

				//Set the timer/counter pin offset to 7, which will put the first
				//timer/counter on FIO7.
				LJUD.ePut (u3.ljhandle,  LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.TIMER_COUNTER_PIN_OFFSET, 7, 0);

				//Enable Counter1 (FIO7).
				LJUD.ePut (u3.ljhandle,  LJUD.IO.PUT_COUNTER_ENABLE, (LJUD.CHANNEL)1, 1, 0);

				//The following commands will use the add-go-get method to group
				//multiple requests into a single low-level function.

				//Request a single-ended reading from AIN0.
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.GET_AIN, 0, 0, 0, 0);

				//Request a single-ended reading from AIN1.
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.GET_AIN, 1, 0, 0, 0);

				//Request a reading from AIN2 using the Special range.
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.GET_AIN_DIFF, 2, 0, 32, 0);

				//Read digital input FIO5.
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.GET_DIGITAL_BIT, 5, 0, 0, 0);

				//Read digital inputs FIO5 through FIO6.
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.GET_DIGITAL_PORT, 5, 0, 2, 0);

				//Request the value of Counter1 (FIO7).
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.GET_COUNTER, 1, 0, 0, 0);
			}
			catch (LabJackUDException exc) 
			{
				ShowErrorMessage(exc);
				return;
			}

			try
			{
				//Execute the requests.
				LJUD.GoOne (u3.ljhandle);

				//Get all the results.  The input measurement results are stored.  All other
				//results are for configuration or output requests so we are just checking
				//whether there was an error.
				LJUD.GetFirstResult(u3.ljhandle, ref ioType, ref channel, ref dblValue, ref dummyInt, ref dummyDouble);
			}
			catch (LabJackUDException exc) 
			{
				ShowErrorMessage(exc);
				return;
			}

			bool finished = false;
			while(!finished)
			{
				switch(ioType)
				{

					case LJUD.IO.GET_AIN :
					switch((int)channel)
					{
						case 0:
							Value0=dblValue;
							break;
						case 1:
							Value1=dblValue;
							break;
					}
						break;

					case LJUD.IO.GET_AIN_DIFF :
						Value2=dblValue;
						break;

					case LJUD.IO.GET_DIGITAL_BIT :
						ValueDIBit=dblValue;
						break;

					case LJUD.IO.GET_DIGITAL_PORT :
						ValueDIPort=dblValue;
						break;

					case LJUD.IO.GET_COUNTER :
						ValueCounter=dblValue;
						break;

				}
				try {LJUD.GetNextResult(u3.ljhandle, ref ioType, ref channel, ref dblValue, ref dummyInt, ref dummyDouble);}
				catch (LabJackUDException exc) 
				{
					// If we get an error, report it.  If the error is NO_MORE_DATA_AVAILABLE we are done
					if(exc.LJUDError == U3.LJUDERROR.NO_MORE_DATA_AVAILABLE)
						finished = true;
					else
						ShowErrorMessage(exc);
				}
			}

			// Display results
			ain0Display.Text = String.Format("{0:0.###}",Value0);
			ain1Display.Text = String.Format("{0:0.###}",Value1);
			ain2Display.Text = String.Format("{0:0.###}",Value2);
			fio5Display.Text = String.Format("{0:0.###}",ValueDIBit);
			fio6Display.Text = String.Format("{0:0.###}",ValueDIPort);  //Will read 3 (binary 11) if both lines are pulled-high as normal.
			counter1Display.Text = String.Format("{0:0.###}",ValueCounter);
		}
	}
}
