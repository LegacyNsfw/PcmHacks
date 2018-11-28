: -fomit-frame-pointer = remove the boilerplate linkw/unlk instructions 
: -c    = compile but do not link
: -O1   = optimization level
: -g    = include debug information - not using this because the
:         disassembly is either corrupt or just incomprehensible

c:\SysGCC\m68k-elf\bin\m68k-elf-gcc.exe -c -fomit-frame-pointer -std=gnu99 -O0 main.c kernel.c 
c:\SysGCC\m68k-elf\bin\m68k-elf-ld.exe -T kernel.ld main.o kernel.o
c:\SysGCC\m68k-elf\bin\m68k-elf-objcopy.exe -O srec a.out kernel.S
C:\SysGCC\m68k-elf\bin\m68k-elf-objdump.exe -d -S kernel.o > kernel.disassembly

..\..\DevTools\SRecordToKernel\bin\Debug\netcoreapp2.1\win10-x64\SRecordToKernel.exe kernel.S test-kernel.bin > test-kernel.log