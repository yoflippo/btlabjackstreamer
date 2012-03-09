//---------------------------------------------------------------------------
//
//  U3_SimpleStream.cs
// 
//	2-channel stream on the U3.
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

namespace SimpleStream
{
	class SimpleStream
	{
		// our U3 variable
		private U3 u3;

		static void Main(string[] args)
		{
			SimpleStream a = new SimpleStream();
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
			long i=0,k=0;
			LJUD.IO ioType=0;
			LJUD.CHANNEL channel=0;
			double dblValue=0, dblCommBacklog=0, dblUDBacklog=0;
			double scanRate = 2000;
			int delayms = 1000;
			double numScans = 4000;  //2x the expected # of scans (2*scanRate*delayms/1000)
			double numScansRequested;
			double[] adblData = new double[8000];  //Max buffer size (#channels*numScansRequested)

			// Variables to satisfy certain method signatures
			int dummyInt = 0;
			double dummyDouble = 0;
			double[] dummyDoubleArray = {0};

			//Read and display the UD version.
			dblValue = LJUD.GetDriverVersion();
			Console.Out.WriteLine("UD Driver Version = {0:0.000}\n\n",dblValue);

			try 
			{
				//Open the first found LabJack U3.
				u3 = new U3(LJUD.CONNECTION.USB, "0", true); // Connection through USB

				//Read and display the hardware version of this U3.
				LJUD.eGet (u3.ljhandle, LJUD.IO.GET_CONFIG, LJUD.CHANNEL.HARDWARE_VERSION, ref dblValue, 0);
				Console.Out.WriteLine("U3 Hardware Version = {0:0.000}\n\n",dblValue);

				//Read and display the firmware version of this U3.
				LJUD.eGet (u3.ljhandle, LJUD.IO.GET_CONFIG, LJUD.CHANNEL.FIRMWARE_VERSION, ref dblValue, 0);
				Console.Out.WriteLine("U3 Firmware Version = {0:0.000}\n\n",dblValue);

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
				showErrorMessage(e);
			}
    
			//Get all the results just to check for errors.
			try {LJUD.GetFirstResult(u3.ljhandle, ref ioType, ref channel, ref dblValue, ref dummyInt, ref dummyDouble);}
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
    
			//Start the stream.
			try
			{ 
				LJUD.eGet(u3.ljhandle, LJUD.IO.START_STREAM, 0, ref dblValue, 0); 
			}
			catch (LabJackUDException e) { showErrorMessage(e); }

			//The actual scan rate is dependent on how the desired scan rate divides into
			//the LabJack clock.  The actual scan rate is returned in the value parameter
			//from the start stream command.
			Console.Out.WriteLine("Actual Scan Rate = {0:0.000}\n",dblValue);
			Console.Out.WriteLine("Actual Sample Rate = {0:0.000}\n",2*dblValue); // # channels * scan rate
    

			//Read data
			while(Win32Interop._kbhit() == 0)	//Loop will run until any key is hit
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
				for(k=0;k<numScans*2;k++)
				{
					adblData[k] = 9999.0;
				}

				try
				{
					//Read the data.  We will request twice the number we expect, to
					//make sure we get everything that is available.
					//Note that the array we pass must be sized to hold enough SAMPLES, and
					//the Value we pass specifies the number of SCANS to read.
					numScansRequested=numScans;
					LJUD.eGet(u3.ljhandle, LJUD.IO.GET_STREAM_DATA, LJUD.CHANNEL.ALL_CHANNELS, ref numScansRequested, adblData);
					
					//The displays the number of scans that were actually read.
					Console.Out.WriteLine("\nIteration # {0:0.#}\n",i);
					Console.Out.WriteLine("Number read = {0:0}\n",numScansRequested);
					
					//This displays just the first scan.
					Console.Out.WriteLine("First scan = {0:0.000}, {1:0.000}\n",adblData[0],adblData[1]);
					
					//Retrieve the current backlog.  The UD driver retrieves stream data from
					//the U3 in the background, but if the computer is too slow for some reason
					//the driver might not be able to read the data as fast as the U3 is
					//acquiring it, and thus there will be data left over in the U3 buffer.
					LJUD.eGet(u3.ljhandle, LJUD.IO.GET_CONFIG, LJUD.CHANNEL.STREAM_BACKLOG_COMM, ref dblCommBacklog, 0);
					Console.Out.WriteLine("Comm Backlog = {0:0}\n",dblCommBacklog);
					LJUD.eGet(u3.ljhandle, LJUD.IO.GET_CONFIG, LJUD.CHANNEL.STREAM_BACKLOG_UD, ref dblUDBacklog, 0);
					Console.Out.WriteLine("UD Backlog = {0:0}\n",dblUDBacklog);
					i++;
				}
				catch (LabJackUDException e) { showErrorMessage(e); }
			}

   
			//Stop the stream
			try{ LJUD.eGet(u3.ljhandle, LJUD.IO.STOP_STREAM, 0, ref dummyDouble, dummyDoubleArray); }
			catch (LabJackUDException e) { showErrorMessage(e); }

			Console.Out.WriteLine("\nDone");
			Console.ReadLine(); // Pause for user	

		}
	}

	public class Win32Interop
	{
		[DllImport("crtdll.dll")]
		public static extern int _kbhit();
	}
}
