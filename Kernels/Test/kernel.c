#define J1850_Config 0xFFF600
#define J1850_Command 0xFFF60C
#define J1850_TX_FIFO 0xFFF60D
#define J1850_Status 0xFFF60E
#define J1850_RX_FIFO 0xFFF60F
#define COP1 0xFFFA27
#define COP2 0xFFD006
#define toolid 0xF0


void ScratchWatchdog() __attribute__((optimize("-O0")));
void LongSleepWithWatchdog();
void WasteTime();
void WriteByte(int address, char value);
char ReadByte(int address);

void ReadPacket();
char requestMode;

void ProcessReadRequest();
void Reboot();

int KernelStart(void)
{
	WasteTime();
	WriteByte(J1850_Command, 3);
	WriteByte(J1850_TX_FIFO, 0);

	char status;
	do
	{
		ScratchWatchdog();
		WasteTime();
		status = ReadByte(J1850_Status) & 0xE0;
	} while (status != 0xE0);

	while (1)
	{
		ReadPacket();
		switch (requestMode)
		{
		case 0x35:
			ProcessReadRequest();
			break;

		case 0x20:
			Reboot();
			break;
		}
	}
}

void ReadPacket()
{

}

void ProcessReadRequest()
{

}

void Reboot()
{

}

void WriteByte(int address, char value)
{
	*((char*)address) = value;
}

char ReadByte(int address)
{
	return *((char*)address);
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
