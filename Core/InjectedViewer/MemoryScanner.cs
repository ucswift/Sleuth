// Code from: http://www.codeproject.com/KB/cs/sojaner_memory_scanner.aspx

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

namespace Sleuth.InjectedViewer
{
	public class RegularMemoryScan
	{
		#region Constant fields
		//Maximum memory block size to read in every read process.

		//Experience tells me that,
		//if ReadStackSize be bigger than 20480, there will be some problems
		//retrieving correct blocks of memory values.
		const int ReadStackSize = 20480;


		//A byte is 8 bits long memory defined variable.

		//A 16 bit variable (Like char) is made up of 16 bits of memory and so 16/8 = 2 bytes of memory.
		const int Int16BytesCount = 16 / 8;
		//A 32 bit variable (Like int) is made up of 32 bits of memory and so 32/8 = 4 bytes of memory.
		const int Int32BytesCount = 32 / 8;
		//A 34 bit variable (Like long) is made up of 64 bits of memory and so 64/8 = 8 bytes of memory.
		const int Int64BytesCount = 64 / 8;
		#endregion

		#region Global fields
		//Instance of ProcessMemoryReader class to be used to read the memory.
		ProcessMemoryReader reader;

		//Start and End addresses to be scaned.
		IntPtr baseAddress;
		IntPtr lastAddress;

		//New thread object to run the scan in
		Thread thread;
		#endregion

		#region Delegate and Event objects
		//Delegate and Event objects for raising the ScanProgressChanged event.
		public delegate void ScanProgressedEventHandler(object sender, ScanProgressChangedEventArgs e);
		public event ScanProgressedEventHandler ScanProgressChanged;

		//Delegate and Event objects for raising the ScanCompleted event.
		public delegate void ScanCompletedEventHandler(object sender, ScanCompletedEventArgs e);
		public event ScanCompletedEventHandler ScanCompleted;

		//Delegate and Event objects for raising the ScanCanceled event.
		public delegate void ScanCanceledEventHandler(object sender, ScanCanceledEventArgs e);
		public event ScanCanceledEventHandler ScanCanceled;
		#endregion

		#region Methods
		//Class entry point.
		//The process, StartAddress and EndAdrress will be defined in the class definition.
		public RegularMemoryScan(Process process, int StartAddress, int EndAddress)
		{
			//Set the reader object an instant of the ProcessMemoryReader class.
			reader = new ProcessMemoryReader();

			//Set the ReadProcess of the reader object to process passed to this method
			//to define the process we are going to scan its memory.
			reader.ReadProcess = process;

			//Set the Start and End addresses of the scan to what is wanted.
			baseAddress = (IntPtr)StartAddress;
			lastAddress = (IntPtr)EndAddress;//The scan starts from baseAddress,
			//and progresses up to EndAddress.
		}

		#region Public methods
		//Get ready to scan the memory for the 16 bit value.
		public void StartScanForInt16(Int16 Int16Value)
		{
			//Check if the thread is already defined or not.
			if (thread != null)
			{
				//If the thread is already defined and is Alive,
				if (thread.IsAlive)
				{
					//raise the event that shows that the last scan task is canceled
					//(because a new task is going to be started as wanted),
					ScanCanceledEventArgs cancelEventArgs = new ScanCanceledEventArgs();
					ScanCanceled(this, cancelEventArgs);

					//and then abort the alive thread and so cancel last scan task.
					thread.Abort();
				}
			}
			//Set the thread object as a new instant of the Thread class and pass
			//a new ParameterizedThreadStart class object with the needed method passed to it
			//to run in the new thread.
			thread = new Thread(new ParameterizedThreadStart(Int16Scaner));

			//Start the new thread and set the 32 bit value to look for.
			thread.Start(Int16Value);
		}

		//Get ready to scan the memory for the 32 bit value.
		public void StartScanForInt32(Int32 Int32Value)
		{
			//Check if the thread is already defined or not.
			if (thread != null)
			{
				//If the thread is already defined and is Alive,
				if (thread.IsAlive)
				{
					//raise the event that shows that the last scan task is canceled
					//(because a new task is going to be started as wanted),
					ScanCanceledEventArgs cancelEventArgs = new ScanCanceledEventArgs();
					ScanCanceled(this, cancelEventArgs);

					//and then abort the alive thread and so cancel last scan task.
					thread.Abort();
				}
			}
			//Set the thread object as a new instant of the Thread class and pass
			//a new ParameterizedThreadStart class object with the needed method passed to it
			//to run in the new thread.
			thread = new Thread(new ParameterizedThreadStart(Int32Scaner));

			//Start the new thread and set the 32 bit value to look for.
			thread.Start(Int32Value);
		}

		//Get ready to scan the memory for the 64 bit value.
		public void StartScanForInt64(Int64 Int64Value)
		{
			//Check if the thread is already defined or not.
			if (thread != null)
			{
				//If the thread is already defined and is Alive,
				if (thread.IsAlive)
				{
					//raise the event that shows that the last scan task is canceled
					//(because a new task is going to be started as wanted),
					ScanCanceledEventArgs cancelEventArgs = new ScanCanceledEventArgs();
					ScanCanceled(this, cancelEventArgs);

					//and then abort the alive thread and so cancel last scan task.
					thread.Abort();
				}
			}
			//Set the thread object as a new instant of the Thread class and pass
			//a new ParameterizedThreadStart class object with the needed method passed to it
			//to run in the new thread.
			thread = new Thread(new ParameterizedThreadStart(Int64Scaner));

			//Start the new thread and set the 32 bit value to look for.
			thread.Start(Int64Value);
		}

