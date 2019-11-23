///////////////////////////////////////////////////////////////////////////////
// This kernel can read PCM memory and supports uploading a replacement kernel.
///////////////////////////////////////////////////////////////////////////////

#include "common.h"
#include "flash.h"

#define COMMAND_REG_AAA (*((volatile uint16_t*)0xAAA))
#define COMMAND_REG_554 (*((volatile uint16_t*)0x554))

///////////////////////////////////////////////////////////////////////////////
// Get the manufacturer and type of flash chip.
///////////////////////////////////////////////////////////////////////////////
uint32_t Amd_GetFlashId()
{
	SIM_CSBAR0 = 0x0006;
	SIM_CSORBT = 0x6820;

	// Switch to flash into ID-query mode.
	SIM_CSOR0 = 0x7060;
	COMMAND_REG_AAA = 0xAAAA;
	COMMAND_REG_554 = 0x5555;
	COMMAND_REG_AAA = 0x9090;

	// Read the identifier from address zero.
	//flashIdentifier = FLASH_IDENTIFIER;
	uint16_t manufacturer = FLASH_MANUFACTURER;
	uint16_t device = FLASH_DEVICE;
	uint32_t id = ((uint32_t)manufacturer << 16) | device;

	// Switch back to standard mode.
	FLASH_BASE = READ_ARRAY_COMMAND;
	SIM_CSOR0 = 0x1060;

	return id;
}

///////////////////////////////////////////////////////////////////////////////
// Erase the given block.
///////////////////////////////////////////////////////////////////////////////
uint8_t Amd_EraseBlock(uint32_t address)
{
	// Return zero if successful, anything else is an error code.
	unsigned short status = 0;

	uint16_t volatile * flashBase = (uint16_t*)address;

	// Tell the chip to erase the given block.
	SIM_CSOR0 = 0x7060;
	COMMAND_REG_AAA = 0xAAAA;
	COMMAND_REG_554 = 0x5555;
	COMMAND_REG_AAA = 0x8080;
	COMMAND_REG_AAA = 0xAAAA;
	COMMAND_REG_554 = 0x5555;

	*flashBase = 0x3030;

	uint16_t read1 = 0;
	uint16_t read2 = 0;

	for (int iterations = 0; iterations < 0x640000; iterations++)
	{
		read1 = *flashBase & 0x40;

		ScratchWatchdog();

		read2 = *flashBase & 0x40;

		if (read1 == read2)
		{
			// Success!
			break;
		}

		uint16_t read3 = *flashBase & 0x20;
		if (read3 == 0)
		{
			continue;
		}

		status = 0xA0;
		break;
	}

	if (status == 0xA0)
	{
		read1 = *flashBase & 0x40;
		read2 = *flashBase & 0x40;
		if (read1 != read2)
		{
			status = 0xB0;
		}
		else
		{
			// Success!
			status = 0;
		}
	}

	// Return to array mode.
	*flashBase = 0xF0F0;
	*flashBase = 0xF0F0;
	SIM_CSOR0 = 0x1060;

	return status;
}

///////////////////////////////////////////////////////////////////////////////
// Write data to flash memory.
// This is invoked by HandleWriteMode36 in common-readwrite.c
///////////////////////////////////////////////////////////////////////////////
uint8_t Amd_WriteToFlash(unsigned int payloadLengthInBytes, unsigned int startAddress, unsigned char *payloadBytes, int testWrite)
{
	char errorCode = 0;
	unsigned short status;

	unsigned short* payloadArray = (unsigned short*) payloadBytes;
	unsigned short* flashArray = (unsigned short*) startAddress;

	for (unsigned index = 0; index < payloadLengthInBytes / 2; index++)
	{
		unsigned short volatile  *address = &(flashArray[index]);
		unsigned short value = payloadArray[index];

		if (!testWrite)
		{
			SIM_CSOR0 = 0x7060;
			COMMAND_REG_AAA = 0xAAAA;
			COMMAND_REG_554 = 0x5555;
			COMMAND_REG_AAA = 0xA0A0;
			*address = value;
		}

		char success = 0;
		for(int iterations = 0; iterations < 0x1000; iterations++)
		{
			ScratchWatchdog();

			uint16_t read = testWrite ? value : *address;

			if (read == value)
			{
				success = 1;
				break;
			}

			WasteTime();
		}

		if (!success)
		{
			// Return flash to normal mode and return the error code.
			errorCode = 0xAA;

			if (!testWrite)
			{
				*address = 0xF0F0;
				*address = 0xF0F0;
				FlashLock();
			}

			return errorCode;
		}
	}

	if (!testWrite)
	{
		// Return flash to normal mode.
		unsigned short* address = (unsigned short*)startAddress;
		*address = 0xF0F0;
		*address = 0xF0F0;
		SIM_CSOR0 = 0x1060;
	}

	return 0;
}
