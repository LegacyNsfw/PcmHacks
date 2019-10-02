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

#define DLC_CONFIGURATION          (*(unsigned char *)0x00FFF600)
#define DLC_INTERRUPTCONFIGURATION (*(unsigned char *)0x00FFF606)
#define DLC_TRANSMIT_COMMAND       (*(unsigned char *)0x00FFF60C)
#define DLC_TRANSMIT_FIFO          (*(unsigned char *)0x00FFF60D)
#define DLC_STATUS                 (*(unsigned char *)0x00FFF60E)
#define DLC_RECEIVE_FIFO           (*(unsigned char *)0x00FFF60F)
#define WATCHDOG1                  (*(unsigned char *)0x00FFFA27)
#define WATCHDOG2                  (*(unsigned char *)0x00FFD006)

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
char __attribute((section(".kerneldata"))) MessageBuffer[MessageBufferSize];

// Code can add data to this buffer while doing something that doesn't work
// well, and then dump this buffer later to find out what was going on.
char __attribute((section(".kerneldata"))) BreadcrumbBuffer[BreadcrumbBufferSize];

// Uncomment one of these to determine which way to use the breadcrumb buffer.
//#define RECEIVE_BREADCRUMBS
//#define TRANSMIT_BREADCRUMBS

///////////////////////////////////////////////////////////////////////////////
// This needs to be called periodically to prevent the PCM from rebooting.
///////////////////////////////////////////////////////////////////////////////
void ScratchWatchdog()
{
	WATCHDOG1 = 0x55;
	WATCHDOG1 = 0xAA;
	WATCHDOG2 &= 0x7F;
	WATCHDOG2 |= 0x80;
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
// Send the given bytes over the VPW bus.
// The DLC will append the checksum byte, so we don't have to.
// The message must be written into MessageBuffer first.
// This function will send 'length' bytes from that buffer onto the wire.
///////////////////////////////////////////////////////////////////////////////
void WriteMessage(int length, int breadcrumbs)
{
	ScratchWatchdog();

#ifdef TRANSMIT_BREADCRUMBS
	if (breadcrumbs)
	{
		ClearBreadcrumbBuffer();
	}
	int breadcrumbIndex = 2;
#endif

	DLC_TRANSMIT_COMMAND = 0x14;

	int lastIndex = length - 1;

	unsigned char status;

	int useBreadcrumbsForBufferLimit = 1;
	int stopUsing = 0;

	// Send message
	for (int index = 0; index < lastIndex; index++)
	{
		DLC_TRANSMIT_FIFO = MessageBuffer[index];
		ScratchWatchdog();

		// Status 2 means the transmit buffer is almost full.
		// In that case, pause until there's room in the buffer.
		status = DLC_STATUS & 0x03;

		// TODO: try looping around 0x02 (almost full) rather than 0x03 (full)
		int loopCount = 0;
		while ((status == 0x02 || status == 0x03) && loopCount < 250)
		{
			loopCount++;


#ifdef TRANSMIT_BREADCRUMBS
			if (useBreadcrumbsForBufferLimit)
			{
				BreadcrumbBuffer[breadcrumbIndex++] = status;
				stopUsing = 1;
			}
#endif

			// With max iterations at 25, we get some 2s and 3s in the loop counter.
			for (int iterations = 0; iterations < 50; iterations++)
			{
				ScratchWatchdog();
				WasteTime();
			}

			ScratchWatchdog();
			status = DLC_STATUS & 0x03;
		}


#ifdef TRANSMIT_BREADCRUMBS
		if (stopUsing)
		{
			BreadcrumbBuffer[1] = loopCount;
			useBreadcrumbsForBufferLimit = 0;
		}
#endif
	}

	// Send last byte
	DLC_TRANSMIT_COMMAND = 0x0C;
	DLC_TRANSMIT_FIFO = MessageBuffer[length - 1];

	// Send checksum?
	WasteTime();
	DLC_TRANSMIT_COMMAND = 0x03;
	DLC_TRANSMIT_FIFO = 0x00;

	// Wait for the message to be flushed.
	//
	// This seems to work as it should, however note that, as per the DLC spec,
	// we'll get a series of 0x03 status values (buffer full) before the status
	// changes immediately to zero. There's no 0x02 (almost full) in between.
	status = DLC_STATUS & 0x03;
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
		status = DLC_STATUS & 0x03;
	}


#ifdef TRANSMIT_BREADCRUMBS
	BreadcrumbBuffer[0] = loopCount;
#endif

	ClearMessageBuffer();
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

		status = (DLC_STATUS & 0xE0) >> 5;

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
			MessageBuffer[length] = DLC_RECEIVE_FIFO;
			length++;
			break;

		case 5: // Buffer contains a completion code, followed by more data bytes.
		case 6: // Buffer contains a completion code, followed by a full frame.
		case 7: // Buffer contains a completion code only.
			*completionCode = DLC_RECEIVE_FIFO;

			// Not sure if this is necessary - the code works without it, but it seems
			// like a good idea according to 5.1.3.2. of the DLC data sheet.
			DLC_TRANSMIT_COMMAND = 0x02;

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
			while (DLC_STATUS & 0xE0 == 0x60)
			{
				char unused = DLC_RECEIVE_FIFO;
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
	}
}

