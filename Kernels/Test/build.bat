: -fomit-frame-pointer = remove the boilerplate linkw/unlk instructions 
: -c = compile but do not link
: -oformat srec = produce an Srecord file
: -Wl,--section-start=.kernelram=FF9100

c:\SysGCC\m68k-elf\bin\m68k-elf-gcc.exe -c -fomit-frame-pointer  main.c kernel.c 
c:\SysGCC\m68k-elf\bin\m68k-elf-ld.exe -T kernel.ld main.o kernel.o
c:\SysGCC\m68k-elf\bin\m68k-elf-objcopy.exe -O srec a.out kernel.S
