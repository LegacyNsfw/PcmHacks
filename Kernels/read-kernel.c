///////////////////////////////////////////////////////////////////////////////
// This kernel can read PCM memory and supports uploading a replacement kernel.
///////////////////////////////////////////////////////////////////////////////

#include "common.h"

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
		if (MessageBuffer[4] == 0x00)
		{
			HandleVersionQuery();
		}
		else
		{
			SendToolPresent(
				0x3D,
				MessageBuffer[4],
				0,
				0);
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
// This is needed to satisfy the compiler until we publish the secret sauce.
///////////////////////////////////////////////////////////////////////////////
unsigned char WriteToFlash(const unsigned length, const unsigned startAddress, unsigned char *data, int testWrite)
{
	// This space intentionally left blank.
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

	DLC_INTERRUPTCONFIGURATION = 0x00;
	LongSleepWithWatchdog();

	// Flush the DLC
	DLC_TRANSMIT_COMMAND = 0x03;
	DLC_TRANSMIT_FIFO = 0x00;

	ClearMessageBuffer();
	WasteTime();

	SendToolPresent(0, 0, 0, 0);
	LongSleepWithWatchdog();

	// If we choose to loop forever we need a good story for how to get out of that state.
	// Pull the PCM fuse? Give the app button to tell the kernel to reboot?
	// A timeout of 10,000 iterations results in a roughly five-second timeout.
	// That's probably good for release, but longer is better for development.
	int iterations = 0;
	int timeout = 10000 * 1000;
	int lastMessage = (iterations - timeout) + 1;

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
//			if (iterations > (lastMessage + timeout))
//			{
//				Reboot(0xFFFFFFFF);
//			}

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

		lastMessage = iterations;

		// Did the tool just request a reboot?
		if (MessageBuffer[3] == 0x20)
		{
			LongSleepWithWatchdog();
			Reboot(0xCC000000 | iterations);
		}

		ProcessMessage();
	}

	// This shouldn't happen. But, just in case...
	Reboot(0xFF000000 | iterations);
}
