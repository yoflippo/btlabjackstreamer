//---------------------------------------------------------------------------
//
//  U3_Asynch.cs
// 
//	Demonstrates asynchronous communication using a loopback from
//	FIO4 to FIO5 on a U3 rev 1.30.  On earlier hardware revisions
//  use SDA and SCL.
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

namespace U3_Asynch
{
	class U3_Asynch
	{
		// our U3 variable
		private U3 u3;

		static void Main(string[] args)
		{
			U3_Asynch a = new U3_Asynch();
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
			//long lngGetNextIteration;
			//LJUD.IO ioType=0, channel=0;
			//double dblValue=0;

			//double numI2CBytesToWrite;
			double numBytes = 0;
			byte[] array = new byte[256];

			try 
			{
				//Open the LabJack.
				u3 = new U3(LJUD.CONNECTION.USB, "0", true); // Connection through USB

				//Start by using the pin_configuration_reset IOType so that all
				//pin assignments are in the factory default condition.
				LJUD.ePut (u3.ljhandle, LJUD.IO.PIN_CONFIGURATION_RESET, 0, 0, 0);


				// 1 MHz timer clock base.
				LJUD.ePut(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.TIMER_CLOCK_BASE, (double) LJUD.TIMERCLOCKS.MHZ1_DIV, 0);

				// Set clock divisor to 1, so timer clock is 1 MHz.
				LJUD.ePut(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.TIMER_CLOCK_DIVISOR, (double) LJUD.TIMERCLOCKS.MHZ1_DIV, 0);

				// Set timer/counter pin offset to 4. TX and RX appear after any timers and counters on U3
				// hardware rev 1.30.  We have no timers or counters enabled, so TX=FIO4 and RX=FIO5.
				LJUD.ePut(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.TIMER_COUNTER_PIN_OFFSET, 4, 0);

				// Set data rate for 9600 bps communication.
				LJUD.ePut(u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.ASYNCH_BAUDFACTOR, 204, 0);

				// Enable UART.
				LJUD.ePut(u3.ljhandle, LJUD.IO.ASYNCH_COMMUNICATION, LJUD.CHANNEL.ASYNCH_ENABLE, 1, 0);

				// Transmit 2 bytes.
				numBytes = 3;
				array[0] = 20;
				array[1] = 75;
				LJUD.eGet(u3.ljhandle, LJUD.IO.ASYNCH_COMMUNICATION, LJUD.CHANNEL.ASYNCH_TX, ref numBytes, array);

				// Read 2 bytes.
				numBytes = 9999;  //Dummy values so we can see them change.
				array[0] = 111;
				array[1] = 111;
				LJUD.eGet(u3.ljhandle, LJUD.IO.ASYNCH_COMMUNICATION, LJUD.CHANNEL.ASYNCH_RX, ref numBytes, array);
			}
			catch (LabJackUDException e) 
			{
				showErrorMessage(e);
			}

			//Display the read data.
			Console.Out.WriteLine("Pre-read buffer size = {0:0}\n\n",numBytes);
			Console.Out.WriteLine("Read data = {0:0.#}, {1:0.#}\n\n",array[0],array[1]);


			Console.ReadLine(); // Pause for user

		}
	}
}