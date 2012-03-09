//---------------------------------------------------------------------------
//
//  U3_TimerCounter.cs
// 
//  Basic U3 example does a PWM output and a counter input, using AddGoGet method.
//
//  support@labjack.com
//  June 8, 2009
//	Revised December 28, 2010
//----------------------------------------------------------------------
//

using System;
using System.Threading;
using System.Runtime.InteropServices;

// Import the UD .NET wrapper object.  The dll referenced is installed by the
// LabJackUD installer.
using LabJack.LabJackUD; 

namespace TimerCounter
{
	class TimerCounter
	{
		// our U3 variable
		private U3 u3;

		static void Main(string[] args)
		{
			TimerCounter a = new TimerCounter();
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
			double dblValue=0;

			// Variables to satisfy certain method signatures
			int dummyInt = 0;
			double dummyDouble = 0;
			double[] dummyDoubleArray = {0};

			try 
			{
				//Open the first found LabJack U3.
				u3 = new U3(LJUD.CONNECTION.USB, "0", true); // Connection through USB
			
				//Start by using the pin_configuration_reset IOType so that all
				//pin assignments are in the factory default condition.
				LJUD.ePut (u3.ljhandle, LJUD.IO.PIN_CONFIGURATION_RESET, 0, 0, 0);


				//First requests to configure the timer and counter.  These will be
				//done with and add/go/get block.

				//Set the timer/counter pin offset to 4, which will put the first
				//timer/counter on FIO4.
				LJUD.AddRequest (u3.ljhandle,  LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.TIMER_COUNTER_PIN_OFFSET, 4, 0, 0);

				//Use the 48 MHz timer clock base with divider.  Since we are using clock with divisor
				//support, Counter0 is not available.
				LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.TIMER_CLOCK_BASE, (double) LJUD.TIMERCLOCKS.MHZ48_DIV, 0, 0);
				//LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.TIMER_CLOCK_BASE, LJUD.TIMERCLOCKS.MHZ24_DIV, 0, 0);  //Use this line instead for hardware rev 1.20.

				//Set the divisor to 48 so the actual timer clock is 1 MHz.
				LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.TIMER_CLOCK_DIVISOR, 48, 0, 0);
				//LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.TIMER_CLOCK_DIVISOR, 24, 0, 0);  //Use this line instead for hardware rev 1.20.

				//Enable 1 timer.  It will use FIO4.
				LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.NUMBER_TIMERS_ENABLED, 1, 0, 0);

				//Configure Timer0 as 8-bit PWM.  Frequency will be 1M/256 = 3906 Hz.
				LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_TIMER_MODE, 0, (double) LJUD.TIMERMODE.PWM8, 0, 0);

				//Set the PWM duty cycle to 50%.
				LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_TIMER_VALUE, 0, 32768, 0, 0);

				//Enable Counter1.  It will use FIO5 since 1 timer is enabled.
				LJUD.AddRequest(u3.ljhandle, LJUD.IO.PUT_COUNTER_ENABLE, 1, 1, 0, 0);

				//Execute the requests.
				LJUD.GoOne (u3.ljhandle);
			}
			catch (LabJackUDException e) 
			{
				showErrorMessage(e);
			}

			//Get all the results just to check for errors.
			try { LJUD.GetFirstResult(u3.ljhandle, ref ioType, ref channel, ref dblValue, ref dummyInt, ref dummyDouble); }
			catch (LabJackUDException e) { showErrorMessage(e); }
			bool finished = false;
			while(!finished)
			{
				try { LJUD.GetNextResult(u3.ljhandle, ref ioType, ref channel, ref dblValue, ref dummyInt, ref dummyDouble); }
				catch (LabJackUDException e) 
				{
					// If we get an error, report it.  If the error is NO_MORE_DATA_AVAILABLE we are done
					if(e.LJUDError == UE9.LJUDERROR.NO_MORE_DATA_AVAILABLE)
						finished = true;
					else
						showErrorMessage(e);
				}
			}

			try
			{
				//Wait 1 second.
				Thread.Sleep(1000);

				//Request a read from the counter.
				LJUD.eGet(u3.ljhandle, LJUD.IO.GET_COUNTER, (LJUD.CHANNEL) 1, ref dblValue, dummyDoubleArray);

				//This should read roughly 4k counts if FIO4 is shorted to FIO5.
				Console.Out.WriteLine("Counter = {0:0.0}\n",dblValue);

				//Wait 1 second.
				Thread.Sleep(1000);

				//Request a read from the counter.
				LJUD.eGet(u3.ljhandle, LJUD.IO.GET_COUNTER, (LJUD.CHANNEL) 1, ref dblValue, dummyDoubleArray);

				//This should read about 3906 counts more than the previous read.
				Console.Out.WriteLine("Counter = {0:0.0}\n",dblValue);

				//Reset all pin assignments to factory default condition.
				LJUD.ePut (u3.ljhandle, LJUD.IO.PIN_CONFIGURATION_RESET, 0, 0, 0);

				//The PWM output sets FIO4 to output, so we do a read here to set
				//it to input.
				LJUD.eGet (u3.ljhandle, LJUD.IO.GET_DIGITAL_BIT, (LJUD.CHANNEL) 4, ref dblValue, dummyDoubleArray);
			}
			catch (LabJackUDException e) 
			{
				showErrorMessage(e);
			}

			Console.ReadLine(); // Pause for user	
	
		}
	}
}