		//Cancel the scan started.
		public void CancelScan()
		{
			//Raise the event that shows that the last scan task is canceled as user asked,
			ScanCanceledEventArgs cancelEventArgs = new ScanCanceledEventArgs();
			ScanCanceled(this, cancelEventArgs);

			//and then abort the thread that scanes the memory.
			thread.Abort();
		}
		#endregion

		#region Private methods
		//The memory scan method for the 16 bit values.
		private void Int16Scaner(object int16Object)
		{
			//The difference of scan start point in all loops except first loop,
			//that doesn't have any difference, is type's Bytes count minus 1.
			int arraysDifference = Int16BytesCount - 1;

			//Get the short value out of the object to look for it.
			Int16 int16Value = (Int16)int16Object;

			//Define a List object to hold the found memory addresses.
			List<int> finalList = new List<int>();

			//Open the pocess to read the memory.
			reader.OpenProcess();

			//Create a new instant of the ScanProgressEventArgs class to be used to raise the
			//ScanProgressed event and pass the percentage of scan, during the scan progress.
			ScanProgressChangedEventArgs scanProgressEventArgs;

			//Calculate the size of memory to scan.
			int memorySize = (int)((int)lastAddress - (int)baseAddress);

			//If more that one block of memory is requered to be read,
			if (memorySize >= ReadStackSize)
			{
				//Count of loops to read the memory blocks.
				int loopsCount = memorySize / ReadStackSize;

				//Look to see if there is any other bytes let after the loops.
				int outOfBounds = memorySize % ReadStackSize;

				//Set the currentAddress to first address.
				int currentAddress = (int)baseAddress;

				//This will be used to check if any bytes have been read from the memory.
				int bytesReadSize;

				//Set the size of the bytes blocks.
				int bytesToRead = ReadStackSize;

				//An array to hold the bytes read from the memory.
				byte[] array;

				//Progress percentage.
				int progress;

				for (int i = 0; i < loopsCount; i++)
				{
					//Calculte and set the progress percentage.
					progress = (int)(((double)(currentAddress - (int)baseAddress) / (double)memorySize) * 100d);

					//Prepare and set the ScanProgressed event and raise the event.
					scanProgressEventArgs = new ScanProgressChangedEventArgs(progress);
					ScanProgressChanged(this, scanProgressEventArgs);

					//Read the bytes from the memory.
					array = reader.ReadProcessMemory((IntPtr)currentAddress, (uint)bytesToRead, out bytesReadSize);

					//If any byte is read from the memory (there has been any bytes in the memory block),
					if (bytesReadSize > 0)
					{
						//Loop through the bytes one by one to look for the values.
						for (int j = 0; j < array.Length - arraysDifference; j++)
						{
							//If any value is equal to what we are looking for,
							if (BitConverter.ToInt16(array, j) == int16Value)
							{
								//add it's memory address to the finalList.
								finalList.Add(j + (int)currentAddress);
							}
						}
					}
					//Move currentAddress after the block already scaned, but
					//move it back some steps backward (as much as arraysDifference)
					//to avoid loosing any values at the end of the array.
					currentAddress += array.Length - arraysDifference;

					//Set the size of the read block, bigger, to  the steps backward.
					//Set the size of the read block, to fit the back steps.
					bytesToRead = ReadStackSize + arraysDifference;
				}
				//If there is any more bytes than the loops read,
				if (outOfBounds > 0)
				{
					//Read the additional bytes.
					byte[] outOfBoundsBytes = reader.ReadProcessMemory((IntPtr)currentAddress, (uint)((int)lastAddress - currentAddress), out bytesReadSize);

					//If any byte is read from the memory (there has been any bytes in the memory block),
					if (bytesReadSize > 0)
					{
						//Loop through the bytes one by one to look for the values.
						for (int j = 0; j < outOfBoundsBytes.Length - arraysDifference; j++)
						{
							//If any value is equal to what we are looking for,
							if (BitConverter.ToInt16(outOfBoundsBytes, j) == int16Value)
							{
								//add it's memory address to the finalList.
								finalList.Add(j + (int)currentAddress);
							}
						}
					}
				}
			}
			//If the block could be read in just one read,
			else
			{
				//Calculate the memory block's size.
				int blockSize = memorySize % ReadStackSize;

				//Set the currentAddress to first address.
				int currentAddress = (int)baseAddress;

				//Holds the count of bytes read from the memory.
				int bytesReadSize;

				//If the memory block can contain at least one 16 bit variable.
				if (blockSize > Int16BytesCount)
				{
					//Read the bytes to the array.
					byte[] array = reader.ReadProcessMemory((IntPtr)currentAddress, (uint)blockSize, out bytesReadSize);

					//If any byte is read,
					if (bytesReadSize > 0)
					{
						//Loop through the array to find the values.
						for (int j = 0; j < array.Length - arraysDifference; j++)
						{
							//If any value equals the value we are looking for,
							if (BitConverter.ToInt16(array, j) == int16Value)
							{
								//add it to the finalList.
								finalList.Add(j + (int)currentAddress);
							}
						}
					}
				}
			}
			//Close the handle to the process to avoid process errors.
			reader.CloseHandle();

			//Prepare the ScanProgressed and set the progress percentage to 100% and raise the event.
			scanProgressEventArgs = new ScanProgressChangedEventArgs(100);
			ScanProgressChanged(this, scanProgressEventArgs);

			//Prepare and raise the ScanCompleted event.
			ScanCompletedEventArgs scanCompleteEventArgs = new ScanCompletedEventArgs(finalList.ToArray());
			ScanCompleted(this, scanCompleteEventArgs);
		}

