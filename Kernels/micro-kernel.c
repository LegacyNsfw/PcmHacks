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
char volatile * const DLC_Status = (unsigned char*)0xFFF60E;
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
#define BreadcrumbBufferSize 10
char __attribute((section(".kerneldata"))) MessageBuffer[MessageBufferSize];
char __attribute((section(".kerneldata"))) BreadcrumbBuffer[BreadcrumbBufferSize];
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

void ClearMessageBuffer()
{
	for (int index = 0; index < MessageBufferSize; index++)
	{
		MessageBuffer[index] = 0;
	}
}

// Send the given bytes over the VPW bus.
// The DLC will append the checksum byte, so we don't have to.
// The message must be written into MessageBuffer first.
// This function will send 'length' bytes from that buffer onto the wire.
//
// It works pretty well with messages under 12 bytes long. Not so great with longer messages.
// I strongly suspect the problems are related to overflowing the FIFO and flushing it at the.
void WriteMessage(int length)
{
	ScratchWatchdog();

	*DLC_Transmit_Command = 0x14;
	
	int lastIndex = length - 1;

	unsigned char status;

	// Send message
	for (int index = 0; index < lastIndex; index++)
	{
		*DLC_Transmit_FIFO = MessageBuffer[index];
		ScratchWatchdog();

		// Status 2 means the transmit buffer is almost full.
		// In that case, pause until there's room in the buffer.
		status = *DLC_Status & 0x03;

		// TODO: Why doesn't this loop exit?
		//while (status == 0x02)
		if (status == 0x02)
		{
			LongSleepWithWatchdog();
			//status = *DLC_Status & 0x03;
		}
	}

	// Send last byte
	*DLC_Transmit_Command = 0x0C;
	*DLC_Transmit_FIFO = MessageBuffer[length - 1];

	// Send checksum 
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

		// If this worked right, the long-sleeps below probably wouldn't be necessary.
		char status = *DLC_Status & 0x03;
		if (status == 0x00)
		{
			break;
		}
	}

	// Should be able to remove these if the flush loop works.
	// But right now, you can't send two messages in a row without these.
	LongSleepWithWatchdog();
	LongSleepWithWatchdog();
	LongSleepWithWatchdog();
	ClearMessageBuffer();
}

// Read a VPW message into the 'MessageBuffer' buffer.
// This mostly works but I don't trust it 100% yet.
int ReadMessage(char *completionCode, char *readState)
{
	ScratchWatchdog();
	unsigned char status;

	for (int index = 0; index < BreadcrumbBufferSize; index++)
	{
		BreadcrumbBuffer[index] = 0;
	}

	int length = 0;
	int breadcrumbIndex = 0;
	for (int iterations = 0; iterations < 200 * 1000; iterations++)
	{
		// Artificial message-length limit for debugging.
		if (length == 25)
		{
			*readState = 0xEE;
			return length;
		}

		// Another artificial limit just for debugging.
		if (breadcrumbIndex == BreadcrumbBufferSize)
		{
			*readState = 0xFF;
			return length;
		}

		status = (*DLC_Status & 0xE0) >> 5;
		BreadcrumbBuffer[breadcrumbIndex] = status;
		breadcrumbIndex++;

		switch (status)
		{
		case 0: // No data to process. It might be better to wait longer here.
			LongSleepWithWatchdog();
			break;

		case 1: // Buffer contains data bytes.
		case 2: // Buffer contains data followed by a completion code.
		case 4:  // Buffer contains just one data byte.
			MessageBuffer[length] = *DLC_Receive_FIFO;
			length++;
			break;

		case 5: // Buffer contains a completion code, followed by more data bytes.
		case 6: // Buffer contains a completion code, followed by a full frame.
		case 7: // Buffer contains a completion code only.
			*completionCode = *DLC_Receive_FIFO;
			if (length != 0)
			{
				*readState = 1;
				return length;
			}
			break;

		case 3:  // Buffer overflow. What do do here?
			// Just throw the message away and hope the tool sends again?
			while (*DLC_Status & 0xE0 == 0x60)
			{
				char unused = *DLC_Receive_FIFO;
			}
			*readState = 0x0B;
			return 0;
		}

		ScratchWatchdog();
	}

	// If we reach this point, the loop above probably just hit maxIterations.
	// Or maybe the tool sent a message bigger than the buffer.
	// Either way, we have "received" an incomplete message.
	// Might be better to return zero and hope the tool sends again.
	// But for debugging we'll just see what we managed to receive.
	*readState = 0x0A;
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

	ClearMessageBuffer();

	// There's one extra byte here for insight into what's going on inside the kernel.
	char toolPresent[] = { 0x8C, 0xFE, 0xF0, 0x3F, 0x00, 0x00, 0x00 };
	char echo[] = { 0x6C, 0xF0, 0x10, 0xAA };

	// This loop runs out quickly, to force the PCM to reboot, to speed up testing.
	// The real kernel should probably loop for a much longer time (possibly forever,
	// to allow the app to recover from any failures. 
	// If we choose to loop forever we need a good story for how to get out of that state.
	// Pull the PCM fuse? Give the app button to tell the kernel to reboot?
	for(int iterations = 0; iterations < 50; iterations++)
	{
		//LongSleepWithWatchdog();
		ScratchWatchdog();
		WasteTime();

		char completionCode = 0xFF;
		char readState = 0xFF;
		int length = ReadMessage(&completionCode, &readState);
		if (length == 0)
		{
			// No message received, so send a heartbeat message and listen again.
			// Note that without this call to LongSleepWithWatchdog, the WriteMessage call will fail.
			// That's probably related to the fact that the ReadMessage function sends a debug message before returning.
			// Should try experimenting with different delay lengths to see just how long we need to wait.

			LongSleepWithWatchdog();
			toolPresent[4] = *DLC_Status;
			toolPresent[5] = completionCode;
			toolPresent[6] = readState;
			CopyToMessageBuffer(toolPresent, 7, 0);
			
			WriteMessage(7);

			MessageBuffer[0] = 0x6C;
			MessageBuffer[1] = 0xF0;
			MessageBuffer[2] = 0x10;
			MessageBuffer[3] = 0xCC;
			MessageBuffer[4] = readState;
			CopyToMessageBuffer(BreadcrumbBuffer, BreadcrumbBufferSize, 5);
			
			WriteMessage(BreadcrumbBufferSize + 5);

			continue;
		}

		LongSleepWithWatchdog();

		// Echo the received message with a 'tool present' header.
		int offset = 4; // Can make this 7 and include 3 more bytes, but WriteMessage doesn't do well with more than 12 bytes.
		CopyToMessageBuffer(MessageBuffer, length, offset);
		MessageBuffer[0] = 0x6C;
		MessageBuffer[1] = 0xF0;
		MessageBuffer[2] = 0x10;
		MessageBuffer[3] = 0xAA;
//		MessageBuffer[4] = (char)(length & 0xFF);
//		MessageBuffer[5] = completionCode;
//		MessageBuffer[6] = readState;

		WriteMessage(length + offset);

		MessageBuffer[0] = 0x6C;
		MessageBuffer[1] = 0xF0;
		MessageBuffer[2] = 0x10;
		MessageBuffer[3] = 0xBB;
		CopyToMessageBuffer(BreadcrumbBuffer, BreadcrumbBufferSize, 4);

		WriteMessage(BreadcrumbBufferSize + 4);
	}

	for (;;)
	{
		// Wait for the watchdog to reboot the PCM
	}
}
