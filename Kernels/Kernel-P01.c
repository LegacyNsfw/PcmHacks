///////////////////////////////////////////////////////////////////////////////
// This kernel can read PCM memory and supports uploading a replacement kernel.
///////////////////////////////////////////////////////////////////////////////

#include "common.h"
#include "flash.h"

uint32_t __attribute((section(".kerneldata"))) flashIdentifier;

// This kernel uses Mode 3D extensively, because apparently nothing else does. Submodes are:
//
// 00 - Get kernel version
// 01 - Query flash chip
// 02 - Query CRC
// 03 - unlock flash
// 04 - lock flash
// 05 - erase calibration
// 06 - erase everything? (not until everything else is thoroughly proven)
// FF - send debug info (because I was curious about the stack address)
//
// Writes to flash use mode 35 and mode 36, like writing to RAM.

///////////////////////////////////////////////////////////////////////////////
// Send a success or failure message.
///////////////////////////////////////////////////////////////////////////////
void SendReply(unsigned char success, unsigned char submode, unsigned char code, unsigned char data)
{
	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;

	if (success)
	{
		MessageBuffer[3] = 0x7D;
		MessageBuffer[4] = submode;
		MessageBuffer[5] = code;
		MessageBuffer[6] = data;
	}
	else
	{
		MessageBuffer[3] = 0x7F;
		MessageBuffer[4] = 0x3D;
		MessageBuffer[5] = submode;
		MessageBuffer[6] = code;
		MessageBuffer[7] = data;
	}

	WriteMessage(MessageBuffer, success ? 7 : 8, Complete);
}

///////////////////////////////////////////////////////////////////////////////
// Get the manufacturer and type of flash chip.
///////////////////////////////////////////////////////////////////////////////
void HandleFlashChipQuery()
{
	ScratchWatchdog();

	// Try the Intel method first
	flashIdentifier = Intel_GetFlashId();

	ScratchWatchdog();

	// If the ID query is unsuccessful, we wont get the Intel ID, so try AMD
	if ((flashIdentifier >> 16) != 0x0089)
	{
		// Try the AMD method next
		flashIdentifier = Amd_GetFlashId();
	}

	ScratchWatchdog();

	// The AllPro and ScanTool devices need a short delay to switch from
	// sending to receiving. Otherwise they'll miss the response.
	VariableSleep(1);

	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x7D;
	MessageBuffer[4] = 0x01;
	MessageBuffer[5] = flashIdentifier >> 24;
	MessageBuffer[6] = flashIdentifier >> 16;
	MessageBuffer[7] = flashIdentifier >> 8;
	MessageBuffer[8] = flashIdentifier;
	WriteMessage(MessageBuffer, 9, Complete);
}

///////////////////////////////////////////////////////////////////////////////
// The the CRC of a memory range.
// This takes just long enough for the app to time out. So we pause just long
// enough for the reply to come back before the second timeout.
///////////////////////////////////////////////////////////////////////////////
void HandleCrcQuery()
{
	unsigned length = (MessageBuffer[5] << 16) + (MessageBuffer[6] << 8) + MessageBuffer[7];
	unsigned address = (MessageBuffer[8] << 16) + (MessageBuffer[9] << 8) + MessageBuffer[10];

	// Convert to names and types that match the CRC code.
	unsigned char *message = (unsigned char*)address;
	int nBytes = length;

	char path;
	if (!crcIsStarted(message, nBytes))
	{
		path = 1;
		crcStart(message, nBytes);
	}
	else
	{
		path = 2;
		crcProcessSlice();
	}

	ElmSleep();

	if (crcIsDone(message, nBytes))
	{
		unsigned crc = crcGetResult();
		MessageBuffer[0] = 0x6C;
		MessageBuffer[1] = 0xF0;
		MessageBuffer[2] = 0x10;
		MessageBuffer[3] = 0x7D;
		MessageBuffer[4] = 0x02;
		MessageBuffer[5] = (char)(crcLength >> 16);
		MessageBuffer[6] = (char)(crcLength >> 8);
		MessageBuffer[7] = (char)crcLength;
		MessageBuffer[8] = (char)((uint32_t)crcStartAddress >> 16);
		MessageBuffer[9] = (char)((uint32_t)crcStartAddress >> 8);
		MessageBuffer[10] = (char)(uint32_t)crcStartAddress;
		MessageBuffer[11] = (char)(crc >> 24);
		MessageBuffer[12] = (char)(crc >> 16);
		MessageBuffer[13] = (char)(crc >> 8);
		MessageBuffer[14] = (char)crc;
		WriteMessage(MessageBuffer, 15, Complete);
	}
	else
	{
		// This is an abuse of the protocol, but I want to keep these
		// messages short, and using 7F 3D 02 would make it hard to tell
		// whether CRC is in progress or the kernel just isn't loaded.
		// So this reply has a legitimate mode, and a bogus submode.
		MessageBuffer[0] = 0x6C;
		MessageBuffer[1] = 0xF0;
		MessageBuffer[2] = 0x10;
		MessageBuffer[3] = 0x7D;
		MessageBuffer[4] = 0xFF;
		MessageBuffer[5] = path;
		WriteMessage(MessageBuffer, 6, Complete);
	}
}

