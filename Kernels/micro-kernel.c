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

typedef enum 
{
	MiddleOfMessageDoesNotWork = 0, 
	StartOfMessage = 1,
	EndOfMessage = 2,
	EntireMessage = StartOfMessage | EndOfMessage,
} MessageParts;

// Send the given bytes over the VPW bus.
// The DLC will append the checksum byte, so we don't have to.
//
// This has known bugs:
//
// If this is called with MiddleOfMessage, the subsequent call with 
// EndOfMessage will drop the final byte of the final payload.
//
// StartOfMessage followed by EndOfMessage sometimes sends just the 
// EndOfMessage data. 
void WriteMessage(const char * const message, int length, MessageParts  parts)
{
	ScratchWatchdog();

	if ((parts & StartOfMessage) != 0)
	{
		*DLC_Transmit_Command = 0x14;
	}

	int lastIndex = ((parts & EndOfMessage) != 0) ?
		length - 1 :
		length;

	// Send message (will we need a watchdog call inside this loop for long messages?)
	for (int index = 0; index < lastIndex; index++)
	{
		*DLC_Transmit_FIFO = message[index];
		WasteTime();
	}

	if ((parts & EndOfMessage) != 0)
	{
		// Send last byte
		*DLC_Transmit_Command = 0x0C;
		*DLC_Transmit_FIFO = message[length - 1];

		// Send checksum (?)
		WasteTime();
		*DLC_Transmit_Command = 0x03;
		*DLC_Transmit_FIFO = 0x00;

		// Wait for the message to be flushed.
		for (int iterations = 0; iterations < length + 10; iterations++)
		{
			ScratchWatchdog();
			WasteTime();
			char status = *DLC_Status & 0xE0;
			if (status == 0xE0)
			{
				break;
			}
		}

		// Consider adding LongSleepWithWatchdog here.
	}
}

// Read a VPW message into the 'IncomingMessage' buffer.
// This doesn't work yet.
int ReadMessage()
{
	ScratchWatchdog();
	char status;
	do
	{
		ScratchWatchdog();
		WasteTime();
		status = *DLC_Status & 0xE0;
	}
	while (status != 0xE0);

		// No message received.
		// We can abuse the 'tool present' message to send arbitrary data to see what the code is doing...
		char debug1[] = { 0x8C, 0xFE, 0xF0, 0x3F, 0x04, status };
		char debug2[] = { 0xFF, 0xFE, 0xFD };

		WriteMessage(debug1, 6, StartOfMessage);
		WriteMessage(debug2, 3, EndOfMessage);
		LongSleepWithWatchdog();


	int length;
	for (length = 0; length < InputBufferSize - 1; length++)
	{
		for (int iterations = 0; iterations < 1000; iterations++)
		{
			ScratchWatchdog();
			status = *DLC_Status & 0xE0;
			if (status != 0x40)
			{
				continue;
			}
		}

		IncomingMessage[length] = *DLC_Receive_FIFO;
		ScratchWatchdog();

		status = *DLC_Status & 0xE0;
		if (status == 0x40)
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
		//LongSleepWithWatchdog();
		ScratchWatchdog();
		WasteTime();

		int length = ReadMessage();
		if (length == 0)
		{
			// No message received, so send a heartbeat message and listen again.
			// Note that without this call to LongSleepWithWatchdog, the WriteMessage call will fail.
			// That's probably related to the fact that the ReadMessage function sends a debug message before returning.
			// Should try experimenting with different delay lengths to see just how long we need to wait.
			//LongSleepWithWatchdog();
			//WriteMessage(toolPresent, 4, EntireMessage);
			continue;
		}

		LongSleepWithWatchdog();

		// Echo the received message with a 'tool present' header.
		WriteMessage(toolPresent, 4, StartOfMessage);
		WriteMessage(IncomingMessage, length, EndOfMessage);
	}
}


