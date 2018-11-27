: -fomit-frame-pointer = remove the boilerplate linkw/unlk instructions 
: -c = compile but do not link
: -oformat srec = produce an Srecord file

c:\SysGCC\m68k-elf\bin\m68k-elf-gcc.exe -fomit-frame-pointer -Wl,--section-start=.kernelram=FF9100 main.c
c:\SysGCC\m68k-elf\bin\m68k-elf-objcopy.exe -O srec a.out kernel.S
