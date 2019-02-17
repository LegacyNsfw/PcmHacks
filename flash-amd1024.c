///////////////////////////////////////////////////////////////////////////////
// This kernel can read PCM memory and supports uploading a replacement kernel.
///////////////////////////////////////////////////////////////////////////////

#include "common.h"
#include "flash.h"

///////////////////////////////////////////////////////////////////////////////
// Get the manufacturer and type of flash chip.
///////////////////////////////////////////////////////////////////////////////
uint32_t Amd1024_GetFlashId()
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
	uint32_t id = ((uint32_t)manufacturer << 16) | device;

	// Switch back to standard mode.
	FLASH_BASE = READ_ARRAY_COMMAND;

	// flash chip 12v A9 disable
	SIM_CSOR0 = 0x1060;

	return id;
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
void Amd1024_Unlock()
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
void Amd1024_Lock()
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
uint8_t Amd1024_EraseBlock(uint32_t address)
{
	unsigned short status = 0;

	Amd1024_Unlock();

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

	Amd1024_Lock();

	return status;
}

///////////////////////////////////////////////////////////////////////////////
// Write data to flash memory.
// This is invoked by HandleWriteMode36 in common-readwrite.c
// read-kernel.c has a stub to keep the compiler happy until this is released.
///////////////////////////////////////////////////////////////////////////////
uint8_t Amd1024_WriteToFlash(unsigned int payloadLengthInBytes, unsigned int startAddress, unsigned char *payloadBytes, int testWrite)
{
	char errorCode = 0;
	unsigned short status;

	if (!testWrite)
	{
		Amd1024_Unlock();
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
				Amd1024_Lock();
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
		Amd1024_Lock();
	}

	// Check the last value we got from the status register.
	if ((status & 0x98) != 0x80)
	{
		errorCode = status;
	}

	return errorCode;
}