		//The memory scan method for the 32 bit values.
		private void Int32Scaner(object int32Object)
		{
			//The difference of scan start point in all loops except first loop,
			//that doesn't have any difference, is type's Bytes count minus 1.
			int arraysDifference = Int32BytesCount - 1;

			//Get the int value out of the object to look for it.
			Int32 int32Value = (Int32)int32Object;

			//Define a List object to hold the found memory addresses.
			List<int> finalList = new List<int>();

			//Open the pocess to read the memory.
			reader.OpenProcess();

			//Create a new instant of the ScanProgressEventArgs class to be used to raise the
			//ScanProgressed event and pass the percentage of scan, during the scan progress.
			ScanProgressChangedEventArgs scanProgressEventArgs;

			//Calculate the size of memory to scan.
			int memorySize = (int)((int)lastAddress - (int)baseAddress);

			//If more that one block of memory is requered to be read,
			if (memorySize >= ReadStackSize)
			{
				//Count of loops to read the memory blocks.
				int loopsCount = memorySize / ReadStackSize;

				//Look to see if there is any other bytes let after the loops.
				int outOfBounds = memorySize % ReadStackSize;

				//Set the currentAddress to first address.
				int currentAddress = (int)baseAddress;

				//This will be used to check if any bytes have been read from the memory.
				int bytesReadSize;

				//Set the size of the bytes blocks.
				int bytesToRead = ReadStackSize;

				//An array to hold the bytes read from the memory.
				byte[] array;

				//Progress percentage.
				int progress;

				for (int i = 0; i < loopsCount; i++)
				{
					//Calculte and set the progress percentage.
					progress = (int)(((double)(currentAddress - (int)baseAddress) / (double)memorySize) * 100d);

					//Prepare and set the ScanProgressed event and raise the event.
					scanProgressEventArgs = new ScanProgressChangedEventArgs(progress);
					ScanProgressChanged(this, scanProgressEventArgs);

					//Read the bytes from the memory.
					array = reader.ReadProcessMemory((IntPtr)currentAddress, (uint)bytesToRead, out bytesReadSize);

					//If any byte is read from the memory (there has been any bytes in the memory block),
					if (bytesReadSize > 0)
					{
						//Loop through the bytes one by one to look for the values.
						for (int j = 0; j < array.Length - arraysDifference; j++)
						{
							//If any value is equal to what we are looking for,
							if (BitConverter.ToInt32(array, j) == int32Value)
							{
								//add it's memory address to the finalList.
								finalList.Add(j + (int)currentAddress);
							}
						}
					}
					//Move currentAddress after the block already scaned, but
					//move it back some steps backward (as much as arraysDifference)
					//to avoid loosing any values at the end of the array.
					currentAddress += array.Length - arraysDifference;

					//Set the size of the read block, bigger, to  the steps backward.
					//Set the size of the read block, to fit the back steps.
					bytesToRead = ReadStackSize + arraysDifference;
				}
				//If there is any more bytes than the loops read,
				if (outOfBounds > 0)
				{
					//Read the additional bytes.
					byte[] outOfBoundsBytes = reader.ReadProcessMemory((IntPtr)currentAddress, (uint)((int)lastAddress - currentAddress), out bytesReadSize);

					//If any byte is read from the memory (there has been any bytes in the memory block),
					if (bytesReadSize > 0)
					{
						//Loop through the bytes one by one to look for the values.
						for (int j = 0; j < outOfBoundsBytes.Length - arraysDifference; j++)
						{
							//If any value is equal to what we are looking for,
							if (BitConverter.ToInt32(outOfBoundsBytes, j) == int32Value)
							{
								//add it's memory address to the finalList.
								finalList.Add(j + (int)currentAddress);
							}
						}
					}
				}
			}
			//If the block could be read in just one read,
			else
			{
				//Calculate the memory block's size.
				int blockSize = memorySize % ReadStackSize;

				//Set the currentAddress to first address.
				int currentAddress = (int)baseAddress;

				//Holds the count of bytes read from the memory.
				int bytesReadSize;

				//If the memory block can contain at least one 32 bit variable.
				if (blockSize > Int32BytesCount)
				{
					//Read the bytes to the array.
					byte[] array = reader.ReadProcessMemory((IntPtr)currentAddress, (uint)blockSize, out bytesReadSize);

					//If any byte is read,
					if (bytesReadSize > 0)
					{
						//Loop through the array to find the values.
						for (int j = 0; j < array.Length - arraysDifference; j++)
						{
							//If any value equals the value we are looking for,
							if (BitConverter.ToInt32(array, j) == int32Value)
							{
								//add it to the finalList.
								finalList.Add(j + (int)currentAddress);
							}
						}
					}
				}
			}
			//Close the handle to the process to avoid process errors.
			reader.CloseHandle();

			//Prepare the ScanProgressed and set the progress percentage to 100% and raise the event.
			scanProgressEventArgs = new ScanProgressChangedEventArgs(100);
			ScanProgressChanged(this, scanProgressEventArgs);

			//Prepare and raise the ScanCompleted event.
			ScanCompletedEventArgs scanCompleteEventArgs = new ScanCompletedEventArgs(finalList.ToArray());
			ScanCompleted(this, scanCompleteEventArgs);
		}

