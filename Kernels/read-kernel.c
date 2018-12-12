///////////////////////////////////////////////////////////////////////////////
// This is a tiny kernel to validate the ability to read and write messages
// using the PCM's DLC. It will echo received messages, and reboot on command.
///////////////////////////////////////////////////////////////////////////////
//
// After we have a trivial kernel that can send and received messages, the
// reusable stuff should be moved into common.c and referenced from there.
//
// But for now I want to keep this kernel as tiny and as simple as possible,
// to simplify debugging.
//
///////////////////////////////////////////////////////////////////////////////

char volatile * const DLC_Configuration = (char*)0xFFF600;
char volatile * const DLC_InterruptConfiguration = (char*)0xFFF606;
char volatile * const DLC_Transmit_Command = (char*)0xFFF60C;
char volatile * const DLC_Transmit_FIFO = (char*)0xFFF60D;
char volatile * const DLC_Status = (unsigned char*)0xFFF60E;
char volatile * const DLC_Receive_FIFO = (char*)0xFFF60F;
char volatile * const Watchdog1 = (char*)0xFFFA27;
char volatile * const Watchdog2 = (char*)0xFFD006;

///////////////////////////////////////////////////////////////////////////////
//
// The linker needs to put these buffers after the kernel code, but before the
// system registers that are at the top of the RAM space.
//
// The code that extracts the kernel bin needs to exclude that address range,
// because it will just add 8kb of 0x00 bytes to the kernel bin file.
//
// 4096 == 0x1000
#define MessageBufferSize 1024
#define BreadcrumbBufferSize 6
unsigned char __attribute((section(".kerneldata"))) MessageBuffer[MessageBufferSize];

// Code can add data to this buffer while doing something that doesn't work
// well, and then dump this buffer later to find out what was going on.
unsigned char __attribute((section(".kerneldata"))) BreadcrumbBuffer[BreadcrumbBufferSize];

// Uncomment one of these to determine which way to use the breadcrumb buffer.
//#define RECEIVE_BREADCRUMBS
//#define TRANSMIT_BREADCRUMBS

///////////////////////////////////////////////////////////////////////////////
// This needs to be called periodically to prevent the PCM from rebooting.
///////////////////////////////////////////////////////////////////////////////
void ScratchWatchdog()
{
	*Watchdog1 = 0x55;
	*Watchdog1 = 0xAA;
	*Watchdog2 = *Watchdog2 & 0x7F;
	*Watchdog2 = *Watchdog2 | 0x80;
}

///////////////////////////////////////////////////////////////////////////////
// Does what it says.
///////////////////////////////////////////////////////////////////////////////
int WasteTime()
{
	asm("nop");
	asm("nop");
	asm("nop");
	asm("nop");
}

///////////////////////////////////////////////////////////////////////////////
// Also does what it says. 10,000 iterations takes a bit less than half a second.
///////////////////////////////////////////////////////////////////////////////
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

///////////////////////////////////////////////////////////////////////////////
// All outgoing messages must be written into this buffer. The WriteMessage
// function will copy from this buffer to the DLC. Resetting the buffer should
// not really be necessary, but it helps to simplify debugging.
///////////////////////////////////////////////////////////////////////////////
void ClearMessageBuffer()
{
	for (int index = 0; index < MessageBufferSize; index++)
	{
		MessageBuffer[index] = 0;
	}
}

///////////////////////////////////////////////////////////////////////////////
// The 'breadcrumb' buffer helps give insight into what happened.
///////////////////////////////////////////////////////////////////////////////
void ClearBreadcrumbBuffer()
{
	for (int index = 0; index < BreadcrumbBufferSize; index++)
	{
		BreadcrumbBuffer[index] = 0;
	}
}

///////////////////////////////////////////////////////////////////////////////
// Indicates whether the buffer passed to WriteMessage contains the beginning,
// middle, or end of a message. 
///////////////////////////////////////////////////////////////////////////////
typedef enum
{
	Invalid = 0,
	Start = 1,
	Middle = 2,
	End = 4,
	Complete = Start | End,
} Segment;

