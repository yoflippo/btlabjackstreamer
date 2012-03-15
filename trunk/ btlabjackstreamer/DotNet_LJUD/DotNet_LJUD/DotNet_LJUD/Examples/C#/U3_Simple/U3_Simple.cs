//---------------------------------------------------------------------------
//
//  U3_Simple.cs
// 
//  Basic command/response U3 example using the UD driver.
//
//  support@labjack.com
//  June 5, 2009
//	Revised December 28, 2010
//----------------------------------------------------------------------
//

using System;
using System.Threading;

// Import the UD .NET wrapper object.  The dll referenced is installed by the
// LabJackUD installer.
using LabJack.LabJackUD; 

namespace U3_Simple
{
	class U3_Simple
	{
		// our U3 variable
		private U3 u3;

		static void Main(string[] args)
		{
			U3_Simple a = new U3_Simple();
			a.performActions();
		}

		// If error occured print a message indicating which one occurred. If the error is a group error (communication/fatal), quit
		public void ShowErrorMessage(LabJackUDException e)
		{
			Console.Out.WriteLine("Error: " + e.ToString());
			if (e.LJUDError > U3.LJUDERROR.MIN_GROUP_ERROR)
			{
				Console.ReadLine(); // Pause for the user
				Environment.Exit(-1);
			}
		}

		public void performActions()
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
			Console.Out.WriteLine("UD Driver Version = {0:0.000}\n\n",dblDriverVersion);

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

				//Set DAC0 to 3.5 volts.
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.PUT_DAC, 0, 3.5, 0, 0);

				//Set digital output FIO4 to output-high.
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.PUT_DIGITAL_BIT, 4, 1, 0, 0);

				//Read digital input FIO5.
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.GET_DIGITAL_BIT, 5, 0, 0, 0);

				//Read digital inputs FIO5 through FIO6.
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.GET_DIGITAL_PORT, 5, 0, 2, 0);

				//Request the value of Counter1 (FIO7).
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.GET_COUNTER, 1, 0, 0, 0);
			}
			catch (LabJackUDException e) 
			{
				ShowErrorMessage(e);
			}
			bool requestedExit = false;
			while (!requestedExit)
			{
				try
				{
					//Execute the requests.
					LJUD.GoOne (u3.ljhandle);

					//Get all the results.  The input measurement results are stored.  All other
					//results are for configuration or output requests so we are just checking
					//whether there was an error.
					LJUD.GetFirstResult(u3.ljhandle, ref ioType, ref channel, ref dblValue, ref dummyInt, ref dummyDouble);
				}
				catch (LabJackUDException e) 
				{
					ShowErrorMessage(e);
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
					catch (LabJackUDException e) 
					{
						// If we get an error, report it.  If the error is NO_MORE_DATA_AVAILABLE we are done
						if(e.LJUDError == U3.LJUDERROR.NO_MORE_DATA_AVAILABLE)
							finished = true;
						else
							ShowErrorMessage(e);
					}
				}

				Console.Out.WriteLine("AIN0 = {0:0.###}\n",Value0);
				Console.Out.WriteLine("AIN1 = {0:0.###}\n",Value1);
				Console.Out.WriteLine("AIN2 = {0:0.###}\n",Value2);
				Console.Out.WriteLine("FIO5 = {0:0.###}\n",ValueDIBit);
				Console.Out.WriteLine("FIO5-FIO6 = {0:0.###}\n",ValueDIPort);  //Will read 3 (binary 11) if both lines are pulled-high as normal.
				Console.Out.WriteLine("Counter1 (FIO7) = {0:0.###}\n",ValueCounter);

				Console.Out.WriteLine("\nPress Enter to go again or (q) to quit\n");
				requestedExit = Console.ReadLine().Equals("q");
			}
		}
	}

}
