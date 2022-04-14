///////////////////////////////////////////////////////////////////////////////
// Code to handle read and write messages.
///////////////////////////////////////////////////////////////////////////////
#include "common.h"

///////////////////////////////////////////////////////////////////////////////
// Process a mode-35 read.
///////////////////////////////////////////////////////////////////////////////
void HandleReadMode35()
{
	unsigned length = (MessageBuffer[5] << 8) + MessageBuffer[6];
	unsigned start = (MessageBuffer[7] << 16) + (MessageBuffer[8] << 8) + MessageBuffer[9];
	// TODO: Validate the start address and length, fail if unreasonable.

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
	WriteMessage((char*)start, length, End|AddSum);
}

///////////////////////////////////////////////////////////////////////////////
// Handle a mode-34 request for permission to write.
///////////////////////////////////////////////////////////////////////////////
void HandleWriteRequestMode34()
{
	unsigned length = (MessageBuffer[5] << 8) + MessageBuffer[6];
	unsigned start = (MessageBuffer[7] << 16) + (MessageBuffer[8] << 8) + MessageBuffer[9];

	if (length > 4096)
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

	WriteMessage(MessageBuffer, 5, Complete);
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
	unsigned length = (MessageBuffer[5] << 8) + MessageBuffer[6];
	unsigned start = (MessageBuffer[7] << 16) + (MessageBuffer[8] << 8) + MessageBuffer[9];

	// Compute checksum
	unsigned short checksum = 0;
	for (unsigned int index = 4; index < length + 10 ; index++) // vpw header = 10 bytes offset from payload length
	{
		if (index % 1024 == 0)
		{
			ScratchWatchdog();
		}
		checksum += MessageBuffer[index];
	}

	// Validate checksum
	unsigned short expected = (MessageBuffer[10 + length] << 8) | MessageBuffer[10 + length + 1];
	if (checksum != expected)
	{
		/*unsigned char tmp = MessageBuffer[1];
		MessageBuffer[1] = MessageBuffer[2];
		MessageBuffer[2] = tmp;
		WriteMessage(MessageBuffer, 10 + length + 1, Complete);*/

		MessageBuffer[0] = 0x6D;
		MessageBuffer[1] = 0xF0;
		MessageBuffer[2] = 0x10;
		MessageBuffer[3] = 0x7F;
		MessageBuffer[4] = 0x36;
		MessageBuffer[5] = checksum >> 8;
		MessageBuffer[6] = checksum;
		MessageBuffer[7] = expected >> 8;
		MessageBuffer[8] = expected;
		MessageBuffer[9] = length >> 8;
		MessageBuffer[10] = length;

		WriteMessage(MessageBuffer, 11, Complete);

		return;
	}

	if (start & 1)
	{
		// Misaligned data.
		SendWriteFail(0xBB, 00);
		return;
	}

	if ((start >= 0xFF8000) && (start + length <= 0xFFCDFF))
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

		// Execute if requested to do so.
		if (command == 0x80)
		{
			EntryPoint entryPoint = (EntryPoint)start;
			entryPoint();
		}
	}
	else
	{
		char flashError = WriteToFlash(length, start, &MessageBuffer[10], command == 0x44);

		if (flashError == 0)
		{
			SendWriteSuccess(command);
		}
		else
		{
			SendWriteFail(0, flashError);
		}
	}
}
