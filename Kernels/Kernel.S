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

|
| NOTICE!
|
| Even with just the required elements for writing, it is tough to get a stable Kernel on the P04, especially Intel based units (SvN:9380717, HdW:9357440
| It can be done, but PCMHammer also needs to be rewritten to work with 3 kernels, 1. Read Kernel, 2. AMD Write Kernel, 3. Intel Write Kernel.
| Which means we need to know before hand what Flash Chip is in the target ... So the Loader needs to be used for all PCM's, and have Flash ID capabilities to report to the Application.
|

| Include Common elements
| These elements cannot be a sub of MainLoop
#include "Common-Assembly.h"

| Flash elements
#if defined P12
  .equ SIM_BASE,             0xFA30
  .equ SIM_20,               (SIM_BASE + 0x20) | Lock functions
  #else
  .equ SIM_BASE,             0xFA00
#endif
.equ SIM_CSBARBT,            (SIM_BASE + 0x48) | (SIM_BASE + 0x48) (FA48)
.equ SIM_CSORBT,             (SIM_BASE + 0x4A) | (SIM_BASE + 0x4A) (FA4A)
.equ SIM_CSBAR0,             (SIM_BASE + 0x4C) | (SIM_BASE + 0x4C) (FA4C)
.equ SIM_CSOR0,              (SIM_BASE + 0x4E) | (SIM_BASE + 0x4E) (FA4E)

#if defined P08
  .equ REG_FFF408,           0xF408
  .equ REG_FFF4C8,           0xF4C8
  .equ VPP_BIT,              2
  .equ HARDWARE_IO,          0xFA11
  #else
  .equ VPP_BIT,              0
  .equ HARDWARE_IO,          0xE2FA
#endif

.equ READ_ARRAY_CMD,         0xFFFF    | Intel
.equ ERASE_RESUME_CONFIRM,   0xD0D0    | Intel
.equ AMD_PROGRAM_CMD,        0xA0A0    | Amd
.equ SIGNATURE_CMD,          0x9090    | Both
.equ AMD_ERASE_SETUP,        0x8080    | AMD
.equ READ_STATUS_REGISTER,   0x7070    | Intel
.equ CLEAR_STATUS_REGISTER,  0x5050    | Intel
.equ INTEL_PROGRAM_CMD,      0x4040    | Intel
.equ AMD_ERASE_RESUME,       0x3030    | AMD
.equ INTEL_ERASE_CMD,        0x2020    | Intel
.equ AMD_CMD_ADDRESS_A,      0x0AAA    | AMD
.equ AMD_CMD_ADDRESS_B,      0x0554    | AMD
.equ AMD_CMD_UNLOCK_A,       0xAAAA    | AMD
.equ AMD_CMD_UNLOCK_B,       0x5555    | AMD
.equ AMD_READ_ARRAY_CMD,     0xF0F0    | AMD
|
|Supported flash chip manufactures
.equ INTEL_ID,               0x0089    | Manufacture ID
.equ AMD_ID,                 0x0001    | Manufacture ID
|
| Intel supported chips
.equ FLASH_ID_INTEL_28F400B,  0x4471   | 512k,  7 Sectors, 0,4000,6000,8000,20000,40000,60000
.equ FLASH_ID_INTEL_28F800B,  0x889D   | 1m,   11 Sectors, 0,4000,6000,8000,20000,40000,60000,80000,A0000,C0000,E0000
| AMD supported chips
.equ FLASH_ID_AMD_AM29F400BB, 0x22AB   | 512k, 11 Sectors, 0,4000,6000,8000,10000,20000,30000,40000,50000,60000,70000
.equ FLASH_ID_AMD_AM29F800BB, 0x2258   | 1m,   19 Sectors, 0,4000,6000,8000,10000,20000,30000,40000,50000,60000,70000,80000,90000,A0000,B0000,C0000,D0000,E0000,F0000
.equ FLASH_ID_AMD_AM29BL802C, 0x2281   | 1m,    9 Sectors, 0,4000,6000,8000,20000,40000,60000,80000,C0000
.equ FLASH_ID_AMD_AM29BL162C, 0x2203   | 2m,   11 Sectors, 0,4000,6000,8000,40000,80000,C0000,100000,140000,180000,1C0000

| CRC32
.equ POLYNOMIAL,         0x4C11DB7

| Modes supported shared in Common-Assembly.S
|.equ KernelID_3D00       0x3D00       | Moved to Common-Assembly.s
.equ FlashID_3D01,       0x3D01        | Return Flash chip ID
.equ CRC_3D02,           0x3D02        | Return CRC for range
.equ OsID_3D03,          0x3D03        | Return OsID
.equ EraseSector_3D05,   0x3D05        | ProcessEraseSector
|.equ Halt_20             0x20         | Moved to Common-Assembly.s
|.equ Mode_34             0x34         | Moved to Common-Assembly.s
.equ Mode_35,            0x35          | Send data from PCM to Tool
|.equ Mode_36             0x36         | Moved to Common-Assembly.s

start:
    ori     #0x700, %sr                | Disable Interrupts

#if defined P08
    movea.w #0xB800, %sp               | P08: Relocate the stack
#endif

    move.b  #0x03, (J1850_Command).l   | Flush frame except for completion code
    move.b  #0x00, (J1850_TX_FIFO).l   | Initiate transfer

Wait01:
    bsr.w   ResetWatchdog              | Scratch the dog
    bsr.w   WasteTime                  | Twiddle thumbs
    move.b  (J1850_Status).l, %d0      | Get the VPW status
    andi.b  #0xE0, %d0                 | Mask Receive FIFO Status Field (RFS) register 1110 0000
    cmpi.b  #0xE0, %d0                 | Check for "completion code only, at head of buffer" message 1110 0000
    bne.b   Wait01                     | Not ready? wait and retry
    move.b  (J1850_RX_FIFO).l, %d0     | Strip off the completion code

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
    cmpi.w  #FlashID_3D01, 3(%a0)      | Is it mode 0x3D 01 Get Flash Chip ID (non-standard extension)
    beq.w   ProcessFlashID             | Process it
    cmpi.w  #CRC_3D02, 3(%a0)          | Is it mode 0x3D 02 Get Flash Sector CRC (non-standard extension)
    beq.w   ProcessCRC                 | Process it
    cmpi.w  #OsID_3D03, 3(%a0)         | Is it mode 0x3D 03 Get Os ID (non-standard extension)
    beq.w   ProcessOsID                | Process it
    cmpi.w  #EraseSector_3D05, 3(%a0)  | Is it mode 0x3D 05 Erase Flash Sector (non-standard extension)
    beq.w   ProcessEraseSector         | Process it
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
|
| TODO: Have request supply address
|
ProcessOsID:
#if defined P04
    movea.l #0x7FFFA, %a1              | Pointer to OsID location in Flash
#elif defined P08
    movea.l #0x8000, %a1               | Pointer to OsID location in Flash
#elif defined P10
    movea.l #0x52E, %a1                | Pointer to OsID location in Flash
#elif defined P12
    movea.l #0x8004, %a1               | Pointer to OsID location in Flash
#elif defined E54
    movea.l #0x20004, %a1              | Pointer to OsID location in Flash
#else
    movea.l #0x504, %a1                | Pointer to OsID location in Flash
#endif
    movea.l #OsIDReply, %a0            | Pointer to OsIDReply buffer
    move.l  (%a1), 5(%a0)              | Move OsID to buffer
    move.l  #9, %d0                    | It's 9 bytes long
    bsr.w   VPWSend                    | Send it
    jmp     MainLoop                   | Return to MainLoop

| =============== S U B R O U T I N E =======================================
ProcessCRC:
    clr.l   %d0                        | Start Clean
    or.b    MessageBuffer + 8, %d0     | First address byte
    lsl.l   #8, %d0                    | Logical Shift Left
    or.b    MessageBuffer + 9, %d0     | Second address byte
    lsl.l   #8, %d0                    | Logical Shift Left
    or.b    MessageBuffer + 10, %d0    | Third address byte
    movea.l %d0, %a1                   | Pointer to address buffer

    clr.l   %d0                        | Start Clean
    or.b    MessageBuffer + 5, %d0     | First length byte
    lsl.l   #8, %d0                    | Logical Shift Left
    or.b    MessageBuffer + 6, %d0     | Second length byte
    lsl.l   #8, %d0                    | Logical Shift Left
    or.b    MessageBuffer + 7, %d0     | Third length byte
    movea.l %d0, %a2                   | Pointer to length buffer

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
    movea.l #MessageBuffer, %a0        | Pointer to MessageBuffer buffer
    move.b  #toolid, 1(%a0)            | To: Tool
    move.b  #pcmid, 2(%a0)             | From: PCM
    move.b  #0x7D, 3(%a0)              | 3D + 40 = Mode 3D, Success
    move.l  %d0, 11(%a0)               | Move long CRC value to reply buffer
    move.l  #15, %d0                   | It's 15 bytes long (6C F0 10 7D 02 01 00 00 01 00 00 67 86 6E C2)
    bsr.w   VPWSend                    | Send it
    jmp     MainLoop                   | Return to MainLoop

ProcessCRCPolynomial:
    eori.l  #POLYNOMIAL, %d0           | ^ POLYNOMIAL;
    subq.b  #1, %d1                    | Next byte
    bne.b   ProcessCRCBitLoop          | Is there another bit in this byte?
    bra.b   ProcessCRCMainLoopCheck    | Next main loop

| =============== S U B R O U T I N E =======================================
|
| Process Flash ID Request
|
| NOTE: Once called, the Flash Chip ID remains at address FlashIDReply, for life (the Kernels life).
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
    movea.w #0, %a0                    | Pointer to address 0 (First word)
    move.w  #SIGNATURE_CMD, (%a0)      | Command Intel Read ID State (0x9090)
    move.w  (%a0), %d1                 | Move MFG ID to d1
    movea.w #2, %a0                    | Pointer to address 2 (Second word)
    move.w  (%a0), %d0                 | Move Flash ID to d0
    movea.w #0, %a0                    | Pointer to address 0 (First word)
    move.w  #READ_ARRAY_CMD, (%a0)     | Command Intel Read Array State (0xFFFF)

    cmp.w   #INTEL_ID, %d1             | Intel ID is 0x0089
    beq.w   ProcessFlashIDFound        | Jump found if Intel

    movea.w #AMD_CMD_ADDRESS_A, %a1    | Pointer to AMD Command Address A (0xAAA)
    movea.w #AMD_CMD_ADDRESS_B, %a2    | Pointer to AMD Command Address B (0x554)
    move.w  #AMD_CMD_UNLOCK_A, (%a1)   | Command AMD Unlock A  (0xAAAA)
    move.w  #AMD_CMD_UNLOCK_B, (%a2)   | Command AMD Unlock B  (0x5555)
    move.w  #SIGNATURE_CMD, (%a1)      | Command AMD Autoselect  (0x9090)
    move.w  (%a0), %d1                 | Move MFG ID to d1
    movea.w #2, %a0                    | Pointer to address 2 (Second word)
    move.w  (%a0), %d0                 | Move Flash ID to d0
    movea.w #0, %a0                    | Pointer to address 0 (First word)
    move.w  #AMD_READ_ARRAY_CMD, (%a0) | Command AMD Reset (0xF0F0)

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
    movea.l #FlashIDReply, %a0         | Pointer to FlashIDReply buffer
    move.w  %d1, 5(%a0)                | Move MFG ID to FlashIDReply buffer
    move.w  %d0, 7(%a0)                | Move Flash ID to FlashIDReply buffer
    move.l  #9, %d0                    | It's 9 bytes long
    bsr.w   VPWSend                    | Send it
    jmp     MainLoop

| =============== S U B R O U T I N E =======================================
|
| Process Erase Flash Sector
|
| WARNING!!! It requires that the application has previously requested the FlashID!
|
| Exit
|   d0 - Status 0=Ok, 1=Fail
|
| Uses
|   MessageBuffer
|   FlashIDReply
|   a0 - Sector Address to Erase
|   a1 - Address FlashIDReply
|   d1 - Scratch
|
ProcessEraseSector:
    clr.l   %d1                        | Start Clean
    or.b    MessageBuffer + 5, %d1     | First address byte
    lsl.l   #8, %d1                    | Logical Shift Left
    or.b    MessageBuffer + 6, %d1     | Second address byte
    lsl.l   #8, %d1                    | Logical Shift Left
    or.b    MessageBuffer + 7, %d1     | Third address byte
    movea.l %d1, %a0                   | Pointer to flash sector address buffer

    clr.l   %d0                        | Start Clean
    movea.l #FlashIDReply, %a1         | Pointer to FlashIDReply buffer

    move.w  7(%a1), %d1

    | Intel
    cmp.w #FLASH_ID_INTEL_28F400B, %d1 | 0x4471 512k
    beq.s ProcessEraseSectorIntel      | Intel Flash Erase
    cmp.w #FLASH_ID_INTEL_28F800B, %d1 | 0x889D 1m
    beq.s ProcessEraseSectorIntel      | Intel Flash Erase

    | AMD
    cmp.w   #FLASH_ID_AMD_AM29F400BB, %d1 | 0x22AB 512k
    beq.s   ProcessEraseSectorAMD         | AMD Flash Erase
    cmp.w   #FLASH_ID_AMD_AM29F800BB, %d1 | 0x2258 1m
    beq.s   ProcessEraseSectorAMD         | AMD Flash Erase
    cmp.w   #FLASH_ID_AMD_AM29BL802C, %d1 | 0x2281 1m
    beq.s   ProcessEraseSectorAMD         | AMD Flash Erase
    cmp.w   #FLASH_ID_AMD_AM29BL162C, %d1 | 0x2203 2m
    beq.s   ProcessEraseSectorAMD         | AMD Flash Erase

    move.b  #1, %d0                    | Set error
    bra.s   ProcessEraseSectorFinal    | Fail

ProcessEraseSectorIntel:
    bsr.w   IntelEraseSector           | Intel Flash Sector Erase
    bra.s   ProcessEraseSectorFinal    | Done

ProcessEraseSectorAMD:
    bsr.w   AMDEraseSector             | AMD Flash Sector Erase
    |bra.s   ProcessEraseSectorFinal   | Done - Waste, Keep as reminder

ProcessEraseSectorFinal:
    movea.l #EraseSectorReply, %a0     | Pointer to EraseSectorReply buffer
    move.b  %d0, 5(%a0)                | Move error value to EraseSectorReply buffer
    move.l  #7, %d0                    | PCMHammer returns 7 bytes, 6C F0 10 7D 05 00 00, last two are Status and Error?, why two??
    bsr.w   VPWSend                    | Send it!
    jmp     MainLoop

| =============== S U B R O U T I N E =======================================
|
| Process Write Flash Sector
|
| WARNING!!! It requires that the application has previously requested the FlashID!
|
| Entry
|   a0 - Source Address
|   a1 - Destination Address
|   d0 - Byte Count
|
| Exit
|   d0 - Status 0=Ok, 1=Fail
|
| Uses
|   FlashIDReply
|   a2 - Address FlashIDReply
|   d1 - Scratch
|
ProcessWriteFlash:
    movea.l #FlashIDReply, %a2         | Pointer to FlashIDReply buffer
    move.w  7(%a2), %d1                | Move Flash Chip ID to d1

    | Intel
    cmp.w   #FLASH_ID_INTEL_28F400B, %d1  | 0x4471 512k
    beq.s   ProcessWriteFlashIntel        | Intel Flash Erase
    cmp.w   #FLASH_ID_INTEL_28F800B, %d1  | 0x889D 1m
    beq.s   ProcessWriteFlashIntel        | Intel Flash Erase

    | AMD
    cmp.w   #FLASH_ID_AMD_AM29F400BB, %d1 | 0x22AB 512k
    beq.s   ProcessWriteFlashAMD          | AMD Flash Erase
    cmp.w   #FLASH_ID_AMD_AM29F800BB, %d1 | 0x2258 1m
    beq.s   ProcessWriteFlashAMD          | AMD Flash Erase
    cmp.w   #FLASH_ID_AMD_AM29BL802C, %d1 | 0x2281 1m
    beq.s   ProcessWriteFlashAMD          | AMD Flash Erase
    cmp.w   #FLASH_ID_AMD_AM29BL162C, %d1 | 0x2203 2m
    beq.s   ProcessWriteFlashAMD          | AMD Flash Erase

    move.b  #1, %d0                    | Set error
    bra.s   ProcessWriteFlashFinal

ProcessWriteFlashIntel:
    bsr.w   IntelWriteSector           | Intel Flash Write
    bra.s   ProcessWriteFlashFinal     | Done

ProcessWriteFlashAMD:
    bsr.w   AMDWriteSector             | AMD Flash Write
    |bra.s   ProcessWriteFlashFinal     | Done - Waste, Keep as reminder

ProcessWriteFlashFinal:
    rts

| =============== S U B R O U T I N E =======================================
|
| AMD Flash Unlock
| Tested on, 
|   AM29F400BB (512k)
|   AM29F800BB (1m)
|   AM29BL802C (1m)
|   AM29BL162C (2m)
|
| Entry
|   d1 - Mode 0=Erase, 1=Write, is only used for the P12 currently (AMD AM29BL chips)
|
AMDFlashUnlock:
#if defined P12
    andi.w  #0xFEFF, (SIM_20).w        | SIM_20 &= 0xFEFF;
    ori.w   #0x1000, (SIM_CSOR0).w     | SIM_CSOR0 |= 0x1000;
    andi.w  #0xFFF8, (SIM_CSBAR0).w    | SIM_CSBAR0 &= 0xFFF8;

    tst.b   %d1                        | Test if Zero
    bne.s   AMDFlashUnlockModeWrite    | Branch if non Zero

|AMDFlashUnlockModeErase
    ori.w   #0x0004, (SIM_CSBAR0).w    | Mode Zero - Erase
    bra.s   AMDFlashUnlockDone

AMDFlashUnlockModeWrite:
    ori.w   #0x0005, (SIM_CSBAR0).w    | Mode One - Write

AMDFlashUnlockDone:
#else
    move.w  #0x7060, (SIM_CSOR0).w     | 0xFA4E
#endif
    rts

| =============== S U B R O U T I N E =======================================
|
| AMD Flash Lock
| Tested on, 
|   AM29F400BB (512k)
|   AM29F800BB (1m)
|   AM29BL802C (1m)
|   AM29BL162C (2m)
|
AMDFlashLock:
#if defined P12
    andi.w  #0xEFFF, (SIM_CSOR0).w     | SIM_CSOR0 &= 0xEFFF;
    andi.w  #0xFEFF, (SIM_20).w        | SIM_20 &= 0xFEFF;
    ori.w   #0x0100, (SIM_20).w        | SIM_20 |= 0x0100;
#else
    move.w  #0x1060, (SIM_CSOR0).w     | 0xFA4E
#endif
    rts

| =============== S U B R O U T I N E =======================================
|
| AMD Flash Erase Sector by Address
| Tested on,
|   AM29F400BB (512k)
|   AM29F800BB (1m)
|   AM29BL802C (1m)
|   AM29BL162C (2m)
|
| Entry
|   a0 - Sector Address to Erase
|
| Exit
|   d0 - Status 0=Ok, 1=Fail
|
| Uses
|   a2 - AMD Command Address A
|   a3 - AMD Command Address B
|   d1 - Scratch
|   d2 - Scratch
|   d3 - Scratch
|
AMDEraseSector:
    bsr.w   ResetWatchdog              | Scratch the dog

    clr.l   %d1                        | Set Unlock Mode 0 = Erase
    bsr.w   AMDFlashUnlock             | d1 is 0=Erase or 1=Write

    lea     AMD_CMD_ADDRESS_A, %a2     | (0xAAA)
    lea     AMD_CMD_ADDRESS_B, %a3     | (0x554)

    move.w  #AMD_CMD_UNLOCK_A, (%a2)   | (0xAAAA, 0xAAA)
    move.w  #AMD_CMD_UNLOCK_B, (%a3)   | (0x5555, 0x554)
    move.w  #AMD_ERASE_SETUP, (%a2)    | (0x8080, 0xAAA)
    move.w  #AMD_CMD_UNLOCK_A, (%a2)   | (0xAAAA, 0xAAA)
    move.w  #AMD_CMD_UNLOCK_B, (%a3)   | (0x5555, 0x554)
    move.w  #AMD_ERASE_RESUME, (%a0)   | (0x3030, ProvidedAddress)

AMDEraseSectorNotReady:
    bsr.w   ResetWatchdog              | Scratch the dog
    move.w  (%a0), %d2                 | Move value at address to d2
    bsr.w   WasteTime                  | Twiddle thumbs
    move.w  (%a0), %d1                 | Move value at address to d1

    andi.w  #0x40, %d2                 | Mask 0100 0000
    andi.w  #0x40, %d1                 | Mask 0100 0000
    cmp.w   %d2, %d1                   | If equal Success
    beq.w   AMDEraseSectorSuccess      | Jump Success

    move.w  (%a0), %d3                 | Move value at address to d3
    btst    #5, %d3                    | Is bit 5 clear
    beq.w   AMDEraseSectorNotReady     | Yes, not ready so retry

    move.w  (%a0), %d2                 | Move value at address to d2
    bsr.w   WasteTime                  | Twiddle thumbs
    move.w  (%a0), %d1                 | Move value at address to d1
    andi.w  #0x40, %d2                 | Mask 0100 0000
    andi.w  #0x40, %d1                 | Mask 0100 0000
    cmp.w   %d2, %d1                   | If equal Success
    beq.w   AMDEraseSectorSuccess

    move.b  #1, %d0                    | Set error
    bra.w   AMDEraseSectorFinish

AMDEraseSectorSuccess:
    clr.b   %d0                        | Set success

AMDEraseSectorFinish:
    move.w  #AMD_READ_ARRAY_CMD, (%a0) | (0xF0F0) Return to Normal Read Array mode
    move.w  #AMD_READ_ARRAY_CMD, (%a0) | (0xF0F0) Return to Normal Read Array mode

    bsr.w   AMDFlashLock
    rts

| =============== S U B R O U T I N E =======================================
|
| AMD Flash Write Sector
| Tested on,
|   AM29F400BB (512k)
|   AM29F800BB (1m)
|   AM29BL802C (1m)
|   AM29BL162C (2m)
|
| Entry
|   a0 - Source Address
|   a1 - Destination Address
|   d0 - Byte count - must be divisible by 2
|
| Exit
|   d0 - Status 0=Ok, 1=Fail
|
| Uses
|   a2 - AMD Command Address A
|   a3 - AMD Command Address B
|   d1 - Scratch
|   d2 - Timeout index
|   d3 - Word to be written
|
AMDWriteSector:
    bsr.w   ResetWatchdog              | Scratch the dog

    move.l  #1, %d1                    | Set Unlock Mode 1 = Write
    bsr.w   AMDFlashUnlock             | d1 is 0=Erase, 1=Write

    lea     AMD_CMD_ADDRESS_A, %a2     | (0x0AAA) Pointer to the command address
    lea     AMD_CMD_ADDRESS_B, %a3     | (0x0554) Pointer to the command address

AMDWriteSectorLoop:
    move.w  #2346, %d2                 | FlashNotReady Loop Timeout index

    | We cannot write by bytes, we must write by word, all data has been forced off word boundries, see NOTE in data section at EOF
    | So we make words out of bytes ...
    clr.l   %d3                        | Start clean
    or.b    (%a0)+, %d3                | First byte, increment source to next byte
    lsl.l   #8, %d3                    | Logical Shift Left (shift byte left into upper word)
    or.b    (%a0)+, %d3                | Second byte to complete the Word, Increment source to next byte

    move.w  #AMD_CMD_UNLOCK_A, (%a2)   | (0xAAAA, 0x0AAA)
    move.w  #AMD_CMD_UNLOCK_B, (%a3)   | (0x5555, 0x0554)
    move.w  #AMD_PROGRAM_CMD, (%a2)    | (0xA0A0, 0x0AAA)

    move.w  %d3, (%a1)                 | Move word to destination ... Write it!

AMDWriteSectorNotReady:
    bsr.w   ResetWatchdog              | Scratch the dog
    move.w  (%a1), %d1                 | Read what was written
    cmp.w   %d3, %d1                   | Compare to what was written
    beq.s   AMDWriteSectorSuccess      | Jump for next byte if match
    dbeq    %d2, AMDWriteSectorNotReady | Retry reading back written value

    move.w  (%a1), %d1                 | One last chance
    cmp.w   %d1, %d3                   | Compare to whats written
    bne.s   AMDWriteSectorFail         | Branch fail if not equal

AMDWriteSectorSuccess:
    adda.w  #2, %a1                    | Increment address by 2 bytes (1 word)
    subq.w  #2, %d0                    | Deincrement byte count by 2 (1 word)
    bne.s   AMDWriteSectorLoop         | Next word if not done

    move.w  #AMD_READ_ARRAY_CMD, (%a1) | 0xF0F0 Return to Normal Read Array mode
    move.w  #AMD_READ_ARRAY_CMD, (%a1) | 0xF0F0 Return to Normal Read Array mode

    clr.b   %d0                        | Set success
    bra.s   AMDWriteSectorFinal

AMDWriteSectorFail:
    move.b  #1, %d0                    | Set error

AMDWriteSectorFinal:
    bsr.w   AMDFlashLock
    rts

| =============== S U B R O U T I N E =======================================
|
| Intel Flash Unlock
| Tested on, 
|   AB28F400B (512k)
|   AB28F800B (1m)
|
| Uses
|   d1 - Scratch
|
IntelFlashUnlock:
#if defined P08
    move.w #0x0F, (REG_FFF408).w       | P08 Hardware Setup
    move.w #0x05, (REG_FFF4C8).w       | P08 Hardware Setup
#endif
    move.w  #0x0007, (SIM_CSBARBT).w   | $FFFFFA48
    move.w  #0x6820, (SIM_CSORBT).w    | $FFFFFA4A
    move.w  #0x0007, (SIM_CSBAR0).w    | $FFFFFA4C
    | Unlock
    move.w  #0x7060, (SIM_CSOR0).w     | $FFFFFA4E
    | Enable +12v Vpp
#if defined P08
    bset    #VPP_BIT, (HARDWARE_IO).w  | Does not work on P01, needed for P08
#else
    move.w  (HARDWARE_IO).w, %d1       | Move Vpp Latch Address value to d1
    bset    #VPP_BIT, %d1              | Set Vpp Latch Bit
    move.w  %d1, (HARDWARE_IO).w       | Move modified value back to Latch Address
#endif
    bsr.w   LongWaitWithWatchdog
    rts

LongWaitWithWatchdog:
    movem.l %d5, -(%sp)                | Save d5
    move.w  #0x8000, %d1               | 32768 loops
LongWaitLoop:
    bsr.w   ResetWatchdog              | Scratch the dog
    bsr.w   WasteTime                  | Twiddle thumbs
    dbf     %d1, LongWaitLoop | If False Decrement and Branch
    movem.l (%sp)+, %d5                | Restore d5
    rts

| =============== S U B R O U T I N E =======================================
|
| Intel Flash Lock
| Tested on,
|   AB28F400B (512k)
|   AB28F800B (1m)
|
| Uses
|   d1 - Scratch
|
IntelFlashLock:
    move.w  #0x0007, (SIM_CSBARBT).w   | $FFFFFA48
    move.w  #0x6820, (SIM_CSORBT).w    | $FFFFFA4A
    move.w  #0x0007, (SIM_CSBAR0).w    | $FFFFFA4C
    | Lock
    move.w  #0x1060, (SIM_CSOR0).w     | $FFFFFA4E
    | Disable +12v Vpp
#if defined P08
    bclr    #VPP_BIT, (HARDWARE_IO).w  | Does not work on P01, needed for P08
#else
    move.w  (HARDWARE_IO).w, %d1       | Move Vpp Latch Address value to d1
    bclr    #VPP_BIT, %d1              | Set Vpp Latch Bit
    move.w  %d1, (HARDWARE_IO).w       | Move modified value back to Latch Address
#endif
    bsr.w   LongWaitWithWatchdog
    rts

| =============== S U B R O U T I N E =======================================
|
| Intel Flash Erase Sector by Address
| Tested on,
|   AB28F400B (512k)
|   AB28F800B (1m)
|
| Entry
|   a0 - Sector Address to Erase
|
| Exit
|   d0 - Status 0=Ok, 1=Fail
|
| Uses
|   d1 - Scratch
|   d2 - Loop Index
|
IntelEraseSector:
    clr.l   %d1                           | clear scratch space
    bsr.w   IntelFlashUnlock

    move.w  #CLEAR_STATUS_REGISTER, (%a0) | 0x5050 Clear Status register
    move.w  #INTEL_ERASE_CMD, (%a0)       | 0x2020 Command Erase
    move.w  #ERASE_RESUME_CONFIRM, (%a0)  | 0xD0D0 Confirm Command Erase
    move.l  #0x320000, %d2                | Erase Loop Timeout index

IntelEraseSectorNotReady:
    bsr.w   LongWaitWithWatchdog
    move.w  #READ_STATUS_REGISTER, (%a0)  | 0x7070 Read Status Register
    move.w  (%a0), %d1                    | Move Status to d1
    btst    #0x007, %d1                   | Test Status
    bne.s   IntelEraseSectorReady         | Status Success - Jump to IntelEraseSectorReady
    subq.l  #1, %d2                       | Decrement index
    bne.s   IntelEraseSectorNotReady      | Fail Status Check Test again

IntelEraseSectorReady:
    clr.l   %d0                           | Set Success (if Status passes)
    andi.w  #0x0E8, %d1                   | Mask 1110 1000
    cmp.w   #0x080, %d1                   | Check status
    beq.s   IntelEraseSectorDone          | Pass ?
    move.b  #1, %d0                       | Set Failure

IntelEraseSectorDone:
    move.w    #READ_ARRAY_CMD, (%a0)      | 0xFFFF Return to Normal Read Array mode
    move.w    #READ_ARRAY_CMD, (%a0)      | 0xFFFF Return to Normal Read Array mode

    bsr.w   IntelFlashLock
    rts

| =============== S U B R O U T I N E =======================================
|
| Intel Flash Write Sector
| Tested on,
|   AB28F400B (512k)
|   AB28F800B (1m)
|
| Entry
|   a0 - Source Address
|   a1 - Destination Address
|   d0 - Byte count - must be divisible by 2
|
| Exit
|   d0 - Status 0=Ok, 1=Fail
|
| Uses
|   d2 - Timeout index
|   d3 - Scratch
|
IntelWriteSector:
    bsr.w   IntelFlashUnlock

IntelWriteSectorLoop:
    move.w  #0x92A, %d2                | FlashNotReady Loop Timeout index (2346)

    | We cannot write by bytes, we must write by word, all data has been forced off word boundries, see NOTE in data section at EOF
    | So we make words out of bytes ...
    clr.l   %d3                        | Start clean
    or.b    (%a0)+, %d3                | First byte, and increment a0 by 1 byte
    lsl.l   #8, %d3                    | Logical Shift Left (shift byte left into upper word)
    or.b    (%a0)+, %d3                | Second byte to complete the Word, and increment a0 by 1 byte

    move.w  #CLEAR_STATUS_REGISTER, (%a1) | 0x5050 Clear Status register
    move.w  #INTEL_PROGRAM_CMD,(%a1)      | 0x4040 Command Program
    move.w  %d3, (%a1)                    | Move (Write) word to Flash
    move.w  #READ_STATUS_REGISTER, (%a1)+ | 0x7070 Read Status Register, and increment a1 by word (2 bytes)

IntelWriteSectorNotReady:
    bsr.w   ResetWatchdog              | Scratch the dog
    move.w  (%a1), %d3                 | Move Status to d3
    btst    #0x007, %d3                | Test Status
    dbne    %d2, IntelWriteSectorNotReady | Status Not Ready, Retest

    move.w  #READ_ARRAY_CMD, (%a1)     | 0xFFFF Return to Normal Read Array mode
    move.w  #READ_ARRAY_CMD, (%a1)     | 0xFFFF Return to Normal Read Array mode

    andi.w  #0x098, %d3                 | Mask 1001 1000
    cmp.w   #0x080, %d3                 | Check status
    bne.s   IntelWriteSectorFail       | Fail Jump

    subq    #2, %d0                    | Deincrement byte count by two (1 word)
    bne.s   IntelWriteSectorLoop       | Next word if not done

    clr.l   %d0                        | Set Success
    bra.s   IntelWriteSectorFinal      | All Done

IntelWriteSectorFail:
    move.b  #1, %d0                    | Set Failure

IntelWriteSectorFinal:
    bsr.w   IntelFlashLock
    rts

| =============== S U B R O U T I N E =======================================
ProcessMode34:
| TODO: Add rejections
    movea.l #Mode34Reply, %a0          | Pointer to Mode34Reply buffer
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
    clr.l   %d0                        | Start Clean
    or.b    MessageBuffer + 7, %d0     | First address byte
    lsl.l   #8, %d0                    | Logical Shift Left
    or.b    MessageBuffer + 8, %d0     | Second address byte
    lsl.l   #8, %d0                    | Logical Shift Left
    or.b    MessageBuffer + 9, %d0     | Third address byte
    movea.l %d0, %a0                   | Pointer to start address

    clr.l   %d0                        | Start Clean
    or.b    MessageBuffer + 5, %d0     | First length byte
    lsl.l   #8, %d0                    | Logical Shift Left
    or.b    MessageBuffer + 6, %d0     | Second length byte

    bsr.w   VPWSendBlock               | Send it
    jmp     MainLoop                   | Back to MainLoop

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
    movea.l #MessageBuffer + 10, %a0   | Pointer to start of data
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
    movea.l #MessageBuffer + 4, %a3    | Pointer to beginning of checksum range, Calculates from MessageBuffer[4] (0=6C 1=10 2=F0 3=36 4=<start>)
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
    cmp.w   %d3, %d4                   | Is the sum OK?
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
    cmpi.b  #0x44, MessageBuffer + 4   | Test Write (0x44) ??
    beq.s   ProcessMode36ResponseOK    | Yes, branch to ProcessMode36ResponseOK
    bsr.w   ProcessWriteFlash          | IntelWriteSector

ProcessMode36ResponseOK:
    movea.l #Mode36Reply, %a0          | Pointer to Mode36Reply buffer
    move.b  MessageBuffer + 4, %d4     | Copy submode to d4
    move.b  %d4, Mode36Reply + 4       | Copy submode to response, and keep it for the execute test
    move.b  %d0, Mode36Reply + 5       | Status code 0=Ok, 1=Fail
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
    move.b  #0x00, (J1850_TX_FIFO).l   | Completion byte

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
    andi.b  #0xFE, %d0                 | Mask 1111 1110
    cmpi.b  #0x6C, %d0                 | Priority 6C or 6D packet?
    bne.s   VPWReceive                 | If not, abort and get next packet
    cmpi.b  #pcmid, 1(%a0)             | Check for device 0x10
    beq.s   VPWReceiveRTS              | If yes, return for processing
    cmpi.b  #0xFE, 1(%a0)              | Check for broadcast device id 0xFE
    bne.s   VPWReceive                 | Not an FE, junk it, get next packet

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
    movea.l #Mode35Reply, %a1          | Pointer to Mode35Reply buffer
    move.l  %a0, %d1                   | Move the requested start address to d1
    move.b  %d1, 9(%a1)                | Move address byte 3 to Mode35Reply buffer
    lsr.l   #8, %d1                    | Logical Shift Right 8 bits
    move.b  %d1, 8(%a1)                | Move address byte 2 to Mode35Reply buffer
    lsr.l   #8, %d1                    | Logical Shift Right 8 bits
    move.b  %d1, 7(%a1)                | Move address byte 1 to Mode35Reply buffer

    move.l  %d0, %d1                   | Move the requested length to d1
    move.b  %d1, 6(%a1)                | Move length byte 2 to Mode35Reply buffer
    lsr.l   #8, %d1                    | Logical Shift Right
    move.b  %d1, 5(%a1)                | Move length byte 1 to Mode35Reply buffer

    subq.l  #1, %d0                    | Zero Index data length
    move.w  #9, %d1                    | Mode35Reply Header is 10 bytes long minus 1 for Zero index

    move.b  #0x14, (J1850_Command).l   | 0x14 to BTAD = Load as first byte of transmit data

VPWSendBlockMode35ReplyHeader:
    bsr.w   ResetWatchdog              | Scratch the dog
    move.b  (%a1)+, (J1850_TX_FIFO).l  | Move Byte from Source to Destination
    dbf     %d1, VPWSendBlockMode35ReplyHeader | Send Mode35 Reply Header 10 bytes

    clr.l   %d1                        | Clear for checksum
    clr.l   %d2                        | Clear checksum scratch pad
    move.b  Mode35Reply + 4, %d2       | Get Submode byte
    add.l   %d2, %d1                   | Add Submode byte to checksum
    move.b  Mode35Reply + 5, %d2       | Get Length byte 1
    add.l   %d2, %d1                   | Add Length byte 1 to checksum
    move.b  Mode35Reply + 6, %d2       | Get Length byte 2
    add.l   %d2, %d1                   | Add Length byte 2 to checksum
    move.b  Mode35Reply + 7, %d2       | Get Address byte 1
    add.l   %d2, %d1                   | Add Address byte 1 to checksum
    move.b  Mode35Reply + 8, %d2       | Get Address byte 2
    add.l   %d2, %d1                   | Add Address byte 2 to checksum
    move.b  Mode35Reply + 9, %d2       | Get Address byte 3
    add.l   %d2, %d1                   | Add Address byte 3 to checksum
                                       | d1 contains the checksum of the packet so far
VPWSendBlockTryNextByte:
    bsr.w   WaitForTXFIFO              | Wait for space in the TX FIFO
    move.b  (%a0), (J1850_TX_FIFO).l   | Move a byte on to the TX FIFO
    move.b  (%a0)+, %d2                | Get same byte for checksum, increment Source pointer
    add.l   %d2, %d1                   | Add byte to checksum
    dbf     %d0, VPWSendBlockTryNextByte | Repeat loop d0 times sending requested data
    move.l  %d1, %d0                   | Move the checksum to d0
    lsr.l   #8, %d0                    | Shift the checksum right by 8 bits

VPWSendBlockWaitForBuffer:
    bsr.w   ResetWatchdog              | Scratch the dog
    move.b  (J1850_Status).l, %d2      | Get Status
    andi.b  #3, %d2                    | Mask TMFS Transmit FIFO Status
    cmpi.b  #3, %d2                    | 3 = Buffer full
    beq.s   VPWSendBlockWaitForBuffer  | Wait for room in the TX FIFO
    move.b  #4, (J1850_Command).l      | Load as transmit data
    move.b  %d0, (J1850_TX_FIFO).l     | Move the first checksum byte to the TX FIFO

VPWSendBlockWaitForBuffer2:
    bsr.w   ResetWatchdog              | Scratch the dog
    move.b  (J1850_Status).l, %d2      | Get Status
    andi.b  #3, %d2                    | Mask TMFS
    cmpi.b  #3, %d2                    | 3=Buffer full
    beq.s   VPWSendBlockWaitForBuffer2 | Wait for TX buffer to have room
    move.b  #0xC, (J1850_Command).l    | BTAD ...0 11.. "Load as last byte of transmit data"
    move.b  %d1, (J1850_TX_FIFO).l     | Move the Second checksum byte  to the TX FIFO
    rts

| ---------------------------------------------------------------------------
.data

| This 0x0D byte is required (any single byte works), ALL data must be knocked off word boundries, this causes a hella nightmare
|  when the desire is to work with words, we should be keeping everything word based (i.e. according to hoyle).
| It has to be before the last data declaration.
|
| If MessageBuffer is on a valid word boundry, VPWReceive fails, it only works when it's misaligned to word boundries.
| I think it needs to get two bytes, make a word, then add it to MessageBuffer by Word, or shift the bytes left by one!
| Every attempt I've made to accomplish this has failed ... Purely due to my lack of knowledge of the DLC!
| Alignment is simple ... Remove this 0x0D and it is aligned, however VPWReceive will fail.

.ascii "(c)2023 pcmhacking.net" | An Antus / Gampy collaboration
| All AA bytes are padding for alignment

| Mode 35 Reply Header 10 bytes + 1 alignment
Mode35Reply:    .byte  0x6D, toolid, pcmid, 0x36, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0xAA

| Flash ID 9 bytes
FlashIDReply:   .byte  0x6C, toolid, pcmid, 0x7D, 0x01, 0x00, 0x00, 0x00, 0x00, 0xAA

| Os ID Reply, 9 bytes
OsIDReply:      .byte  0x6C, toolid, pcmid, 0x7D, 0x03, 0x00, 0x00, 0x00, 0x00

| Kernel ID Response, 9 bytes + 1 alignment
| 4 bytes, the first 12 bits are a static ID, next 12 bits are version, 4th byte = PCM ID
| P01/P59 = 01
| P04     = 04
| P08     = 08
| P10     = 0A
| P12     = 0C
| E54     = 36
KernelIDReply:  .byte  0x6C, toolid, pcmid, 0x7D, 0x00, 0x82, 0x40, 0x02, 0x00, 0xAA

| Mode34 Reply, 6 bytes - 6C F0 10 74 00 44, Default Success
Mode34Reply:    .byte  0x6C, toolid, pcmid, 0x74, 0x00, 0x44

| Mode36 Reply, 6 bytes - 6C F0 10 76 00 00
Mode36Reply:    .byte  0x6C, toolid, pcmid, 0x76, 0x00, 0x00

| Halt Kernel Reply, 4 bytes + 1 alignment
Mode60Reply:    .byte  0x6C, toolid, pcmid, 0x60, 0xAA

| Erase Sector Reply, 7 bytes
EraseSectorReply:   .byte  0x6C, toolid, pcmid, 0x7D, 0x05, 0x00, 0x00

.byte 0xAA
| Global buffer, it's at the end and it's not transported, thus length is irrelevant!
  .globl   MessageBuffer
  .section .kerneldata, "aw", @progbits | Kernel data section, how it's excluded from transportation.
  .type    MessageBuffer, @object
  .size    MessageBuffer, 4128          | 4128 bytes is the J2534 J1850 spec, however size doesn't matter.
MessageBuffer:
.end

