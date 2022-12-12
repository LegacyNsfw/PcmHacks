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

///////////////////////////////////////////////////////////////////////////////
// Code to handle read and write messages.
///////////////////////////////////////////////////////////////////////////////
#define EXTERN
#include "common.h"

unsigned char __attribute((section(".kerneldata"))) MessageBuffer[MessageBufferSize];

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

void WaitTXFiFo()
{
  unsigned char status;
  
  // Status 2 means the transmit buffer is almost full.
  // In that case, pause until there's room in the buffer.
  status = DLC_STATUS & 0x03;
  // TODO: try looping around 0x02 (almost full) rather than 0x03 (full)
  int loopCount = 0;
  while ((status > 0x01 ) && loopCount < 250)
  {
    loopCount++;

    // With max iterations at 25, we get some 2s and 3s in the loop counter.
    for (int iterations = 0; iterations < 50; iterations++)
    {
      ScratchWatchdog();
      WasteTime();
    }

    ScratchWatchdog();
    status = DLC_STATUS & 0x03;
  }
  ElmSleep(); // needed for stability on the P04 DLC
}

void FlushTXFiFo()
{
  unsigned char status;
  do {
    status = DLC_STATUS & 0x03;
    WasteTime();
  } while (status);
}

///////////////////////////////////////////////////////////////////////////////
// Send the given bytes over the VPW bus.
// The DLC will append the checksum byte, so we don't have to.
// The message must be written into MessageBuffer first.
// This function will send 'length' bytes from that buffer onto the wire.
///////////////////////////////////////////////////////////////////////////////
void WriteMessage(unsigned char *message, unsigned short length, Segment segment)
{
  ScratchWatchdog();

  unsigned short checksum;
  int stopUsing = 0;
  unsigned short last = length - 1; // the DLC needs some funky stuff for the last byte. Cheaper to store it than re-calculate it.

  /* -------FRAME START ONLY ------------*/
	if ((segment & Start) != 0)
	{
    last = length; // no last byte to consider in this frame
		DLC_TRANSMIT_COMMAND = 0x14;
	}
  /* ----------------------------------- */

  /* ----- SEND PAYLOAD -------*/
  checksum = StartBlockChecksum(); // the buffer must still contain the first segment, so checksum can be calculated in the second call for a block.
  // Send message
  for (int index = 0; index < last; index++)
  {
    DLC_TRANSMIT_FIFO = message[index];
    checksum += message[index];
    ScratchWatchdog();
    WaitTXFiFo();
  }
  /* ----------------------------------- */

  /* ------- BLOCK CHECKSUM, CLOSE TRANSMISSION -------*/
  // What is the last byte? is it message, or 
  if ((segment & AddSum) != 0) 
  {
    checksum += message[last];
    DLC_TRANSMIT_FIFO = message[last];
    WaitTXFiFo();
    DLC_TRANSMIT_FIFO = checksum >> 8;
    WaitTXFiFo();
    DLC_TRANSMIT_COMMAND = 0x0C; // must set command to 0C to tell the DLC the next byte is the last.
    ElmSleep();
    DLC_TRANSMIT_FIFO = checksum;
    WaitTXFiFo();
    DLC_TRANSMIT_COMMAND = 0x03;
    WasteTime();
    DLC_TRANSMIT_FIFO = 0x00;
    FlushTXFiFo();
  }
  
  if ((segment & End) != 0)
	{
    // Send last byte
    DLC_TRANSMIT_FIFO = message[last];
    WaitTXFiFo();
    DLC_TRANSMIT_COMMAND = 0x03;
    WasteTime();
    DLC_TRANSMIT_FIFO = 0x00;
    FlushTXFiFo();

    /* ------------- CLEANUP -------------*/
    ClearMessageBuffer();
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
	unsigned char loopCount = 200;
	while ((status == 0x02 || status == 0x03) && loopCount--)
	{
		LongSleepWithWatchdog();
		status = DLC_STATUS & 0x03;
	}
	DLC_TRANSMIT_FIFO = byte;
}

///////////////////////////////////////////////////////////////////////////////
// Compute the checksum for the header of an outgoing message.
///////////////////////////////////////////////////////////////////////////////
unsigned short StartBlockChecksum()
{
	unsigned short checksum = 0;
	for (int index = 4; index < 10; index++)
	{
		checksum += MessageBuffer[index];
	}
	return checksum;
}

///////////////////////////////////////////////////////////////////////////////
// Read a VPW message into the 'MessageBuffer' buffer.
///////////////////////////////////////////////////////////////////////////////
int ReadMessage(unsigned char *completionCode, unsigned char *readState)
{
  ScratchWatchdog();
  unsigned char status;

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
// Send a message to explain why we're rebooting, then reboot.
///////////////////////////////////////////////////////////////////////////////
void Reboot(unsigned int value)
{
	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x60;
	MessageBuffer[4] = (unsigned char)((value & 0xFF000000) >> 24);
	MessageBuffer[5] = (unsigned char)((value & 0x00FF0000) >> 16);
	MessageBuffer[6] = (unsigned char)((value & 0x0000FF00) >> 8);
	MessageBuffer[7] = (unsigned char)((value & 0x000000FF) >> 0);
	WriteMessage(MessageBuffer, 8, Complete);

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

  // This loop runs out quickly, to force the PCM to reboot, to speed up testing.
  // The real kernel should probably loop for a much longer time (possibly forever),
  // to allow the app to recover from any failures.
  // If we choose to loop forever we need a good story for how to get out of that state.
  // Pull the PCM fuse? Give the app button to tell the kernel to reboot?
  //for(int iterations = 0; iterations < 10000; iterations++)
  while (1)
  {
    ScratchWatchdog();
    WasteTime();

    char completionCode = 0xFF;
    char readState = 0xFF;
    int length = ReadMessage(&completionCode, &readState);
    if (length == 0)
    {
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
      Reboot(0xEE);
    }

    // Process a mode-35 read.
    // 6D F0 10  36 01  AA AA  BB BB BB  CC CC CC CC ..... DD DD
    // AA = size, BB = Address, CC = Payload, DD = Block sum
    if (MessageBuffer[3] == 0x35)
    {

      unsigned length = (MessageBuffer[5] << 8) + MessageBuffer[6];
      unsigned start = (MessageBuffer[7] << 16) + (MessageBuffer[8] << 8) + MessageBuffer[9];

      // Send the payload
      MessageBuffer[0] = 0x6D;
      MessageBuffer[1] = 0xF0;
      MessageBuffer[2] = 0x10;
      MessageBuffer[3] = 0x36;
      MessageBuffer[4] = 0x01;
      MessageBuffer[5] = length >> 8;
      MessageBuffer[6] = length;
      MessageBuffer[7] = start >> 16;
      MessageBuffer[8] = start >> 8;
      MessageBuffer[9] = start;

	    ElmSleep();
	    WriteMessage(MessageBuffer, 10, Start);
	    WriteMessage((char*)start, length, AddSum); // its assumed AddSum will not start, and will includes last byte and DLC close
      continue;
    }
    
    // Support the version request used by the PCM Hammer app
    if (MessageBuffer[3] == 0x3D && MessageBuffer[4] == 0x00)
    {
      MessageBuffer[0] = 0x6C;
      MessageBuffer[1] = 0xF0;
      MessageBuffer[2] = 0x10;
      MessageBuffer[3] = 0x7D;
      MessageBuffer[4] = 0x00;
      MessageBuffer[5] = 0x08;
      MessageBuffer[6] = 0x02;
      MessageBuffer[7] = 0x04;
      MessageBuffer[8] = 0xFC;
      WriteMessage(MessageBuffer, 9, Complete);
      continue;
    }

  }

  Reboot(0xFF);
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
// This needs to be called periodically to prevent the PCM from rebooting.
///////////////////////////////////////////////////////////////////////////////

void ScratchWatchdog() //P04 edition
{
  WATCHDOG1 = 0x55;
  WATCHDOG1 = 0xAA;
  WATCHDOG2 &= 0x7F;
  WATCHDOG2 |= 0x80;
}

