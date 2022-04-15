///////////////////////////////////////////////////////////////////////////////
// Code that will be useful in different types of kernels.
///////////////////////////////////////////////////////////////////////////////
#define EXTERN
#include "common.h"

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
	WATCHDOG1 = 0x55;
	WATCHDOG1 = 0xAA;
	WATCHDOG2 ^= 0x80;
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
// Shared sleep code for multiple scenarios
///////////////////////////////////////////////////////////////////////////////
void PrivateSleep(int outerLoop, int innerLoop)
{
	for (int outer = 0; outer < outerLoop; outer++)
	{
		for (int inner = 0; inner < innerLoop; inner++)
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
// Pause for about half a second. TODO: remove this, replace with either
// ElmSleep or VariableSleep, as appropriate.
///////////////////////////////////////////////////////////////////////////////
void LongSleepWithWatchdog()
{
	PrivateSleep(10 * 1000, 5);
}

///////////////////////////////////////////////////////////////////////////////
// ELM-based devices need a short pause between transmit and receive, otherwise
// they will miss the responses from the PCM. This function should be tuned to
// provide the right delay with AllPro and Scantool devices.
///////////////////////////////////////////////////////////////////////////////
void ElmSleep()
{
	// 1,25 worked well for a long series of kernel-version requests with both
	// the AllPro and Scantool at 1x speed.
	// CRC responses aren't received by either device. Not sure if timing related.
	// Have not tested 4x yet. Have not tried smaller delay either.
	PrivateSleep(1, 50);
}

///////////////////////////////////////////////////////////////////////////////
// Sleep for a variable amount of time. This should be close to Dimented24x7's
// assembly-language implementation.
// Consider cutting the inner loop down, and increasing the parameter at all
// call sites. Consecutive message-sends (e.g. in HandleReadMode35) work fine
// with an inner loop of 100. Haven't tried 50 yet.
///////////////////////////////////////////////////////////////////////////////
void VariableSleep(unsigned int iterations)
{
	PrivateSleep(iterations, 250);
}

///////////////////////////////////////////////////////////////////////////////
// All outgoing messages must be written into this buffer. The WriteMessage
// function will copy from this buffer to the DLC. Resetting the buffer should
// not really be necessary, but it helps to simplify debugging. Use sparingly,
// this is a slow function and contributes to blocking the DLC.
///////////////////////////////////////////////////////////////////////////////
void ClearMessageBuffer()
{
	for (int index = 0; index < MessageBufferSize; index++)
	{
		// This is not needed for P01, but P59 will reboot without it.
		if (index % 500 == 0)
		{
			ScratchWatchdog();
		}

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
// Send a byte - used by WriteMessage
///////////////////////////////////////////////////////////////////////////////
void WriteByte(unsigned char byte)
{
	unsigned char status;

	// Status 2 means the transmit buffer is almost full.
	// In that case, pause until there's room in the buffer.
	status = DLC_STATUS & 0x03;

	// TODO: try looping around 0x02 (almost full) rather than 0x03 (full)
	unsigned char loopCount = 0;
	while ((status == 0x02 || status == 0x03) && loopCount < 250)
	{
		loopCount++;

		// With max iterations at 25, we get some 2s and 3s in the loop counter.
		for (int iterations = 0; iterations < 50; iterations++)
		{
			WasteTime();
		}

		ScratchWatchdog();
		status = DLC_STATUS & 0x03;
	}
	DLC_TRANSMIT_FIFO = byte;
}

///////////////////////////////////////////////////////////////////////////////
// Send the given bytes over the VPW bus.
// The DLC will append the checksum byte, so we don't have to.
// The message must be written into MessageBuffer first.
// This function will send 'length' bytes from that buffer onto the wire.
// does not init the blocksum global variable so we can continue an existing
// count. Use StartChecksum() to begin with the header sum, and let this
// finish and transmit it.
///////////////////////////////////////////////////////////////////////////////
void WriteMessage(unsigned char *start, unsigned short length, Segment segment)
{
	ScratchWatchdog();

	if ((segment & Start) != 0)
	{
		DLC_TRANSMIT_COMMAND = 0x14;
	}

	unsigned short lastIndex = (segment & End) ? length - 1 : length;

	unsigned char status;
	unsigned char lastbyte;

	unsigned short checksum = StartChecksum();

	// Send a message body
	unsigned short index;
	for (index = 0; index < lastIndex; index++)
	{
		checksum += (unsigned char) start[index];
		WriteByte(start[index]);
	}

	// transmit a a block sum?
	if (segment & AddSum)
	{
		checksum += (unsigned char) start[index]; // complete the sum
		WriteByte(start[index]);  // send the last payload byte
		WriteByte(checksum >> 8);      // send the first block sum byte
		lastbyte = checksum;  // load the second block sum byte as the last byte
	}
	else
	{
		lastbyte = start[index];    // No block sum, last byte as normal
	}

	if ((segment & End) != 0)
	{
		// Send last byte
		DLC_TRANSMIT_COMMAND = 0x0C;
		DLC_TRANSMIT_FIFO = lastbyte;

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
		while (status != 0 && loopCount < 500)
		{
			loopCount++;

			for (int iterations = 0; iterations < 100; iterations++)
			{
				ScratchWatchdog();
				WasteTime();
			}

			ScratchWatchdog();
			status = DLC_STATUS & 0x03;
		}
	}
}

///////////////////////////////////////////////////////////////////////////////
// Read a VPW message into the 'MessageBuffer' buffer.
///////////////////////////////////////////////////////////////////////////////
int ReadMessage(unsigned char *completionCode, unsigned char *readState)
{
	ScratchWatchdog();
	unsigned char status;

	unsigned int iterations = 0;
	int length = 0;
	for (;;)
	{
		ScratchWatchdog();
		iterations++;

		// If no message received for N iterations, exit.
		if (iterations > 0x30000)
		{
			return 0;
		}

		// Let us not overflow MessageBuffer.
		if (length > MessageBufferSize)
		{
			*readState = 0xEE;
			return length;
		}

		status = DLC_STATUS >> 5;
		switch (status)
		{
			case 0: // No data to process.
				break;
			case 1: // Buffer contains 2-12 data bytes.
			case 2: // Buffer contains data followed by a completion code.
			case 4: // Buffer contains just one data byte.
				do {
					MessageBuffer[length++] = DLC_RECEIVE_FIFO;
					status = (DLC_STATUS >> 5);
				} while ( status == 1 || status == 2 || status == 4 );
				iterations = 0; // reset the timer every byte received
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
					break;
				}

				if (*completionCode & 0x30)
				{
					*readState = 2;
					return 0;
				}

				*readState = 1;
				return length;

			case 3:  // Buffer overflow. What to do here?
				// Just throw the message away and hope the tool sends again?
				while (DLC_STATUS & 0xE0 == 0x60)
				{
					MessageBuffer[length] = DLC_RECEIVE_FIFO;
				}
				*readState = 0x0B;
				return 0;
		}
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
void CopyToMessageBuffer(unsigned char* start, unsigned int length, unsigned int offset)
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
		if (index % 512 == 0)
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

#if defined P12
	asm("reset");
#else
	// If you stop scratching the watchdog, it will kill you.
	for (;;);
#endif
}

///////////////////////////////////////////////////////////////////////////////
// Send a tool-present message with 3 extra data bytes for debugging.
///////////////////////////////////////////////////////////////////////////////
void SendToolPresent(unsigned char b1, unsigned char b2, unsigned char b3, unsigned char b4)
{
	unsigned char toolPresent[] = { 0x8C, 0xFE, 0xF0, 0x3F, b1, b2, b3, b4 };
	WriteMessage(toolPresent, 8, Complete);
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
void SendBreadcrumbs(unsigned char code)
{
	unsigned char toolPresent[] = { 0x8C, 0xFE, 0xF0, 0x3F, code };
	WriteMessage(toolPresent, 5, Start);
	WriteMessage(BreadcrumbBuffer, breadcrumbs, End);
	LongSleepWithWatchdog();
	LongSleepWithWatchdog();
}

///////////////////////////////////////////////////////////////////////////////
// Send the breadcrumb array, then reboot.
// This is useful in figuring out how the kernel got into a bad state.
///////////////////////////////////////////////////////////////////////////////
void SendBreadcrumbsReboot(unsigned char code, unsigned int breadcrumbs)
{
	SendBreadcrumbs(code);
	Reboot(code);
}
#endif

///////////////////////////////////////////////////////////////////////////////
// Compute the checksum for the header of an outgoing message.
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
unsigned short AddReadPayloadChecksum(unsigned char *start, unsigned int length)
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
void SetBlockChecksum(unsigned int length, unsigned short checksum)
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
	MessageBuffer[6] = 0x03; // minor
	MessageBuffer[7] = 0x05; // patch
#if defined P12
	MessageBuffer[8] = 0x0C;
#elif defined P10
	MessageBuffer[8] = 0x0A;
#else
	MessageBuffer[8] = 0x01;
#endif
	// The AllPro and ScanTool devices need a short delay to switch from
	// sending to receiving. Otherwise they'll miss the response.
	ElmSleep();

	WriteMessage(MessageBuffer, 9, Complete);
}
