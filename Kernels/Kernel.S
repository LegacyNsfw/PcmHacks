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
.equ SIM_CSBAR0,         0xFA4C
.equ SIM_CSORBT,         0xFA4A
#if defined P12
  .equ SIM_CSOR0,        0xFA7E
#else
  .equ SIM_CSOR0,        0xFA4E
#endif
.equ SIGNATURE_COMMAND,  0x9090
.equ READ_ARRAY_COMMAND, 0xFFFF

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
    move.b  #0x03, (J1850_Command).l   | Flush frame except for completion code
    move.b  #0x00, (J1850_TX_FIFO).l   | Initiate transfer

wait01:
    bsr.w   ResetWatchdog              | Scratch the dog
    bsr.w   WasteTime                  | Twiddle thumbs
    move.b  (J1850_Status).l, %d0      | Get the VPW status
    andi.b  #0xE0, %d0                 | Mask Receive FIFO Status Field (RFS) register 1110 0000
    cmpi.b  #0xE0, %d0                 | Check for "completion code only, at head of buffer" message 1110 0000
    bne.s   wait01                     | Not ready? wait and retry
    move.b  (J1850_RX_FIFO).l, %d0     | Strip off the completion code

#if defined P12
    movea.l #Mode36Reply, %a0          | Reply Success to mode 3680, only P12 needs this.
    move.l  #6, %d0                    | It's 6 bytes long
    bsr.w   VPWSend                    | Send message
    bsr.w   WasteTime                  | Twiddle thumbs
#endif

MainLoop:
|
| This loop needs to be redesigned!
| The problem is subs cannot use rts, we've stomped on MessageBuffer using it elsewhere
| this makes sharing common code difficult.
|
| Maybe save and pop the stack in the subs, or ...
|
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
    cmpi.w  #0x3DFE, 3(%a0) | Temp alternate Halt ... 0x20 crashes the PCM requireing battery disconnect! It only seems to happen from VPWExplorer++, makes no sense, it uses PCMHammer code!
    beq.w   Reboot                     | Reboot PCM
    cmpi.b  #Halt_20, 3(%a0)           | Is it mode 0x20 (return to normal comms)
    bne.w   MainLoop                   | No more options, next packet or reboot

| =============== S U B R O U T I N E =======================================
Reboot:
    movea.l #Mode60Reply, %a0          | Address Mode 20 reply (Sends a mode 60 (20+40) message)
    move.l  #4, %d0                    | It's 6 bytes long
    bsr.w   VPWSend                    | Send it
    bsr.w   WasteTime                  | Twiddle thumbs
    reset                              | Reset External Devices

EndlessLoop:                           | Kills pcm, lets COPs expire
    bra.s   EndlessLoop

| =============== S U B R O U T I N E =======================================
ResetWatchdog:
    move.b  #0x55, (COP1).l            | Reset COP1
    move.b  #0xAA, (COP1).l            | Reset COP1
    eori.b  #0x80, (COP2).l            | Reset COP2 ... COP2 ^= 0x80
    rts

| =============== S U B R O U T I N E =======================================
LongWaitWithWatchdog:
    move.l  #0x2710, %d5               | 10,000 loops

LongWaitLoop:
    bsr.w   ResetWatchdog              | Scratch the dog
    bsr.w   WasteTime                  | Twiddle thumbs
    bsr.w   WasteTime
    bsr.w   WasteTime
    bsr.w   WasteTime
    bsr.w   WasteTime
    bsr.w   WasteTime
    bsr.w   WasteTime
    bsr.w   WasteTime
    bsr.w   WasteTime
    bsr.w   WasteTime
    dbf     %d5, LongWaitLoop          | If False Decrement and Branch
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
    bra.w   MainLoop                   | Return to MainLoop

| =============== S U B R O U T I N E =======================================
|
| TODO: Have request supply address
|
ProcessOsID:
#if defined P04
    movea.l #0x7FFFA, %a1              | OsID location in Flash
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
    bra.w   MainLoop                   | Return to MainLoop

| =============== S U B R O U T I N E =======================================
ProcessFlashID:
#if defined P12
    move.w  #0xF322, (SIM_CSOR0).w     | 0xFA7E