		//The memory scan method for the 64 bit values.
		private void Int64Scaner(object int64Object)
		{
			//The difference of scan start point in all loops except first loop,
			//that doesn't have any difference, is type's Bytes count minus 1.
			int arraysDifference = Int64BytesCount - 1;

			//Get the long value out of the object to look for it.
			Int64 int64Value = (Int64)int64Object;

			//Define a List object to hold the found memory addresses.
			List<int> finalList = new List<int>();

			//Open the pocess to read the memory.
			reader.OpenProcess();

			//Create a new instant of the ScanProgressEventArgs class to be used to raise the
			//ScanProgressed event and pass the percentage of scan, during the scan progress.
			ScanProgressChangedEventArgs scanProgressEventArgs;

			//Calculate the size of memory to scan.
			int memorySize = (int)((int)lastAddress - (int)baseAddress);

			//If more that one block of memory is requered to be read,
			if (memorySize >= ReadStackSize)
			{
				//Count of loops to read the memory blocks.
				int loopsCount = memorySize / ReadStackSize;

				//Look to see if there is any other bytes let after the loops.
				int outOfBounds = memorySize % ReadStackSize;

				//Set the currentAddress to first address.
				int currentAddress = (int)baseAddress;

				//This will be used to check if any bytes have been read from the memory.
				int bytesReadSize;

				//Set the size of the bytes blocks.
				int bytesToRead = ReadStackSize;

				//An array to hold the bytes read from the memory.
				byte[] array;

				//Progress percentage.
				int progress;

				for (int i = 0; i < loopsCount; i++)
				{
					//Calculte and set the progress percentage.
					progress = (int)(((double)(currentAddress - (int)baseAddress) / (double)memorySize) * 100d);

					//Prepare and set the ScanProgressed event and raise the event.
					scanProgressEventArgs = new ScanProgressChangedEventArgs(progress);
					ScanProgressChanged(this, scanProgressEventArgs);

					//Read the bytes from the memory.
					array = reader.ReadProcessMemory((IntPtr)currentAddress, (uint)bytesToRead, out bytesReadSize);

					//If any byte is read from the memory (there has been any bytes in the memory block),
					if (bytesReadSize > 0)
					{
						//Loop through the bytes one by one to look for the values.
						for (int j = 0; j < array.Length - arraysDifference; j++)
						{
							//If any value is equal to what we are looking for,
							if (BitConverter.ToInt64(array, j) == int64Value)
							{
								//add it's memory address to the finalList.
								finalList.Add(j + (int)currentAddress);
							}
						}
					}
					//Move currentAddress after the block already scaned, but
					//move it back some steps backward (as much as arraysDifference)
					//to avoid loosing any values at the end of the array.
					currentAddress += array.Length - arraysDifference;

					//Set the size of the read block, bigger, to  the steps backward.
					//Set the size of the read block, to fit the back steps.
					bytesToRead = ReadStackSize + arraysDifference;
				}
				//If there is any more bytes than the loops read,
				if (outOfBounds > 0)
				{
					//Read the additional bytes.
					byte[] outOfBoundsBytes = reader.ReadProcessMemory((IntPtr)currentAddress, (uint)((int)lastAddress - currentAddress), out bytesReadSize);

					//If any byte is read from the memory (there has been any bytes in the memory block),
					if (bytesReadSize > 0)
					{
						//Loop through the bytes one by one to look for the values.
						for (int j = 0; j < outOfBoundsBytes.Length - arraysDifference; j++)
						{
							//If any value is equal to what we are looking for,
							if (BitConverter.ToInt64(outOfBoundsBytes, j) == int64Value)
							{
								//add it's memory address to the finalList.
								finalList.Add(j + (int)currentAddress);
							}
						}
					}
				}
			}
			//If the block could be read in just one read,
			else
			{
				//Calculate the memory block's size.
				int blockSize = memorySize % ReadStackSize;

				//Set the currentAddress to first address.
				int currentAddress = (int)baseAddress;

				//Holds the count of bytes read from the memory.
				int bytesReadSize;

				//If the memory block can contain at least one 64 bit variable.
				if (blockSize > Int64BytesCount)
				{
					//Read the bytes to the array.
					byte[] array = reader.ReadProcessMemory((IntPtr)currentAddress, (uint)blockSize, out bytesReadSize);

					//If any byte is read,
					if (bytesReadSize > 0)
					{
						//Loop through the array to find the values.
						for (int j = 0; j < array.Length - arraysDifference; j++)
						{
							//If any value equals the value we are looking for,
							if (BitConverter.ToInt64(array, j) == int64Value)
							{
								//add it to the finalList.
								finalList.Add(j + (int)currentAddress);
							}
						}
					}
				}
			}
			//Close the handle to the process to avoid process errors.
			reader.CloseHandle();

			//Prepare the ScanProgressed and set the progress percentage to 100% and raise the event.
			scanProgressEventArgs = new ScanProgressChangedEventArgs(100);
			ScanProgressChanged(this, scanProgressEventArgs);

			//Prepare and raise the ScanCompleted event.
			ScanCompletedEventArgs scanCompleteEventArgs = new ScanCompletedEventArgs(finalList.ToArray());
			ScanCompleted(this, scanCompleteEventArgs);
		}
		#endregion
		#endregion
	}

	public class IrregularMemoryScan
	{
		#region Constant fields
		//A byte is 8 bits long memory defined variable.

