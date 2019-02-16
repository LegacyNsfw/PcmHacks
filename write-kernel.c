///////////////////////////////////////////////////////////////////////////////
// This kernel can read PCM memory and supports uploading a replacement kernel.
///////////////////////////////////////////////////////////////////////////////

#include "common.h"

#define SIM_BASE        0x00FFFA00
#define SIM_CSBARBT     (*(unsigned short *)(SIM_BASE + 0x48)) // CSRBASEREG, boot chip select, chip select base addr boot ROM reg,
                                                               // must be updated to $0006 on each update of flash CE/WE states
#define SIM_CSORBT      (*(unsigned short *)(SIM_BASE + 0x4a)) // CSROPREG, Chip select option boot ROM reg., $6820 for normal op
#define SIM_CSBAR0      (*(unsigned short *)(SIM_BASE + 0x4c)) // CSBASEREG, chip selects
#define SIM_CSOR0       (*(unsigned short *)(SIM_BASE + 0x4e)) // CSOPREG, *Chip select option reg., $1060 for normal op, $7060 for accessing flash chip
#define HARDWARE_IO     (*(unsigned short *)(0xFFFFE2FA))      // Hardware I/O reg

#define FLASH_BASE         (*(unsigned short *)(0x00000000))
#define FLASH_IDENTIFIER   (*(uint32_t *)(0x00000000))
#define FLASH_MANUFACTURER (*(uint16_t *)(0x00000000))
#define FLASH_DEVICE       (*(uint16_t *)(0x00000002))

#define SIGNATURE_COMMAND  0x9090
#define READ_ARRAY_COMMAND 0xFFFF

/*char volatile * const  SIM_MCR      =   SIM_BASE + 0x00; // Module Control register
char volatile * const  SIM_SYNCR    =   SIM_BASE + 0x04; // Clock synthesiser control register
char volatile * const  SIM_RSR      =   SIM_BASE + 0x07; // Reset Status
char volatile * const  SIM_SYPCR    =   SIM_BASE + 0x21; // System Protection
char volatile * const  SIM_PICR     =   SIM_BASE + 0x22; // Periodic Timer
char volatile * const  SIM_PITR     =   SIM_BASE + 0x24; //
char volatile * const  SIM_SWSR     =   SIM_BASE + 0x27; //
char volatile * const  SIM_CSPAR0   =   SIM_BASE + 0x44; // chip sellect pin assignment
char volatile * const  SIM_CSPAR1   =   SIM_BASE + 0x46; //

char volatile * const  SIM_CSBAR1   =   SIM_BASE + 0x50;
char volatile * const  SIM_CSOR1    =   SIM_BASE + 0x52;
char volatile * const  SIM_CSBAR2   =   SIM_BASE + 0x54;
char volatile * const  SIM_CSOR2    =   SIM_BASE + 0x56;
char volatile * const  SIM_CSBAR3   =   SIM_BASE + 0x58;
char volatile * const  SIM_CSOR3    =   SIM_BASE + 0x5a;
char volatile * const  SIM_CSBAR4   =   SIM_BASE + 0x5c;
char volatile * const  SIM_CSOR4    =   SIM_BASE + 0x5e;
char volatile * const  SIM_CSBAR5   =   SIM_BASE + 0x60;
char volatile * const  SIM_CSOR5    =   SIM_BASE + 0x62;
char volatile * const  SIM_CSBAR6   =   SIM_BASE + 0x64;
char volatile * const  SIM_CSOR6    =   SIM_BASE + 0x66;*/

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
void SendReply(unsigned char success, unsigned char submode, unsigned char code)
{
	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;

	if (success)
	{
		MessageBuffer[3] = 0x7D;
		MessageBuffer[4] = submode;
		MessageBuffer[5] = code;
	}
	else
	{
		MessageBuffer[3] = 0x7F;
		MessageBuffer[4] = 0x3D;
		MessageBuffer[5] = submode;
		MessageBuffer[6] = code;
	}

	WriteMessage(MessageBuffer, success ? 6 : 7, Complete);
}

