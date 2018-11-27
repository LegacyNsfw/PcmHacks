#define J1850_Config 0xFFF600
#define J1850_Command 0xFFF60C
#define J1850_TX_FIFO 0xFFF60D
#define J1850_Status 0xFFF60E
#define J1850_RX_FIFO 0xFFF60F
#define COP1 0xFFFA27
#define COP2 0xFFD006
#define toolid 0xF0
#define InputBufferSize 20

void ScratchWatchdog() __attribute__((optimize("-O0")));
void LongSleepWithWatchdog();
void WasteTime();
void WriteByte(int address, char value);
char ReadByte(int address);

void ReadPacket();
char inputBuffer[];

void ProcessReadRequest();
void ProcessResetRequest();
void Reboot();

void SendMessage(char* message, int length);

char ResetResponseMessage[] = { 0x00 };

int 
__attribute__((section(".kernelstart")))
KernelStart(void)
{
	WasteTime();
	WriteByte(J1850_Command, 3);
	WriteByte(J1850_TX_FIFO, 0);

	char status;
	do
	{
		ScratchWatchdog();
		WasteTime();
		status = ReadByte(J1850_Status);
	} while ((status & 0xE0) != 0xE0);

	while (1)
	{
		ReadPacket();
		switch (inputBuffer[3])
		{
		case 0x35:
			ProcessReadRequest();
			break;

		case 0x20:
			ProcessResetRequest();
			break;
		}
	}
}

int IsStatusComplete()
{
	char status = ReadByte(J1850_Status);

	// Unwrapped to make the assembly easier to follow...
	// return (status & 0xE0) == 0x40;
	status = status & 0xE0;
	if (status == 0x40)
	{
		return 1;
	}

	return 0;
	
}

void ReadPacket()
{
	int bytesReceived = 0;
	for (int i = 0; i < 0x300000; i++)
	{		
		ScratchWatchdog();
		if (IsStatusComplete())
		{
			inputBuffer[bytesReceived] = ReadByte(J1850_RX_FIFO);
			bytesReceived++;
			continue;
		}

		// Read the last byte.
		inputBuffer[bytesReceived] = ReadByte(J1850_RX_FIFO);

		// If unknown priority, try again.
		if (inputBuffer[0] != 0x6C)
		{
			bytesReceived = 0;
			continue;
		}

		// If unknown destination, try again;
		if (inputBuffer[1] != 0x10)
		{
			bytesReceived = 0;
			continue;
		}

		// Let the caller process this message.
		return;
	}

	// Too many retries. Pretend we got a reboot request
	inputBuffer[0] = 0x6C;
	inputBuffer[1] = 0x10;
	inputBuffer[2] = 0xF0;
	inputBuffer[3] = 0x20;
	return;
}

void ProcessReadRequest()
{
	inputBuffer[0] = 0;
	inputBuffer[1] = 0;
	inputBuffer[2] = 0;
	inputBuffer[3] = 0;
}

void ProcessResetRequest()
{
	SendMessage(ResetResponseMessage, 3);

	WriteByte(J1850_Command, 0x40);
	WriteByte(J1850_TX_FIFO, 0x00);
	LongSleepWithWatchdog();
	Reboot();
}

void Reboot()
{
	// Reset peripherals
	asm("reset");

	// Wait for watchdog to trigger a reboot
	for (;;) {}
}

void SendMessage(char* message, int length)
{

}

void WriteByte(int address, char value)
{
	*((char volatile *)address) = value;
}


char ReadByte(int address)
{
	return *((char volatile *)address);
}

void ScratchWatchdog() 
{
	*((char*)COP1) = 0x55;
	*((char*)COP1) = 0xAA;

	// No sure how to use the COP2 preprocessor symbol here, but that would be cleaner
	asm("bclr #7, (0xFFD006).l");
	asm("bset #7, (0xFFD006).l");
}

void LongSleepWithWatchdog()
{
	for (int i = 0; i < 10 * 1000; i++)
	{
		ScratchWatchdog();
		for (int j = 0; j < 10; j++)
		{
			WasteTime();
		}
	}
}

void WasteTime()
{
	asm("nop");
	asm("nop");
	asm("nop");
	asm("nop");
}

char inputBuffer[InputBufferSize];