///////////////////////////////////////////////////////////////////////////////
// Tell the app which OS is installed on this PCM.
//
// It was tempting to just implement the same OS ID query as the PCM software,
// but then the app would need to do a kernel request of some type to determine
// whether the PCM is running the GM OS or the kernel. However, in almost all
// cases, the PCM will be running the GM OS, and that kernel request would slow
// down the most common usage scenario.
//
// So we keep the common scenario fast by having the standard OS ID request
// succeed only when the standard operating system is running. When it fails,
// that puts the app into a slower path were it checks for a kernel and then
// asks the kernel what OS is installed. Or it checks for recovery mode, loads
// the kernel, and then asks the kernel what OS is installed.
///////////////////////////////////////////////////////////////////////////////
void HandleOperatingSystemQuery()
{
	ElmSleep();
#if defined P10
	uint8_t *osid = (uint8_t*)0x52E;
#elif defined P12
	uint8_t *osid = (uint8_t*)0x8004;
#else
	uint8_t *osid = (uint8_t*)0x504;
#endif
	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x7D;
	MessageBuffer[4] = 0x03;
	MessageBuffer[5] = osid[0];
	MessageBuffer[6] = osid[1];
	MessageBuffer[7] = osid[2];
	MessageBuffer[8] = osid[3];
	WriteMessage(MessageBuffer, 9, Complete);
}

///////////////////////////////////////////////////////////////////////////////
// Erase the given block.
///////////////////////////////////////////////////////////////////////////////
void HandleEraseBlock()
{
	unsigned address = (MessageBuffer[5] << 16) + (MessageBuffer[6] << 8) + MessageBuffer[7];
	uint8_t status = 0;

	switch (flashIdentifier)
	{
		case FLASH_ID_INTEL_28F400B:
		case FLASH_ID_INTEL_28F800B:
			status = Intel_EraseBlock(address);
			break;

		case FLASH_ID_AMD_AM29F800BB:
		case FLASH_ID_AMD_AM29BL162C:
		case FLASH_ID_AMD_AM29BL802C:
			status = Amd_EraseBlock(address);
			break;

		default:
			VariableSleep(2);
			SendReply(0, 0x05, 0xFF, 0xFF);
			return;
	}

	// The AllPro and ScanTool devices need a short delay to switch from
	// sending to receiving. Otherwise they'll miss the response.
	// Also, give the lock-flash operation time to take full effect, because
	// the signal quality is degraded and the AllPro and ScanTool can't read
	// messages when the PCM is in that state.
	VariableSleep(2);

	SendReply(1, 0x05, status, 0x00);
}

///////////////////////////////////////////////////////////////////////////////
// Erase everything? Nope, not yet.
///////////////////////////////////////////////////////////////////////////////
void HandleEraseEverythingRequest()
{
	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x7F; // Reject
	MessageBuffer[4] = 0x3D;
	MessageBuffer[5] = 0x06;
	MessageBuffer[6] = 0x00;

	WriteMessage(MessageBuffer, 7, Complete);
}

