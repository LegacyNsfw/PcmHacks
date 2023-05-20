| GM '0411 Kernel for PCMHammer
| 2018-2023 Antus / PCMHacking.net
| 2023-03-25 @21:00 Gampy <@pcmhacking.net>
| ===========================================================================
|
| C directives will only work if the source filename is .S, yup, capital S.
| That is the extension gnu has asscoaited the assembler with and it is case sensitive.
| When using gcc.exe with -x assembler option and anyother extension, the C directives will fail.
| We need to use gcc.exe for C directives, as.exe does not provide for them.
|

| Include Common elements
| These elements cannot be a sub of MainLoop, see note in Kernel.S MainLoop
#include "Common-Assembly.h"

| Flash stuff
.equ SIM_CSBAR0,            0xFA4C
.equ SIM_CSORBT,            0xFA4A
#if defined P12
  .equ SIM_CSOR0,           0xFA7E
#else
  .equ SIM_CSBARBT,         0xFA48
  .equ SIM_CSOR0,           0xFA4E
#endif
|
.equ READ_ARRAY_COMMAND,    0xFFFF
.equ ERASE_RESUME_CONFIRM,  0xD0D0
.equ SIGNATURE_COMMAND,     0x9090
.equ READ_STATUS_REGISTER,  0x7070
.equ CLEAR_STATUS_REGISTER, 0x5050
.equ PROGRAM_COMMAND,       0x4040
.equ ERASE_COMMAND,         0x2020
.equ AMD_COMMAND_ADDRESS_A, 0xAAA
.equ AMD_COMMAND_ADDRESS_B, 0x554
.equ AMD_COMMAND_UNLOCK_A,  0xAAAA
.equ AMD_COMMAND_UNLOCK_B,  0x5555
.equ AMD_COMMAND_RESET,     0xF0F0
|
.equ INTEL_ID,              0x0089
.equ AMD_ID,                0x0001

| CRC32
.equ POLYNOMIAL,         0x4C11DB7

| Modes supported shared in Common-Assembly.S
|.equ KernelID_3D00                    | Moved to Common-Assembly.s
.equ FlashID_3D01,       0x3D01        | Return Flash chip ID
.equ CRC_3D02,           0x3D02        | Return CRC for range
.equ OsID_3D03,          0x3D03        | Return OsID
|.equ Halt_20                          | Moved to Common-Assembly.s
|.equ Mode_34                          | Moved to Common-Assembly.s
.equ Mode_35,            0x35          | Send data from PCM to Tool
|.equ Mode_36                          | Moved to Common-Assembly.s

start:
    ori     #0x700, %sr                | Disable Interrupts

    move.b  #0x03, (J1850_Command).l   | Flush frame except for completion code
    move.b  #0x00, (J1850_TX_FIFO).l   | Initiate transfer

wait01:
    bsr.w   ResetWatchdog              | Scratch the dog
    bsr.w   WasteTime                  | Twiddle thumbs
    move.b  (J1850_Status).l, %d0      | Get the VPW status
    andi.b  #0xE0, %d0                 | Mask Receive FIFO Status Field (RFS) register 1110 0000
    cmpi.b  #0xE0, %d0                 | Check for "completion code only, at head of buffer" message 1110 0000
    bne.b   wait01                     | Not ready? wait and retry
    move.b  (J1850_RX_FIFO).l, %d0     | Strip off the completion code

#if defined P12
    movea.l #Mode36Reply, %a0          | Reply Success to mode 3680, only P12 needs this.
    move.l  #6, %d0                    | It's 6 bytes long
    bsr.w   VPWSend                    | Send message
    bsr.w   WasteTime                  | Twiddle thumbs
#endif

