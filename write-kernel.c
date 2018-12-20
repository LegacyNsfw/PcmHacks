///////////////////////////////////////////////////////////////////////////////
// This kernel can read PCM memory and supports uploading a replacement kernel.
///////////////////////////////////////////////////////////////////////////////

#include "common.h"

#define  SIM_BASE  ((void*)0xffffFA00)
char volatile * const  SIM_MCR      =   SIM_BASE + 0x00; // Module Control register
char volatile * const  SIM_SYNCR    =   SIM_BASE + 0x04; // Clock synthesiser control register
char volatile * const  SIM_RSR      =   SIM_BASE + 0x07; // Reset Status
char volatile * const  SIM_SYPCR    =   SIM_BASE + 0x21; // System Protection
char volatile * const  SIM_PICR     =   SIM_BASE + 0x22; // Periodic Timer
char volatile * const  SIM_PITR     =   SIM_BASE + 0x24; //
char volatile * const  SIM_SWSR     =   SIM_BASE + 0x27; //
char volatile * const  SIM_CSPAR0   =   SIM_BASE + 0x44; // chip sellect pin assignment
char volatile * const  SIM_CSPAR1   =   SIM_BASE + 0x46; //
unsigned short volatile * const  SIM_CSBARBT  =   SIM_BASE + 0x48; // CSRBASEREG, boot chip select, chip select base addr boot ROM reg, 
                                                                   // must be updated to $0006 on each update of flash CE/WE states
unsigned short volatile * const  SIM_CSORBT   =   SIM_BASE + 0x4a; // CSROPREG, Chip select option boot ROM reg., $6820 for normal op 
unsigned short volatile * const  SIM_CSBAR0   =   SIM_BASE + 0x4c; // CSBASEREG, chip selects
unsigned short volatile * const  SIM_CSOR0    =   SIM_BASE + 0x4e; // CSOPREG, *Chip select option reg., $1060 for normal op, $7060 for accessing flash chip
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
char volatile * const  SIM_CSOR6    =   SIM_BASE + 0x66;

unsigned short volatile * const HardwareIO     =  (void*)0xFFFFE2FA; // Hardware I/O reg

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
void SendReply(char success, unsigned char submode, unsigned char code)
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
void HandleFlashChipQuery()
{
	ScratchWatchdog();
	*SIM_CSBAR0 = 0x0006;
	*SIM_CSORBT = 0x6820;
	*SIM_CSOR0 = 0x7060;

	unsigned short signature_command = 0x0090;
	unsigned short read_array_command = 0x00FF;

	unsigned short volatile * const zeroPointer = (void*)0;
	unsigned volatile * const wideZeroPointer = (void*)0;

	// This tells the flash chip we want to read the manufacturer and type IDs.
	*zeroPointer = signature_command;
	unsigned manufacturerAndType = *wideZeroPointer;

	// This tells the flash chip to operate normally again.
	*zeroPointer = read_array_command;

	// Not sure if this is necessary.
	*SIM_CSOR0 = 0x1060;

	// The AllPro and ScanTool devices need a short delay to switch from 
	// sending to receiving. Otherwise they'll miss the response.
	VariableSleep(1);

	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x7D;
	MessageBuffer[4] = 0x01;
	MessageBuffer[5] = (char)(manufacturerAndType >> 24);
	MessageBuffer[6] = (char)(manufacturerAndType >> 16);
	MessageBuffer[7] = (char)(manufacturerAndType >> 8);
	MessageBuffer[8] = (char)(manufacturerAndType >> 0);
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
	
	unsigned start = MessageBuffer[8];
	start <<= 8;
	start |= MessageBuffer[9];
	start <<= 8;
	start |= MessageBuffer[10];

#ifdef RECEIVE_BREADCRUMBS
	SendBreadcrumbs(0x3D);
#else
	// I discovered by accident that the app is much better at getting
	// the CRC responses if the kernel pauses here. That gives the app
	// time to send a tool-present response, so the slow response to
	// the CRC request gets processed as a response to the tool-present
	// message. 
	//
	// This is fragile. There has to be a better way. But for now, this
	// seems to work well enough.
	LongSleepWithWatchdog();
	LongSleepWithWatchdog();
#endif

	ScratchWatchdog();
	crcInit();

	ScratchWatchdog();
	unsigned crc = crcFast((unsigned char*) start, length);

	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x7D;
	MessageBuffer[4] = 0x02;
	MessageBuffer[5] = (char)(length >> 16);
	MessageBuffer[6] = (char)(length >> 8);
	MessageBuffer[7] = (char)length;
	MessageBuffer[8] = (char)(start >> 16);
	MessageBuffer[9] = (char)(start >> 8);
	MessageBuffer[10] = (char)start;
	MessageBuffer[11] = (char)(crc >> 24);
	MessageBuffer[12] = (char)(crc >> 16);
	MessageBuffer[13] = (char)(crc >> 8);
	MessageBuffer[14] = (char)crc;
	WriteMessage(MessageBuffer, 15, Complete);
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
	*SIM_CSBARBT = 0x0006;
	*SIM_CSORBT = 0x6820;
	*SIM_CSBAR0 = 0x0006;		
	*SIM_CSOR0 = 0x7060;

	unsigned short hardwareFlags = *HardwareIO;
	hardwareFlags |= 0x0001;
	WasteTime();
	WasteTime();
	*HardwareIO = hardwareFlags;

	VariableSleep(0x50);
}

