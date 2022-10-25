///////////////////////////////////////////////////////////////////////////////
// Code that will be useful in different types of kernels.
///////////////////////////////////////////////////////////////////////////////
#ifndef EXTERN
#define EXTERN extern
#endif

#define bool	_Bool
#define true	1
#define false	0

typedef unsigned char  uint8_t;
typedef unsigned short uint16_t;
typedef unsigned       uint32_t;
typedef int            int32_t;

#define P04
#ifndef DLC_CONFIGURATION
	#if defined P01 || defined P10 || defined P12
		#define DLC_CONFIGURATION			(*(unsigned char *)0x00FFF600)
		#define DLC_INTERRUPTCONFIGURATION	(*(unsigned char *)0x00FFF606)
		#define DLC_TRANSMIT_COMMAND		(*(unsigned char *)0x00FFF60C)
		#define DLC_TRANSMIT_FIFO			(*(unsigned char *)0x00FFF60D)
		#define DLC_STATUS					(*(unsigned char *)0x00FFF60E)
		#define DLC_RECEIVE_FIFO			(*(unsigned char *)0x00FFF60F)
		#if defined P10
			#define WATCHDOG1				(*(unsigned char *)0x00FFFA27)
			#define WATCHDOG2				(*(unsigned char *)0xFF800806)
		#elif defined P12
			#define WATCHDOG1				(*(unsigned char *)0x00FFFA55)
			#define WATCHDOG2				(*(unsigned char *)0x00FFFA21)
		#else // Default to P01
			#define WATCHDOG1				(*(unsigned char *)0x00FFFA27)
			#define WATCHDOG2				(*(unsigned char *)0x00FFD006)
		#endif
	#elif defined P04
		#define DLC_CONFIGURATION			(*(unsigned char *)0x00FFE800)
		#define DLC_INTERRUPTCONFIGURATION	(*(unsigned char *)0x00FFE800)
		#define DLC_TRANSMIT_COMMAND		(*(unsigned char *)0x00FFE800)
		#define DLC_TRANSMIT_FIFO			(*(unsigned char *)0x00FFE801)
		#define DLC_STATUS					(*(unsigned char *)0x00FFE800)
		#define DLC_RECEIVE_FIFO			(*(unsigned char *)0x00FFE801)
		#define WATCHDOG1					(*(unsigned char *)0x00FFFA27)
		#define WATCHDOG2					(*(unsigned char *)0x00FFC006)
	#endif
#endif

///////////////////////////////////////////////////////////////////////////////
//
// The linker needs to put these buffers after the kernel code, but before the
// system registers that are at the top of the RAM space.
//
// Usable RAM is four 4k blocks starting at FF8000.
// We reserve 10kb for the kernel, and start global variables at FFA800,
// leaving room for 6kb of globals. That's a little over 4k for the message
// buffer, 1k for the CRC table, and a bit less than 1k left over.
//
// If necessary we could probably overlay the CRC buffer atop the message
// buffer, since we don't need to start computing the CRC until after we
// process the incoming message that requested the CRC.
//
// Message buffer is larger than the max payload size (4k for the AVT) plus
// message header bytes (10 bytes header, 2 bytes checksum).

#define MessageBufferSize (1024+20)
EXTERN unsigned char __attribute((section(".kerneldata"))) MessageBuffer[MessageBufferSize];

// Code can add data to this buffer while doing something that doesn't work
// well, and then dump this buffer later to find out what was going on.
//#define BreadcrumbBufferSize 100
//EXTERN unsigned char __attribute((section(".kerneldata"))) BreadcrumbBuffer[BreadcrumbBufferSize];
//EXTERN unsigned __attribute((section(".kerneldata"))) breadcrumbs;

// Uncomment one of these to determine which way to use the breadcrumb buffer.
//#define RECEIVE_BREADCRUMBS
//#define TRANSMIT_BREADCRUMBS
//#define MODEBYTE_BREADCRUMBS

///////////////////////////////////////////////////////////////////////////////
// This needs to be called periodically to prevent the PCM from rebooting.
///////////////////////////////////////////////////////////////////////////////
void ScratchWatchdog();

///////////////////////////////////////////////////////////////////////////////
// Does what it says.
///////////////////////////////////////////////////////////////////////////////
void WasteTime();

///////////////////////////////////////////////////////////////////////////////
// All uses of this should be replaced with ElmSleep or VariableSleep.
///////////////////////////////////////////////////////////////////////////////
void LongSleepWithWatchdog();

///////////////////////////////////////////////////////////////////////////////
// ELM-based devices need a short pause between transmit and receive, otherwise
// they will miss the responses from the PCM. This function should be tuned to
// provide the right delay with AllPro and Scantool devices.
///////////////////////////////////////////////////////////////////////////////
void ElmSleep();

///////////////////////////////////////////////////////////////////////////////
// Sleep for a variable amount of time. This should be close to Dimented24x7's
// assembly-language implementation.
///////////////////////////////////////////////////////////////////////////////
void VariableSleep(unsigned int iterations);

