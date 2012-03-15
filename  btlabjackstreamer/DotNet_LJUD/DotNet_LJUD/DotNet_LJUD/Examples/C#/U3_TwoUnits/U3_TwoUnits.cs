//---------------------------------------------------------------------------
//
//  U3_TwoUnits.cs
// 
//  Simple example demonstrates communication with 2 U3s.
//  Local ids can be set from LJControlPannel
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

namespace TwoUnits
{
	class TwoUnits
	{
		// our U3 variables
		private U3 unit2, unit3; // Units with ids 2 and 3

		static void Main(string[] args)
		{
			TwoUnits a = new TwoUnits();
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
			double dblDriverVersion;
			LJUD.IO ioType=0;
			LJUD.CHANNEL channel=0;
			double dblValue=0;
			double Value22=9999,Value32=9999,Value42=9999;
			double Value23=9999,Value33=9999,Value43=9999;
			
			// Variables to satisfy certain method signatures
			int dummyInt = 0;
			double dummyDouble = 0;

			//Read and display the UD version.
			dblDriverVersion = LJUD.GetDriverVersion();
			Console.Out.WriteLine("UD Driver Version = {0:0.000}\n\n",dblDriverVersion);


			//Open the U3 with local ID 2.
			try 
			{
				unit2 = new U3(LJUD.CONNECTION.USB, "2", false); // Connection through USB
				unit3 = new U3(LJUD.CONNECTION.USB, "3", false); // Connection through USB

				//Start by using the pin_configuration_reset IOType so that all
				//pin assignments are in the factory default condition.
				LJUD.ePut (unit2.ljhandle, LJUD.IO.PIN_CONFIGURATION_RESET, 0, 0, 0);
				LJUD.ePut (unit3.ljhandle, LJUD.IO.PIN_CONFIGURATION_RESET, 0, 0, 0);


				//First a configuration command.  These will be done with the ePut
				//function which combines the add/go/get into a single call.

				//Configure FIO2-FIO4 as analog, all else as digital, on both devices.
				//That means we will start from channel 0 and update all 16 flexible bits.
				//We will pass a value of b0000000000011100 or d28.
				LJUD.ePut (unit2.ljhandle, LJUD.IO.PUT_ANALOG_ENABLE_PORT, 0, 28, 16);
				LJUD.ePut (unit3.ljhandle, LJUD.IO.PUT_ANALOG_ENABLE_PORT, 0, 28, 16);


				//The following commands will use the add-go-get method to group
				//multiple requests into a single low-level function.

				//Request a single-ended reading from AIN2.
				LJUD.AddRequest (unit2.ljhandle, LJUD.IO.GET_AIN, 2, 0, 0, 0);
				LJUD.AddRequest (unit3.ljhandle, LJUD.IO.GET_AIN, 2, 0, 0, 0);

				//Request a single-ended reading from AIN3.
				LJUD.AddRequest (unit2.ljhandle, LJUD.IO.GET_AIN, 3, 0, 0, 0);
				LJUD.AddRequest (unit3.ljhandle, LJUD.IO.GET_AIN, 3, 0, 0, 0);

				//Request a reading from AIN4 using the Special 0-3.6 range.
				LJUD.AddRequest (unit2.ljhandle, LJUD.IO.GET_AIN_DIFF, 4, 0, 32, 0);
				LJUD.AddRequest (unit3.ljhandle, LJUD.IO.GET_AIN_DIFF, 4, 0, 32, 0);
			}
			catch (LabJackUDException e) 
			{
				showErrorMessage(e);
			}

			bool isFinished = false;
			while (!isFinished)
			{

				try
				{
					//Execute all requests on all open LabJacks.
					LJUD.Go();

					//Get all the results for unit 2.  The input measurement results are stored.
					LJUD.GetFirstResult(unit2.ljhandle, ref ioType, ref channel, ref dblValue, ref dummyInt, ref dummyDouble);
				}
				catch (LabJackUDException e) 
				{
					showErrorMessage(e);
				}

				bool unit2Finished = false;
				while(!unit2Finished)
				{
					switch(ioType)
					{

						case LJUD.IO.GET_AIN :
						switch((int)channel)
						{
							case 2:
								Value22=dblValue;
								break;
							case 3:
								Value32=dblValue;
								break;
						}
							break;

						case LJUD.IO.GET_AIN_DIFF :
							Value42=dblValue;
							break;

					}

					try { LJUD.GetNextResult(unit2.ljhandle, ref ioType, ref channel, ref dblValue, ref dummyInt, ref dummyDouble); }
					catch (LabJackUDException e)
					{
						// If we get an error, report it.  If the error is NO_MORE_DATA_AVAILABLE we are done
						if(e.LJUDError == UE9.LJUDERROR.NO_MORE_DATA_AVAILABLE)
							unit2Finished = true;
						else
							showErrorMessage(e);
					}

				}


				//Get all the results for unit 3.  The input measurement results are stored.
				try { LJUD.GetFirstResult(unit3.ljhandle, ref ioType, ref channel, ref dblValue, ref dummyInt, ref dummyDouble); }
				catch (LabJackUDException e)  { showErrorMessage(e); }

				bool unit3Finished = false;
				while(!unit3Finished)
				{
					switch(ioType)
					{

						case LJUD.IO.GET_AIN :
						switch((int)channel)
						{
							case 2:
								Value23=dblValue;
								break;
							case 3:
								Value33=dblValue;
								break;
						}
							break;

						case LJUD.IO.GET_AIN_DIFF :
							Value43=dblValue;
							break;

					}

					try { LJUD.GetNextResult(unit3.ljhandle, ref ioType, ref channel, ref dblValue, ref dummyInt, ref dummyDouble); }
					catch (LabJackUDException e)
					{
						// If we get an error, report it.  If the error is NO_MORE_DATA_AVAILABLE we are done
						if(e.LJUDError == UE9.LJUDERROR.NO_MORE_DATA_AVAILABLE)
							unit3Finished = true;
						else
							showErrorMessage(e);
					}

				}

				Console.Out.WriteLine("AIN2 (Unit 2) = {0:0.###}\n",Value22);
				Console.Out.WriteLine("AIN2 (Unit 3) = {0:0.###}\n",Value23);
				Console.Out.WriteLine("AIN3 (Unit 2) = {0:0.###}\n",Value32);
				Console.Out.WriteLine("AIN3 (Unit 3) = {0:0.###}\n",Value33);
				Console.Out.WriteLine("AIN4 (Unit 2) = {0:0.###}\n",Value42);
				Console.Out.WriteLine("AIN4 (Unit 3) = {0:0.###}\n",Value43);
	
				Console.Out.WriteLine("\nPress Enter to go again or (q) to quit\n");
				isFinished = Console.ReadLine() == "q"; // Pause for user
			}
		}
	}
}
