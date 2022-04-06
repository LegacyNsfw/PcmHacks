///////////////////////////////////////////////////////////////////////////////
// This kernel can read PCM memory and supports uploading a replacement kernel.
///////////////////////////////////////////////////////////////////////////////

#include "common.h"


///////////////////////////////////////////////////////////////////////////////
// Echo the received message
///////////////////////////////////////////////////////////////////////////////
void HandleEchoRequest(int length)
{
	ElmSleep();

	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x7E;
	MessageBuffer[4] = (unsigned char)length;

	WriteMessage(MessageBuffer, length, Complete);
}

#define SIM_BASE        0x00FFFA00
#define SIM_CSBARBT     (*(unsigned short *)(SIM_BASE + 0x48)) // CSRBASEREG, boot chip select, chip select base addr boot ROM reg,

#define SIM_CSORBT      (*(unsigned short *)(SIM_BASE + 0x4a)) // CSROPREG, Chip select option boot ROM reg., $6820 for normal op
#define SIM_CSBAR0      (*(unsigned short *)(SIM_BASE + 0x4c)) // CSBASEREG, chip selects
#define SIM_CSOR0       (*(unsigned short *)(SIM_BASE + 0x4e)) // CSOPREG, *Chip select option reg., $1060 for normal op, $7060 for accessing flash chip

#define FLASH_BASE         (*(unsigned short *)(0x00000000))
#define FLASH_MANUFACTURER (*(unsigned short *)(0x00000000))
#define FLASH_DEVICE       (*(unsigned short *)(0x00000002))

#define SIGNATURE_COMMAND  0x9090
#define READ_ARRAY_COMMAND 0xFFFF

void Handle_512kb_FlashChipQuery()
{
	unsigned char manufacturer;
	short device;

	ScratchWatchdog();

	SIM_CSBAR0 = 0x0006;
	SIM_CSORBT = 0x6820;
	SIM_CSOR0 = 0x7060; // flash chip 12v A9 enable

	FLASH_BASE = SIGNATURE_COMMAND; // flash chip command
	manufacturer = FLASH_MANUFACTURER; // read manufacturer
	device = FLASH_DEVICE;
	FLASH_BASE = READ_ARRAY_COMMAND;

	SIM_CSOR0 = 0x1060; // flash chip 12v A9 disable

	// The AllPro and ScanTool devices need a short delay to switch from
	// sending to receiving. Otherwise they'll miss the response.
	VariableSleep(1);

	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x7D;
	MessageBuffer[4] = 0x01;
	MessageBuffer[5] = manufacturer >> 8;
	MessageBuffer[6] = manufacturer;
	MessageBuffer[7] = device >> 8;
	MessageBuffer[8] = device;
	WriteMessage(MessageBuffer, 9, Complete);
}

void Handle_1mb_FlashChipQuery()
{
	ScratchWatchdog();
	uint32_t id = 0;

	SIM_CSBAR0 = 0x0006;
	SIM_CSORBT = 0x6820;
	SIM_CSOR0 = 0x7060; // flash chip 12v A9 enable

	*((volatile uint16_t*)0xAAA) = 0xAAAA;
	*((volatile uint16_t*)0x554) = 0x5555;
	*((volatile uint16_t*)0xAAA) = 0x9090;

	uint16_t manufacturer = FLASH_MANUFACTURER; // read manufacturer
	uint16_t device = FLASH_DEVICE;

	FLASH_BASE = READ_ARRAY_COMMAND;
	SIM_CSOR0 = 0x1060; // flash chip 12v A9 disable

	// The AllPro and ScanTool devices need a short delay to switch from
	// sending to receiving. Otherwise they'll miss the response.
	VariableSleep(1);

	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x7D;
	MessageBuffer[4] = 0x01;
	MessageBuffer[5] = manufacturer >> 8;
	MessageBuffer[6] = manufacturer;
	MessageBuffer[7] = device >> 8;
	MessageBuffer[8] = device; 
	WriteMessage(MessageBuffer, 9, Complete);
}


///////////////////////////////////////////////////////////////////////////////
// Process an incoming message.
///////////////////////////////////////////////////////////////////////////////
void ProcessMessage(int length)
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
	case 0x3E:
		HandleEchoRequest(length);
		break;

	case 0x3D:
		switch (MessageBuffer[4])
		{
		case 0x00:
			HandleVersionQuery();
			break;

		case 0x01:
			Handle_1mb_FlashChipQuery();
			break;

		default:
			SendToolPresent(
				0xBB,
				MessageBuffer[3],
				MessageBuffer[4],
				MessageBuffer[5]);
			break;
		}
		break;

		// Fall through:

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

		ProcessMessage(length);
	}

	// This shouldn't happen. But, just in case...
	Reboot(0xFF000000 | iterations);
}
