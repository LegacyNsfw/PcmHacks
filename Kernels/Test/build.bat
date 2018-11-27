: -fomit-frame-pointer = remove the boilerplate linkw/unlk instructions 
: -c = compile but do not link

c:\SysGCC\m68k-elf\bin\m68k-elf-gcc.exe -c -fomit-frame-pointer -std=gnu99 -O2 main.c kernel.c 
c:\SysGCC\m68k-elf\bin\m68k-elf-ld.exe -T kernel.ld main.o kernel.o
c:\SysGCC\m68k-elf\bin\m68k-elf-objcopy.exe -O srec a.out kernel.S
