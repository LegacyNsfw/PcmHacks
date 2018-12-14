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

	// Give the tool time to proces that message (especially the AllPro)
	LongSleepWithWatchdog();

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

	WriteMessage((char*)start, length, Middle);

	WriteMessage((char*)&checksum, 2, End);
}

void WriteRequest()
{
	unsigned length = MessageBuffer[5];
	length <<= 8;
	length |= MessageBuffer[6];

	unsigned start = MessageBuffer[7];
	start <<= 8;
	start |= MessageBuffer[8];
	start <<= 8;
	start |= MessageBuffer[9];

	if ((length > 4096) || (start != 0xFFA000))
	{
		MessageBuffer[0] = 0x6C;
		MessageBuffer[1] = 0xF0;
		MessageBuffer[2] = 0x10;
		MessageBuffer[3] = 0x7F;
		MessageBuffer[4] = 0x34;

		WriteMessage(MessageBuffer, 5, Complete);
		return;
	}

	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x74;
	MessageBuffer[4] = 0x00;
	WriteMessage(MessageBuffer, 5, Complete);
}

typedef void(*EntryPoint)();

void Write()
{
	unsigned char command = MessageBuffer[4];

	unsigned length = MessageBuffer[5];
	length <<= 8;
	length |= MessageBuffer[6];

	unsigned start = MessageBuffer[7];
	start <<= 8;
	start |= MessageBuffer[8];
	start <<= 8;
	start |= MessageBuffer[9];

	// Validate range
	if ((length > 4096) || (start < 0xFFA000) || (start > 0xFFB000))
	{
		MessageBuffer[0] = 0x6D;
		MessageBuffer[1] = 0xF0;
		MessageBuffer[2] = 0x10;
		MessageBuffer[3] = 0x7F;
		MessageBuffer[4] = 0x36;
		WriteMessage(MessageBuffer, 5, Complete);
		return;
	}

	// Compute checksum
	unsigned short checksum = 0;
	for (int index = 4; index < length + 10; index++)
	{
		if (index % 1024 == 0)
		{
			ScratchWatchdog();
		}

		checksum = checksum + MessageBuffer[index];
	}

	// Validate checksum
	unsigned short expected = (MessageBuffer[10 + length] << 8) | MessageBuffer[10 + length + 1];
	if (checksum != expected)
	{
		MessageBuffer[0] = 0x6D;
		MessageBuffer[1] = 0xF0;
		MessageBuffer[2] = 0x10;
		MessageBuffer[3] = 0x7F; 
		MessageBuffer[4] = 0x36;
		MessageBuffer[5] = (char)((expected & 0xFF00) >> 8);
		MessageBuffer[6] = (char)(expected & 0x00FF);
		WriteMessage(MessageBuffer, 7, Complete);
		return;
	}

	// Copy content
	unsigned int address = 0;
	for (int index = 0; index < length; index++)
	{
		if (index % 50 == 1)
		{
			ScratchWatchdog();
		}

		address = start + index;
		*((unsigned char*)address) = MessageBuffer[10 + index];
	}

	// Send response
	MessageBuffer[0] = 0x6D;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x76;

	WriteMessage(MessageBuffer, 4, Complete);
		
	// Execute
	if (command == 0x80)
	{
		EntryPoint entryPoint = (EntryPoint)start;
		entryPoint();
	}
}

///////////////////////////////////////////////////////////////////////////////
// Process an incoming message.
///////////////////////////////////////////////////////////////////////////////
void ProcessMessage()
{
	if ((MessageBuffer[1] != 0x10) && (MessageBuffer[1] != 0xFE))
	{
		// We're not the destination, ignore this message.
		return;
	}

	if (MessageBuffer[2] != 0xF0)
	{
		// This didn't come from the tool, ignore this message.
		return;
	}

	switch (MessageBuffer[3])
	{
	case 0x34:
		WriteRequest();
		ClearBreadcrumbBuffer();
		break;

	case 0x35:
		ReadMode35();
		break;

	case 0x36:
		if (MessageBuffer[0] == 0x6D)
		{
			Write();
		}
		break;

	case 0x37:
		// ReadMode37();
		SendToolPresent(0xB2, MessageBuffer[3], 0, 0);
		break;

	default:
		SendToolPresent(
			0xAA,
			MessageBuffer[3],
			MessageBuffer[4],
			MessageBuffer[5]);
		break;
	}
}

void SendBreadcrumbsReboot(char code, int breadcrumbs)
{
	char toolPresent[] = { 0x8C, 0xFE, 0xF0, 0x3F, code };
	WriteMessage(toolPresent, 5, Start);
	WriteMessage(BreadcrumbBuffer, breadcrumbs, End);
	LongSleepWithWatchdog();
	LongSleepWithWatchdog();
	Reboot(code);
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
	int lastMessage = (iterations - timeout) + 1;

#ifdef MODEBYTE_BREADCRUMBS
	int breadcrumbIndex = 0;
#endif

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

		if (readState != 1)
		{
			SendToolPresent(0xBB, 0xBB, readState, readState);
			LongSleepWithWatchdog();
			continue;
		}

		BreadcrumbBuffer[breadcrumbIndex] = MessageBuffer[3];
		breadcrumbIndex++;

		lastMessage = iterations;

		// Did the tool just request a reboot?
		if (MessageBuffer[3] == 0x20)
		{
			LongSleepWithWatchdog();
#ifdef MODEBYTE_BREADCRUMBS
			SendBreadcrumbsReboot(0xEE, breadcrumbIndex);
#else
			Reboot(0xCC000000 | iterations);
#endif
		}

		ProcessMessage();
	}

	// This shouldn't happen. But, just in case...
#ifdef MODEBYTE_BREADCRUMBS
	SendBreadcrumbsReboot(0xFF, breadcrumbIndex);
#else
	Reboot(0xFF000000 | iterations);
#endif
}