#else
    move.w  #0x0007, (SIM_CSBAR0).w    | 0xFA4C Set CS registers
    move.w  #0x6820, (SIM_CSORBT).w    | 0xFA4A
    move.w  #0x7060, (SIM_CSOR0).w     | 0xFA4E
#endif
    movea.w #0, %a2                    | Address 0
    move.w  #SIGNATURE_COMMAND, (%a2)  | Command Intel Read ID State (0x9090)
    move.w  (%a2), %d4                 | Move MFG ID to d4
    movea.w #2, %a2                    | Address 2
    move.w  (%a2), %d3                 | Move Flash ID to d3
    movea.w #0, %a2                    | Address 0
    move.w  #READ_ARRAY_COMMAND, (%a2) | Command Intel Read Array State (0xFFFF)

    move.w  #0x0089, %d5               | Intel ID is 0x0089
    cmp.w   %d4, %d5                   | Is it Intel?
    beq.w   ProcessFlashIDFound        | Jump found if Intel

    movea.w #0xAAA, %a3                | AMD Command Address A
    movea.w #0x554, %a4                | AMD Command Address B
    move.w  #0xAAAA, (%a3)             | Command AMD Unlock A
    move.w  #0x5555, (%a4)             | Command AMD Unlock B
    move.w  #0x9090, (%a3)             | Command AMD Autoselect
    move.w  (%a2), %d4                 | Move MFG ID to d4
    movea.w #2, %a2                    | Address 2
    move.w  (%a2), %d3                 | Move Flash ID to d3
    movea.w #0, %a2                    | Address 0
    move.w  #0xF0F0, (%a2)             | Command AMD Reset

    move.w  #0x0001, %d5               | Amd ID is 0x0001
    cmp.w   %d4, %d5                   | Is it AMD?
    beq.w   ProcessFlashIDFound        | Jump found if AMD

    clr.w   %d3                        | Not found
    clr.w   %d4                        | Not found

ProcessFlashIDFound:
#if defined P12
    move.w  #0xA332, (SIM_CSOR0).w     | 0xFA7E
#else
    move.w  #0x1060, (SIM_CSOR0).w     | 0xFA4E
#endif
    move.w  %d4, (FlashIDManu).l       | Move MFG to FlashIDManu
    move.w  %d3, (FlashIDDev).l        | Move Flash ID to FlashIDDev
    movea.l #FlashIDReply, %a0         | Move address of FlashIDReply to a0
    move.l  #9, %d0                    | It's 9 bytes long
    bsr.w   VPWSend                    | Send it
    bra.w   MainLoop

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
    blt.s   ProcessCRCPolynomial       | Then jump to the polynomial part
    subq.b  #1, %d1                    | Next bit
    bne.s   ProcessCRCBitLoop          | Any bits left?

ProcessCRCMainLoopCheck:
    bsr.w   ResetWatchdog              | Scratch the dog
    cmpa.l  %a1, %a2                   | Is byte < nBytes?
    bne.s   ProcessCRCMainLoop         | If not, iterate main loop

| Process CRC Response
    movea.l #MessageBuffer, %a0        | Rebuild in the MessageBuffer
    move.b  #toolid, 0x1(%a0)          | To: Tool
    move.b  #pcmid, 0x2(%a0)           | From: PCM
    move.b  #0x7D, 0x3(%a0)            | 3D + 40 = Mode 3D, Success
    move.l  %d0, 0xB(%a0)              | Move long CRC value to reply buffer
    move.l  #0x0F, %d0                 | It's 15 bytes long (6C F0 10 7D 02 01 00 00 01 00 00 67 86 6E C2)
    bsr.w   VPWSend                    | Send it
    bra.w   MainLoop                   | Return to MainLoop

ProcessCRCPolynomial:
    eori.l  #POLYNOMIAL, %d0           | ^ POLYNOMIAL;
    subq.b  #1, %d1                    | Next byte
    bne.s   ProcessCRCBitLoop          | Is there another bit in this byte?
    bra.s   ProcessCRCMainLoopCheck    | Next main loop

| =============== S U B R O U T I N E =======================================
ProcessMode34:
| TODO: Add rejections
    movea.l #Mode34Reply, %a0          | Move address of Mode34Reply to a0
    move.l  #6, %d0                    | It's 6 bytes long
    bsr.w   VPWSend                    | Send it
    bra.w   MainLoop