///////////////////////////////////////////////////////////////////////////////
// Send the given bytes over the VPW bus.
// The DLC will append the checksum byte, so we don't have to.
// The message must be written into MessageBuffer first.
// This function will send 'length' bytes from that buffer onto the wire.
///////////////////////////////////////////////////////////////////////////////
void WriteMessage(char* start, int length, Segment segment)
{
	ScratchWatchdog();

	if ((segment & Start) != 0)
	{
		*DLC_Transmit_Command = 0x14;
	}
		
	int lastIndex = (segment & End) ? length - 1 : length;

	unsigned char status;

	int useBreadcrumbsForBufferLimit = 1;
	int stopUsing = 0;

	// Send message
	for (int index = 0; index < lastIndex; index++)
	{
		*DLC_Transmit_FIFO = start[index];
		ScratchWatchdog();

		// Status 2 means the transmit buffer is almost full.
		// In that case, pause until there's room in the buffer.
		status = *DLC_Status & 0x03;

		// TODO: try looping around 0x02 (almost full) rather than 0x03 (full)
		int loopCount = 0;
		while ((status == 0x02 || status == 0x03) && loopCount < 250)
		{
			loopCount++;

			// With max iterations at 25, we get some 2s and 3s in the loop counter.
			for (int iterations = 0; iterations < 50; iterations++)
			{
				ScratchWatchdog();
				WasteTime();
			}

			ScratchWatchdog();
			status = *DLC_Status & 0x03;
		}
	}

	if ((segment & End) != 0)
	{
		// Send last byte
		*DLC_Transmit_Command = 0x0C;
		*DLC_Transmit_FIFO = start[length - 1];

		// Send checksum? 
		WasteTime();
		*DLC_Transmit_Command = 0x03;
		*DLC_Transmit_FIFO = 0x00;

		// Wait for the message to be flushed.
		//
		// This seems to work as it should, however note that, as per the DLC spec,
		// we'll get a series of 0x03 status values (buffer full) before the status
		// changes immediately to zero. There's no 0x02 (almost full) in between.
		status = *DLC_Status & 0x03;
		int loopCount = 0;
		while (status != 0 && loopCount < 250)
		{
			loopCount++;

			// Set the max iterations in the following loop to 100 if you uncomment this line.
			// BreadcrumbBuffer[breadcrumbIndex++] = status;

			for (int iterations = 0; iterations < 25; iterations++)
			{
				ScratchWatchdog();
				WasteTime();
			}

			ScratchWatchdog();
			status = *DLC_Status & 0x03;
		}

		ClearMessageBuffer();
	}
}

