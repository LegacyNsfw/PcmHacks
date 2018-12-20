///////////////////////////////////////////////////////////////////////////////
// Code to handle read and write messages.
///////////////////////////////////////////////////////////////////////////////
#include "common.h"

///////////////////////////////////////////////////////////////////////////////
// Process a mode-35 read.
///////////////////////////////////////////////////////////////////////////////
void HandleReadMode35()
{
	unsigned length = MessageBuffer[5];
	length <<= 8;
	length |= MessageBuffer[6];

	unsigned start = MessageBuffer[7];
	start <<= 8;
	start |= MessageBuffer[8];
	start <<= 8;
	start |= MessageBuffer[9];

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

	// Give the tool time to proces that message (especially the AllPro)
	LongSleepWithWatchdog();

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

	WriteMessage((char*)start, length, Middle);

	WriteMessage((char*)&checksum, 2, End);
}

///////////////////////////////////////////////////////////////////////////////
// Handle a mode-34 request for permission to write.
///////////////////////////////////////////////////////////////////////////////
void HandleWriteRequestMode34()
{
	unsigned length = MessageBuffer[5];
	length <<= 8;
	length |= MessageBuffer[6];

	unsigned start = MessageBuffer[7];
	start <<= 8;
	start |= MessageBuffer[8];
	start <<= 8;
	start |= MessageBuffer[9];

	if ((length > 4096) || (start != 0xFFA000))
	{
		MessageBuffer[0] = 0x6C;
		MessageBuffer[1] = 0xF0;
		MessageBuffer[2] = 0x10;
		MessageBuffer[3] = 0x7F;
		MessageBuffer[4] = 0x34;

		WriteMessage(MessageBuffer, 5, Complete);
		return;
	}

	MessageBuffer[0] = 0x6C;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x74;
	MessageBuffer[4] = 0x00;
	WriteMessage(MessageBuffer, 5, Complete);
}



///////////////////////////////////////////////////////////////////////////////
// Handle a mode-36 write.
///////////////////////////////////////////////////////////////////////////////
void SendWriteSuccess(unsigned char code)
{
	// Send response
	MessageBuffer[0] = 0x6D;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x76;
	MessageBuffer[4] = code;

	WriteMessage(MessageBuffer, 4, Complete);
}

void SendWriteFail(unsigned char callerError, unsigned char flashError)
{
	MessageBuffer[0] = 0x6D;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x7F;
	MessageBuffer[4] = 0x36;
	MessageBuffer[5] = callerError;
	MessageBuffer[6] = flashError;
	WriteMessage(MessageBuffer, 7, Complete);
}

typedef void(*EntryPoint)();

void HandleWriteMode36()
{
	unsigned char command = MessageBuffer[4];

	unsigned length = MessageBuffer[5];
	length <<= 8;
	length |= MessageBuffer[6];

	unsigned start = MessageBuffer[7];
	start <<= 8;
	start |= MessageBuffer[8];
	start <<= 8;
	start |= MessageBuffer[9];

	// Compute checksum
	unsigned short checksum = 0;
	for (int index = 4; index < length + 10; index++)
	{
		if (index % 1024 == 0)
		{
			ScratchWatchdog();
		}

		checksum = checksum + MessageBuffer[index];
	}

	// Validate checksum
	unsigned short expected = (MessageBuffer[10 + length] << 8) | MessageBuffer[10 + length + 1];
	if (checksum != expected)
	{
		MessageBuffer[0] = 0x6D;
		MessageBuffer[1] = 0xF0;
		MessageBuffer[2] = 0x10;
		MessageBuffer[3] = 0x7F; 
		MessageBuffer[4] = 0x36;
		MessageBuffer[5] = (char)((checksum & 0xFF00) >> 8);
		MessageBuffer[6] = (char)(checksum & 0x00FF);
		MessageBuffer[7] = (char)((expected & 0xFF00) >> 8);
		MessageBuffer[8] = (char)(expected & 0x00FF);
		MessageBuffer[9] = (char)((length & 0xFF00) >> 8);
		MessageBuffer[10] = (char)(length & 0x00FF);

		WriteMessage(MessageBuffer, 11, Complete);
		return;
	}

	if (start & 1)
	{
		// Misaligned data.
		SendWriteFail(0xFF, 00);
		return;
	}

	if ((start >= 0xFF8000) && (start+length <= 0xFFCDFF))
	{
		// Copy content	
		unsigned int address = 0;
		for (int index = 0; index < length; index++)
		{
			if (index % 50 == 1)
			{
				ScratchWatchdog();
			}

			address = start + index;
			*((unsigned char*)address) = MessageBuffer[10 + index];
		}

		// Notify the tool that the write succeeded.
		SendWriteSuccess(command);

		// Let the success message flush.
		LongSleepWithWatchdog();		

		// Execute if requested to do so.
		if (command == 0x80)
		{
			EntryPoint entryPoint = (EntryPoint)start;
			entryPoint();
		}
	}
	else if ((start >= 0x8000) && ((start+length) <= 0x20000))
	{		
		// Write to flash memory.
		char flashError = WriteToFlash(length, start, &MessageBuffer[10], command == 0x44);

		// Send success or failure.
		if (flashError == 0)
		{
			SendWriteSuccess(command);
		}
		else
		{
			SendWriteFail(0, flashError);
		}
	}
	else
	{
		// Bad memory range
		SendWriteFail(0xEE, 0x00);
	}
}