| =============== S U B R O U T I N E =======================================
ProcessMode35:
    clr.l   %d0                        | Clear d0
    or.b    MessageBuffer + 7, %d0     | First address byte
    lsl.l   #8, %d0                    | Logical Shift Left
    or.b    MessageBuffer + 8, %d0     | Second address byte
    lsl.l   #8, %d0                    | Logical Shift Left
    or.b    MessageBuffer + 9, %d0     | Third address byte
    movea.l %d0, %a0                   | Move address to a0
    clr.l   %d0                        | Clear d0
    or.b    MessageBuffer + 5, %d0     | First length byte
    lsl.l   #8, %d0                    | Logical Shift Left
    or.b    MessageBuffer + 6, %d0     | Second length byte
    bsr.w   VPWSendBlock               | Send the actual data
    bra.w   MainLoop                   | Back to main

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
    beq.s   WaitForTXFIFO              | Wait for the TX buffer to be non-full
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
    bne.s   VPWSendWaitForFlush        | Wait until it is true
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
    beq.B   VPWReceiveReadFrame        | Nothing, try again

    cmpi.b  #0x80, %d0                 | 100x xxxx = All codes *greater* than this mean completion code at the buffer head
    bgt.s   VPWReceiveReadComplete     | Done, cleanup

| VPW Receive Read Byte                | Anything else, read a byte
    move.b  (J1850_RX_FIFO).l, (%a0)+  | Read from the RX FIFO to the input buffer, and move the pointer to the next byte
    bra.s   VPWReceiveReadFrame        | Next try

VPWReceiveReadComplete:
    move.b  (J1850_RX_FIFO).l, (%a0)+  | This is the completion code, eat it.
    movea.l #MessageBuffer, %a0        | Point a0 to the head of the input buffer
    move.b  (%a0), %d0                 |
    andi.b  #0xFE, %d0                 | mask 1111 1110
    cmpi.b  #0x6C, %d0                 | Is it a priority 6C or 6D packet?
    bne.s   VPWReceive                 | If it is not, abort and get the next packet
    cmpi.b  #pcmid, 1(%a0)             | Check for device 0x10
    beq.w   VPWReceiveRTS              | If yes, then return for processing
    cmpi.b  #0xFE, 1(%a0)              | Check for broadcast device id 0xFE
    bne     VPWReceive                 | Not an FE, junk it and get the next packet

VPWReceiveRTS:
    rts                                | Must have been an FE, return for processing

| =============== S U B R O U T I N E =======================================
VPWSendBlock:
    move.l  %a0, %d1                     | Move the address ready to go to d1
    move.b  %d1, (Mode35ReplyAddress3).l | Move 8 bits of it to the outbound buffer
    lsr.l   #8, %d1                      | Logical Shift Right
    move.b  %d1, (Mode35ReplyAddress2).l | Move 8 bits of it to the outbound buffer
    lsr.l   #8, %d1                      | Logical Shift Right
    move.b  %d1, (Mode35ReplyAddress1).l | Move 8 bits of it to the outbound buffer
    move.l  %d0, %d1                     | Move the length in d0 to d1
    move.b  %d1, (Mode35ReplyLength2).l  | Move 8 bits of length to the outbound buffer
    lsr.l   #8, %d1                      | Logical Shift Right
    move.b  %d1, (Mode35ReplyLength1).l  | Move 8 bits of length to the outbound buffer
    move.w  #10, %d1                     | Mode35ReplyHeader is 10 bytes long
    movea.l #Mode35ReplyHeader, %a1      | Move Address
    move.b  #0x14, (J1850_Command).l     | 0x14 to BTAD = Load as first byte of transmit data

    subq.l   #1, %d0                     |* It's always 1 byte long ...
    subq.l   #1, %d1                     |* It's always 1 byte long ...