MainLoop:
    movea.l #MessageBuffer, %a0        | Address the MessageBuffer
    clr.w   3(%a0)                     | Clear last command, prevent repeating it
    bsr.w   ResetWatchdog              | Scratch the dog
    bsr.w   VPWReceive                 | Wait for and read next packet
    bsr.w   WasteTime                  | Twiddle thumbs
    cmpi.w  #KernelID_3D00, 3(%a0)     | Is it mode 0x3D 00 Get Kernel ID (non-standard extension)
    beq.w   ProcessKernelID            | Process it
    cmpi.w  #FlashID_3D01, 3(%a0)      | Is it mode 0x3D 01 Get Flash Chip ID (non-standard extension)
    beq.w   ProcessFlashID             | Process it
    cmpi.w  #CRC_3D02, 3(%a0)          | Is it mode 0x3D 02 Get Flash Sector CRC (non-standard extension)
    beq.w   ProcessCRC                 | Process it
    cmpi.w  #OsID_3D03, 3(%a0)         | Is it mode 0x3D 03 Get Os ID (non-standard extension)
    beq.w   ProcessOsID                | Process it
    cmpi.b  #Mode_34, 3(%a0)           | Is it mode 0x34 (Tool asking PCM, ok to send X bytes to address)
    beq.w   ProcessMode34              | Process it
    cmpi.b  #Mode_35, 3(%a0)           | Is it mode 0x35 (Tool asking PCM to send data from Flash)
    beq.w   ProcessMode35              | Process it
    cmpi.b  #Mode_36, 3(%a0)           | Is it mode 0x36 (Tool sending data to write, either to RAM or Flash)
    beq.w   ProcessMode36              | Process it
    cmpi.b  #Halt_20, 3(%a0)           | Is it mode 0x20 (return to normal comms)
    bne.w   MainLoop                   | No more options, next packet or reboot

| =============== S U B R O U T I N E =======================================
Reboot:
    movea.l #Mode60Reply, %a0          | Address Mode 20 reply (Sends a mode 60 (20+40) message)
    move.l  #4, %d0                    | It's 4 bytes long
    bsr.w   VPWSend                    | Send it
    bsr.w   WasteTime                  | Twiddle thumbs
    reset                              | Reset External Devices

EndlessLoop:                           | Kills pcm, lets COPs expire
    bra.b   EndlessLoop

| =============== S U B R O U T I N E =======================================
ResetWatchdog:
    move.b  #0x55, (COP1).l            | Reset COP1
    move.b  #0xAA, (COP1).l            | Reset COP1
    eori.b  #0x80, (COP2).l            | Reset COP2 ... COP2 ^= 0x80
    rts

| =============== S U B R O U T I N E =======================================
WasteTime:
    nop                                | No Operation
    nop
    nop
    nop
    rts

| =============== S U B R O U T I N E =======================================
ProcessKernelID:
    movea.l #KernelIDReply, %a0        | Address the KernelIDReply buffer
#if defined P01
    move.b  #0x01, 8(%a0)              | P01, P59
#elif defined P04
    move.b  #0x04, 8(%a0)              | P04
#elif defined P08
    move.b  #0x08, 8(%a0)              | P08
#elif defined P10
    move.b  #0x0A, 8(%a0)              | P10
#elif defined P12
    move.b  #0x0C, 8(%a0)              | P12
#elif defined E54
    move.b  #0x36, 8(%a0)              | E54
#endif
    move.l  #9, %d0                    | It's 9 bytes long
    bsr.w   VPWSend                    | Send it
    jmp     MainLoop                   | Return to MainLoop

| =============== S U B R O U T I N E =======================================
|
| TODO: Have request supply address
|
ProcessOsID:
#if defined P04
    movea.l #0x7FFFA, %a1              | OsID location in Flash
#elif defined P08
    movea.l #0x8000, %a1               | OsID location in Flash
#elif defined P10
    movea.l #0x52E, %a1                | OsID location in Flash
#elif defined P12
    movea.l #0x8004, %a1               | OsID location in Flash
#elif defined E54
    movea.l #0x20004, %a1              | OsID location in Flash
