///////////////////////////////////////////////////////////////////////////////
// This is the smallest possible kernel. 
// It just runs a tight loop that keeps the watchdog happy.
///////////////////////////////////////////////////////////////////////////////

// After these are proven to work, they'll be moved into common.c and referenced from there.
// But for now I want to keep this kernel as tiny as possible to simplify debugging.

char volatile * const DLC_Configuration = (char*)0xFFF600;
char volatile * const DLC_InterruptConfiguration = (char*)0xFFF606;
char volatile * const DLC_Transmit_Command = (char*)0xFFF60C;
char volatile * const DLC_Transmit_FIFO = (char*)0xFFF60D;
char volatile * const DLC_Status = (char*)0xFFF60E;
char volatile * const DLC_Receive_FIFO = (char*)0xFFF60F;
char volatile * const Watchdog1 = (char*)0xFFFA27;
char volatile * const Watchdog2 = (char*)0xFFD006;
asm("asm_Watchdog2 = 0xFFD006");

void ScratchWatchdog()
{
	*Watchdog1 = 0x55;
	*Watchdog1 = 0xAA;
	*Watchdog2 = *Watchdog2 & 0x7F;
	*Watchdog2 = *Watchdog2 | 0x80;
}

int WasteTime()
{
	asm("nop");
	asm("nop");
	asm("nop");
	asm("nop");
}

int LongSleepWithWatchdog()
{
	for (int outerLoop = 0; outerLoop < 10 * 1000; outerLoop++)
	{
		ScratchWatchdog();
		for (int innerLoop = 0; innerLoop < 10; innerLoop++)
		{
			WasteTime();
		}
	}
}

void SendMessage(const char * const message, int length)
{
	ScratchWatchdog();
	*DLC_Transmit_Command = 0x14;

	// Send message
	for (int index = 0; index < length - 1; index++)
	{
		*DLC_Transmit_FIFO = message[index];
		WasteTime();
	}

	// Send last byte
	*DLC_Transmit_Command = 0x0C;
	*DLC_Transmit_FIFO = message[length - 1];
	WasteTime();

	*DLC_Transmit_Command = 0x03;
	*DLC_Transmit_FIFO = 0x00;

	for(int iterations = 0; iterations < length + 10; iterations++)
	{
		ScratchWatchdog();
		WasteTime();
		char status = *DLC_Status & 0xE0;
		if (status == 0xE0)
		{
			break;
		}
	}


}

int 
__attribute__((section(".kernelstart")))
KernelStart(void)
{
	// Disable peripheral interrupts
	asm("ORI #0x700, %SR"); 

	ScratchWatchdog();

	// Flush the DLC
	*DLC_Transmit_Command = 0x03;
	*DLC_Transmit_FIFO = 0x00;

	char toolPresent[] = { 0x8C, 0xFE, 0xF0, 0x3F, 0x2C };

	for(;;)
	{
		LongSleepWithWatchdog();
		SendMessage(toolPresent, 5);
	}
}


