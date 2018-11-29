///////////////////////////////////////////////////////////////////////////////
// This is the smallest possible kernel. 
// It just runs a tight loop that keeps the watchdog happy.
///////////////////////////////////////////////////////////////////////////////

#include "common.h"

// A few constants are duplicated here for simplicity.
asm("asm_DLC_Configuration = 0xFFF600");
asm("asm_DLC_InterruptConfiguration = 0xFFF606");
asm("asm_DLC_Transmit_Command = 0xFFF60C");
asm("asm_DLC_Transmit_FIFO = 0xFFF60D");
asm("asm_DLC_Receive_Status = 0xFFF60E");
asm("asm_DLC_Receive_FIFO = 0xFFF60F");

int 
__attribute__((section(".kernelstart")))
KernelStart(void)
{
	// Disable peripheral interrupts
	asm("ORI #0x700, %SR"); 
	ScratchWatchdog();

	// Disable DLC interrupts
	//WriteByte(DLC_InterruptConfiguration, 0);
	asm("mov.b 0x00, (asm_DLC_InterruptConfiguration).l");

	//WasteTime();
	LongSleepWithWatchdog(10);
	asm("move.b 0x03, (asm_DLC_Transmit_Command).l");
	asm("move.b 0x00, (asm_DLC_Transmit_FIFO).l");

	// Bare minimum functionality.
	for(;;)
	{
		LongSleepWithWatchdog(10);
	}
}