#else
    movea.l #0x504, %a1                | OsID location in Flash
#endif
    movea.l #OsIDReply, %a0            | Address the OsIDReply buffer
    move.l  (%a1), 5(%a0)              | Move OsID into buffer
    move.l  #9, %d0                    | It's 9 bytes long
    bsr.w   VPWSend                    | Send it
    jmp     MainLoop                   | Return to MainLoop

| =============== S U B R O U T I N E =======================================
|
| Process Flash ID Request
|
| Uses
|   FlashIDReply
|   a0 - Scratch 
|   a1 - AMD Command Address A
|   a2 - AMD Command Address B
|   d0 - Flash ID
|   d1 - Manufacture ID
|
ProcessFlashID:
#if defined P12
    move.w  #0xF322, (SIM_CSOR0).w     | 0xFA7E
#else
    move.w  #0x0007, (SIM_CSBAR0).w    | 0xFA4C Set CS registers
    move.w  #0x6820, (SIM_CSORBT).w    | 0xFA4A
    move.w  #0x7060, (SIM_CSOR0).w     | 0xFA4E
#endif
    movea.w #0, %a0                    | Address 0
    move.w  #SIGNATURE_COMMAND, (%a0)  | Command Intel Read ID State (0x9090)
    move.w  (%a0), %d1                 | Move MFG ID to d1
    movea.w #2, %a0                    | Address 2
    move.w  (%a0), %d0                 | Move Flash ID to d0
    movea.w #0, %a0                    | Address 0
    move.w  #READ_ARRAY_COMMAND, (%a0) | Command Intel Read Array State (0xFFFF)

    cmp.w   #INTEL_ID, %d1             | Intel ID is 0x0089
    beq.w   ProcessFlashIDFound        | Jump found if Intel

    movea.w #AMD_COMMAND_ADDRESS_A, %a1  | AMD Command Address A (0xAAA)
    movea.w #AMD_COMMAND_ADDRESS_B, %a2  | AMD Command Address B (0x554)
    move.w  #AMD_COMMAND_UNLOCK_A, (%a1) | Command AMD Unlock A  (0xAAAA)
    move.w  #AMD_COMMAND_UNLOCK_B, (%a2) | Command AMD Unlock B  (0x5555)
    move.w  #SIGNATURE_COMMAND, (%a1)  | Command AMD Autoselect  (0x9090)
    move.w  (%a0), %d1                 | Move MFG ID to d1
    movea.w #2, %a0                    | Address 2
    move.w  (%a0), %d0                 | Move Flash ID to d0
    movea.w #0, %a0                    | Address 0
    move.w  #AMD_COMMAND_RESET, (%a0)  | Command AMD Reset (0xF0F0)

    cmp.w   #AMD_ID, %d1               | Amd ID is 0x0001
    beq.w   ProcessFlashIDFound        | Jump found if AMD

    clr.w   %d0                        | Not found
    clr.w   %d1                        | Not found

ProcessFlashIDFound:
#if defined P12
    move.w  #0xA332, (SIM_CSOR0).w     | 0xFA7E
#else
    move.w  #0x1060, (SIM_CSOR0).w     | 0xFA4E
#endif
    movea.l #FlashIDReply, %a0         | Address FlashIDReply buffer
    move.w  %d1, 5(%a0)                | Move MFG ID to FlashIDReply buffer
    move.w  %d0, 7(%a0)                | Move Flash ID to FlashIDReply buffer
    move.l  #9, %d0                    | It's 9 bytes long
    bsr.w   VPWSend                    | Send it
    jmp     MainLoop

