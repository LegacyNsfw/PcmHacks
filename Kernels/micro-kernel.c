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
#define MessageBufferSize 1024
char __attribute((section(".kerneldata"))) MessageBuffer[MessageBufferSize];
//char __attribute((section(".kerneldata"))) MessageCompletionCode;

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
// The message must be written into MessageBuffer first.
// This function will send 'length' bytes from that buffer onto the wire.
void WriteMessage(int length)
{
	ScratchWatchdog();

	*DLC_Transmit_Command = 0x14;
	
	int lastIndex = length - 1;

	// Send message (will we need a watchdog call inside this loop for long messages?)
	for (int index = 0; index < lastIndex; index++)
	{
		*DLC_Transmit_FIFO = MessageBuffer[index];
		WasteTime();
	}

	// Send last byte
	*DLC_Transmit_Command = 0x0C;
	*DLC_Transmit_FIFO = MessageBuffer[length - 1];

	// Send checksum (?)
	WasteTime();
	*DLC_Transmit_Command = 0x03;
	*DLC_Transmit_FIFO = 0x00;

	// Wait for the message to be flushed.
	for (int iterations = 0; iterations < length + 10; iterations++)
	{
		ScratchWatchdog();
		WasteTime();
		WasteTime();
		WasteTime();
		WasteTime();
		char status = *DLC_Status & 0xE0;
		if (status == 0xE0)
		{
			break;
		}
	}
}

// Read a VPW message into the 'MessageBuffer' buffer.
// This doesn't work yet.
int ReadMessage(char *completionCode)
{
	char status;
	int length;
	int iterations;
	int maxIterations = 30 * 1000; // This is just to guarantee that the loop doesn't execute forever. Feel free to suggest a different number.
	for (length = 0, iterations = 0; 
		length < MessageBufferSize - 1 && iterations < 10 * 1000; 
		iterations++)
	{
		ScratchWatchdog();

		status = *DLC_Status >> 5;
		switch (status)
		{
		case 0x00: // No data to process. It might be better to wait longer here.
			LongSleepWithWatchdog();
			break;

		case 0x01: // Buffer contains more than one byte.
		case 0x04: // Buffer contains just one data byte.
			MessageBuffer[length++] = *DLC_Receive_FIFO;
			break;

		case 0x02: // Buffer contains a completion code.
		case 0x05: // Buffer contains a completion code, followed by more data bytes.
		case 0x06: // Buffer contains a completion code, followed by a full frame.
		case 0x07: // Buffer contains a completion code only.
			*completionCode = *DLC_Receive_FIFO;
			return length;

		case 0x03: // Buffer overflow. What do do here?
			// Just throw the message away and hope the tool sends again?
			return 0;
		}
	}

	// If we reach this point, the loop above probably just hit maxIterations.
	// Or maybe the tool sent a message bigger than the buffer.
	// Either way, we have "received" an incomplete message.
	// Might be better to return zero and hope the tool sends again.
	// But for debugging we'll just see what we managed to receive.
	// This should use a completion code that the DLC will never actually use.
	*completionCode = 0x00;
	return length;
}

void CopyToMessageBuffer(char* start, int length, int offset)
{
	// This is the obvious way to do it, but one of the usage scenarios
	// involves moving data in the buffer to a location further into the
	// buffer, which would overwrite the data if the offset is shorter
	// than the message.
	//
	// for (int index = 0; index < length; index++)
	// {
	//   MessageBuffer[offset + index] = start[index];
	// }
	//
	// So instead we copy from back to front:
	for (int index = length - 1; index >= 0; index--)
	{
		MessageBuffer[index + offset] = start[index];
	}
}

// This is the entry point for the kernel.
int
__attribute__((section(".kernelstart")))
KernelStart(void)
{
	// Disable peripheral interrupts
	asm("ORI #0x700, %SR");

	ScratchWatchdog();

	*DLC_InterruptConfiguration = 0x00;
	LongSleepWithWatchdog();

	// Flush the DLC
	*DLC_Transmit_Command = 0x03;
	*DLC_Transmit_FIFO = 0x00;

	// There's one extra byte here for insight into what's going on inside the kernel.
	char toolPresent[] = { 0x8C, 0xFE, 0xF0, 0x3F, 0x00, 0x00 };
	char echo[] = { 0x6C, 0xF0, 0x10, 0xAA };

	for(int iterations = 0; iterations < 50 * 1000; iterations++)
	{
		//LongSleepWithWatchdog();
		ScratchWatchdog();
		WasteTime();

		char completionCode;
		int length = ReadMessage(&completionCode);
		if (length == 0)
		{
			// No message received, so send a heartbeat message and listen again.
			// Note that without this call to LongSleepWithWatchdog, the WriteMessage call will fail.
			// That's probably related to the fact that the ReadMessage function sends a debug message before returning.
			// Should try experimenting with different delay lengths to see just how long we need to wait.
			LongSleepWithWatchdog();
			toolPresent[4] = *DLC_Status;
			toolPresent[5] = completionCode;
			CopyToMessageBuffer(toolPresent, 6, 0);
			WriteMessage(5);
			continue;
		}

		LongSleepWithWatchdog();

		// Echo the received message with a 'tool present' header.
		// This copy has to be done back-to-front to avoid overwriting data.
		int offset = 6;
		CopyToMessageBuffer(MessageBuffer, length, offset);
		MessageBuffer[0] = 0x6C;
		MessageBuffer[1] = 0xF0;
		MessageBuffer[2] = 0x10;
		MessageBuffer[3] = 0xAA;
		MessageBuffer[4] = (char)(length & 0xFF);
		MessageBuffer[5] = completionCode;
		WriteMessage(length + offset);
	}

	for (;;)
	{
		// Wait for the watchdog to reboot the PCM
	}
}