///////////////////////////////////////////////////////////////////////////////
// Send a message to explain why we're rebooting, then reboot.
///////////////////////////////////////////////////////////////////////////////
void Reboot(unsigned char reason)
{
	LongSleepWithWatchdog();

	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = reason;
	WriteMessage(4, 0);

	LongSleepWithWatchdog();

	// If you stop scratching the watchdog, it will kill you.
	for (;;);
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

	DLC_INTERRUPTCONFIGURATION = 0x00;
	LongSleepWithWatchdog();

	// Flush the DLC
	DLC_TRANSMIT_COMMAND = 0x03;
	DLC_TRANSMIT_FIFO = 0x00;

	ClearMessageBuffer();

	// There are some extra bytes here for debugging / diagnostics.
	char toolPresent[] = { 0x8C, 0xFE, 0xF0, 0x3F, 0x00, 0x00, 0x00 };
	char echo[] = { 0x6C, 0xF0, 0x10, 0xAA };

	// This loop runs out quickly, to force the PCM to reboot, to speed up testing.
	// The real kernel should probably loop for a much longer time (possibly forever),
	// to allow the app to recover from any failures.
	// If we choose to loop forever we need a good story for how to get out of that state.
	// Pull the PCM fuse? Give the app button to tell the kernel to reboot?
	for(int iterations = 0; iterations < 100; iterations++)
	{
		//LongSleepWithWatchdog();
		ScratchWatchdog();
		WasteTime();

		char completionCode = 0xFF;
		char readState = 0xFF;
		int length = ReadMessage(&completionCode, &readState);
		if (length == 0)
		{
			// If no message received, sent a tool-present message with a couple
			// of extra bytes to help us understand what's going on in the DLC.
			toolPresent[4] = DLC_STATUS;
			toolPresent[5] = completionCode;
			toolPresent[6] = readState;
			CopyToMessageBuffer(toolPresent, 7, 0);

			WriteMessage(7, 0);
			ClearMessageBuffer();

			// Enable this for breadcrumb debugging.
			// It gives insight into the code, but it causes side-effects in the
			// tool and in the kernel TX/RX behavior. Partly by keeping the DLC
			// busy, and partly due to the long sleeps that are needed to make sure
			// the breadcrumb message gets sent out reliably.
			if (0)
			{
				LongSleepWithWatchdog();
				LongSleepWithWatchdog();

				MessageBuffer[0] = 0x6C;
				MessageBuffer[1] = 0xF0;
				MessageBuffer[2] = 0x10;
				MessageBuffer[3] = 0xCC;
				MessageBuffer[4] = readState;
				CopyToMessageBuffer(BreadcrumbBuffer, BreadcrumbBufferSize, 5);

				WriteMessage(BreadcrumbBufferSize + 5, 0);
				ClearMessageBuffer();
			}

			continue;
		}

		if ((completionCode & 0x30) != 0x00)
		{
			// This is a transmit error. Just ignore it and wait for the tool to retry.
			continue;
		}

		// Did the tool just request a reboot?
		if (MessageBuffer[3] == 0x20)
		{
			LongSleepWithWatchdog();
			Reboot(0xEE);
		}

		// Support the version request used by the PCM Hammer app
		if (MessageBuffer[3] == 0x3D && MessageBuffer[4] == 0x00)
		{
			MessageBuffer[0] = 0x6C;
			MessageBuffer[1] = 0xF0;
			MessageBuffer[2] = 0x10;
			MessageBuffer[3] = 0x7D;
			MessageBuffer[4] = 0x00;
			MessageBuffer[5] = 0x01;
			MessageBuffer[6] = 0x00;
			MessageBuffer[7] = 0x00;
			MessageBuffer[8] = 0xDD;

			WriteMessage(9, 0);
			LongSleepWithWatchdog();
			LongSleepWithWatchdog();
			continue;
		}

		// Echo the received message
		int offset = 7;
		CopyToMessageBuffer(MessageBuffer, length, offset);
		MessageBuffer[0] = 0x6C;
		MessageBuffer[1] = 0xF0;
		MessageBuffer[2] = 0x10;
		MessageBuffer[3] = 0xAA;
		MessageBuffer[4] = (char)(length & 0xFF);
		MessageBuffer[5] = completionCode;
		MessageBuffer[6] = readState;

		WriteMessage(length + offset, 1);

		ClearMessageBuffer();

		// Enable this for breadcrumb debugging.
		// It gives insight into the code, but it causes side-effects in the
		// tool and in the kernel TX/RX behavior. Partly by keeping the DLC
		// busy, and partly due to the long sleeps that are needed to make sure
		// the breadcrumb message gets sent out reliably.
		if (0)
		{
			LongSleepWithWatchdog();
			LongSleepWithWatchdog();

			MessageBuffer[0] = 0x6C;
			MessageBuffer[1] = 0xF0;
			MessageBuffer[2] = 0x10;
			MessageBuffer[3] = 0xBB;
			CopyToMessageBuffer(BreadcrumbBuffer, BreadcrumbBufferSize, 4);

			WriteMessage(BreadcrumbBufferSize + 4, 0);
			ClearMessageBuffer();
		}
	}

	Reboot(0xFF);
}