void HandleFlashUnlockRequest()
{
	UnlockFlash();
	
	// No delay necessary, the UnlockFlash function already takes a long time.
	SendReply(1, 0x03, 0x00);
}

///////////////////////////////////////////////////////////////////////////////
// Lock the flash memory.
//
// See notes above.
///////////////////////////////////////////////////////////////////////////////
void LockFlash()
{
	*SIM_CSBARBT = 0x0006;
	*SIM_CSORBT = 0x6820;
	*SIM_CSBAR0 = 0x0006;	
	*SIM_CSOR0 = 0x1060;

	unsigned short hardwareFlags = *HardwareIO;
	hardwareFlags &= 0xFFFE;
	WasteTime();
	WasteTime();
	*HardwareIO = hardwareFlags;

	VariableSleep(0x50);
}

void HandleFlashLockRequest()
{
	LockFlash();
	
	// No delay necessary, the UnlockFlash function already takes a long time.
	SendReply(1, 0x04, 0x00);
}

///////////////////////////////////////////////////////////////////////////////
// Erase the calibration blocks.
///////////////////////////////////////////////////////////////////////////////
void HandleEraseCalibrationRequest()
{
	UnlockFlash();

	unsigned short *flashBase = (void*)0x8000;
	*flashBase = 0x5050;
	*flashBase = 0x2020;
	*flashBase = 0xD0D0;

	WasteTime();
	WasteTime();

	*flashBase = 0x7070;

	unsigned short status = 0;
	unsigned success = 0;

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
	if (status == 0x0080)
	{
		success = 1;
	}

	*flashBase == 0xFFFF;
	*flashBase == 0xFFFF;

	LockFlash();

	// The AllPro and ScanTool devices need a short delay to switch from 
	// sending to receiving. Otherwise they'll miss the response.
	// Also, give the lock-flash operation time to take full effect, because
	// the signal quality is degraded and the AllPro and ScanTool can't read
	// messages when the PCM is in that state.
	VariableSleep(2);

	SendReply(success, 0x05, (char)status);
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
	unsigned value = *SIM_CSBAR0;
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

	value = *SIM_CSOR0;
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
unsigned char WriteToFlash(const unsigned length, const unsigned startAddress, unsigned char *data, int testWrite)
{
	char errorCode = 0;
	unsigned short status;

	if (!testWrite)
	{
		UnlockFlash();
	}

	for (unsigned index = 0; index < length; index+=2)
	{
		unsigned short *address = (unsigned short*) (startAddress + index);		
		unsigned short value = *((unsigned short*) data + index);

		if (!testWrite)
		{
			*address = 0x5050; // Clear status register
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

		if (index == length-2)
		{
			if (!testWrite)
			{
				// Return flash to normal mode.
				*address = 0xFFFF;
				*address = 0xFFFF;
			}	
		}
	}

	if (!testWrite)
	{
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
			HandleVersionQuery();
			break;

		case 0x01:
			HandleFlashChipQuery();
			break;

		case 0x02:
			HandleCrcQuery();
			break;

		case 0x03:
			HandleFlashUnlockRequest();
			break;

		case 0x04:
			HandleFlashLockRequest();
			break;

		case 0x05:
			HandleEraseCalibrationRequest();
			break;
			
		case 0x06:
			HandleEraseEverythingRequest();
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

	*DLC_InterruptConfiguration = 0x00;
	LongSleepWithWatchdog();

	// Flush the DLC
	*DLC_Transmit_Command = 0x03;
	*DLC_Transmit_FIFO = 0x00;

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