///////////////////////////////////////////////////////////////////////////////
// All outgoing messages must be written into this buffer. The WriteMessage
// function will copy from this buffer to the DLC. Resetting the buffer should
// not really be necessary, but it helps to simplify debugging.
///////////////////////////////////////////////////////////////////////////////
void ClearMessageBuffer();

///////////////////////////////////////////////////////////////////////////////
// The 'breadcrumb' buffer helps give insight into what happened.
///////////////////////////////////////////////////////////////////////////////
void ClearBreadcrumbBuffer();

///////////////////////////////////////////////////////////////////////////////
// Message handlers
///////////////////////////////////////////////////////////////////////////////
void HandleReadMode35();
void HandleWriteRequestMode34();
void HandleWriteMode36();
void SendWriteSuccess(unsigned char code);

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
	AddSum = 8,
	Complete = Start | End,
} Segment;

///////////////////////////////////////////////////////////////////////////////
// Send the given bytes over the VPW bus.
// The DLC will append the checksum byte, so we don't have to.
// The message must be written into MessageBuffer first.
// This function will send 'length' bytes from that buffer onto the wire.
///////////////////////////////////////////////////////////////////////////////
void WriteMessage(unsigned char* start, unsigned short length, Segment segment);

// poll the dlc until the fifo has room
void WaitFiFo();
void FlushTXFiFo();

///////////////////////////////////////////////////////////////////////////////
// Send a byte - used by WriteMessage
///////////////////////////////////////////////////////////////////////////////
void WriteByte(unsigned char byte);

///////////////////////////////////////////////////////////////////////////////
// Read a VPW message into the 'MessageBuffer' buffer.
///////////////////////////////////////////////////////////////////////////////
int ReadMessage(unsigned char *completionCode, unsigned char *readState);

///////////////////////////////////////////////////////////////////////////////
// TODO: REMOVE.
// Copy the given buffer into the message buffer.
///////////////////////////////////////////////////////////////////////////////
void CopyToMessageBuffer(unsigned char* start, unsigned int length, unsigned int offset);

///////////////////////////////////////////////////////////////////////////////
// Send a message to explain why we're rebooting, then reboot.
///////////////////////////////////////////////////////////////////////////////
void Reboot(unsigned int value);

///////////////////////////////////////////////////////////////////////////////
// Send a tool-present message with 3 extra data bytes for debugging.
///////////////////////////////////////////////////////////////////////////////
void SendToolPresent(unsigned char b1, unsigned char b2, unsigned char b3, unsigned char b4);
void SendToolPresent2(unsigned int value);

///////////////////////////////////////////////////////////////////////////////
// Send the breadcrumb array as a payload of a tool-present message.
///////////////////////////////////////////////////////////////////////////////
void SendBreadcrumbs(unsigned char code);

///////////////////////////////////////////////////////////////////////////////
// Send the breadcrumb array, then reboot.
// This is useful in figuring out how the kernel got into a bad state.
///////////////////////////////////////////////////////////////////////////////
void SendBreadcrumbsReboot(unsigned char code, unsigned int breadcrumbs);

///////////////////////////////////////////////////////////////////////////////
// Compute the checksum for the header of an outgoing message.
///////////////////////////////////////////////////////////////////////////////
unsigned short StartBlockChecksum();

///////////////////////////////////////////////////////////////////////////////
// Copy the payload for a read request, while updating the checksum.
///////////////////////////////////////////////////////////////////////////////
unsigned short AddReadPayloadChecksum(unsigned char* start, unsigned int length);

///////////////////////////////////////////////////////////////////////////////
// Set the checksum for a data block.
///////////////////////////////////////////////////////////////////////////////
void SetBlockChecksum(unsigned int length, unsigned short checksum);

///////////////////////////////////////////////////////////////////////////////
// Get the version of the kernel. (Mode 3D, submode 00)
// Kernel Types:
// 01 = P01/P59
// 0A = P10
// 0C = P12
///////////////////////////////////////////////////////////////////////////////
void HandleVersionQuery();

///////////////////////////////////////////////////////////////////////////////
// Utility functions to compute CRC for memory ranges.
// TODO: move this into a new crc.h file.
///////////////////////////////////////////////////////////////////////////////
//extern int __attribute((section(".kerneldata"))) crcInitialized;
//extern uint8_t __attribute((section(".kerneldata"))) *crcStartAddress;
//extern int __attribute((section(".kerneldata"))) crcLength;
//extern int __attribute((section(".kerneldata"))) crcIndex;
//extern uint32_t __attribute((section(".kerneldata"))) crcRemainder;

//void crcInit(void);
//void crcReset(void);

//int crcIsStarted(uint8_t *message, int nBytes);
//int crcIsDone(uint8_t *message, int nBytes);
//void crcStart(uint8_t *message, int nBytes);
//uint32_t crcGetResult();
//void crcProcessSlice();

///////////////////////////////////////////////////////////////////////////////
// Write data to flash memory.
//
// Return value is 0 on success, or the value of the flash status register if
// there is a flash error.
///////////////////////////////////////////////////////////////////////////////
//unsigned char WriteToFlash(const unsigned start, const unsigned length, unsigned char *data, int testWrite);
