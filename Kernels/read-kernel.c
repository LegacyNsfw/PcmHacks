///////////////////////////////////////////////////////////////////////////////
// This is a tiny kernel to validate the ability to read and write messages
// using the PCM's DLC. It will echo received messages, and reboot on command.
///////////////////////////////////////////////////////////////////////////////
//
// After we have a trivial kernel that can send and received messages, the
// reusable stuff should be moved into common.c and referenced from there.
//
// But for now I want to keep this kernel as tiny and as simple as possible,
// to simplify debugging.
//
///////////////////////////////////////////////////////////////////////////////

#include "common.h"

///////////////////////////////////////////////////////////////////////////////
// Read the specified address range and send the data to the tool.
///////////////////////////////////////////////////////////////////////////////
void ReadAndSend(unsigned start, unsigned length)
{
	// TODO: Validate the start address and length, fail if unreasonable.


	// Send the "agree" response.
	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x75;

	MessageBuffer[4] = 0x01;
	MessageBuffer[5] = 0x54;
	MessageBuffer[6] = 0x6C;
	MessageBuffer[7] = 0xF0;

	WriteMessage(MessageBuffer, 8, Complete);
	
	// Send the payload
	MessageBuffer[0] = 0x6D;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x36;
	MessageBuffer[4] = 0x01;
	MessageBuffer[5] = (char)(length >> 8);
	MessageBuffer[6] = (char)length;
	MessageBuffer[7] = (char)(start >> 16);
	MessageBuffer[8] = (char)(start >> 8);
	MessageBuffer[9] = (char)start;
	WriteMessage(MessageBuffer, 10, Start);

	unsigned short checksum = StartChecksum();
	checksum += AddReadPayloadChecksum((char*)start, length);

	WriteMessage((char*) start, length, Middle);

	WriteMessage((char*)&checksum, 2, End);
}

///////////////////////////////////////////////////////////////////////////////
// Process a mode-35 read request.
///////////////////////////////////////////////////////////////////////////////
void ReadMode35()
{
	unsigned length = MessageBuffer[5];
	length <<= 8;
	length |= MessageBuffer[6];

	unsigned start = MessageBuffer[7];
	start <<= 8;
	start |= MessageBuffer[8];
	start <<= 8;
	start |= MessageBuffer[9];

	ReadAndSend(start, length);
}

///////////////////////////////////////////////////////////////////////////////
// Process an incoming message.
///////////////////////////////////////////////////////////////////////////////
void ProcessMessage()
{
	if (MessageBuffer[1] != 0x10)
	{
		// We're not the destination, ignore this message.
		// SendToolPresent(0xB0, MessageBuffer[1], 0);
		return;
	}

	if (MessageBuffer[2] != 0xF0)
	{
		// This didn't come from the tool, ignore this message.
		//SendToolPresent(0xB1, MessageBuffer[2], 0);
		return;
	}

	if (MessageBuffer[3] == 0x35)
	{
		//SendToolPresent(0x00, MessageBuffer[3], 0);
		ReadMode35();
		return;
	}

	if (MessageBuffer[3] == 0x37)
	{
		// ReadMode37();
		SendToolPresent(0xB2, MessageBuffer[3], 0, 0);
		return;
	}
}

///////////////////////////////////////////////////////////////////////////////
// This is the entry point for the kernel.
///////////////////////////////////////////////////////////////////////////////
int
__attribute__((section(".kernelstart")))
KernelStart(void)
{
	// Disable peripheral interrupts
	asm("ORI #0x700, %SR");

	ScratchWatchdog();

	*DLC_InterruptConfiguration = 0x00;
	LongSleepWithWatchdog();

	// Flush the DLC
	*DLC_Transmit_Command = 0x03;
	*DLC_Transmit_FIFO = 0x00;

	ClearMessageBuffer();
	WasteTime();

	SendToolPresent(0, 0, 0, 0);
	LongSleepWithWatchdog();

	// This loop runs out quickly, to force the PCM to reboot, to speed up testing.
	// The real kernel should probably loop for a much longer time (possibly forever),
	// to allow the app to recover from any failures. 
	// If we choose to loop forever we need a good story for how to get out of that state.
	// Pull the PCM fuse? Give the app button to tell the kernel to reboot?
	// for(int iterations = 0; iterations < 100; iterations++)
	int iterations = 0;
	int timeout = 100;
	int lastMessage = iterations - timeout;
	for(;;)
	{
		iterations++;

		ScratchWatchdog();

		char completionCode = 0xFF;
		char readState = 0xFF;
		int length = ReadMessage(&completionCode, &readState);
		if (length == 0)
		{
			// If no message received for N iterations, reboot.
			if (iterations > (lastMessage + timeout))
			{
				Reboot(0xFFFFFFFF);
			}

			continue;
		}

		if ((completionCode & 0x30) != 0x00)
		{
			// This is a transmit error. Just ignore it and wait for the tool to retry.
			continue;
		}

		lastMessage = iterations;

		// Did the tool just request a reboot?
		if (MessageBuffer[3] == 0x20)
		{
			LongSleepWithWatchdog();
			Reboot(iterations | 0x0E000000);
		}

		ProcessMessage();
	}

	// This shouldn't happen. But, just in case...
	Reboot(iterations | 0x0F000000);
}