		//A 16 bit variable (Like char) is made up of 16 bits of memory and so 16/8 = 2 bytes of memory.
		const int Int16BytesCount = 16 / 8;
		//A 32 bit variable (Like int) is made up of 32 bits of memory and so 32/8 = 4 bytes of memory.
		const int Int32BytesCount = 32 / 8;
		//A 34 bit variable (Like long) is made up of 64 bits of memory and so 64/8 = 8 bytes of memory.
		const int Int64BytesCount = 64 / 8;
		#endregion

		#region Global fields
		//Instance of ProcessMemoryReader class to be used to read the memory.
		ProcessMemoryReader reader;

		//New thread object to run the scan in
		Thread thread;

		//An array to hold the addresses.
		int[] addresses;
		#endregion

		#region Delegate and Event objects
		//Delegate and Event objects for raising the ScanProgressChanged event.
		public delegate void ScanProgressedEventHandler(object sender, ScanProgressChangedEventArgs e);
		public event ScanProgressedEventHandler ScanProgressChanged;

		//Delegate and Event objects for raising the ScanCompleted event.
		public delegate void ScanCompletedEventHandler(object sender, ScanCompletedEventArgs e);
		public event ScanCompletedEventHandler ScanCompleted;

		//Delegate and Event objects for raising the ScanCanceled event.
		public delegate void ScanCanceledEventHandler(object sender, ScanCanceledEventArgs e);
		public event ScanCanceledEventHandler ScanCanceled;
		#endregion

		#region Methods
		//Class entry point.
		//The process, StartAddress and EndAdrress will be defined in the class definition.
		public IrregularMemoryScan(Process process, int[] AddressesList)
		{
			//Set the reader object an instant of the ProcessMemoryReader class.
			reader = new ProcessMemoryReader();

			//Set the ReadProcess of the reader object to process passed to this method
			//to define the process we are going to scan its memory.
			reader.ReadProcess = process;

			//Take the addresses list and store them in local array.
			addresses = AddressesList;
		}

		#region Public methods
		//Get ready to scan the memory for the 16 bit value.
		public void StartScanForInt16(Int16 Int16Value)
		{
			//Check if the thread is already defined or not.
			if (thread != null)
			{
				//If the thread is already defined and is Alive,
				if (thread.IsAlive)
				{
					//raise the event that shows that the last scan task is canceled
					//(because a new task is going to be started as wanted),
					ScanCanceledEventArgs cancelEventArgs = new ScanCanceledEventArgs();
					ScanCanceled(this, cancelEventArgs);

					//and then abort the alive thread and so cancel last scan task.
					thread.Abort();
				}
			}
			//Set the thread object as a new instant of the Thread class and pass
			//a new ParameterizedThreadStart class object with the needed method passed to it
			//to run in the new thread.
			thread = new Thread(new ParameterizedThreadStart(Int16Scaner));

			//Start the new thread and set the 32 bit value to look for.
			thread.Start(Int16Value);
		}

		//Get ready to scan the memory for the 32 bit value.
		public void StartScanForInt32(Int32 Int32Value)
		{
			//Check if the thread is already defined or not.
			if (thread != null)
			{
				//If the thread is already defined and is Alive,
				if (thread.IsAlive)
				{
					//raise the event that shows that the last scan task is canceled
					//(because a new task is going to be started as wanted),
					ScanCanceledEventArgs cancelEventArgs = new ScanCanceledEventArgs();
					ScanCanceled(this, cancelEventArgs);

					//and then abort the alive thread and so cancel last scan task.
					thread.Abort();
				}
			}
			//Set the thread object as a new instant of the Thread class and pass
			//a new ParameterizedThreadStart class object with the needed method passed to it
			//to run in the new thread.
			thread = new Thread(new ParameterizedThreadStart(Int32Scaner));

			//Start the new thread and set the 32 bit value to look for.
			thread.Start(Int32Value);
		}

		//Get ready to scan the memory for the 64 bit value.
		public void StartScanForInt64(Int64 Int64Value)
		{
			//Check if the thread is already defined or not.
			if (thread != null)
			{
				//If the thread is already defined and is Alive,
				if (thread.IsAlive)
				{
					//raise the event that shows that the last scan task is canceled
					//(because a new task is going to be started as wanted),
					ScanCanceledEventArgs cancelEventArgs = new ScanCanceledEventArgs();
					ScanCanceled(this, cancelEventArgs);

					//and then abort the alive thread and so cancel last scan task.
					thread.Abort();
				}
			}
			//Set the thread object as a new instant of the Thread class and pass
			//a new ParameterizedThreadStart class object with the needed method passed to it
			//to run in the new thread.
			thread = new Thread(new ParameterizedThreadStart(Int64Scaner));

			//Start the new thread and set the 32 bit value to look for.
			thread.Start(Int64Value);
		}

		//Cancel the scan started.
		public void CancelScan()
		{
			//Raise the event that shows that the last scan task is canceled as user asked,
			ScanCanceledEventArgs cancelEventArgs = new ScanCanceledEventArgs();
			ScanCanceled(this, cancelEventArgs);

			//and then abort the thread that scanes the memory.
			thread.Abort();
		}
		#endregion

