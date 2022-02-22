///////////////////////////////////////////////////////////////////////////////
// This kernel can read PCM memory and supports uploading a replacement kernel.
///////////////////////////////////////////////////////////////////////////////

#include "common.h"
#include "flash.h"

///////////////////////////////////////////////////////////////////////////////
// Unlock / Lock Intel flash memory.
///////////////////////////////////////////////////////////////////////////////
void FlashUnlock(bool unlock)
{
	SIM_CSBARBT = 0x0007;
	SIM_CSORBT = 0x6820;
	SIM_CSBAR0 = 0x0007;

	if(unlock)
	{
		// Unlock
		SIM_CSOR0 = 0x7060;
		HARDWARE_IO |= 0x0001;
	}
	else
	{
		// Lock
		SIM_CSOR0 = 0x1060;
		HARDWARE_IO &= 0xFFFE;
	}
	// P01 Critical
	VariableSleep(0x50);
}

///////////////////////////////////////////////////////////////////////////////
// Get the manufacturer and type of flash chip.
///////////////////////////////////////////////////////////////////////////////
uint32_t Intel_GetFlashId()
{
	SIM_CSBAR0 = 0x0007;
	SIM_CSORBT = 0x6820;

	// flash chip 12v A9 enable
	SIM_CSOR0 = 0x7060;

	// Switch the flash into ID-query mode
	FLASH_BASE = SIGNATURE_COMMAND;

	// Read the identifier from address zero.
	uint32_t id = FLASH_IDENTIFIER;

	// Switch back to standard mode
	FLASH_BASE = READ_ARRAY_COMMAND;

	SIM_CSOR0 = 0x1060; // flash chip 12v A9 disable

	return id;
}

///////////////////////////////////////////////////////////////////////////////
// Erase the given block.
///////////////////////////////////////////////////////////////////////////////
uint8_t Intel_EraseBlock(uint32_t address)
{
	unsigned short status = 0;

	FlashUnlock(true);

	uint16_t *flashBase = (uint16_t*)address;
	*flashBase = 0x5050; // TODO: Move these commands to defines
	*flashBase = 0x2020;
	*flashBase = 0xD0D0;
	*flashBase = 0x7070;

	for (int iterations = 0; iterations < 0x640000; iterations++)
	{
		ScratchWatchdog();
		status = *flashBase;
		if ((status & 0x80) != 0)
		{
			break;
		}
	}

	status &= 0x00E8;

	*flashBase = READ_ARRAY_COMMAND;
	*flashBase = READ_ARRAY_COMMAND;

	FlashUnlock(false);

	// Return zero if successful, anything else is an error code.
	if (status == 0x80)
	{
		status = 0;
	}

	return status;
}

///////////////////////////////////////////////////////////////////////////////
// Write data to flash memory.
// This is invoked by HandleWriteMode36 in common-readwrite.c
// read-kernel.c has a stub to keep the compiler happy until this is released.
///////////////////////////////////////////////////////////////////////////////
uint8_t Intel_WriteToFlash(unsigned int payloadLengthInBytes, unsigned int startAddress, unsigned char *payloadBytes, int testWrite)
{
	char errorCode = 0;
	unsigned short status;

	if (!testWrite)
	{
		FlashUnlock(true);
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
		}

		if (!success)
		{
			// Return flash to normal mode and return the error code.
			errorCode = status;

			if (!testWrite)
			{
				*address = 0xFFFF;
				*address = 0xFFFF;
				FlashUnlock(false);
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
		FlashUnlock(false);
	}

	// Check the last value we got from the status register.
	if ((status & 0x98) != 0x80)
	{
		errorCode = status;
	}

	return errorCode;
}

