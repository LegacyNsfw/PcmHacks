///////////////////////////////////////////////////////////////////////////////
// This is the smallest possible kernel. 
// It just runs a tight loop that keeps the watchdog happy.
///////////////////////////////////////////////////////////////////////////////

// After we have a trivial kernel that can send and received messages, the 
// reusable stuff should be moved into common.c and referenced from there.
//
// But for now I want to keep this kernel as tiny and as simple as possible,
// to simplify debugging.

char volatile * const DLC_Configuration = (char*)0xFFF600;
char volatile * const DLC_InterruptConfiguration = (char*)0xFFF606;
char volatile * const DLC_Transmit_Command = (char*)0xFFF60C;
char volatile * const DLC_Transmit_FIFO = (char*)0xFFF60D;
char volatile * const DLC_Status = (char*)0xFFF60E;
char volatile * const DLC_Receive_FIFO = (char*)0xFFF60F;
char volatile * const Watchdog1 = (char*)0xFFFA27;
char volatile * const Watchdog2 = (char*)0xFFD006;

// The linker needs to put these buffers after the kernel code, but before the
// system registers that are at the top of the RAM space.
//
// The code that extracts the kernel bin needs to exclude that address range, 
// because it will just add 8kb of 0x00 bytes to the kernel bin file. 
//
// 4096 == 0x1000
#define InputBufferSize 4096
char __attribute((section(".kerneldata"))) IncomingMessage[InputBufferSize];
char __attribute((section(".kerneldata"))) OutgoingMessage[InputBufferSize];

// This needs to be called periodically to prevent the PCM from rebooting.
void ScratchWatchdog()
{
	*Watchdog1 = 0x55;
	*Watchdog1 = 0xAA;
	*Watchdog2 = *Watchdog2 & 0x7F;
	*Watchdog2 = *Watchdog2 | 0x80;
}

// Does what it says.
int WasteTime()
{
	asm("nop");
	asm("nop");
	asm("nop");
	asm("nop");
}

// Also does what it says. 10,000 iterations takes a bit less than half a second.
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

// Send the given bytes over the VPW bus.
// The DLC will append the checksum byte, so we don't have to.
void WriteMessage(const char * const message, int length)
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

// Read a VPW message into the 'IncomingMessage' buffer.
// This doesn't work yet.
int ReadMessage()
{
	LongSleepWithWatchdog();
	char status = *DLC_Status & 0xE0;
	if (status != 0x40)
	{
		// No message received.
		// We can abuse the 'tool present' message to send arbitrary data to see what the code is doing...
		char debug1[] = { 0x8C, 0xFE, 0xF0, 0x3F, 0x01, status };
		WriteMessage(debug1, 6);
		return 0;
	}

	int length;
	for (length = 0; length < InputBufferSize - 1; length++)
	{
		IncomingMessage[length] = *DLC_Receive_FIFO;
		ScratchWatchdog();

		status = *DLC_Status & 0xE0;
		if (status != 0x40)
		{
			break;
		}
	}

	IncomingMessage[length] = *DLC_Receive_FIFO;
	return length;
}

// This is the entry point for the kernel.
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

	char toolPresent[] = { 0x8C, 0xFE, 0xF0, 0x3F };

	for(;;)
	{
		LongSleepWithWatchdog();

		int length = ReadMessage();
		if (length == 0)
		{
			// No message received, so send a heartbeat message and listen again.
			// Note that without this call to LongSleepWithWatchdog, the WriteMessage call will fail.
			// Should try experimenting with different delay lengths to see just how long we need to wait.
			LongSleepWithWatchdog();
			WriteMessage(toolPresent, 4);
			continue;
		}

		// Echo the received message with a 'tool present' header.
		for (int index1 = 0; index1 < 4; index1++)
		{
			OutgoingMessage[index1] = toolPresent[index1];
		}

		for (int index2 = 0; index2 < length; index2++)
		{
			OutgoingMessage[4 + index2] = IncomingMessage[index2];
		}

		WriteMessage(OutgoingMessage, 4 + length);
	}
}


