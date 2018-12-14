@echo off
: -fomit-frame-pointer = remove the boilerplate linkw/unlk instructions
: -c    = compile but do not link
: -O1   = optimization level
: -g    = include debug information - not using this because the
:         disassembly is either corrupt or just incomprehensible

c:\SysGCC\m68k-elf\bin\m68k-elf-gcc.exe -c -fomit-frame-pointer -std=gnu99 -mcpu=68332 main.c micro-kernel.c
c:\SysGCC\m68k-elf\bin\m68k-elf-ld.exe -T micro-kernel.ld main.o micro-kernel.o -o micro-kernel.elf
c:\SysGCC\m68k-elf\bin\m68k-elf-objcopy.exe -O binary --only-section=.kernel_code --only-section=.rodata micro-kernel.elf micro-kernel.bin
C:\SysGCC\m68k-elf\bin\m68k-elf-objdump.exe -d -S micro-kernel.elf > micro-kernel.disassembly

c:\SysGCC\m68k-elf\bin\m68k-elf-gcc.exe -c -fomit-frame-pointer -std=gnu99 -mcpu=68332 -O0 main.c read-kernel.c common.c common-readwrite.c
c:\SysGCC\m68k-elf\bin\m68k-elf-ld.exe -T read-kernel.ld main.o read-kernel.o common.o common-readwrite.o -o read-kernel.elf
c:\SysGCC\m68k-elf\bin\m68k-elf-objcopy.exe -O binary --only-section=.kernel_code --only-section=.rodata read-kernel.elf read-kernel.bin
C:\SysGCC\m68k-elf\bin\m68k-elf-objdump.exe -d -S read-kernel.elf > read-kernel.disassembly

:c:\SysGCC\m68k-elf\bin\m68k-elf-gcc.exe -c -fomit-frame-pointer -std=gnu99 -mcpu=68332 -O0 main.c write-kernel.c common.c
:c:\SysGCC\m68k-elf\bin\m68k-elf-ld.exe -T read-kernel.ld main.o write-kernel.o common.o -o write-kernel.elf
:c:\SysGCC\m68k-elf\bin\m68k-elf-objcopy.exe -O binary --only-section=.kernel_code --only-section=.rodata write-kernel.elf read-kernel.bin
:C:\SysGCC\m68k-elf\bin\m68k-elf-objdump.exe -d -S write-kernel.elf > write-kernel.disassembly

copy *-kernel.bin ..\PcmHacks\Apps\PcmHammer\bin\debug
dir *.bin
