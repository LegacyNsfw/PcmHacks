del *.o *.bin *.elf
c:\SysGCC\m68k-elf\bin\m68k-elf-gcc.exe -c -fomit-frame-pointer -std=gnu99 -mcpu=68332 -O0 main.c kernel-p04.c
c:\SysGCC\m68k-elf\bin\m68k-elf-ld.exe -T kernel-p04.ld kernel-p04.o main.o -o kernel-p04.elf
c:\SysGCC\m68k-elf\bin\m68k-elf-objcopy.exe -O binary --only-section=.kernel_code --only-section=.rodata kernel-p04.elf Kernel-P04.bin
copy Kernel-P04.bin ..\..\Apps\PcmHammer\bin\Debug
dir ..\..\Apps\PcmHammer\bin\Debug\Kernel-P04.bin
pause