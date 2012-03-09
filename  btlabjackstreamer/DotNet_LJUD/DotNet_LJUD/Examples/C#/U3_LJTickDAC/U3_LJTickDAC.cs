//---------------------------------------------------------------------------
//
//  U3_LJTickDAC.cs
// 
//	Demonstrates using the add/go/get method to efficiently write and read
//	virtually all analog and digital I/O on the LabJack U3.
//	Records the time for 1000 iterations and divides by 1000, to allow
//	verification of the basic command/response communication times of the
//	LabJack U3 as documented in Section 3.1 of the U3 User's Guide.
//
//  support@labjack.com
//  November 9, 2009
//----------------------------------------------------------------------
//

using System;

// Import the UD .NET wrapper object.  The dll referenced is installed by the
// LabJackUD installer.
using LabJack.LabJackUD;

namespace LJTickDAC
{
	class LJTickDAC
	{
		// our device variable
		private U3 device;

		static void Main(string[] args)
		{
			LJTickDAC a = new LJTickDAC();
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

			
			//	long lngGetNextIteration;
			//	LJUD.IO ioType=0, channel=0;
			//	double dblValue=0;

			long i=0;
			double pinNum = 4;  //4 means the LJTick-DAC is connected to FIO4/FIO5.
			int[] achrUserMem = new int[64];
			double[] adblCalMem = new double[4];
			double serialNumber=0;
			Random random = new Random();

			// Dummy variables to satisfy certain method signatures
			double dummyDouble = 0;

			//Open the LabJack.
			try 
			{
				device = new U3(LJUD.CONNECTION.USB, "0", true); // Connection through USB
			}
			catch (LabJackUDException e) 
			{
				showErrorMessage(e);
			}

			try
			{
				//Specify where the LJTick-DAC is plugged in.
				//This is just setting a parameter in the driver, and not actually talking
				//to the hardware, and thus executes very fast.
				LJUD.ePut(device.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.TDAC_SCL_PIN_NUM,pinNum,0);

				//Set DACA to 1.2 volts.  If the driver has not previously talked to an LJTDAC
				//on the specified pins, it will first retrieve and store the cal constants.  The
				//low-level I2C command can only update 1 DAC channel at a time, so there
				//is no advantage to doing two updates within a single add-go-get block.
				LJUD.ePut(device.ljhandle, LJUD.IO.TDAC_COMMUNICATION, LJUD.CHANNEL.TDAC_UPDATE_DACA, 1.2, 0);
				Console.Out.WriteLine("DACA set to 1.2 volts\n\n");

				//Set DACB to 2.3 volts.
				LJUD.ePut(device.ljhandle, LJUD.IO.TDAC_COMMUNICATION, LJUD.CHANNEL.TDAC_UPDATE_DACB, 2.3, 0);
				Console.Out.WriteLine("DACB set to 2.3 volts\n\n");



				//Now for more advanced operations.


				//If at this point you removed that LJTDAC and plugged a different one
				//into the same pins, the driver would not know and would use the wrong
				//cal constants on future updates.  If we do a cal constant read,
				//the driver will store the constants from the new read.
				LJUD.eGet(device.ljhandle, LJUD.IO.TDAC_COMMUNICATION, LJUD.CHANNEL.TDAC_READ_CAL_CONSTANTS, ref dummyDouble, adblCalMem);
				Console.Out.WriteLine("DACA Slope = {0:0.0} bits/volt\n",adblCalMem[0]);
				Console.Out.WriteLine("DACA Offset = {0:0.0} bits\n",adblCalMem[1]);
				Console.Out.WriteLine("DACB Slope = {0:0.0} bits/volt\n",adblCalMem[2]);
				Console.Out.WriteLine("DACB Offset = {0:0.0} bits\n\n",adblCalMem[3]);



				//Read the serial number.
				LJUD.eGet(device.ljhandle, LJUD.IO.TDAC_COMMUNICATION, LJUD.CHANNEL.TDAC_SERIAL_NUMBER, ref serialNumber, 0);
				Console.Out.WriteLine("LJTDAC Serial Number = {0:0}\n\n",serialNumber);

			}
			catch (LabJackUDException e) 
			{
				showErrorMessage(e);
			}
			Console.ReadLine(); // Pause for user	return;
		}
	}
}
