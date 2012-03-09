//---------------------------------------------------------------------------
//
//  U3_EFunctions.cs
// 
//  Demonstrates the UD E-functions with the LabJack U3.  For timer/counter
//  testing, connect FIO4 to FIO5 and FIO6.
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

namespace U3_EFunctions
{
	class U3_EFunctions
	{
		// our U3 variable
		private U3 u3;

		static void Main(string[] args)
		{
			U3_EFunctions a = new U3_EFunctions();
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
			double dblValue=0;
			int intValue=0;

			int binary;
			int[] aEnableTimers = new int[2];
			int[] aEnableCounters = new int[2];
			int tcpInOffset;
			int timerClockDivisor;
			LJUD.TIMERCLOCKS timerClockBaseIndex;
			int[] aTimerModes = new int[2];
			double[] adblTimerValues = new double[2];
			int[] aReadTimers = new int[2];
			int[] aUpdateResetTimers = new int[2];
			int[] aReadCounters = new int[2];
			int[] aResetCounters = new int[2];
			double[] adblCounterValues = {0,0};
			double highTime, lowTime, dutyCycle;
			
			try 
			{
				//Open the first found LabJack U3.
				u3 = new U3(LJUD.CONNECTION.USB, "0", true); // Connection through USB
			
				//Start by using the pin_configuration_reset IOType so that all
				//pin assignments are in the factory default condition.
				LJUD.ePut (u3.ljhandle, LJUD.IO.PIN_CONFIGURATION_RESET, 0, 0, 0);

				//Take a single-ended measurement from AIN3.
				binary = 0;
				LJUD.eAIN( u3.ljhandle, 3, 31, ref dblValue, -1, -1, -1, binary);
				Console.Out.WriteLine("AIN3 = {0:0.###}\n",dblValue);

				//Set DAC0 to 3.0 volts.
				dblValue = 3.0;
				binary = 0;
				LJUD.eDAC (u3.ljhandle, 0, dblValue, binary, 0, 0);
				Console.Out.WriteLine("DAC0 set to {0:0.###} volts\n",dblValue);

				//Read state of FIO4.
				LJUD.eDI (u3.ljhandle, 4, ref intValue);
				Console.Out.WriteLine("FIO4 = {0:0.#}\n",intValue);

				//Set the state of FIO7.
				intValue = 1;
				LJUD.eDO (u3.ljhandle, 7, intValue);
				Console.Out.WriteLine("FIO7 set to = {0:0.#}\n\n",intValue);

				//Timers and Counters example.
				//First, a call to eTCConfig.  Fill the arrays with the desired values, then make the call.
				aEnableTimers[0] = 1; //Enable Timer0 (uses FIO4).
				aEnableTimers[1] = 1; //Enable Timer1 (uses FIO5).
				aEnableCounters[0] = 0; //Disable Counter0.
				aEnableCounters[1] = 1; //Enable Counter1 (uses FIO6).
				tcpInOffset = 4;  //Offset is 4, so timers/counters start at FIO4.
				timerClockBaseIndex = LJUD.TIMERCLOCKS.MHZ48_DIV;  //Base clock is 48 MHz with divisor support, so Counter0 is not available.
				//timerClockBaseIndex = LJUD.TIMERCLOCKS.MHZ24_DIV;  //Use this line instead for hardware rev 1.20.
				timerClockDivisor = 48; //Thus timer clock is 1 MHz.
				//timerClockDivisor = 24;  //Use this line instead for hardware rev 1.20.
				aTimerModes[0] = (int) LJUD.TIMERMODE.PWM8; //Timer0 is 8-bit PWM output.  Frequency is 1M/256 = 3906 Hz.
				aTimerModes[1] = (int) LJUD.TIMERMODE.DUTYCYCLE; //Timer1 is duty cyle input.
				adblTimerValues[0] = 16384; //Set PWM8 duty-cycle to 75%.
				adblTimerValues[1] = 0;
				LJUD.eTCConfig(u3.ljhandle, aEnableTimers, aEnableCounters, tcpInOffset, (int) timerClockBaseIndex, timerClockDivisor, aTimerModes, adblTimerValues, 0, 0);
				Console.Out.WriteLine("Timers and Counters enabled.\n\n");

				Thread.Sleep(1000); //Wait 1 second.

				//Now, a call to eTCValues.
				aReadTimers[0] = 0; //Don't read Timer0 (output timer).
				aReadTimers[1] = 1; //Read Timer1;
				aUpdateResetTimers[0] = 1; //Update Timer0;
				aUpdateResetTimers[1] = 1; //Reset Timer1;
				aReadCounters[0] = 0;
				aReadCounters[1] = 1; //Read Counter1;
				aResetCounters[0] = 0;
				aResetCounters[1] = 1; //Reset Counter1.
				adblTimerValues[0] = 32768; //Change Timer0 duty-cycle to 50%.
				adblTimerValues[1] = 0;
				LJUD.eTCValues(u3.ljhandle, aReadTimers, aUpdateResetTimers, aReadCounters, aResetCounters, adblTimerValues, adblCounterValues, 0, 0);
				Console.Out.WriteLine("Timer1 value = {0:0.000}\n",adblTimerValues[1]);
				Console.Out.WriteLine("Counter1 value = {0:0.000}\n",adblCounterValues[1]);

				//Convert Timer1 value to duty-cycle percentage.
				//High time is LSW
				highTime = (double)(((ulong)adblTimerValues[1]) % (65536));
				//Low time is MSW
				lowTime = (double)(((ulong)adblTimerValues[1]) / (65536));
				//Calculate the duty cycle percentage.
				dutyCycle = 100*highTime/(highTime+lowTime);
				Console.Out.WriteLine("\nHigh clicks Timer1 = {0:0.0}\n",highTime);
				Console.Out.WriteLine("Low clicks Timer1 = {0:0.0}\n",lowTime);
				Console.Out.WriteLine("Duty cycle Timer1 = {0:0.0}\n",dutyCycle);


				//Disable all timers and counters.
				aEnableTimers[0] = 0;
				aEnableTimers[1] = 0;
				aEnableCounters[0] = 0;
				aEnableCounters[1] = 0;
				LJUD.eTCConfig(u3.ljhandle, aEnableTimers, aEnableCounters, 4, (int)timerClockBaseIndex, timerClockDivisor, aTimerModes, adblTimerValues, 0, 0);
			}
			catch (LabJackUDException e) 
			{
				showErrorMessage(e);
			}
			Console.ReadLine(); // Pause for user
		}
	}
}