///////////////////////////////////////////////////////////////////////////////
// Read a VPW message into the 'MessageBuffer' buffer.
///////////////////////////////////////////////////////////////////////////////
int ReadMessage(char *completionCode, char *readState)
{
	ScratchWatchdog();
	unsigned char status;

#ifdef RECEIVE_BREADCRUMBS
	ClearBreadcrumbBuffer();
	int breadcrumbIndex = 0; 
#endif

	int length = 0;
	for (int iterations = 0; iterations < 30 * 1000; iterations++)
	{
		// Artificial message-length limit for debugging.
		if (length == 25)
		{
			*readState = 0xEE;
			return length;
		}

		status = (*DLC_Status & 0xE0) >> 5;
		
#ifdef RECEIVE_BREADCRUMBS
		// Another artificial limit just for debugging.
		if (breadcrumbIndex == BreadcrumbBufferSize)
		{
			*readState = 0xFF;
			return length;
		}

//		BreadcrumbBuffer[breadcrumbIndex] = status;
//		breadcrumbIndex++;
#endif
		switch (status)
		{
		case 0: // No data to process. It might be better to wait longer here.
			WasteTime();
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

			// Not sure if this is necessary - the code works without it, but it seems
			// like a good idea according to 5.1.3.2. of the DLC data sheet.
			*DLC_Transmit_Command = 0x02;

			// If we return here when the length is zero, we'll never return 
			// any message data at all. Not sure why.
			if (length == 0)
			{
#ifdef RECEIVE_BREADCRUMBS
				BreadcrumbBuffer[breadcrumbIndex++] = *completionCode;
#endif
				break;
			}

			if ((*completionCode & 0x30) == 0x30)
			{
				*readState = 2;
				return 0;
			}

			*readState = 1;
			return length;

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

///////////////////////////////////////////////////////////////////////////////
// Copy the given buffer into the message buffer.
///////////////////////////////////////////////////////////////////////////////
void CopyToMessageBuffer(char* start, int length, int offset)
{
	// This is the obvious way to do it, but one of the usage scenarios
	// involves moving data in the buffer to a location further into the
	// buffer, which would overwrite the data if the offset is shorter
	// than the message:
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

		if (index % 100 == 0)
		{
			ScratchWatchdog();
		}
	}
}

///////////////////////////////////////////////////////////////////////////////
// Send a message to explain why we're rebooting, then reboot.
///////////////////////////////////////////////////////////////////////////////
void Reboot(unsigned int value)
{
	LongSleepWithWatchdog();

	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x60;
	MessageBuffer[4] = (unsigned char)((value & 0xFF000000) >> 24);
	MessageBuffer[5] = (unsigned char)((value & 0x00FF0000) >> 16);
	MessageBuffer[6] = (unsigned char)((value & 0x0000FF00) >> 8);
	MessageBuffer[7] = (unsigned char)((value & 0x000000FF) >> 0);
	WriteMessage(MessageBuffer, 8, Complete);

	LongSleepWithWatchdog();

	// If you stop scratching the watchdog, it will kill you.
	for (;;);
}

///////////////////////////////////////////////////////////////////////////////
// Send a tool-present message with 3 extra data bytes for debugging.
///////////////////////////////////////////////////////////////////////////////
void SendToolPresent(unsigned char b1, unsigned char b2, unsigned char b3, unsigned char b4)
{
	char toolPresent[] = { 0x8C, 0xFE, 0xF0, 0x3F, 0x00, 0x00, 0x00 };
	toolPresent[4] = b1;
	toolPresent[5] = b2;
	toolPresent[6] = b3;
	toolPresent[6] = b4;

	WriteMessage(toolPresent, 8, Complete);
	ClearMessageBuffer();
}

///////////////////////////////////////////////////////////////////////////////
// Comput the checksum for the header of an outgoing message.
///////////////////////////////////////////////////////////////////////////////
unsigned short StartChecksum()
{
	unsigned short checksum = 0;
	for (int index = 4; index < 10; index++)
	{
		checksum += MessageBuffer[index];
	}

	return checksum;
}

///////////////////////////////////////////////////////////////////////////////
// Copy the payload for a read request, while updating the checksum.
///////////////////////////////////////////////////////////////////////////////
unsigned short AddReadPayloadChecksum(char* start, int length)
{
	ScratchWatchdog();

	unsigned short checksum = 0;

	for (int index = 0; index < length; index++)
	{
		unsigned char value = start[index];
		checksum += value;

		if (index % 100 == 0)
		{
			ScratchWatchdog();
		}
	}

	ScratchWatchdog();

	return checksum;
}

///////////////////////////////////////////////////////////////////////////////
// Set the checksum for a data block.
///////////////////////////////////////////////////////////////////////////////
void SetBlockChecksum(int length, unsigned short checksum)
{
	MessageBuffer[10 + length] = (unsigned char)((checksum & 0xFF00) >> 8);
	MessageBuffer[11 + length] = (unsigned char)(checksum & 0xFF);
}

///////////////////////////////////////////////////////////////////////////////
// Read the specified address range and send the data to the tool.
///////////////////////////////////////////////////////////////////////////////
void ReadAndSend(unsigned start, unsigned length)
{
	// TODO: Validate the start address and length, fail if unreasonable.
	
	// Send the "agree" response.
	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x75;

	MessageBuffer[4] = 0x01;
	MessageBuffer[5] = 0x54;
	MessageBuffer[6] = 0x6C;
	MessageBuffer[7] = 0xF0;

	WriteMessage(MessageBuffer, 8, Complete);
	
	// Send the payload
	MessageBuffer[0] = 0x6D;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x36;
	MessageBuffer[4] = 0x01;
	MessageBuffer[5] = (char)(length >> 8);
	MessageBuffer[6] = (char)length;
	MessageBuffer[7] = (char)(start >> 16);
	MessageBuffer[8] = (char)(start >> 8);
	MessageBuffer[9] = (char)start;
	WriteMessage(MessageBuffer, 10, Start);

	unsigned short checksum = StartChecksum();
	checksum += AddReadPayloadChecksum((char*)start, length);

	WriteMessage((char*) start, length, Middle);

	WriteMessage((char*)&checksum, 2, End);
}

///////////////////////////////////////////////////////////////////////////////
// Process a mode-35 read request.
///////////////////////////////////////////////////////////////////////////////
void ReadMode35()
{
	unsigned length = MessageBuffer[5];
	length <<= 8;
	length |= MessageBuffer[6];

	unsigned start = MessageBuffer[7];
	start <<= 8;
	start |= MessageBuffer[8];
	start <<= 8;
	start |= MessageBuffer[9];

	ReadAndSend(start, length);
}

///////////////////////////////////////////////////////////////////////////////
// Process an incoming message.
///////////////////////////////////////////////////////////////////////////////
void ProcessMessage()
{
	if (MessageBuffer[1] != 0x10)
	{
		// We're not the destination, ignore this message.
		// SendToolPresent(0xB0, MessageBuffer[1], 0);
		return;
	}

	if (MessageBuffer[2] != 0xF0)
	{
		// This didn't come from the tool, ignore this message.
		//SendToolPresent(0xB1, MessageBuffer[2], 0);
		return;
	}

	if (MessageBuffer[3] == 0x35)
	{
		//SendToolPresent(0x00, MessageBuffer[3], 0);
		ReadMode35();
		return;
	}

	if (MessageBuffer[3] == 0x37)
	{
		// ReadMode37();
		SendToolPresent(0xB2, MessageBuffer[3], 0, 0);
		return;
	}
}

///////////////////////////////////////////////////////////////////////////////
// This is the entry point for the kernel.
///////////////////////////////////////////////////////////////////////////////
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
	WasteTime();

	// This loop runs out quickly, to force the PCM to reboot, to speed up testing.
	// The real kernel should probably loop for a much longer time (possibly forever),
	// to allow the app to recover from any failures. 
	// If we choose to loop forever we need a good story for how to get out of that state.
	// Pull the PCM fuse? Give the app button to tell the kernel to reboot?
	// for(int iterations = 0; iterations < 100; iterations++)
	int iterations = 0;
	int timeout = 100;
	int lastMessage = iterations - timeout;
	for(;;)
	{
		iterations++;

		ScratchWatchdog();

		char completionCode = 0xFF;
		char readState = 0xFF;
		int length = ReadMessage(&completionCode, &readState);
		if (length == 0)
		{
			// If no message received for N iterations, reboot.
			if (iterations > (lastMessage + timeout))
			{
				Reboot(0xFFFFFFFF);
			}

			continue;
		}

		LongSleepWithWatchdog();

		if ((completionCode & 0x30) != 0x00)
		{
			// This is a transmit error. Just ignore it and wait for the tool to retry.
			continue;
		}

		lastMessage = iterations;

		// Did the tool just request a reboot?
		if (MessageBuffer[3] == 0x20)
		{
			LongSleepWithWatchdog();
			Reboot(iterations | 0x0E000000);
		}

		ProcessMessage();
	}

	// This shouldn't happen. But, just in case...
	Reboot(iterations | 0x0F000000);
}
