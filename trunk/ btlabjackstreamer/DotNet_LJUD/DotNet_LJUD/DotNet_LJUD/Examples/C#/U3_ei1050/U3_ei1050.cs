//---------------------------------------------------------------------------
//
//  U3_ei1050.cs
// 
//	Demonstrates talking to 1 or 2 EI-1050 probes.
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

namespace U3_ei1050
{
	class U3_ei1050
	{
		// our U3 variable
		private U3 u3;

		static void Main(string[] args)
		{
			U3_ei1050 a = new U3_ei1050();
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

			//Open the first found LabJack U3.
			try 
			{
				u3 = new U3(LJUD.CONNECTION.USB, "0", true); // Connection through USB
			}
			catch (LabJackUDException e) 
			{
				showErrorMessage(e);
			}

			try
			{
				//Start by using the pin_configuration_reset IOType so that all
				//pin assignments are in the factory default condition.
				LJUD.ePut (u3.ljhandle, LJUD.IO.PIN_CONFIGURATION_RESET, 0, 0, 0);

				//Set the Data line to FIO4, which is the default anyway. 
				LJUD.ePut (u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.SHT_DATA_CHANNEL, 4, 0);

				//Set the Clock line to FIO5, which is the default anyway. 
				LJUD.ePut (u3.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.SHT_CLOCK_CHANNEL, 5, 0);

				//Set FIO6 to output-high to provide power to the EI-1050. 
				LJUD.ePut (u3.ljhandle, LJUD.IO.PUT_DIGITAL_BIT, (LJUD.CHANNEL)6, 1, 0);
			}
			catch (LabJackUDException e) 
			{
				showErrorMessage(e);
			}
			
			/*
			//Use this code if only a single EI-1050 is connected.
			//	Connections for one probe:
			//	Red (Power)         FIO6
			//	Black (Ground)      GND
			//	Green (Data)        FIO4
			//	White (Clock)       FIO5
			//	Brown (Enable)      FIO6

			try
			{
				//Now, an add/go/get block to get the temp & humidity at the same time.
				//Request a temperature reading from the EI-1050.
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.SHT_GET_READING, LJUD.CHANNEL.SHT_TEMP, 0, 0, 0);

				//Request a humidity reading from the EI-1050.
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.SHT_GET_READING, LJUD.CHANNEL.SHT_RH, 0, 0, 0);

				//Execute the requests.  Will take about 0.5 seconds with a USB high-high
				//or Ethernet connection, and about 1.5 seconds with a normal USB connection.
				LJUD.GoOne (u3.ljhandle);

				//Get the temperature reading.
				LJUD.GetResult (u3.ljhandle, LJUD.IO.SHT_GET_READING, LJUD.CHANNEL.SHT_TEMP, ref dblValue);
				Console.Out.WriteLine("Temp Probe A = {0:0.###} deg K\n",dblValue);
				Console.Out.WriteLine("Temp Probe A = {0:0.###} deg C\n",(dblValue-273.15));
				Console.Out.WriteLine("Temp Probe A = {0:0.###} deg F\n",(((dblValue-273.15)*1.8)+32));

				//Get the humidity reading.
				LJUD.GetResult (u3.ljhandle, LJUD.IO.SHT_GET_READING, LJUD.CHANNEL.SHT_RH, ref dblValue);
				Console.Out.WriteLine("RH Probe A = {0:0.###} percent\n\n",dblValue);
			}
			catch (LabJackUDException e) 
			{
				showErrorMessage(e);
			}

			//End of single probe code.
			*/


			///*
			//Use this code if two EI-1050 probes are connected.
			//	Connections for both probes:
			//	Red (Power)         FIO6
			//	Black (Ground)      GND
			//	Green (Data)        FIO4
			//	White (Clock)       FIO5
			//
			//	Probe A:
			//	Brown (Enable)    FIO7
			//
			//	Probe B:
			//	Brown (Enable)    DAC0

			try
			{
				//Set FIO7 to output-low to disable probe A. 
				LJUD.ePut (u3.ljhandle, LJUD.IO.PUT_DIGITAL_BIT, (LJUD.CHANNEL) 7, 0, 0);

				//Set DAC0 to 0 volts to disable probe B.
				LJUD.ePut (u3.ljhandle, LJUD.IO.PUT_DAC, 0, 0.0, 0);

				//Set FIO7 to output-high to enable probe A. 
				LJUD.ePut (u3.ljhandle, LJUD.IO.PUT_DIGITAL_BIT, (LJUD.CHANNEL) 7, 1, 0);

				//Now, an add/go/get block to get the temp & humidity at the same time.
				//Request a temperature reading from the EI-1050.
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.SHT_GET_READING, LJUD.CHANNEL.SHT_TEMP, 0, 0, 0);

				//Request a humidity reading from the EI-1050.
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.SHT_GET_READING, LJUD.CHANNEL.SHT_RH, 0, 0, 0);

