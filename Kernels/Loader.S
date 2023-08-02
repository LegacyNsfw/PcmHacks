| GM '0411 Kernel Loader for PCMHammer
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

| Modes supported shared in Common-Assembly.S
|.equ KernelID_3D00       0x3D00        | Moved to Common-Assembly.s
|.equ Halt_20             0x20          | Moved to Common-Assembly.s
|.equ Mode_34             0x34          | Moved to Common-Assembly.s
|.equ Mode_36             0x36          | Moved to Common-Assembly.s

start:
    ori     #0x700, %sr                | Disable Interrupts

#if defined P04
    movea.l #0xFFA000, %sp             | P04: Relocate the stack
#endif

#if defined P12
    movea.l #Mode36Reply, %a0          | Reply Success to mode 3680, only P12 needs this.
    move.l  #6, %d0                    | It's 6 bytes long
    bsr.w   VPWSend                    | Send message
    bsr.w   WasteTime                  | Twiddle thumbs
#endif

MainLoop:
    movea.l #MessageBuffer, %a0        | Pointer to MessageBuffer buffer
    clr.w   3(%a0)                     | Clear last command, prevent repeating it
    bsr.w   ResetWatchdog              | Scratch the dog
    bsr.w   VPWReceive                 | Wait for and read next packet
    bsr.w   WasteTime                  | Twiddle thumbs
    cmpi.w  #KernelID_3D00, 3(%a0)     | Is it mode 0x3D 00 Get Kernel ID (non-standard extension)
    beq.w   ProcessKernelID            | Process it
    cmpi.b  #Mode_34, 3(%a0)           | Is it mode 0x34 (Tool asking PCM, ok to send X bytes to address)
    beq.w   ProcessMode34              | Process it
    cmpi.b  #Mode_36, 3(%a0)           | Is it mode 0x36 (Tool sending data to write, either to RAM or Flash)
    beq.w   ProcessMode36              | Process it
    cmpi.b  #Halt_20, 3(%a0)           | Is it mode 0x20 (return to normal comms)
    bne.w   MainLoop                   | No more options, next packet or reboot

| =============== S U B R O U T I N E =======================================
Reboot:
    movea.l #Mode60Reply, %a0          | Pointer to Mode 20 reply buffer
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
    nop                                | Non Operation
    nop
    nop
    nop
    rts

| =============== S U B R O U T I N E =======================================
ProcessKernelID:
    movea.l #KernelIDReply, %a0        | Pointer to KernelIDReply buffer
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
ProcessMode34:
| TODO: Add rejections
    movea.l #Mode34Reply, %a0          | Pointer to Mode34Reply buffer
    move.l  #6, %d0                    | It's 6 bytes long
    bsr.w   VPWSend                    | Send it
    jmp     MainLoop

| =============== S U B R O U T I N E =======================================
| The packet is in the buffer when this function is called
| This function loads the target address, length, and checksum to registers
| Then it calculates the checksum of the received data and validates the block
| If OK it proceeds to copy the data, then it responds with success or failure
|
| Uses
|   a0
|   a1
|   a2
|   a3
|   d0
|   d2
|   d3
|   d4
|
ProcessMode36:
    clr.l   %d0                        | Clear for Target Address
    lea     MessageBuffer + 10, %a0    | Pointer to start of data
    or.b    MessageBuffer + 7, %d0     | First address byte
    lsl.l   #8, %d0                    | Logical Shift Left
    or.b    MessageBuffer + 8, %d0     | Second address byte
    lsl.l   #8, %d0                    | Logical Shift Left
    or.b    MessageBuffer + 9, %d0     | Third address byte
    movea.l %d0, %a1                   | Pointer to target address
    movea.l %d0, %a2                   | Pointer to target address for execute jump

    clr.l   %d0                        | Start Clean
    or.b    MessageBuffer + 5, %d0     | First length byte
    lsl.l   #8, %d0                    | Logical Shift Left
    or.b    MessageBuffer + 6, %d0     | Second length byte

