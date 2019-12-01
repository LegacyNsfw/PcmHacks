@echo off
: -fomit-frame-pointer = remove the boilerplate linkw/unlk instructions
: -c    = compile but do not link
: -O1   = optimization level
: -g    = include debug information - not using this because the
:         disassembly is either corrupt or just incomprehensible

if -%KernelBuild%- == -- call fixpath.bat

call clean.bat

c:\SysGCC\m68k-elf\bin\m68k-elf-gcc.exe -c -fomit-frame-pointer -std=gnu99 -mcpu=68332 -O0 main.c write-kernel.c crc.c common.c common-readwrite.c flash.c flash-intel.c flash-amd.c
c:\SysGCC\m68k-elf\bin\m68k-elf-ld.exe -T kernel.ld main.o write-kernel.o crc.o common.o common-readwrite.o -o kernel.elf flash.o flash-intel.o flash-amd.o
c:\SysGCC\m68k-elf\bin\m68k-elf-objcopy.exe -O binary --only-section=.kernel_code --only-section=.rodata kernel.elf kernel.bin
C:\SysGCC\m68k-elf\bin\m68k-elf-objdump.exe -d -S kernel.elf > kernel.disassembly

c:\mingw\bin\g++ -o test.exe test.cpp crc.c

copy kernel.bin ..\Apps\PcmHammer\bin\debug\kernel.bin
dir *.bin