				//Execute the requests.  Will take about 0.5 seconds with a USB high-high
				//or Ethernet connection, and about 1.5 seconds with a normal USB connection.
				LJUD.GoOne (u3.ljhandle);

				//Get the temperature reading.
				LJUD.GetResult (u3.ljhandle, LJUD.IO.SHT_GET_READING, LJUD.CHANNEL.SHT_TEMP, ref dblValue);
				Console.Out.WriteLine("Temp Probe A = {0:0.###} deg K\n",dblValue);
				Console.Out.WriteLine("Temp Probe A = {0:0.###} deg C\n",(dblValue-273.15));
				Console.Out.WriteLine("Temp Probe A = {0:0.###} deg F\n",(((dblValue-273.15)*1.8)+32));

				//Get the humidity reading.
				LJUD.GetResult (u3.ljhandle, LJUD.IO.SHT_GET_READING, LJUD.CHANNEL.SHT_RH, ref dblValue);
				Console.Out.WriteLine("RH Probe A = {0:0.###} percent\n\n",dblValue);

				//Set FIO7 to output-low to disable probe A. 
				LJUD.ePut (u3.ljhandle, LJUD.IO.PUT_DIGITAL_BIT, (LJUD.CHANNEL) 7, 0, 0);

				//Set DAC0 to 3.3 volts to enable probe B.
				LJUD.ePut (u3.ljhandle, LJUD.IO.PUT_DAC, 0, 3.3, 0);

				//Since the DACs on the U3 are slower than the communication speed,
				//we put a delay here to make sure the DAC has time to rise to 3.3 volts
				//before communicating with the EI-1050.
				Thread.Sleep(30);  //Wait 30 ms.

				//Now, an add/go/get block to get the temp & humidity at the same time.
				//Request a temperature reading from the EI-1050.
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.SHT_GET_READING, LJUD.CHANNEL.SHT_TEMP, 0, 0, 0);

				//Request a humidity reading from the EI-1050.
				LJUD.AddRequest (u3.ljhandle, LJUD.IO.SHT_GET_READING, LJUD.CHANNEL.SHT_RH, 0, 0, 0);

				//Execute the requests.  Will take about 0.5 seconds with a USB high-high
				//or Ethernet connection, and about 1.5 seconds with a normal USB connection.
				LJUD.GoOne (u3.ljhandle);

				//Get the temperature reading.
				LJUD.GetResult (u3.ljhandle, LJUD.IO.SHT_GET_READING, LJUD.CHANNEL.SHT_TEMP, ref dblValue);
				Console.Out.WriteLine("Temp Probe B = {0:0.###} deg K\n",dblValue);
				Console.Out.WriteLine("Temp Probe B = {0:0.###} deg C\n",(dblValue-273.15));
				Console.Out.WriteLine("Temp Probe B = {0:0.###} deg F\n",(((dblValue-273.15)*1.8)+32));

				//Get the humidity reading.
				LJUD.GetResult (u3.ljhandle, LJUD.IO.SHT_GET_READING, LJUD.CHANNEL.SHT_RH, ref dblValue);
				Console.Out.WriteLine("RH Probe B = {0:0.###} percent\n\n",dblValue);

				//Set DAC0 to 0 volts to disable probe B.
				LJUD.ePut (u3.ljhandle, LJUD.IO.PUT_DAC, 0, 0.0, 0);
			
				//If we were going to loop and talk to probe A next, we would
				//want a delay here to make sure the DAC falls to 0 volts
				//before enabling probe A.
				Thread.Sleep(30);  //Wait 30 ms.
			}
			catch (LabJackUDException e) 
			{
				showErrorMessage(e);
			}

			//End of dual probe code.
			//*/

			Console.ReadLine(); // Pause for user

		}
	}
}