| =============== S U B R O U T I N E =======================================
ProcessCRC:
    clr.l   %d0                        | Clear d0
    or.b    MessageBuffer + 8, %d0     | First address byte
    lsl.l   #8, %d0                    | Logical Shift Left
    or.b    MessageBuffer + 9, %d0     | Second address byte
    lsl.l   #8, %d0                    | Logical Shift Left
    or.b    MessageBuffer + 10, %d0    | Third address byte
    movea.l %d0, %a1                   | Move address to a1

    clr.l   %d0                        | Clear d0
    or.b    MessageBuffer + 5, %d0     | First length byte
    lsl.l   #8, %d0                    | Logical Shift Left
    or.b    MessageBuffer + 6, %d0     | Second length byte
    lsl.l   #8, %d0                    | Logical Shift Left
    or.b    MessageBuffer + 7, %d0     | Third length byte
    movea.l %d0, %a2                   | Move length to a2

    lea     (%a1, %a2.l), %a2          | for (int byte = 0; byte < nBytes; ++byte)
    moveq   #0x18, %d2                 | WIDTH-8 to d2
    clr.l   %d0                        | unsigned int remainder = 0

ProcessCRCMainLoop:
    bsr.w   ResetWatchdog              | Scratch the dog
    move.b  (%a1)+, %d1                | message[byte] to register d1
    lsl.l   %d2, %d1                   | Shift left copy of message[byte] by (WIDTH-8)
    eor.l   %d1, %d0                   | xor in to remainder
    moveq   #8, %d1                    | for (unsigned char bit = 8; bit > 0; --bit)

ProcessCRCBitLoop:
    tst.l   %d0                        | if (remainder & TOPBIT)
    add.l   %d0, %d0                   | remainder = (remainder << 1); (optimised by tricky use of an add opcode)
    blt.b   ProcessCRCPolynomial       | Then jump to the polynomial part
    subq.b  #1, %d1                    | Next bit
    bne.b   ProcessCRCBitLoop          | Any bits left?

ProcessCRCMainLoopCheck:
    bsr.w   ResetWatchdog              | Scratch the dog
    cmpa.l  %a1, %a2                   | Is byte < nBytes?
    bne.b   ProcessCRCMainLoop         | If not, iterate main loop

| Process CRC Response
    movea.l #MessageBuffer, %a0        | Rebuild in the MessageBuffer
    move.b  #toolid, 0x1(%a0)          | To: Tool
    move.b  #pcmid, 0x2(%a0)           | From: PCM
    move.b  #0x7D, 0x3(%a0)            | 3D + 40 = Mode 3D, Success
    move.l  %d0, 0xB(%a0)              | Move long CRC value to reply buffer
    move.l  #0x0F, %d0                 | It's 15 bytes long (6C F0 10 7D 02 01 00 00 01 00 00 67 86 6E C2)
    bsr.w   VPWSend                    | Send it
    jmp     MainLoop                   | Return to MainLoop

ProcessCRCPolynomial:
    eori.l  #POLYNOMIAL, %d0           | ^ POLYNOMIAL;
    subq.b  #1, %d1                    | Next byte
    bne.b   ProcessCRCBitLoop          | Is there another bit in this byte?
    bra.b   ProcessCRCMainLoopCheck    | Next main loop

| =============== S U B R O U T I N E =======================================
ProcessMode34:
| TODO: Add rejections
    movea.l #Mode34Reply, %a0          | Move address of Mode34Reply to a0
    move.l  #6, %d0                    | It's 6 bytes long
    bsr.w   VPWSend                    | Send it
    jmp     MainLoop

| =============== S U B R O U T I N E =======================================
|
| Process Mode35 request to send data
|
| Uses
|   MessageBuffer
|   a0 - Data start address
|   d0 - Data byte count (length)
|
ProcessMode35:
    clr.l   %d0                          | Clear d0
    or.b    MessageBuffer + 7, %d0       | First address byte
    lsl.l   #8, %d0                      | Logical Shift Left
    or.b    MessageBuffer + 8, %d0       | Second address byte
    lsl.l   #8, %d0                      | Logical Shift Left
    or.b    MessageBuffer + 9, %d0       | Third address byte
    movea.l %d0, %a0                     | Move data start address to a0

    clr.l   %d0                          | Clear d0
    or.b    MessageBuffer + 5, %d0       | First length byte
    lsl.l   #8, %d0                      | Logical Shift Left
    or.b    MessageBuffer + 6, %d0       | Second length byte

    bsr.w   VPWSendBlock                 | Send it
    jmp     MainLoop                     | Back to MainLoop