///////////////////////////////////////////////////////////////////////////////
// Get the manufacturer and type of flash chip.
///////////////////////////////////////////////////////////////////////////////
uint32_t GetIntelId()
{
	SIM_CSBAR0 = 0x0006;
	SIM_CSORBT = 0x6820;

	// flash chip 12v A9 enable
	SIM_CSOR0 = 0x7060; 

	// Switch the flash into ID-query mode
	FLASH_BASE = SIGNATURE_COMMAND; 

	// Read the identifier from address zero.
	flashIdentifier = FLASH_IDENTIFIER;

	// Switch back to standard mode
	FLASH_BASE = READ_ARRAY_COMMAND; 

	SIM_CSOR0 = 0x1060; // flash chip 12v A9 disable
}

uint32_t GetAmdId()
{
	SIM_CSBAR0 = 0x0006;
	SIM_CSORBT = 0x6820;

	// flash chip 12v A9 enable
	SIM_CSOR0 = 0x7060; 

	// Switch to flash into ID-query mode.
	*((volatile uint16_t*)0xAAA) = 0xAAAA;
	*((volatile uint16_t*)0x554) = 0x5555;
	*((volatile uint16_t*)0xAAA) = 0x9090;

	// Read the identifier from address zero.
	//flashIdentifier = FLASH_IDENTIFIER;
	uint16_t manufacturer = FLASH_MANUFACTURER;
	uint16_t device = FLASH_DEVICE;
	flashIdentifier = ((uint32_t)manufacturer << 16) | device;

	// Switch back to standard mode.
	FLASH_BASE = READ_ARRAY_COMMAND;

	// flash chip 12v A9 disable
	SIM_CSOR0 = 0x1060; 
}

