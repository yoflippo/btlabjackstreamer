//---------------------------------------------------------------------------
//
//  U3_LJTickDAC.cs
// 
//	Demonstrates basic I2C using an LJTickDAC attached to FIO4/5 block 
//
//  support@labjack.com
//  November 9, 2009
//	Revised December 28, 2010
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
			LJUD.IO ioType=0;
			LJUD.CHANNEL channel=0;
			double dblValue=0;
			double dummyDouble=0;
			int dummyInt=0;

			double numI2CBytesToWrite;
			double numI2CBytesToRead;
			byte[] writeArray = new byte[128];
			byte[] readArray = new byte[128];
			long i=0;
			long serialNumber=0;
			double slopeDACA=0, offsetDACA=0, slopeDACB=0, offsetDACB=0;
			double writeACKS=0, expectedACKS=0;
			byte[] bytes;

			//Open the LabJack.
			try 
			{
				device = new U3(LJUD.CONNECTION.USB, "0", true); // Connection through USB
			}
			catch (LabJackUDException e) 
			{
				showErrorMessage(e);
			}

			//Configure the I2C communication.
			//The address of the EEPROM on the LJTick-DAC is 0xA0.
			LJUD.AddRequest(device.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.I2C_ADDRESS_BYTE,160,0,0);

			//SCL is FIO4
			LJUD.AddRequest(device.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.I2C_SCL_PIN_NUM,4,0,0);

			//SDA is FIO5
			LJUD.AddRequest(device.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.I2C_SDA_PIN_NUM,5,0,0);

			//See description of low-level I2C function.
			LJUD.AddRequest(device.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.I2C_OPTIONS,0,0,0);

			//See description of low-level I2C function.  0 is max speed of about 130 kHz.
			LJUD.AddRequest(device.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.I2C_SPEED_ADJUST,0,0,0);
	
			//Execute the requests on a single LabJack.
			LJUD.GoOne(device.ljhandle);


			//Get all the results just to check for errors.
			LJUD.GetFirstResult(device.ljhandle, ref ioType, ref channel, ref dblValue, ref dummyInt, ref dummyDouble);
			bool finished = false;
			while (!finished)
			{
				try{ LJUD.GetNextResult(device.ljhandle, ref ioType, ref channel, ref dblValue, ref dummyInt, ref dummyDouble); }
				catch (LabJackUDException e) 
				{
					if (e.LJUDError == LJUD.LJUDERROR.NO_MORE_DATA_AVAILABLE)
						finished = true;
					else
						showErrorMessage(e);
				}
			}

			//Initial read of EEPROM bytes 0-3 in the user memory area.
			//We need a single I2C transmission that writes the address and then reads
			//the data.  That is, there needs to be an ack after writing the address,
			//not a stop condition.  To accomplish this, we use Add/Go/Get to combine
			//the write and read into a single low-level call.
			numI2CBytesToWrite = 1;
			writeArray[0] = 0;  //Memory address.  User area is 0-63.
			LJUD.AddRequest(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_WRITE, numI2CBytesToWrite, writeArray, 0);

			LJUD.AddRequest(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_GET_ACKS, 0, 0, 0);

			numI2CBytesToRead = 4;
			LJUD.AddRequest(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_READ, numI2CBytesToRead, readArray, 0);

			//Execute the requests.
			LJUD.GoOne(device.ljhandle);

			//Get the result of the write just to check for an error.
			LJUD.GetResult(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_WRITE, ref dummyDouble);

			//Get the write ACKs and compare to the expected value.  We expect bit 0 to be
			//the ACK of the last data byte progressing up to the ACK of the address
			//byte (data bytes only for Control firmware 1.43 and less).  So if n is the
			//number of data bytes, the ACKs value should be (2^(n+1))-1.
			LJUD.GetResult(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_GET_ACKS, ref writeACKS);
			expectedACKS = Math.Pow(2,numI2CBytesToWrite+1) - 1;
			if(writeACKS != expectedACKS) Console.Out.WriteLine("Expected ACKs = {0:0}, Received ACKs = %0.f\n",expectedACKS,writeACKS);


			//When the GoOne processed the read request, the read data was put into the readArray buffer that
			//we passed, so this GetResult is also just to check for an error.
			LJUD.GetResult(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_READ, ref dummyDouble);

			//Display the first 4 elements.
			Console.Out.WriteLine("Read User Mem [0-3] = {0:0.#}, {1:0.#}, {2:0.#}, {3:0.#}\n",readArray[0],readArray[1],readArray[2],readArray[3]);




			//Write EEPROM bytes 0-3 in the user memory area, using the page write technique.  Note
			//that page writes are limited to 16 bytes max, and must be aligned with the 16-byte
			//page intervals.  For instance, if you start writing at address 14, you can only write
			//two bytes because byte 16 is the start of a new page.
			numI2CBytesToWrite = 5;
			writeArray[0] = 0;  //Memory address.  User area is 0-63.

			//Create 4 new pseudo-random numbers to write.
			Random rand = new Random((int)DateTime.Now.Ticks);
			for(i=1;i<5;i++)
			{
				writeArray[i] = (byte)(rand.NextDouble()*255);
			}
	
			LJUD.AddRequest(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_WRITE, numI2CBytesToWrite, writeArray, 0);

			LJUD.AddRequest(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_GET_ACKS, 0, 0, 0);

			//Execute the requests.
			LJUD.GoOne(device.ljhandle);

			//Get the result of the write just to check for an error.
			LJUD.GetResult(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_WRITE, ref dummyDouble);

			//Get the write ACKs and compare to the expected value.  We expect bit 0 to be
			//the ACK of the last data byte progressing up to the ACK of the address
			//byte (data bytes only for Control firmware 1.43 and less).  So if n is the
			//number of data bytes, the ACKs value should be (2^(n+1))-1.
			LJUD.GetResult(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_GET_ACKS, ref writeACKS);
			expectedACKS = Math.Pow(2,numI2CBytesToWrite+1) - 1;
			if(writeACKS != expectedACKS) Console.Out.WriteLine("Expected ACKs = {0:0}, Received ACKs = %0.f\n",expectedACKS,writeACKS);

			//Delay to allow the EEPROM to complete the write cycle.  Datasheet says 1.5 ms max.
			System.Threading.Thread.Sleep(2);

			Console.Out.WriteLine("Write User Mem [0-3] = {0:0.#}, {1:0.#}, {2:0.#}, {3:0.#}\n",writeArray[1],writeArray[2],writeArray[3],writeArray[4]);




			//Final read of EEPROM bytes 0-3 in the user memory area.
			//We need a single I2C transmission that writes the address and then reads
			//the data.  That is, there needs to be an ack after writing the address,
			//not a stop condition.  To accomplish this, we use Add/Go/Get to combine
			//the write and read into a single low-level call.
			numI2CBytesToWrite = 1;
			writeArray[0] = 0;  //Memory address.  User area is 0-63.
			LJUD.AddRequest(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_WRITE, numI2CBytesToWrite, writeArray, 0);

			LJUD.AddRequest(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_GET_ACKS, 0, 0, 0);

			numI2CBytesToRead = 4;
			LJUD.AddRequest(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_READ, numI2CBytesToRead, readArray, 0);

			//Execute the requests.
			LJUD.GoOne(device.ljhandle);

			//Get the result of the write just to check for an error.
			LJUD.GetResult(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_WRITE, ref dummyDouble);

			//Get the write ACKs and compare to the expected value.  We expect bit 0 to be
			//the ACK of the last data byte progressing up to the ACK of the address
			//byte (data bytes only for Control firmware 1.43 and less).  So if n is the
			//number of data bytes, the ACKs value should be (2^(n+1))-1.
			LJUD.GetResult(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_GET_ACKS, ref writeACKS);
			expectedACKS = Math.Pow(2,numI2CBytesToWrite+1) - 1;
			if(writeACKS != expectedACKS) Console.Out.WriteLine("Expected ACKs = {0:0}, Received ACKs = %0.f\n",expectedACKS,writeACKS);

			//When the GoOne processed the read request, the read data was put into the readArray buffer that
			//we passed, so this GetResult is also just to check for an error.
			LJUD.GetResult(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_READ, ref dummyDouble);

			//Display the first 4 elements.
			Console.Out.WriteLine("Read User Mem [0-3] = {0:0.#}, {1:0.#}, {2:0.#}, {3:0.#}\n\n",readArray[0],readArray[1],readArray[2],readArray[3]);




			//Read cal constants and serial number.
			//We need a single I2C transmission that writes the address and then reads
			//the data.  That is, there needs to be an ack after writing the address,
			//not a stop condition.  To accomplish this, we use Add/Go/Get to combine
			//the write and read into a single low-level call.
			//
			//64-71   DACA Slope
			//72-79   DACA Offset
			//80-87   DACB Slope
			//88-95   DACB Offset
			//96-99   Serial Number
			//
			numI2CBytesToWrite = 1;
			writeArray[0] = 64;  //Memory address.  Cal constants start at 64.
			LJUD.AddRequest(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_WRITE, numI2CBytesToWrite, writeArray, 0);

			LJUD.AddRequest(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_GET_ACKS, 0, 0, 0);

			numI2CBytesToRead = 36;
			LJUD.AddRequest(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_READ, numI2CBytesToRead, readArray, 0);

			//Execute the requests.
			LJUD.GoOne(device.ljhandle);

			//Get the result of the write just to check for an error.
			LJUD.GetResult(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_WRITE, ref dummyDouble);

			//Get the write ACKs and compare to the expected value.  We expect bit 0 to be
			//the ACK of the last data byte progressing up to the ACK of the address
			//byte (data bytes only for Control firmware 1.43 and less).  So if n is the
			//number of data bytes, the ACKs value should be (2^(n+1))-1.
			LJUD.GetResult(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_GET_ACKS, ref writeACKS);
			expectedACKS = Math.Pow(2,numI2CBytesToWrite+1) - 1;
			if(writeACKS != expectedACKS) Console.Out.WriteLine("Expected ACKs = {0:0}, Received ACKs = %0.f\n",expectedACKS,writeACKS);

			//When the GoOne processed the read request, the read data was put into the readArray buffer that
			//we passed, so this GetResult is also just to check for an error.
			LJUD.GetResult(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_READ, ref dummyDouble);

			//Convert fixed point values to floating point doubles.
			//double[] readArray;
			//readArray = (double[])(readArray);
			slopeDACA = BitConverter.ToInt64(readArray, 0)/(double)4294967296;
			offsetDACA = BitConverter.ToInt64(readArray, 8)/(double)4294967296;
			slopeDACB = BitConverter.ToInt64(readArray, 16)/(double)4294967296;
			offsetDACB = BitConverter.ToInt64(readArray, 24)/(double)4294967296;
			Console.Out.WriteLine("DACA Slope = {0:0.0} bits/volt\n",slopeDACA);
			Console.Out.WriteLine("DACA Offset = {0:0.0} bits\n",offsetDACA);
			Console.Out.WriteLine("DACB Slope = {0:0.0} bits/volt\n",slopeDACB);
			Console.Out.WriteLine("DACB Offset = {0:0.0} bits\n",offsetDACB);

			//Convert serial number bytes to long.
			serialNumber = (int)readArray[32] + ((int) readArray[33] << 8) + ((int) readArray[34] << 16) + ((int) readArray[35] << 24);
			Console.Out.WriteLine("Serial Number = {0:0.#}\n\n",serialNumber);




			//Update both DAC outputs.

			//Set the I2C address in the UD driver so that we not talk to the DAC chip.
			//The address of the DAC chip on the LJTick-DAC is 0x24.
			LJUD.ePut(device.ljhandle, LJUD.IO.PUT_CONFIG, LJUD.CHANNEL.I2C_ADDRESS_BYTE,36,0);


			///Set DACA to 2.4 volts.
			numI2CBytesToWrite = 3;
			writeArray[0] = 48;  //Write and update DACA.
			bytes = BitConverter.GetBytes((long)((2.4*slopeDACB)+offsetDACB)/256);
			writeArray[1] = bytes[0];  //Upper byte of binary DAC value.
			writeArray[2] = bytes[1];  //Lower byte of binary DAC value.

			LJUD.AddRequest(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_WRITE, numI2CBytesToWrite, writeArray, 0);

			LJUD.AddRequest(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_GET_ACKS, 0, 0, 0);

			//Execute the requests.
			LJUD.GoOne(device.ljhandle);

			//Get the result of the write just to check for an error.
			LJUD.GetResult(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_WRITE, ref dummyDouble);

			//Get the write ACKs and compare to the expected value.  We expect bit 0 to be
			//the ACK of the last data byte progressing up to the ACK of the address
			//byte (data bytes only for Control firmware 1.43 and less).  So if n is the
			//number of data bytes, the ACKs value should be (2^(n+1))-1.
			LJUD.GetResult(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_GET_ACKS, ref writeACKS);
			expectedACKS = Math.Pow(2,numI2CBytesToWrite+1) - 1;
			if(writeACKS != expectedACKS) Console.Out.WriteLine("Expected ACKs = {0:0}, Received ACKs = %0.f\n",expectedACKS,writeACKS);

			Console.Out.WriteLine("DACA set to 2.4 volts\n\n");


			//Set DACB to 1.5 volts.
			numI2CBytesToWrite = 3;
			writeArray[0] = 49;  //Write and update DACB.
			bytes = BitConverter.GetBytes((long)((1.5*slopeDACB)+offsetDACB)/256);
			writeArray[1] = bytes[0];  //Upper byte of binary DAC value.
			writeArray[2] = bytes[1];  //Lower byte of binary DAC value.
	
			LJUD.AddRequest(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_WRITE, numI2CBytesToWrite, writeArray, 0);

			LJUD.AddRequest(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_GET_ACKS, 0, 0, 0);

			//Execute the requests.
			LJUD.GoOne(device.ljhandle);

			//Get the result of the write just to check for an error.
			LJUD.GetResult(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_WRITE, ref dummyDouble);

			//Get the write ACKs and compare to the expected value.  We expect bit 0 to be
			//the ACK of the last data byte progressing up to the ACK of the address
			//byte (data bytes only for Control firmware 1.43 and less).  So if n is the
			//number of data bytes, the ACKs value should be (2^(n+1))-1.
			LJUD.GetResult(device.ljhandle, LJUD.IO.I2C_COMMUNICATION, LJUD.CHANNEL.I2C_GET_ACKS, ref writeACKS);
			expectedACKS = Math.Pow(2,numI2CBytesToWrite+1) - 1;
			if(writeACKS != expectedACKS) Console.Out.WriteLine("Expected ACKs = {0:0}, Received ACKs = %0.f\n",expectedACKS,writeACKS);
	
			Console.Out.WriteLine("DACB set to 1.5 volts\n");

			Console.ReadLine(); // Pause for user	return;
		}
	}
}