| Process Mode36 Validate Sum          | a3 = buffer address, d2 = temp, d3 = counter, d4=sum
    clr.l   %d2                        | Init temporary register
    lea     MessageBuffer + 4, %a3     | Pointer to beginning of checksum range, Calculates from MessageBuffer[4] (0=6C 1=10 2=F0 3=36 4=<start>)
    clr.l   %d4                        | Init sum
    move.l  %d0, %d3                   | Payload length (Byte count)
    addq.l  #5, %d3                    | Add submode (1), address (3), length(2) = 6 bytes to sum length minus 1 for Zero Index

ProcessMode36NextSum:
    move.b  (%a3)+, %d2                | Copy byte to d2
    add.l   %d2, %d4                   | Add to sum
    dbf     %d3, ProcessMode36NextSum  | Loop until sum calculated

    clr.l   %d3                        | Clear for Payload sum
    or.b    (%a3)+, %d3                | First byte
    lsl.l   #8, %d3                    | Logical Shift Left
    or.b    (%a3)+, %d3                | Second byte
    cmp.l   %d3, %d4                   | Is the sum OK?
    beq.s   ProcessMode36Data          | Yes, Do it 

| Process Mode36 Response Fail
    movea.l #Mode36Reply, %a0          | Pointer to Mode36Reply buffer
    move.b  #0x7F, 3(%a0)              | 7F Failure
    move.b  MessageBuffer + 3, 4(%a0)  | Mode
    move.b  MessageBuffer + 4, 5(%a0)  | Submode
    move.l  #6, %d0                    | Message is 6 bytes long
    bsr.w   VPWSend                    | Send it
    jmp     MainLoop                   | Return to main loop

ProcessMode36Data:
    | a0 - Source Address
    | a1 - Destination Address
    | d0 - Byte Count
    move.b  (%a0)+, (%a1)+             | Move a byte from A0 to A1, increment pointers
    dbf     %d0, ProcessMode36Data     | If False Decrement and Branch

| Process Mode36 Response OK
    movea.l #Mode36Reply, %a0          | Pointer to Mode36Reply buffer
    move.b  MessageBuffer + 4, %d4     | Copy submode to d4
    move.b  %d4, Mode36Reply + 4       | Copy submode to response, and keep it for the execute test
    move.l  #6, %d0                    | Message is 6 bytes long

| Send Mode36 Response
    bsr.w   VPWSend                    | Send it
    andi.b  #0x80, %d4                 | Execute flag is bit 7 of submode
    cmpi.b  #0x80, %d4                 | Test for Execute
    bne.w   MainLoop                   | If not executing return to MainLoop
    jmp     (%a2)                      | Execute the payload

| =============== S U B R O U T I N E =======================================
WaitForTXFIFO:
    bsr.w   ResetWatchdog
    move.b  (J1850_Status).l, %d2      | Copy status to d2
    andi.b  #3, %d2                    | Mask the TMFS (Transmit FIFO Status) bits
    cmpi.b  #3, %d2                    | 3 = buffer full
    beq.s   WaitForTXFIFO              | Wait for the TX buffer to be non-full
    move.b  #4, (J1850_Command).l      | Load as transmit data to BTAD
    rts

| =============== S U B R O U T I N E =======================================
VPWSend:
    move.b  #0x14, (J1850_Command).l   | BTAD Byte type and destination field to 101 (000 101 00)
                                       | 101 = Load as first byte of transmit data
    subq.l  #2, %d0                    | First and Last bytes are not counted

VPWSendNextByte:
    bsr.w   ResetWatchdog              | Scratch the dog
    move.b  (%a0)+, (J1850_TX_FIFO).l  | Write byte to TX FIFO
    bsr.w   WasteTime                  | Twiddle thumbs
    bsr.w   WaitForTXFIFO              | Wait for space in TX FIFO
    dbf     %d0, VPWSendNextByte       | If False Decrement and Branch

    move.b  #0x0C, (J1850_Command).l   | 011 to BTAD - Load as last byte of transmit data
    move.b  (%a0)+, (J1850_TX_FIFO).l  | Drop the last byte in TX FIFO
    bsr.w   WasteTime                  | Twiddle thumbs
    move.b  #0x03, (J1850_Command).l   | Flush buffer
    move.b  #0x00, (J1850_TX_FIFO).l   | Needed for flush buffer?