| =============== S U B R O U T I N E =======================================
ProcessMode36:
| For future use
    jmp     MainLoop                   | Back to main

| =============== S U B R O U T I N E =======================================
WaitForTXFIFO:
    bsr.w   ResetWatchdog
    move.b  (J1850_Status).l, %d2      | Copy the status to d2
    andi.b  #3, %d2                    | Mask the TMFS (Transmit FIFO Status) bits
    cmpi.b  #3, %d2                    | 3 = buffer full
    beq.b   WaitForTXFIFO              | Wait for the TX buffer to be non-full
    move.b  #4, (J1850_Command).l      | Load as transmit data to BTAD
    rts                                | Return from Subroutine

| =============== S U B R O U T I N E =======================================
VPWSend:
    move.b  #0x14, (J1850_Command).l   | BTAD Byte type and destination field to 101 (000 101 00)
                                       | 101 = Load as first byte of transmit data
    subq.l   #2, %d0                   | First and Last bytes are not counted

VPWSendNextByte:
    bsr.w   ResetWatchdog              | Scratch the dog
    move.b  (%a0)+, (J1850_TX_FIFO).l  | Write a byte to to the TX FIFO
    bsr.w   WasteTime                  | Twiddle thumbs
    bsr.w   WaitForTXFIFO              | Wait for space in the TX FIFO
    dbf     %d0, VPWSendNextByte       | If False Decrement and Branch

    move.b  #0x0C, (J1850_Command).l   | 011 to BTAD - Load as laste byte of transmit data
    move.b  (%a0)+, (J1850_TX_FIFO).l  | Drop the last byte in the TX FIFO
    bsr.w   WasteTime                  | Twiddle thumbs
    move.b  #0x03, (J1850_Command).l   | Flush buffer
    move.b  #0x00, (J1850_TX_FIFO).l   | Needed for flush buffer?

VPWSendWaitForFlush:
    bsr.w   ResetWatchdog              | Scratch the dog
    bsr.w   WasteTime                  | Twiddle thumbs
    move.b  (J1850_Status).l, %d0      | Get status byte
    andi.b  #0xE0, %d0                 | Check the RFS
    cmpi.b  #0xE0, %d0                 | Empty except for completion byte status
    bne.b   VPWSendWaitForFlush        | Wait until it is true
    move.b  (J1850_RX_FIFO).l, %d0     | Read FIFO
    rts

| =============== S U B R O U T I N E =======================================
VPWReceive:
    movea.l #MessageBuffer, %a0        | Move the input buffer address to a0

VPWReceiveReadFrame:
    bsr.w   ResetWatchdog              | Scratch the dog
    move.b  (J1850_Status).l, %d0      | Get status byte
    andi.b  #0xE0, %d0                 | Mask The RFS register

    tst.b   %d0                        | 000x xxxx = no data (or invalid)
    beq.b   VPWReceiveReadFrame        | Nothing, try again

    cmpi.b  #0x80, %d0                 | 100x xxxx = All codes *greater* than this mean completion code at the buffer head
    bgt.b   VPWReceiveReadComplete     | Done, cleanup

| VPW Receive Read Byte                | Anything else, read a byte
    move.b  (J1850_RX_FIFO).l, (%a0)+  | Read from the RX FIFO to the input buffer, and move the pointer to the next byte
    bra.b   VPWReceiveReadFrame        | Next try

