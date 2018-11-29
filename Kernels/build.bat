@echo off
: -fomit-frame-pointer = remove the boilerplate linkw/unlk instructions 
: -c    = compile but do not link
: -O1   = optimization level
: -g    = include debug information - not using this because the
:         disassembly is either corrupt or just incomprehensible

c:\SysGCC\m68k-elf\bin\m68k-elf-gcc.exe -c -fomit-frame-pointer -std=gnu99 -O0 main.c common.c micro-kernel.c 
c:\SysGCC\m68k-elf\bin\m68k-elf-ld.exe -T micro-kernel.ld main.o common.o micro-kernel.o
c:\SysGCC\m68k-elf\bin\m68k-elf-objcopy.exe -O srec a.out micro-kernel.S
C:\SysGCC\m68k-elf\bin\m68k-elf-objdump.exe -d -S a.out > micro-kernel.disassembly

c:\SysGCC\m68k-elf\bin\m68k-elf-gcc.exe -c -fomit-frame-pointer -std=gnu99 -O0 main.c common.c read-kernel.c 
c:\SysGCC\m68k-elf\bin\m68k-elf-ld.exe -T read-kernel.ld main.o common.o read-kernel.o
c:\SysGCC\m68k-elf\bin\m68k-elf-objcopy.exe -O srec a.out read-kernel.S
C:\SysGCC\m68k-elf\bin\m68k-elf-objdump.exe -d -S a.out > read-kernel.disassembly

del micro-kernel.bin
..\DevTools\SRecordToKernel\bin\Debug\netcoreapp2.1\win10-x64\SRecordToKernel.exe micro-kernel.S micro-kernel.bin > micro-kernel.log
dir micro-kernel.bin | findstr File

del read-kernel.bin
..\DevTools\SRecordToKernel\bin\Debug\netcoreapp2.1\win10-x64\SRecordToKernel.exe read-kernel.S read-kernel.bin > read-kernel.log
dir read-kernel.bin | findstr File

copy *-kernel.bin ..\Apps\PcmHammer\bin\debug