VPWSendWaitForFlush:
    bsr.w   ResetWatchdog              | Scratch the dog
    bsr.w   WasteTime                  | Twiddle thumbs
    move.b  (J1850_Status).l, %d0      | Get status byte
    andi.b  #0xE0, %d0                 | Mask RFS 1110 0000
    cmpi.b  #0xE0, %d0                 | Empty except for completion byte status
    bne.s   VPWSendWaitForFlush        | Loop until true
    move.b  (J1850_RX_FIFO).l, %d0     | Read FIFO
    rts

| =============== S U B R O U T I N E =======================================
VPWReceive:
    movea.l #MessageBuffer, %a0        | Pointer to MessageBuffer buffer

VPWReceiveReadFrame:
    bsr.w   ResetWatchdog              | Scratch the dog
    move.b  (J1850_Status).l, %d0      | Get status byte
    andi.b  #0xE0, %d0                 | Mask RFS register 1110 0000
    tst.b   %d0                        | 000x xxxx = no data (or invalid)
    beq.s   VPWReceiveReadFrame        | Nothing, try again

    cmpi.b  #0x80, %d0                 | 100x xxxx = All codes *greater* than this mean completion code at buffer head
    bgt.s   VPWReceiveReadComplete     | Done, cleanup

| VPW Receive Read Byte                | Anything else, read a byte
    move.b  (J1850_RX_FIFO).l, (%a0)+  | Read from RX FIFO to input buffer, and move pointer to next byte
    bra.s   VPWReceiveReadFrame        | Next try

VPWReceiveReadComplete:
    move.b  (J1850_RX_FIFO).l, (%a0)+  | The completion code, so eat it
    movea.l #MessageBuffer, %a0        | Pointer to MessageBuffer buffer
    move.b  (%a0), %d0                 |
    andi.b  #0xFE, %d0                 | mask 1111 1110
    cmpi.b  #0x6C, %d0                 | Priority 6C or 6D packet?
    bne.s   VPWReceive                 | If not, abort and get next packet
    cmpi.b  #pcmid, 1(%a0)             | Check for device 0x10
    beq.s   VPWReceiveRTS              | If yes, return for processing
    cmpi.b  #0xFE, 1(%a0)              | Check for broadcast device id 0xFE
    bne.s   VPWReceive                 | Not an FE, junk it, get next packet

VPWReceiveRTS:
    rts                                | Must have been an FE, return for processing

| ---------------------------------------------------------------------------
.data

.byte  0x0D
.ascii "(c)2023 pcmhacking.net" | An Antus / Gampy collaboration
| All AA bytes are padding for alignment

| Kernel ID Response, 9 bytes + 1 alignment
| 4 bytes, the first 12 bits are a static ID, next 12 bits are version, 4th byte = PCM ID
| P01/P59 = 01
| P04     = 04
| P08     = 08
| P10     = 0A
| P12     = 0C
| E54     = 36
KernelIDReply:  .byte  0x6C, toolid, pcmid, 0x7D, 0x00, 0x69, 0x00, 0x01, 0x00, 0xAA

| Mode34 Reply, 6 bytes - 6C F0 10 74 00 44, Default Success
Mode34Reply:    .byte  0x6C, toolid, pcmid, 0x74, 0x00, 0x44

| Mode36 Reply, 6 bytes - 6C F0 10 76 00 00
Mode36Reply:    .byte  0x6C, toolid, pcmid, 0x76, 0x00, 0x01

| Halt Kernel Reply, 4 bytes
Mode60Reply:   .byte  0x6C, toolid, pcmid, 0x60

| Global buffer, it's at the end and it's not transported, thus length is irrelevant!
  .globl   MessageBuffer
  .section .kerneldata, "aw", @progbits | Kernel data section, how it's excluded from transportation.
  .type    MessageBuffer, @object
  .size    MessageBuffer, 4128          | 4128 bytes is the J2534 J1850 spec, however size doesn't matter.
MessageBuffer:
.end