void HandleFlashChipQuery()
{
	ScratchWatchdog();

	GetIntelId();

	// If the ID query is unsuccessful, the "chipId" will actually be the
	// initial value of the stack pointer, as that's what's stored in the 
	// first four bytes of ROM.
	if (flashIdentifier == 0xFFCE00)
	{
		GetAmdId();
	}

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
	unsigned length = MessageBuffer[5];
	length <<= 8;
	length |= MessageBuffer[6];
	length <<= 8;
	length |= MessageBuffer[7];

	unsigned address = MessageBuffer[8];
	address <<= 8;
	address |= MessageBuffer[9];
	address <<= 8;
	address |= MessageBuffer[10];

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

	uint8_t *osid = (uint8_t*)0x504;

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
// Unlock the flash memory.
//
// We tried having separate commands for lock and unlock, however the PCM's
// VPW signal becomes noisy while the flash is unlocked. The AVT was able to
// deal with that, but the ScanTool and AllPro interfaces couldn't read the
// signal at all.
//
// So instead of VPW commands to unlock and re-lock, we just unlock during the
// erase and write operations, and re-lock when the operation is complete.
//
// These commands remain supported, just in case we find a way to use them.
///////////////////////////////////////////////////////////////////////////////
void UnlockFlash()
{
	SIM_CSBARBT = 0x0006;
	SIM_CSORBT  = 0x6820;
	SIM_CSBAR0  = 0x0006;
	SIM_CSOR0   = 0x7060;

	// TODO: can we just |= HARDWAREIO?
	unsigned short hardwareFlags = HARDWARE_IO;
	WasteTime();
	hardwareFlags |= 0x0001;
	WasteTime();
	HARDWARE_IO = hardwareFlags;

	VariableSleep(0x50);
}

///////////////////////////////////////////////////////////////////////////////
// Lock the flash memory.
//
// See notes above.
///////////////////////////////////////////////////////////////////////////////
void LockFlash()
{
	SIM_CSBARBT = 0x0006;
	SIM_CSORBT  = 0x6820;
	SIM_CSBAR0  = 0x0006;
	SIM_CSOR0   = 0x1060;

	unsigned short hardwareFlags = HARDWARE_IO;
	hardwareFlags &= 0xFFFE;
	WasteTime();
	WasteTime();
	HARDWARE_IO = hardwareFlags;

	VariableSleep(0x50);
}

///////////////////////////////////////////////////////////////////////////////
// Erase the given block.
///////////////////////////////////////////////////////////////////////////////
void HandleEraseBlock()
{
	unsigned address = MessageBuffer[5];
	address <<= 8;
	address |= MessageBuffer[6];
	address <<= 8;
	address |= MessageBuffer[7];

	// Only allow known addresses. Anything else probably indicates a bug.
	switch (address)
	{
	case 0: // Boot
	case 0x4000: // Parameters
	case 0x6000: // Parameters
	case 0x8000: // Calibration
		break;

	default:
		VariableSleep(2);
		SendReply(0, 0x05, 0xFF);
		return;
	}

	unsigned short status = 0;

	UnlockFlash();

	uint16_t *flashBase = (uint16_t*)address;
	*flashBase = 0x5050; // TODO: Move these commands to defines
	*flashBase = 0x2020;
	*flashBase = 0xD0D0;

	WasteTime();
	WasteTime();

	*flashBase = 0x7070;

	for (int iterations = 0; iterations < 0x640000; iterations++)
	{
		ScratchWatchdog();
		WasteTime();
		WasteTime();
		status = *flashBase;
		if ((status & 0x80) != 0)
		{
			break;
		}
	}

	status &= 0x00E8;

	*flashBase = READ_ARRAY_COMMAND;
	*flashBase = READ_ARRAY_COMMAND;

	LockFlash();

	// The AllPro and ScanTool devices need a short delay to switch from
	// sending to receiving. Otherwise they'll miss the response.
	// Also, give the lock-flash operation time to take full effect, because
	// the signal quality is degraded and the AllPro and ScanTool can't read
	// messages when the PCM is in that state.
	VariableSleep(2);

	SendReply(status == 0x80, 0x05, (char)status);
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
	char test = 0;
	unsigned value = SIM_CSBAR0;
	//(unsigned)&test;

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

	value = SIM_CSOR0;
	//(unsigned)crcTable;

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
	char errorCode = 0;
	unsigned short status;

	if (!testWrite)
	{
		UnlockFlash();
	}

	unsigned short* payloadArray = (unsigned short*) payloadBytes;
	unsigned short* flashArray = (unsigned short*) startAddress;

	for (unsigned index = 0; index < payloadLengthInBytes / 2; index++)
	{
		unsigned short *address = &(flashArray[index]);
		unsigned short value = payloadArray[index];

		if (!testWrite)
		{
			*address = 0x5050; // Clear status register TODO: use #define
			*address = 0x4040; // Program setup
			*address = value;  // Program
			*address = 0x7070; // Prepare to read status register
		}

		char success = 0;
		for(int iterations = 0; iterations < 0x1000; iterations++)
		{
			if  (testWrite)
			{
				status = 0x80;
			}
			else
			{
				status = *address;
			}

			ScratchWatchdog();

			if (status & 0x80)
			{
				success = 1;
				break;
			}

			WasteTime();
			WasteTime();
		}

		if (!success)
		{
			// Return flash to normal mode and return the error code.
			errorCode = status;

			if (!testWrite)
			{
				*address = 0xFFFF;
				*address = 0xFFFF;
				LockFlash();
			}

			return errorCode;
		}

	}

	if (!testWrite)
	{
				// Return flash to normal mode.
				unsigned short* address = (unsigned short*)startAddress;
				*address = 0xFFFF;
				*address = 0xFFFF;
		LockFlash();
	}

	// Check the last value we got from the status register.
	if ((status & 0x98) != 0x80)
	{
		errorCode = status;
	}

//	VariableSleep(3);
//	SendToolPresent(0x99, testWrite, 0x99, testWrite);

	return errorCode;
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
			HandleVersionQuery(0xBB);
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

	SendToolPresent(1, 2, 3, 4);
	LongSleepWithWatchdog();

	// This loop runs out quickly, to force the PCM to reboot, to speed up testing.
	// The real kernel should probably loop for a much longer time (possibly forever),
	// to allow the app to recover from any failures.
	// If we choose to loop forever we need a good story for how to get out of that state.
	// Pull the PCM fuse? Give the app button to tell the kernel to reboot?
	// for(int iterations = 0; iterations < 100; iterations++)
	uint32_t iterations = 0;
	uint32_t timeout = 2500; // Timeout of 2500 = 2.2 seconds between messages. 5,000 = 3.9 seconds.
	uint32_t lastMessage = (iterations - timeout) + 1;
	uint32_t lastActivity = iterations - timeout;

	for(;;)
	{
		iterations++;

		ScratchWatchdog();

		crcProcessSlice();

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
			LongSleepWithWatchdog();
			continue;
		}

		lastMessage = iterations;
		lastActivity = iterations;

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
