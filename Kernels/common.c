///////////////////////////////////////////////////////////////////////////////
// Code that will be useful for more than one type of kernel.
///////////////////////////////////////////////////////////////////////////////

#include "common.h"

#define COP1 0xFFFA27
#define COP2 0xFFD006
#define toolid 0xF0
#define InputBufferSize 20

const int DLC_Configuration = 0xFFF600;
const int DLC_InterruptConfiguration = 0xFFF606;
const int DLC_Transmit_Command = 0xFFF60C;
const int DLC_Transmit_FIFO = 0xFFF60D;
const int DLC_Receive_Status = 0xFFF60E;
const int DLC_Receive_FIFO = 0xFFF60F;

asm("asm_DLC_Configuration = 0xFFF600");
asm("asm_DLC_InterruptConfiguration = 0xFFF606");
asm("asm_DLC_Transmit_Command = 0xFFF60C");
asm("asm_DLC_Transmit_FIFO = 0xFFF60D");
asm("asm_DLC_Receive_Status = 0xFFF60E");
asm("asm_DLC_Receive_FIFO = 0xFFF60F");

char ResetResponseMessage[] = { 0x6C, 0xF0, 0x10, 0x60 };

void Startup()
{
	// Disable peripheral interrupts
	asm("ORI #0x700, %SR");
	ScratchWatchdog();

	// Should be able to do these with WriteByte - will confirm after these are proven.

	// Disable DLC interrupts
	asm("mov.b 0x00, (asm_DLC_InterruptConfiguration).l");

	//WasteTime();
	LongSleepWithWatchdog(10);
	asm("move.b 0x03, (asm_DLC_Transmit_Command).l");
	asm("move.b 0x00, (asm_DLC_Transmit_FIFO).l");
}

void WriteByte(int address, char value)
{
	*((char volatile *)address) = value;
}

char ReadByte(int address)
{
	return *((char volatile *)address);
}

int IsStatusComplete()
{
	char status = ReadByte(DLC_Receive_Status);

	// Unwrapped to make the assembly easier to follow...
	// return (status & 0xE0) == 0x40;
	status = status & 0xE0;
	if (status == 0x40)
	{
		return 1;
	}

	return 0;
	
}

int ReadMessage()
{
	int bytesReceived = 0;
	for (int i = 0; i < 0x300000; i++)
	{		
		ScratchWatchdog();
		if (IsStatusComplete())
		{
			inputBuffer[bytesReceived] = ReadByte(DLC_Receive_FIFO);
			bytesReceived++;
			continue;
		}
		else
		{
			if (bytesReceived == 0)
			{
				continue;
			}
		}

		// Read the last byte.
		inputBuffer[bytesReceived] = ReadByte(DLC_Receive_FIFO);

		// If unknown priority, try again.
		if (inputBuffer[0] != 0x6C)
		{
			bytesReceived = 0;
			continue;
		}

		// If unknown destination, try again;
		if (inputBuffer[1] != 0x10)
		{
			bytesReceived = 0;
			continue;
		}

		// Let the caller process this message.
		return bytesReceived;
	}

	// Too many retries. Pretend we got a reboot request
	inputBuffer[0] = 0x6C;
	inputBuffer[1] = 0x10;
	inputBuffer[2] = 0xF0;
	inputBuffer[3] = 0x20;
	return 4;
}

int ProcessResetRequest()
{
	// TODO: confirm that it is really a reset message
	// return 0 if it is not;

	SendMessage(ResetResponseMessage, 4);
	return 1;
}

void Reboot()
{
	// Reset peripherals
	asm("reset");

	// Wait for watchdog to trigger a reboot
	for (;;) {}
}

void WriteShort(int address, short value)
{
	*((short volatile*)address) = value;
}

void SendMessage(char* message, int length)
{
	short start = 0x1400 + message[0];
	WriteShort(DLC_Transmit_Command, start);
	WasteTime();

	for (int index = 1; index < length - 1; index++)
	{
		WriteByte(DLC_Transmit_FIFO, message[index]);
		WasteTime();
	}

	short end = 0xC000 + message[length - 1];
	WriteShort(DLC_Transmit_Command, end);

	LongSleepWithWatchdog(16);
}

void SendToolPresent()
{
	//char message[] = { 0x8C, 0xFE, 0x10, 0x3F };
	//SendMessage(message, 4);

	asm("CLR %d0");		//	*Clear D0
	asm("move.w #0x148C, %d0");//		*Load command to send first data byte
//	asm("add.w 0x006C, D0");//		*Load first byte of message
	asm("move %d0, (asm_DLC_Transmit_Command).l");	//*save to DLC
	WasteTime();
	asm("move.b #0xFE, (asm_DLC_Transmit_FIFO).l");
	WasteTime();
	asm("move.b #0x10, (asm_DLC_Transmit_FIFO).l");
	WasteTime();
	asm("move.b #0x3F, (asm_DLC_Transmit_FIFO).l");
	WasteTime();

	asm("move.w 0x0C12, %d0");
	asm("move %d0, (asm_DLC_Transmit_Command).l");
	LongSleepWithWatchdog(16);


///		BSR LBL_DELAY		*Provide delay
//		*
	//	MOVE.B D1, DLCTXFIFO	*Load Target to Tx buffer
//		BSR LBL_DELAY		*Provide delay
//		*
//		MOVE.B #$10, DLCTXFIFO	*Load source to Tx buffer
//		BSR LBL_DELAY		*Provide delay
//		*
//		MOVE.B #$7F, DLCTXFIFO	*Load mode to Tx buffer
//		BSR LBL_DELAY		*Provide delay
//		*
//		CLR D0			*Clear D0
//		MOVE #$0C00, D0		*Load command for last byte to xmit
//		ADD #$0012, D0		*Load with last byte
//		MOVE D0, DLCTXCMD	*Send last byte and command byte to DLC
//		MOVE #10, D1		*Load D1 with # of cycles to wait
//		BSR LBL_LONGDELAY	*Long dela
}

void ScratchWatchdog() 
{
	//*((char*)COP1) = 0x55;
	//*((char*)COP1) = 0xAA;
	asm("move.b 0x55, (0xFFFA27).l");
	asm("move.b 0xAA, (0xFFFA27).l");

	// No sure how to use the COP2 preprocessor symbol here, but that would be cleaner
	asm("bset #7, (0xFFD006).l");
	asm("bclr #7, (0xFFD006).l");
}

void LongSleepWithWatchdog(int time)
{
	/*
	for (int i = 0; i < 10 * 1000; i++)
	{
		ScratchWatchdog();
		for (int j = 0; j < 10; j++)
		{
			WasteTime();
		}
	}
	*/

	for (int iterations = 0; iterations < time; iterations++)
	{
		for (int i = 0; i < 256; i++)
		{
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

void WasteTime()
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

char inputBuffer[InputBufferSize];
