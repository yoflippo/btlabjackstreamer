//---------------------------------------------------------------------------
//
//  U3_AllIO.cs
// 
//	Demonstrates using the add/go/get method to efficiently write and read
//	virtually all analog and digital I/O on the LabJack U3.
//	Records the time for 1000 iterations and divides by 1000, to allow
//	verification of the basic command/response communication times of the
//	LabJack U3 as documented in Section 3.1 of the U3 User's Guide.
//
//  support@labjack.com
//  June 5, 2009
//	Revised December 28, 2010
//----------------------------------------------------------------------
//

using System;

// Import the UD .NET wrapper object.  The dll referenced is installed by the
// LabJackUD installer.
using LabJack.LabJackUD; 

namespace AllIO
{
	class AllIO
	{
		// our U3 variable
		private U3 u3;

		static void Main(string[] args)
		{
			AllIO a = new AllIO();
			a.performActions();
		}

		// If error occured print a message indicating which one occurred. If the error is a group error (communication/fatal), quit
		public void showErrorMessage(LabJackUDException e)
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

			LJUD.IO ioType=0;
			LJUD.CHANNEL channel=0;
			double dblVal=0;
			double ValueDIPort=0;
			int intVal=0;
			double val=0;
			double[] ValueAIN = new double[16];
			

			long time=0;
			long numIterations = 1;
			int numChannels = 16;  //Number of AIN channels, 0-16.
			long quickSample = 0;  //Set to TRUE for quick AIN sampling. See section 2.6 / 3.1 of the User's Guide
			long longSettling = 1;  //Set to TRUE for extra AIN settling time.

			try 
			{
				//Open the first found LabJack.
				u3 = new U3(LJUD.CONNECTION.USB, "0", true); // Connection through USB
			
				//Start by using the pin_configuration_reset IOType so that all
				//pin assignments are in the factory default condition.
				LJUD.ePut (u3.ljhandle, LJUD.IO.PIN_CONFIGURATION_RESET, 0, 0, 0);

				//Configure quickSample.
				LJUD.ePut (u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.AIN_RESOLUTION, quickSample, 0);

				//Configure longSettling.
				LJUD.ePut (u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.AIN_SETTLING_TIME, longSettling, 0);
			
				//Configure the necessary lines as analog.
				LJUD.ePut (u3.ljhandle, LJUD.IO.PUT_ANALOG_ENABLE_PORT, 0, Math.Pow(2,numChannels)-1, numChannels);
			
				//Now an Add/Go/Get block to configure the timers and counters.  These
				//are configured on EIO0-EIO3, so if more than 8 analog inputs are
				//enabled then the analog inputs use these lines.
				if(numChannels <= 8)
				{
					//Set the timer/counter pin offset to 8, which will put the first
					//timer/counter on EIO0.
					LJUD.AddRequest (u3.ljhandle,  LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.TIMER_COUNTER_PIN_OFFSET, 8, 0, 0);

					//Use the default clock source.
					LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.TIMER_CLOCK_BASE, (double) LJUD.TIMERCLOCKS.MHZ48, 0, 0);

					//Enable 2 timers.
					LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.NUMBER_TIMERS_ENABLED, 2, 0, 0);

					//Configure Timer0 as 8-bit PWM.
					LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_TIMER_MODE, 0, (double) LJUD.TIMERMODE.PWM8, 0, 0);

					//Set the PWM duty cycle to 50%.
					LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_TIMER_VALUE, 0, 32768, 0, 0);

					//Configure Timer1 as 8-bit PWM.
					LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_TIMER_MODE, 1, (double) LJUD.TIMERMODE.PWM8, 0, 0);

					//Set the PWM duty cycle to 50%.
					LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_TIMER_VALUE, 1, 32768, 0, 0);

					//Enable Counter0.
					LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_COUNTER_ENABLE, 0, 1, 0, 0);

					LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_COUNTER_ENABLE, 1, 1, 0, 0);

					//Execute the requests.
					LJUD.GoOne (u3.ljhandle);
				}



				//Now add requests that will be processed every iteration of the loop.

				//Add analog input requests.
				for(int j=0; j<numChannels; j++)
				{
					LJUD.AddRequest (u3.ljhandle, LJUD.IO.GET_AIN, j, 0, 0, 0);
				}

				//Set DAC0 to 2.5 volts.
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.PUT_DAC, 0, 2.5, 0, 0);

				//Read CIO digital lines.
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.GET_DIGITAL_PORT, 16, 0, 4, 0);

				//Only do the timer/counter stuff if there are less than 8 analog inputs.
				if(numChannels <= 8)
				{
					LJUD.AddRequest (u3.ljhandle, LJUD.IO.GET_COUNTER, 0, 0, 0, 0);

					LJUD.AddRequest (u3.ljhandle, LJUD.IO.GET_COUNTER, 1, 0, 0, 0);

					LJUD.AddRequest (u3.ljhandle, LJUD.IO.GET_TIMER, 0, 0, 0, 0);

					LJUD.AddRequest (u3.ljhandle, LJUD.IO.GET_TIMER, 1, 0, 0, 0);

					//Set the PWM duty cycle to 50%.
					LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_TIMER_VALUE, 0, 32768, 0, 0);

					//Set the PWM duty cycle to 50%.
					LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_TIMER_VALUE, 1, 32768, 0, 0);
				}


				time = Environment.TickCount;
			}
			catch (LabJackUDException e) 
			{
				showErrorMessage(e);
			}

			for(int i=0;i<numIterations;i++)
			{
				
				try
				{
					//Execute the requests.
					LJUD.GoOne(u3.ljhandle);

					//Get all the results.  The input measurement results are stored.  All other
					//results are for configuration or output requests so we are just checking
					//whether there was an error.
					LJUD.GetFirstResult(u3.ljhandle, ref ioType, ref channel, ref val, ref intVal, ref dblVal);
				}
				catch (LabJackUDException e) 
				{
					showErrorMessage(e);
				}
				
				// Get results until there is no more data available
				bool isFinished = false;
				while(!isFinished)
				{
					switch(ioType)
					{

						case LJUD.IO.GET_AIN:
							ValueAIN[(int)channel]=val;
							break;

						case LJUD.IO.GET_DIGITAL_PORT:
							ValueDIPort=val;
							break;
					}

					try 
					{
						LJUD.GetNextResult(u3.ljhandle, ref ioType, ref channel, ref val, ref intVal, ref dblVal);
					}
					catch (LabJackUDException e) 
					{
						// If we get an error, report it.  If there is no more data available we are done
						if(e.LJUDError == UE9.LJUDERROR.NO_MORE_DATA_AVAILABLE)
							isFinished = true;
						else
							showErrorMessage(e);
					}
				}

			}


			time = Environment.TickCount - time;

			Console.Out.WriteLine("Milleseconds per iteration = {0:0.000}\n", (double)time / (double)numIterations);

			Console.Out.WriteLine("\nDigital Input = {0:0.###}\n",ValueDIPort);

			Console.Out.WriteLine("\nAIN readings from last iteration:\n");
			for(int j=0; j<numChannels; j++)
			{
				Console.Out.WriteLine("{0:0.000}\n", ValueAIN[j]);
			}

			Console.ReadLine(); // Pause for user	

		}
	}
}