		#region Private methods
		//The memory scan method for the 16 bit values.
		private void Int16Scaner(object int16Object)
		{
			//Get the short value out of the object to look for it.
			Int16 int16Value = (Int16)int16Object;

			//Define a List object to hold the found memory addresses.
			List<int> finalList = new List<int>();

			//Open the pocess to read the memory.
			reader.OpenProcess();

			//Create a new instant of the ScanProgressEventArgs class to be used to raise the
			//ScanProgressed event and pass the percentage of scan, during the scan progress.
			ScanProgressChangedEventArgs scanProgressEventArgs;

			//Progress percentage.
			int progress;

			//This will be used to check if any bytes have been read from the memory.
			int bytesReadSize;

			//An array to hold the bytes read from the memory.
			byte[] array;

			for (int i = 0; i < addresses.Length; i++)
			{
				//Calculte and set the progress percentage.
				progress = (int)(((double)i / (double)addresses.Length) * 100d);

				//Prepare and set the ScanProgressed event and raise the event.
				scanProgressEventArgs = new ScanProgressChangedEventArgs(progress);
				ScanProgressChanged(this, scanProgressEventArgs);

				//Read the bytes from the memory.
				array = reader.ReadProcessMemory((IntPtr)addresses[i], Int16BytesCount, out bytesReadSize);

				//If any byte is read from the memory (there has been any bytes in the memory block),
				if (bytesReadSize > 0)
				{
					//If any value is equal to what we are looking for,
					if (BitConverter.ToInt16(array, 0) == int16Value)
					{
						//add it's memory address to the finalList.
						finalList.Add(addresses[i]);
					}
				}
			}
			//Close the handle to the process to avoid process errors.
			reader.CloseHandle();

			//Prepare the ScanProgressed and set the progress percentage to 100% and raise the event.
			scanProgressEventArgs = new ScanProgressChangedEventArgs(100);
			ScanProgressChanged(this, scanProgressEventArgs);

			//Prepare and raise the ScanCompleted event.
			ScanCompletedEventArgs scanCompleteEventArgs = new ScanCompletedEventArgs(finalList.ToArray());
			ScanCompleted(this, scanCompleteEventArgs);
		}

		//The memory scan method for the 32 bit values.
		private void Int32Scaner(object int32Object)
		{
			//Get the int value out of the object to look for it.
			Int32 int32Value = (Int32)int32Object;

			//Define a List object to hold the found memory addresses.
			List<int> finalList = new List<int>();

			//Open the pocess to read the memory.
			reader.OpenProcess();

			//Create a new instant of the ScanProgressEventArgs class to be used to raise the
			//ScanProgressed event and pass the percentage of scan, during the scan progress.
			ScanProgressChangedEventArgs scanProgressEventArgs;

			//Progress percentage.
			int progress;

			//This will be used to check if any bytes have been read from the memory.
			int bytesReadSize;

			//An array to hold the bytes read from the memory.
			byte[] array;

			for (int i = 0; i < addresses.Length; i++)
			{
				//Calculte and set the progress percentage.
				progress = (int)(((double)i / (double)addresses.Length) * 100d);

				//Prepare and set the ScanProgressed event and raise the event.
				scanProgressEventArgs = new ScanProgressChangedEventArgs(progress);
				ScanProgressChanged(this, scanProgressEventArgs);

				//Read the bytes from the memory.
				array = reader.ReadProcessMemory((IntPtr)addresses[i], Int32BytesCount, out bytesReadSize);

				//If any byte is read from the memory (there has been any bytes in the memory block),
				if (bytesReadSize > 0)
				{
					//If any value is equal to what we are looking for,
					if (BitConverter.ToInt32(array, 0) == int32Value)
					{
						//add it's memory address to the finalList.
						finalList.Add(addresses[i]);
					}
				}
			}
			//Close the handle to the process to avoid process errors.
			reader.CloseHandle();

			//Prepare the ScanProgressed and set the progress percentage to 100% and raise the event.
			scanProgressEventArgs = new ScanProgressChangedEventArgs(100);
			ScanProgressChanged(this, scanProgressEventArgs);

			//Prepare and raise the ScanCompleted event.
			ScanCompletedEventArgs scanCompleteEventArgs = new ScanCompletedEventArgs(finalList.ToArray());
			ScanCompleted(this, scanCompleteEventArgs);
		}

		//The memory scan method for the 64 bit values.
		private void Int64Scaner(object int64Object)
		{
			//Get the long value out of the object to look for it.
			Int64 int64Value = (Int64)int64Object;

			//Define a List object to hold the found memory addresses.
			List<int> finalList = new List<int>();

			//Open the pocess to read the memory.
			reader.OpenProcess();

			//Create a new instant of the ScanProgressEventArgs class to be used to raise the
			//ScanProgressed event and pass the percentage of scan, during the scan progress.
			ScanProgressChangedEventArgs scanProgressEventArgs;

			//Progress percentage.
			int progress;

			//This will be used to check if any bytes have been read from the memory.
			int bytesReadSize;

			//An array to hold the bytes read from the memory.
			byte[] array;

			for (int i = 0; i < addresses.Length; i++)
			{
				//Calculte and set the progress percentage.
				progress = (int)(((double)i / (double)addresses.Length) * 100d);

				//Prepare and set the ScanProgressed event and raise the event.
				scanProgressEventArgs = new ScanProgressChangedEventArgs(progress);
				ScanProgressChanged(this, scanProgressEventArgs);

				//Read the bytes from the memory.
				array = reader.ReadProcessMemory((IntPtr)addresses[i], Int64BytesCount, out bytesReadSize);

				//If any byte is read from the memory (there has been any bytes in the memory block),
				if (bytesReadSize > 0)
				{
					//If any value is equal to what we are looking for,
					if (BitConverter.ToInt64(array, 0) == int64Value)
					{
						//add it's memory address to the finalList.
						finalList.Add(addresses[i]);
					}
				}
			}
			//Close the handle to the process to avoid process errors.
			reader.CloseHandle();

			//Prepare the ScanProgressed and set the progress percentage to 100% and raise the event.
			scanProgressEventArgs = new ScanProgressChangedEventArgs(100);
			ScanProgressChanged(this, scanProgressEventArgs);

			//Prepare and raise the ScanCompleted event.
			ScanCompletedEventArgs scanCompleteEventArgs = new ScanCompletedEventArgs(finalList.ToArray());
			ScanCompleted(this, scanCompleteEventArgs);
		}
		#endregion
		#endregion
	}

