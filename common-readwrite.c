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

typedef void(*EntryPoint)();

///////////////////////////////////////////////////////////////////////////////
// Handle a mode-36 write.
///////////////////////////////////////////////////////////////////////////////
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

	// Validate range
	if ((length > 4096) || (start < 0xFFA000) || (start > 0xFFB000))
	{
		MessageBuffer[0] = 0x6D;
		MessageBuffer[1] = 0xF0;
		MessageBuffer[2] = 0x10;
		MessageBuffer[3] = 0x7F;
		MessageBuffer[4] = 0x36;
		WriteMessage(MessageBuffer, 5, Complete);
		return;
	}

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

	// Send response
	MessageBuffer[0] = 0x6D;
	MessageBuffer[1] = 0xF0;
	MessageBuffer[2] = 0x10;
	MessageBuffer[3] = 0x76;

	WriteMessage(MessageBuffer, 4, Complete);
		
	// Execute
	if (command == 0x80)
	{
		EntryPoint entryPoint = (EntryPoint)start;
		entryPoint();
	}
}
