///////////////////////////////////////////////////////////////////////////////
// This kernel can read PCM memory and supports uploading a replacement kernel.
///////////////////////////////////////////////////////////////////////////////

#include "common.h"

void HandleFlashChipQueryMode3D()
{
	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x7D;
	MessageBuffer[4] = 0x00;
	MessageBuffer[5] = 0x12;
	MessageBuffer[6] = 0x34;
	MessageBuffer[7] = 0x12;
	MessageBuffer[8] = 0x34;
	WriteMessage(MessageBuffer, 9, Complete);
}

void HandleCrcQueryMode3D()
{
	unsigned length = MessageBuffer[5];
	length <<= 8;
	length |= MessageBuffer[6];

	unsigned start = MessageBuffer[7];
	start <<= 8;
	start |= MessageBuffer[8];
	start <<= 8;
	start |= MessageBuffer[9];

	//unsigned crc = 0x12345678;
//	ScratchWatchdog();
	crcInit();
//	ScratchWatchdog();
	unsigned crc = crcFast((char*) start, length);

	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x7D;
	MessageBuffer[4] = 0x01;
	MessageBuffer[5] = (char)(length >> 8);
	MessageBuffer[6] = (char)length;
	MessageBuffer[7] = (char)(start >> 16);
	MessageBuffer[8] = (char)(start >> 8);
	MessageBuffer[9] = (char)start;
	MessageBuffer[10] = (char)(crc >> 24);
	MessageBuffer[11] = (char)(crc >> 16);
	MessageBuffer[12] = (char)(crc >> 8);
	MessageBuffer[13] = (char)crc;
	WriteMessage(MessageBuffer, 14, Complete);
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
		HandleWriteRequestMode34();
		ClearBreadcrumbBuffer();
		break;

	case 0x35:
		HandleReadMode35();
		break;

	case 0x36:
		if (MessageBuffer[0] == 0x6D)
		{
			HandleWriteMode36();
		}
		break;

	case 0x37:
		// ReadMode37();
		SendToolPresent(0xB2, MessageBuffer[3], 0, 0);
		break;

	case 0x3D:
		switch(MessageBuffer[4])
		{
		case 0x00:
			HandleFlashChipQueryMode3D();
			break;

		case 0x01:
			HandleCrcQueryMode3D();
			break;
		}
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

#ifdef MODEBYTE_BREADCRUMBS
		BreadcrumbBuffer[breadcrumbIndex] = MessageBuffer[3];
		breadcrumbIndex++;
#endif
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