	public class MemoryFreeze
	{
		#region Enum and structs
		//Struct with 3 properties to retrieve a memory addresse's information.
		public struct MemoryRecords
		{
			//Internal fields.
			internal int address;
			internal object value;
			internal DataType type;

			//Public properties.
			public int Address
			{
				get
				{
					return address;
				}
			}
			public object Value
			{
				get
				{
					return value;
				}
			}
			public DataType Type
			{
				get
				{
					return type;
				}
			}
		}

		//A struct to hold the memory address, value and data type.
		struct memoryRecord
		{
			public int address;
			public DataType type;
			public object value;
		}

		//Enum of data types(16 bit, 32 bit or 64 bit).
		public enum DataType : int
		{
			Int16 = 2,
			Int32 = 4,
			Int64 = 8
		}
		#endregion

		#region Fields
		//List of memoryRecords to hold the addresses and their related information.
		List<memoryRecord> records;

		//A ProcessMemoryReader object to write the values to the memory addresses.
		ProcessMemoryReader writer;

		//Timer object to tick and freeze.
		System.Timers.Timer timer;
		#endregion

		#region Property
		//a property to retrieve the current list of memory addresses set to be freezed.
		public MemoryRecords[] FreezedMemoryAddresses
		{
			get
			{
				//Create an array of MemoryRecords by the lenght of
				//records.Count (the number of memory addresses are being freezed).
				MemoryRecords[] memoryRecords = new MemoryRecords[records.Count];
				//Loop and set the information.
				for (int i = 0; i < memoryRecords.Length; i++)
				{
					memoryRecords[i].address = records[i].address;
					memoryRecords[i].type = records[i].type;
					memoryRecords[i].value = records[i].value;
				}
				//Return the array.
				return memoryRecords;
			}
		}
		#endregion

		#region Methods
		#region Public methods
		//Class entry point.
		//Define fields and event handlers and also set the process of the ProcessMemoryReader object.
		public MemoryFreeze(Process process)
		{
			timer = new System.Timers.Timer();

			writer = new ProcessMemoryReader();

			writer.ReadProcess = process;

			records = new List<memoryRecord>();

			timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
		}

		//Add a memory address and a 16 bit value to be written in the address.
		public void AddMemoryAddress(int MemoryAddress, Int16 Value)
		{
			memoryRecord item;
			item.address = MemoryAddress;
			item.type = DataType.Int16;
			item.value = Value;
			records.Add(item);
		}

		//Add a memory address and a 32 bit value to be written in the address.
		public void AddMemoryAddress(int MemoryAddress, Int32 Value)
		{
			memoryRecord item;
			item.address = MemoryAddress;
			item.type = DataType.Int32;
			item.value = Value;
			records.Add(item);
		}

		//Add a memory address and a 64 bit value to be written in the address.
		public void AddMemoryAddress(int MemoryAddress, Int64 Value)
		{
			memoryRecord item;
			item.address = MemoryAddress;
			item.type = DataType.Int64;
			item.value = Value;
			records.Add(item);
		}

		//Start the timer with the given Interval to start looping and so start freezing.
		public void StartFreezing(double Interval)
		{
			timer.Interval = Interval;

			timer.Start();
		}

		//Stop the timer and so stop the freezing loops.
		public void StopFreezing()
		{
			timer.Stop();
		}
		#endregion

		#region Private method
		//The method will be called in every timer's ticking.
		void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			//Open the process.
			writer.OpenProcess();

			//This is used, just to make the write process work but the value is not used.
			int bytesWritten;