///////////////////////////////////////////////////////////////////////////////
// This is available for arbitrary diagnostic / troubleshooting use.
///////////////////////////////////////////////////////////////////////////////
extern unsigned int *crcTable;
void HandleDebugQuery()
{
	uint32_t value = 0x12345678;

	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x7D;
	MessageBuffer[4] = 0xFF;
	MessageBuffer[5] = (char)(value >> 24);
	MessageBuffer[6] = (char)(value >> 16);
	MessageBuffer[7] = (char)(value >> 8);
	MessageBuffer[8] = (char)(value >> 0);
	WriteMessage(MessageBuffer, 9, Complete);
}

///////////////////////////////////////////////////////////////////////////////
// Write data to flash memory.
// This is invoked by HandleWriteMode36 in common-readwrite.c
// read-kernel.c has a stub to keep the compiler happy until this is released.
///////////////////////////////////////////////////////////////////////////////
unsigned char WriteToFlash(unsigned int payloadLengthInBytes, unsigned int startAddress, unsigned char *payloadBytes, int testWrite)
{
	switch (flashIdentifier)
	{
		case FLASH_ID_INTEL_28F400B:
		case FLASH_ID_INTEL_28F800B:
			return Intel_WriteToFlash(payloadLengthInBytes, startAddress, payloadBytes, testWrite);

		case FLASH_ID_AMD_AM29F800BB:
		case FLASH_ID_AMD_AM29BL162C:
		case FLASH_ID_AMD_AM29BL802C:
			return Amd_WriteToFlash(payloadLengthInBytes, startAddress, payloadBytes, testWrite);

		default:
			return 0xEE;
	}
}

///////////////////////////////////////////////////////////////////////////////
// Process an incoming message.
///////////////////////////////////////////////////////////////////////////////
void ProcessMessage(int iterations)
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
	case 0x20:
		LongSleepWithWatchdog();
		Reboot(0xCC000000 | iterations);
		break;

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
			HandleVersionQuery();
			break;

		case 0x01:
			HandleFlashChipQuery();
			break;

		case 0x02:
			HandleCrcQuery();
			break;

		case 0x03:
			HandleOperatingSystemQuery();
			break;

		case 0x04:
			// This submode is available for future use.
			//
			// It was originally intended for flash lock (0x03 was for unlock) but that
			// creates so much noise on the VPW line that communication stops working.
			break;

		case 0x05:
			HandleEraseBlock();
			crcReset();
			break;

		case 0xFF:
			HandleDebugQuery();
			break;

		default:
			SendToolPresent(
				0x3D,
				MessageBuffer[4],
				0,
				0);
			break;
		}
		break;

	case 0x3F:
		// Ignore tool-present messages.
		break;

	default:
		SendToolPresent(
			0xAA,
			MessageBuffer[2],
			MessageBuffer[3],
			MessageBuffer[4]);
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

	DLC_INTERRUPTCONFIGURATION = 0x00;
	crcInit();

	// Flush the DLC
	DLC_TRANSMIT_COMMAND = 0x03;
	DLC_TRANSMIT_FIFO = 0x00;

	ClearMessageBuffer();
	WasteTime();
	ScratchWatchdog();

	// The factory code jumps into the kernel before sending this message
	SendWriteSuccess(0);
	
	// This message proves that the kernel is now running (if you're watching the data bus).
	SendToolPresent(1, 2, 3, 4);
	LongSleepWithWatchdog();

	uint32_t iterations = 0;
	uint32_t timeout = 2500; // Timeout of 2500 = 2.2 seconds between messages. 5,000 = 3.9 seconds.
	uint32_t lastMessage = (iterations - timeout) + 1;
	uint32_t lastActivity = iterations - timeout;

	for(;;)
	{
		iterations++;

		ScratchWatchdog();

		char completionCode = 0xFF;
		char readState = 0xFF;
		int length = ReadMessage(&completionCode, &readState);
		if (length == 0)
		{
			if (iterations > (lastActivity + timeout))
			{
				SendToolPresent(110, 115, 102, 119);
				lastActivity = iterations;
			}

///////////////////////////////////////////////////////////////////////////////
// For now the plan is to let the kernel run indefinitely, and give the app
// an exit-kernel button. This should give us some flexibility in recovering
// from failed flashes, especially those involving the boot block.
//
//			// If no message received for N iterations, reboot.
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
			continue;
		}

		lastMessage = iterations;
		lastActivity = iterations;

		ProcessMessage(iterations);
	}

	// This shouldn't happen. But, just in case...
	Reboot(0xFF000000 | iterations);
}
