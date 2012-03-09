//---------------------------------------------------------------------------
//
//  U3_Thermocouple.cs
// 
//  Demonstrates using the TCVoltsToTemp to yield temperature readings for
//	a thermocouple connected to a U3 via a LJTick-InAmp
//
//  Setup:
//  TC plus to INA+.
//	TC minus to INA-.
//	10k resistor from INA- to GND on LJTIA (a short is also acceptable in most cases).
//	LJTIA connected to FIO4/FIO5 block on U3.
//	LJTIA offset set to 0.4 volts.
//	LJTIA channel A gain set to 51.
//
//  For the best accuracy, we recommend doing a quick offset calibration as
//	described in the comments below, and using an external cold-junction
//	temperature sensor rather than the internal U3 temp sensor.  The internal
//	temp sensor is often sufficient, but an external sensor, such as 
//	the LM34CAZ (national.com) or the EI-1034 (labjack.com), placed near
//	the LJTIA, can provide better CJC, particularly when the U3 itself
//	is subjected to varying temperatures.
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

namespace Thermocouple
{
	class Thermocouple
	{
		// our U3 variable
		private U3 u3;

		static void Main(string[] args)
		{
			Thermocouple a = new Thermocouple();
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
			double valueAIN = 0;  //Analog Voltage Value

			// Variables to satisfy certain method signatures
			int dummyInt = 0;
			double dummyDouble = 0;
	
			long time=0, j=0;
			LJUD.CHANNEL tempChannel = (LJUD.CHANNEL)4;  //Channel which the TC/LJTIA is on (FIO4).

			double tcVolts = 0, cjTempK = 0, pTCTempK = 0;
			LJUD.THERMOCOUPLETYPE tcType = LJUD.THERMOCOUPLETYPE.K;
			//Set the temperature sensor to a k type thermocouple
			//Possible Thermocouple types are:
			//B = 6001
			//E = 6002
			//J = 6003
			//K = 6004
			//N = 6005
			//R = 6006
			//S = 6007
			//T = 6008

			//Offset calibration:  The nominal voltage offset of the LJTick is
			//0.4 volts.  For improved accuracy, though, you should measure the
			//overall system offset.  We know that if the end of the TC is at the
			//same temperature as the cold junction, the voltage should be zero.
			//Put the end of the TC near the LJTIA to make sure they are at the same
			//temperature, and note the voltage measured by FIO4.  This is the actual
			//offset that can be entered below.
			double offsetVoltage = 0.4;

			try 
			{
				//Open the first found LabJack U3 via USB.
				u3 = new U3(LJUD.CONNECTION.USB, "0", true); // Connection through USB

				//Reset all U3 ports to their default configuration.  
				//This is recommended prior to using the U3 to ensure the U3's ports are 
				//in the proper state.
				LJUD.ePut (u3.ljhandle, LJUD.IO.PIN_CONFIGURATION_RESET, 0, 0, 0);
	
				LJUD.ePut (u3.ljhandle, LJUD.IO.PUT_ANALOG_ENABLE_BIT, (LJUD.CHANNEL)tempChannel, 1, 0);
			}
			catch (LabJackUDException e) 
			{
				showErrorMessage(e);
			}

			Console.Out.WriteLine("Press any key to quit\n");

			//Constantly acquire temperature readings until a key is pressed
			bool keyPressed = false;
			while(!keyPressed) 
			{
				ioType = 0;
				channel = 0;
				time = 0;
				tcVolts = 0;
				cjTempK = 0;
				pTCTempK = 0;
				
				//Add analog input requests.
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.GET_AIN, tempChannel, 0, 0, 0);

				//Add request for internal temperature reading -- Internal temp sensor uses 
				//analog input channel 30.
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.GET_AIN, 30, 0, 0, 0);

				//Execute all requests on the labjack u3.ljhandle.
				LJUD.GoOne (u3.ljhandle);

				//Get all the results.  The first result should be the voltage reading of the 
				//temperature channel.
				LJUD.GetFirstResult(u3.ljhandle, ref ioType, ref channel, ref dblValue, ref dummyInt, ref dummyDouble);

				//Get the rest of the results.  There should only be one more on the request 
				//queue.
				bool finished = false;
				while(!finished) 
				{
					if(ioType == LJUD.IO.GET_AIN) 
					{
						if(channel == tempChannel)
							valueAIN = dblValue;
				
						if(channel == (LJUD.CHANNEL)30)
							cjTempK = dblValue;
					}	
			
					try { LJUD.GetNextResult(u3.ljhandle, ref ioType, ref channel, ref dblValue, ref dummyInt, ref dummyDouble); }
					catch (LabJackUDException e)
					{
						if (e.LJUDError == LJUD.LJUDERROR.NO_DATA_AVAILABLE)
							finished = true;
						else if (e.LJUDError > LJUD.LJUDERROR.MIN_GROUP_ERROR)
							finished = true;
						else
							showErrorMessage(e);
					}
				}


				//Display Voltage Reading
				Console.Out.WriteLine("Analog {0:0}:           {1:0.######}\n", (int)tempChannel, valueAIN);

				//Display the internal temperature sensor reading.  This example uses
				//that value for cold junction compensation.
				Console.Out.WriteLine("U3 internal sensor:  {0:0.0} deg K\n", (double)cjTempK);

				//To get the thermocouple voltage we subtract the offset from the AIN
				//voltage and divide by the LJTIA gain.
				tcVolts = (valueAIN - offsetVoltage)/51;

				//Convert TC voltage to temperature.
				LJUD.TCVoltsToTemp(tcType, tcVolts, cjTempK, ref pTCTempK);

				//Display Temperature
				Console.Out.WriteLine("Thermocouple sensor:  {0:0.0} deg K\n\n", pTCTempK);

				Thread.Sleep(1500); // Short pause

				keyPressed = Win32Interop._kbhit() != 0; // If a key was hit break out of the loop
			}
		}
		
	}
	public class Win32Interop
	{
		[DllImport("crtdll.dll")]
		public static extern int _kbhit();
	}
}