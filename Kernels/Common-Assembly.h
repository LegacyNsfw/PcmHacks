| 2023-03-25 Gampy <@pcmhacking.net>
| Common elements between Loader.S and Kernel.S
| ===========================================================================
|
| C directives will only work if the source filename is .S, yup, capital S.
| That is the extension gnu has asscoaited the assembler with and it is case sensitive.
| When using gcc.exe with -x assembler option and anyother extension, the C directives will fail.
| We need to use gcc.exe for C directives, as.exe does not provide for them.
|

| J1850 registers
#if defined P01 || defined P10 || defined P12 || defined E54
  .equ J1850_Config,     0xFFF600
  .equ J1850_Command,    0xFFF60C
  .equ J1850_TX_FIFO,    0xFFF60D
  .equ J1850_Status,     0xFFF60E
  .equ J1850_RX_FIFO,    0xFFF60F
  #if defined P10
    .equ COP1,           0xFFFA27
    .equ COP2,           0x800806
  #elif defined P12
    .equ COP1,           0xFFFA55
    .equ COP2,           0xFFFA21
  #else
    .equ COP1,           0xFFFA27
    .equ COP2,           0xFFD006
  #endif
#elif defined P04
  .equ J1850_Config,     0xFFE800
  .equ J1850_Command,    0xFFE800
  .equ J1850_TX_FIFO,    0xFFE801
  .equ J1850_Status,     0xFFE800
  .equ J1850_RX_FIFO,    0xFFE801
  .equ COP1,             0xFFFA27
  .equ COP2,             0xFFC006
#endif

| Misc
.equ toolid,             0xF0
.equ pcmid,              0x10

| Modes supported
.equ KernelID_3D00,      0x3D00        | Return the Kernel version (Id).
.equ Halt_20,            0x20          | Reset PCM (Halt Kernel)
.equ Mode_34,            0x34          | Request Permission to upload X bytes to Address
.equ Mode_36,            0x36          | Tool sending data for writing to RAM or Flash.

| ==============================DLC Library===================================
| Register reference
| For more information see the MC68HC58 Data Link Controller Datasheet
|
| Command Byte Register
| --7-6-5---4-3-2---1-0--
| | GCOM  | BTAD  | RFC |
|
| BTAD Byte Type and Destination Field, Section 5.1.2:
| 000 Do Not Load
| 001 Load as transmit data
| 010 Reserved
| 011 Load as last byte of transmit data
| 100 Load as configuration byte
| 101 Load as first byte of transmit data
| 110 Load as configuration byte-immediate
| 111 Load as first and last byte of transmit data
|
| RFC Receive FIFO Command Field, Section 5.1.3:
| 00 Do Nothing
| 01 Reserved
| 10 Flush byte or completion code
| 11 Flush frame EXCEPT for completion code
|
| Status Byte Register
| --7-6-5---4-----3------2-----1-0---
| | RFS  | DLI | NETF | 4XMD | TMFS |
|
| RFS Receive FIFO Status Field, Section 5.3.1:
| 000 Buffer invalid or empty
| 001 Buffer contains more than one byte
| 010 Buffer contains a completion code
| 011 Data byte in 13th buffer position, no completion code
| 100 One byte in buffer
| 101 Completion code at head of buffer, more bytes available
| 110 Completion code at head of buffer, another frame available
| 111 Completion code only at head of buffer
|
| TMFS TransMit FIFO Status, Section 5.3.5:
| 00 Buffer Empty
| 01 Buffer contains data bytes
| 10 Buffer almost full
| 11 Buffer full
|
| ---------------------------------------------------------------------------

