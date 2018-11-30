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
char volatile * const DLC_Receive_Status = (char*)0xFFF60E;
char volatile * const DLC_Receive_FIFO = (char*)0xFFF60F;
char volatile * const Watchdog1 = (char*)0xFFFA27;
char volatile * const Watchdog2 = (char*)0xFFD006;
asm("asm_Watchdog2 = 0xFFD006");

void ScratchWatchdog()
{
	*Watchdog1 = 0xFF;
	*Watchdog1 = 0xFF;
	*Watchdog2 = *Watchdog2 & 0x7F;
	*Watchdog2 = *Watchdog2 | 0x80;
//	WriteByte(Watchdog1, 0x55);
//	WriteByte(Watchdog1, 0xAA);

//	asm("bclr #7, (asm_Watchdog2).l");
//	asm("bset #7, (asm_Watchdog2).l");
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
	//WriteByte(DLC_Transmit_Command, 0x03);
	//WriteByte(DLC_Transmit_FIFO, 0x00);
	*DLC_Transmit_Command = 0x03;
	*DLC_Transmit_FIFO = 0x00;

	// Bare minimum functionality: just don't reboot.
	for(;;)
	{
		LongSleepWithWatchdog();
	}
}