VPWReceiveReadComplete:
    move.b  (J1850_RX_FIFO).l, (%a0)+  | This is the completion code, eat it.
    movea.l #MessageBuffer, %a0        | Point a0 to the head of the input buffer
    move.b  (%a0), %d0                 |
    andi.b  #0xFE, %d0                 | mask 1111 1110
    cmpi.b  #0x6C, %d0                 | Is it a priority 6C or 6D packet?
    bne.b   VPWReceive                 | If it is not, abort and get the next packet
    cmpi.b  #pcmid, 1(%a0)             | Check for device 0x10
    beq.b   VPWReceiveRTS              | If yes, then return for processing
    cmpi.b  #0xFE, 1(%a0)              | Check for broadcast device id 0xFE
    bne.b   VPWReceive                 | Not an FE, junk it and get the next packet

VPWReceiveRTS:
    rts                                | Must have been an FE, return for processing

| =============== S U B R O U T I N E =======================================
|
| Sends requested number of bytes from supplied address
|
| Entry
|   a0 - Source Start Address
|   d0 - Number of bytes requested
|
| Uses
|   Mode35Reply
|   a1 - Mode35Reply header address
|   d1 - Scratch, Checksum
|   d2 - Scratch
|
VPWSendBlock:
    movea.l #Mode35Reply, %a1            | Address Mode35Reply buffer
    move.l  %a0, %d1                     | Move the requested start address to d1
    move.b  %d1, 9(%a1)                  | Move address byte 3 to Mode35Reply buffer
    lsr.l   #8, %d1                      | Logical Shift Right 8 bits
    move.b  %d1, 8(%a1)                  | Move address byte 2 to Mode35Reply buffer
    lsr.l   #8, %d1                      | Logical Shift Right 8 bits
    move.b  %d1, 7(%a1)                  | Move address byte 1 to Mode35Reply buffer

    move.l  %d0, %d1                     | Move the requested length to d1
    move.b  %d1, 6(%a1)                  | Move length byte 2 to Mode35Reply buffer
    lsr.l   #8, %d1                      | Logical Shift Right
    move.b  %d1, 5(%a1)                  | Move length byte 1 to Mode35Reply buffer

    subq.l   #1, %d0                     | Zero Index data length
    move.w  #9, %d1                      | Mode35Reply Header is 10 bytes long minus 1 for Zero index

    move.b  #0x14, (J1850_Command).l     | 0x14 to BTAD = Load as first byte of transmit data

VPWSendBlockMode35ReplyHeader:
    bsr.w   ResetWatchdog                | Scratch the dog
    move.b  (%a1)+, (J1850_TX_FIFO).l    | Move Byte from Source to Destination
    dbf     %d1, VPWSendBlockMode35ReplyHeader | Send Mode35 Reply Header 10 bytes

    clr.l   %d1                          | Clear d1 for checksum
    clr.l   %d2                          | Clear d2 for checksum scratch pad
    move.b  Mode35Reply + 4, %d2         | Get Submode byte
    add.l   %d2, %d1                     | Add Submode byte to checksum
    move.b  Mode35Reply + 5, %d2         | Get Length byte 1
    add.l   %d2, %d1                     | Add Length byte 1 to checksum
    move.b  Mode35Reply + 6, %d2         | Get Length byte 2
    add.l   %d2, %d1                     | Add Length byte 2 to checksum
    move.b  Mode35Reply + 7, %d2         | Get Address byte 1
    add.l   %d2, %d1                     | Add Address byte 1 to checksum
    move.b  Mode35Reply + 8, %d2         | Get Address byte 2
    add.l   %d2, %d1                     | Add Address byte 2 to checksum
    move.b  Mode35Reply + 9, %d2         | Get Address byte 3
    add.l   %d2, %d1                     | Add Address byte 3 to checksum
                                         | d1 contains the checksum of the packet so far