			//Loop and set the value of all addresses.
			for (int i = 0; i < records.Count; i++)
			{
				//If the value type for the current address is 16 bit so take the Int16 out of the object,
				//and write the memory with the given value.
				if (records[i].type == DataType.Int16)
				{
					writer.WriteProcessMemory((IntPtr)records[i].address, BitConverter.GetBytes((Int16)records[i].value), out bytesWritten);
				}
				//If the value type for the current address is 32 bit so take the Int32 out of the object,
				//and write the memory with the given value.
				if (records[i].type == DataType.Int32)
				{
					writer.WriteProcessMemory((IntPtr)records[i].address, BitConverter.GetBytes((Int32)records[i].value), out bytesWritten);
				}
				//If the value type for the current address is 64 bit so take the Int64 out of the object,
				//and write the memory with the given value.
				if (records[i].type == DataType.Int64)
				{
					writer.WriteProcessMemory((IntPtr)records[i].address, BitConverter.GetBytes((Int64)records[i].value), out bytesWritten);
				}
			}
			//Close the handle to the process.
			writer.CloseHandle();
		}
		#endregion
		#endregion
	}

	#region EventArgs classes
	public class ScanProgressChangedEventArgs : EventArgs
	{
		public ScanProgressChangedEventArgs(int Progress)
		{
			progress = Progress;
		}
		private int progress;
		public int Progress
		{
			set
			{
				progress = value;
			}
			get
			{
				return progress;
			}
		}
	}

	public class ScanCompletedEventArgs : EventArgs
	{
		public ScanCompletedEventArgs(int[] MemoryAddresses)
		{
			memoryAddresses = MemoryAddresses;
		}
		private int[] memoryAddresses;
		public int[] MemoryAddresses
		{
			set
			{
				memoryAddresses = value;
			}
			get
			{
				return memoryAddresses;
			}
		}
	}

	public class ScanCanceledEventArgs : EventArgs
	{
		public ScanCanceledEventArgs()
		{
		}
	}
	#endregion

	#region ProcessMemoryReader class
	//Thanks goes to Arik Poznanski for P/Invokes and methods needed to read and write the Memory
	//For more information refer to "Minesweeper, Behind the scenes" article by Arik Poznanski at Codeproject.com
	class ProcessMemoryReader
	{

		public ProcessMemoryReader()
		{
		}

		/// <summary>	
		/// Process from which to read		
		/// </summary>
		public Process ReadProcess
		{
			get
			{
				return m_ReadProcess;
			}
			set
			{
				m_ReadProcess = value;
			}
		}

		private Process m_ReadProcess = null;

		private IntPtr m_hProcess = IntPtr.Zero;

		public void OpenProcess()
		{
			//			m_hProcess = ProcessMemoryReaderApi.OpenProcess(ProcessMemoryReaderApi.PROCESS_VM_READ, 1, (uint)m_ReadProcess.Id);
			ProcessMemoryReaderApi.ProcessAccessType access;
			access = ProcessMemoryReaderApi.ProcessAccessType.PROCESS_VM_READ
					| ProcessMemoryReaderApi.ProcessAccessType.PROCESS_VM_WRITE
					| ProcessMemoryReaderApi.ProcessAccessType.PROCESS_VM_OPERATION;
			m_hProcess = ProcessMemoryReaderApi.OpenProcess((uint)access, 1, (uint)m_ReadProcess.Id);
		}

		public void CloseHandle()
		{
			try
			{
				int iRetValue;
				iRetValue = ProcessMemoryReaderApi.CloseHandle(m_hProcess);
				if (iRetValue == 0)
				{
					throw new Exception("CloseHandle failed");
				}
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(ex.Message, "error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
			}
		}

		public byte[] ReadProcessMemory(IntPtr MemoryAddress, uint bytesToRead, out int bytesRead)
		{
			byte[] buffer = new byte[bytesToRead];

			IntPtr ptrBytesRead;
			ProcessMemoryReaderApi.ReadProcessMemory(m_hProcess, MemoryAddress, buffer, bytesToRead, out ptrBytesRead);

			bytesRead = ptrBytesRead.ToInt32();

			return buffer;
		}

		public void WriteProcessMemory(IntPtr MemoryAddress, byte[] bytesToWrite, out int bytesWritten)
		{
			IntPtr ptrBytesWritten;
			ProcessMemoryReaderApi.WriteProcessMemory(m_hProcess, MemoryAddress, bytesToWrite, (uint)bytesToWrite.Length, out ptrBytesWritten);

			bytesWritten = ptrBytesWritten.ToInt32();
		}


		/// <summary>
		/// ProcessMemoryReader is a class that enables direct reading a process memory
		/// </summary>
		class ProcessMemoryReaderApi
		{
			// constants information can be found in <winnt.h>
			[Flags]
			public enum ProcessAccessType
			{
				PROCESS_TERMINATE = (0x0001),
				PROCESS_CREATE_THREAD = (0x0002),
				PROCESS_SET_SESSIONID = (0x0004),
				PROCESS_VM_OPERATION = (0x0008),
				PROCESS_VM_READ = (0x0010),
				PROCESS_VM_WRITE = (0x0020),
				PROCESS_DUP_HANDLE = (0x0040),
				PROCESS_CREATE_PROCESS = (0x0080),
				PROCESS_SET_QUOTA = (0x0100),
				PROCESS_SET_INFORMATION = (0x0200),
				PROCESS_QUERY_INFORMATION = (0x0400)
			}

			// function declarations are found in the MSDN and in <winbase.h> 

			//		HANDLE OpenProcess(
			//			DWORD dwDesiredAccess,  // access flag
			//			BOOL bInheritHandle,    // handle inheritance option
			//			DWORD dwProcessId       // process identifier
			//			);
			[DllImport("kernel32.dll")]
			public static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);

			//		BOOL CloseHandle(
			//			HANDLE hObject   // handle to object
			//			);
			[DllImport("kernel32.dll")]
			public static extern Int32 CloseHandle(IntPtr hObject);

			//		BOOL ReadProcessMemory(
			//			HANDLE hProcess,              // handle to the process
			//			LPCVOID lpBaseAddress,        // base of memory area
			//			LPVOID lpBuffer,              // data buffer
			//			SIZE_T nSize,                 // number of bytes to read
			//			SIZE_T * lpNumberOfBytesRead  // number of bytes read
			//			);
			[DllImport("kernel32.dll")]
			public static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesRead);

			//		BOOL WriteProcessMemory(
			//			HANDLE hProcess,                // handle to process
			//			LPVOID lpBaseAddress,           // base of memory area
			//			LPCVOID lpBuffer,               // data buffer
			//			SIZE_T nSize,                   // count of bytes to write
			//			SIZE_T * lpNumberOfBytesWritten // count of bytes written
			//			);
			[DllImport("kernel32.dll")]
			public static extern Int32 WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesWritten);


		}
	}
	#endregion
}