VPWSendBlockMode35ReplyHeader:
    bsr.w   ResetWatchdog
    move.b  (%a1)+, (J1850_TX_FIFO).l    | Move Data from Source to Destination
    dbf     %d1, VPWSendBlockMode35ReplyHeader | Queue up the reply header
    clr.l   %d1                          | Clear d1
    clr.l   %d2                          | Clear d2
    move.b  (Mode35ReplySubMode).l, %d2  | Move a 1 to d2
    add.l   %d2, %d1                     | Add
    move.b  (Mode35ReplyLength1).l, %d2  | Start at 1 and add the length and address to get the stop address
    add.l   %d2, %d1                     | Add
    move.b  (Mode35ReplyLength2).l, %d2  | Move Data from Source to Destination
    add.l   %d2, %d1                     | Add
    move.b  (Mode35ReplyAddress1).l, %d2 | Move Data from Source to Destination
    add.l   %d2, %d1                     | Add
    move.b  (Mode35ReplyAddress2).l, %d2 | Move Data from Source to Destination
    add.l   %d2, %d1                     | Add
    move.b  (Mode35ReplyAddress3).l, %d2 | Move Data from Source to Destination
    add.l   %d2, %d1                     | d1 now contains the last byte address
                                         | or d1 contains a sum of the packet so far
VPWSendBlockTryNextByte:
    bsr.w   WaitForTXFIFO                | Wait for space in the TX FIFO
    move.b  (%a0), (J1850_TX_FIFO).l     | Move a byte on to the TX FIFO
    move.b  (%a0)+, %d2                  | Move the same byte to d2, and increment pointer in a0
    add.l   %d2, %d1                     | Maintain a sum in d1
    dbf     %d0, VPWSendBlockTryNextByte | Repeat if more bytes remaining
    move.l  %d1, %d0                     | Move the sum to d0
    lsr.l   #8, %d0                      | Shift the sum right by 8 bits

VPWSendBlockWaitForBuffer:
    bsr.w   ResetWatchdog
    move.b  (J1850_Status).l, %d2        | Get the status byte in d2
    andi.b  #3, %d2                      | Mask TMFS Transmit FIFO Status
    cmpi.b  #3, %d2                      | 3 = Buffer full
    beq.s   VPWSendBlockWaitForBuffer    | Wait for room in the TX FIFO
    move.b  #4, (J1850_Command).l        | Load as transmit data
    move.b  %d0, (J1850_TX_FIFO).l       | Put the sum in the TX FIFO

VPWSendBlockWaitForBuffer2:
    bsr.w   ResetWatchdog                | Branch to Subroutine
    move.b  (J1850_Status).l, %d2        | Status byte to d2
    andi.b  #3, %d2                      | Mask TMFS
    cmpi.b  #3, %d2                      | 3=Buffer full
    beq.s   VPWSendBlockWaitForBuffer2   | Wait for TX buffer to have room
    move.b  #0xC, (J1850_Command).l      | BTAD ...0 11.. "Load as last byte of transmit data"
    move.b  %d1, (J1850_TX_FIFO).l       | Move Data from Source to Destination
    rts                                  | Return from Subroutine

| ---------------------------------------------------------------------------
.data

.byte  0x0D
.ascii "(c)2023 pcmhacking.net" | An Antus / Gampy collaboration
| All AA bytes are padding for alignment

| Mode 35 Reply Header 10 bytes
Mode35ReplyHeader:   .byte  0x6D, toolid, pcmid, 0x36
Mode35ReplySubMode:  .byte  0x01
Mode35ReplyLength1:  .byte  0x00
Mode35ReplyLength2:  .byte  0x00
Mode35ReplyAddress1: .byte  0x00
Mode35ReplyAddress2: .byte  0x00
Mode35ReplyAddress3: .byte  0x00

| Flash ID 9 bytes + 1 alignment
FlashIDReply:   .byte  0x6C, toolid, pcmid, 0x7D, 0x01
FlashIDManu:    .byte  0x00, 0x00
FlashIDDev:     .byte  0x00, 0x00, 0x00

| Os ID Reply, 9 bytes + 1 alignment
OsIDReply:      .byte  0x6C, toolid, pcmid, 0x7D, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00

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
Mode60Reply:   .byte  0x6C, toolid, pcmid, 0x60

| Global buffer, it's at the end and it's not transported, thus length is irrelevant!
  .globl   MessageBuffer
  .section .kerneldata, "aw", @progbits | Kernel data section, how it's excluded from transportation.
  .type    MessageBuffer, @object
  .size    MessageBuffer, 4128          | 4128 bytes is the J2534 J1850 spec, however size doesn't matter.
MessageBuffer:
  .zero 4128                            | Except to zero (initalize) the area.
.end

