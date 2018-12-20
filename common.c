///////////////////////////////////////////////////////////////////////////////
// Code that will be useful in different types of kernels.
///////////////////////////////////////////////////////////////////////////////
#define EXTERN
#include "common.h"
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
unsigned char __attribute((section(".kerneldata"))) MessageBuffer[MessageBufferSize];

// Code can add data to this buffer while doing something that doesn't work
// well, and then dump this buffer later to find out what was going on.
unsigned char __attribute((section(".kerneldata"))) BreadcrumbBuffer[BreadcrumbBufferSize];

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
void WasteTime()
{
	asm("nop");
	asm("nop");
	asm("nop");
	asm("nop");
}

///////////////////////////////////////////////////////////////////////////////
// Also does what it says. 10,000 iterations takes a bit less than half a second.
///////////////////////////////////////////////////////////////////////////////
void LongSleepWithWatchdog()
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
// Sleep for a variable amount of time. This should be close to Dimented24x7's
// assembly-language implementation.
// Consider cutting the inner loop down, and increasing the parameter at all
// call sites. Consecutive messages-sends (e.g. in HandleReadMode35) work fine
// with an inner loop of 100. Haven't tried 50 yet.
///////////////////////////////////////////////////////////////////////////////
void VariableSleep(int iterations)
{
	for (int outer = 0; outer < iterations; outer++)
	{
		for (int inner = 0; inner < 250; inner++)
		{
			asm("nop");
			asm("nop");
			asm("nop");
			asm("nop");

			asm("nop");
			asm("nop");
			asm("nop");
			asm("nop");
		}

		ScratchWatchdog();
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
void WriteMessage(char* start, int length, Segment segment)
{
	ScratchWatchdog();

	if ((segment & Start) != 0)
	{
		*DLC_Transmit_Command = 0x14;
	}
		
	int lastIndex = (segment & End) ? length - 1 : length;

	unsigned char status;

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
		while (status != 0 && loopCount < 500)
		{
			loopCount++;

			for (int iterations = 0; iterations < 100; iterations++)
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
		if (length == 5100)
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
		case 0: // No data to process. This wait period may need more tuning.

			for (int waits = 0; waits < 10; waits++)
			{
				ScratchWatchdog();
				WasteTime();
			}
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

#ifdef RECEIVE_BREADCRUMBS
			BreadcrumbBuffer[breadcrumbIndex++] = *completionCode;
#endif
			// Not sure if this is necessary - the code works without it, but it seems
			// like a good idea according to 5.1.3.2. of the DLC data sheet.
			*DLC_Transmit_Command = 0x02;

			// If we return here when the length is zero, we'll never return 
			// any message data at all. Not sure why.
			if (length == 0)
			{
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
	char toolPresent[] = { 0x8C, 0xFE, 0xF0, 0x3F, 0x00, 0x00, 0x00, 0x00 };
	toolPresent[4] = b1;
	toolPresent[5] = b2;
	toolPresent[6] = b3;
	toolPresent[7] = b4;

	WriteMessage(toolPresent, 8, Complete);
	ClearMessageBuffer();
}

void SendToolPresent2(unsigned int value)
{
	SendToolPresent(
		(value & 0xFF000000) >> 24,
		(value & 0x00FF0000) >> 16,
		(value & 0x0000FF00) >> 8,
		(value & 0x000000FF));
}

#if defined(RECEIVE_BREADCRUMBS) || defined(TRANSMIT_BREADCRUMBS) || defined(MODEBYTE_BREADCRUMBS)
void SendBreadcrumbs(char code)
{
	char toolPresent[] = { 0x8C, 0xFE, 0xF0, 0x3F, code };
	WriteMessage(toolPresent, 5, Start);
	WriteMessage(BreadcrumbBuffer, breadcrumbs, End);
	LongSleepWithWatchdog();
	LongSleepWithWatchdog();
}

///////////////////////////////////////////////////////////////////////////////
// Send the breadcrumb array, then reboot.
// This is useful in figuring out how the kernel got into a bad state.
///////////////////////////////////////////////////////////////////////////////
void SendBreadcrumbsReboot(char code, int breadcrumbs)
{
	SendBreadcrumbs(code);
	Reboot(code);
}
#endif

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
// Get the version of the kernel. (Mode 3D, submode 00)
///////////////////////////////////////////////////////////////////////////////
void HandleVersionQuery()
{
	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x7D;
	MessageBuffer[4] = 0x00;
	MessageBuffer[5] = 0x01; // major
	MessageBuffer[6] = 0x00; // minor
	MessageBuffer[7] = 0x00; // patch
	MessageBuffer[8] = 0xAA; // quality (AA = alpha, BB = beta, 00 = release)

	// The AllPro and ScanTool devices need a short delay to switch from 
	// sending to receiving. Otherwise they'll miss the response.
	VariableSleep(2);

	WriteMessage(MessageBuffer, 9, Complete);
}
