///////////////////////////////////////////////////////////////////////////////
// This is the smallest possible kernel. 
// It just runs a tight loop that keeps the watchdog happy.
///////////////////////////////////////////////////////////////////////////////

// After these are proven to work, they'll be moved into common.c and referenced from there.
// But for now I want to keep this kernel as tiny as possible to simplify debugging.

const int DLC_Configuration = 0xFFF600;
const int DLC_InterruptConfiguration = 0xFFF606;
const int DLC_Transmit_Command = 0xFFF60C;
const int DLC_Transmit_FIFO = 0xFFF60D;
const int DLC_Receive_Status = 0xFFF60E;
const int DLC_Receive_FIFO = 0xFFF60F;
const int Watchdog1 = 0xFFFA27;
const int Watchdog2 = 0xFFD006;
asm("asm_Watchdog2 = 0xFFD006");

void WriteByte(int address, char value)
{
	*((char volatile *)address) = value;
}

void ScratchWatchdog()
{
	WriteByte(Watchdog1, 0x55);
	WriteByte(Watchdog1, 0xAA);

	asm("bclr #7, (asm_Watchdog2).l");
	asm("bset #7, (asm_Watchdog2).l");
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

int 
__attribute__((section(".kernelstart")))
KernelStart(void)
{
	// Disable peripheral interrupts
	asm("ORI #0x700, %SR"); 

	ScratchWatchdog();

	// Flush the DLC
	WriteByte(DLC_Transmit_Command, 0x03);
	WriteByte(DLC_Transmit_FIFO, 0x00);

	// Bare minimum functionality: just don't reboot.
	for(;;)
	{
		LongSleepWithWatchdog();
	}
}