VPWSendBlockTryNextByte:
    bsr.w   WaitForTXFIFO                | Wait for space in the TX FIFO
    move.b  (%a0), (J1850_TX_FIFO).l     | Move a byte on to the TX FIFO
    move.b  (%a0)+, %d2                  | Get same byte for checksum, increment Source pointer
    add.l   %d2, %d1                     | Add byte to checksum
    dbf     %d0, VPWSendBlockTryNextByte | Repeat loop d0 times sending requested data
    move.l  %d1, %d0                     | Move the checksum to d0
    lsr.l   #8, %d0                      | Shift the checksum right by 8 bits

VPWSendBlockWaitForBuffer:
    bsr.w   ResetWatchdog                | Scratch the dog
    move.b  (J1850_Status).l, %d2        | Get Status
    andi.b  #3, %d2                      | Mask TMFS Transmit FIFO Status
    cmpi.b  #3, %d2                      | 3 = Buffer full
    beq.b   VPWSendBlockWaitForBuffer    | Wait for room in the TX FIFO
    move.b  #4, (J1850_Command).l        | Load as transmit data
    move.b  %d0, (J1850_TX_FIFO).l       | Move the first checksum byte to the TX FIFO

VPWSendBlockWaitForBuffer2:
    bsr.w   ResetWatchdog                | Scratch the dog
    move.b  (J1850_Status).l, %d2        | Get Status
    andi.b  #3, %d2                      | Mask TMFS
    cmpi.b  #3, %d2                      | 3=Buffer full
    beq.b   VPWSendBlockWaitForBuffer2   | Wait for TX buffer to have room
    move.b  #0xC, (J1850_Command).l      | BTAD ...0 11.. "Load as last byte of transmit data"
    move.b  %d1, (J1850_TX_FIFO).l       | Move the Second checksum byte  to the TX FIFO
    rts                                  | Return from Subroutine

| ---------------------------------------------------------------------------
.data

.byte  0x0D
.ascii "(c)2023 pcmhacking.net" | An Antus / Gampy collaboration
| All AA bytes are padding for alignment

| Mode 35 Reply Header 10 bytes
Mode35Reply:    .byte  0x6D, toolid, pcmid, 0x36, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00

| Flash ID 9 bytes + 1 alignment
FlashIDReply:   .byte  0x6C, toolid, pcmid, 0x7D, 0x01, 0x00, 0x00, 0x00, 0x00, 0xAA

| Os ID Reply, 9 bytes + 1 alignment
OsIDReply:      .byte  0x6C, toolid, pcmid, 0x7D, 0x03, 0x00, 0x00, 0x00, 0x00, 0xAA

| Kernel ID Response, 9 bytes + 1 alignment
| 4 bytes, the first 12 bits are a static ID, next 12 bits are version, 4th byte = PCM ID
| P01/P59 = 01
| P04     = 04
| P08     = 08
| P10     = 0A
| P12     = 0C
| E54     = 36
KernelIDReply:  .byte  0x6C, toolid, pcmid, 0x7D, 0x00, 0x82, 0x40, 0x01, 0x00, 0xAA

| Mode34 Reply, 6 bytes - 6C F0 10 74 00 44, Default Success
Mode34Reply:    .byte  0x6C, toolid, pcmid, 0x74, 0x00, 0x44

| Mode36 Reply, 6 bytes - 6C F0 10 76 00 00
Mode36Reply:    .byte  0x6C, toolid, pcmid, 0x76, 0x00, 0x00

| Halt Kernel Reply, 4 bytes
Mode60Reply:    .byte  0x6C, toolid, pcmid, 0x60

| Global buffer, it's at the end and it's not transported, thus length is irrelevant!
  .globl   MessageBuffer
  .section .kerneldata, "aw", @progbits | Kernel data section, how it's excluded from transportation.
  .type    MessageBuffer, @object
  .size    MessageBuffer, 4128          | 4128 bytes is the J2534 J1850 spec, however size doesn't matter.
  .zero 4128                            | Except to zero (initalize) the area.
MessageBuffer:
.end

