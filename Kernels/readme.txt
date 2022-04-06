Build.cmd encapsulates the options needed to convert C code into kernel binaries on Windows machines.
See Build.cmd -h for help

gcc.bat is mostly just for experimenting with gcc options before
moving those options into the Build.cmd batch file.

--

The GCC-m68k toolchain for Windows is available here:
http://gnutoolchains.com/m68k-elf/

--

The gcc toolchain for linux can be built on most linux distros with https://github.com/haarer/toolchain68k

You will need to comment out avr in the build scripts then uncomment m68k-elf

If you do not use the default install location update the makefile prefix to point to the new location

$ cd Kernels
$ make clean
rm -f *.bin *.o *.elf *.asm
$ make
/opt/crosschain/bin/m68k-elf-gcc -c -fomit-frame-pointer -std=gnu99 -mcpu=68332 micro-kernel.c -o micro-kernel.o
/opt/crosschain/bin/m68k-elf-gcc -c -fomit-frame-pointer -std=gnu99 -mcpu=68332 main.c -o main.o
/opt/crosschain/bin/m68k-elf-gcc -c -fomit-frame-pointer -std=gnu99 -mcpu=68332 common.c -o common.o
/opt/crosschain/bin/m68k-elf-ld  -T micro-kernel.ld main.o micro-kernel.o -o micro-kernel.elf
/opt/crosschain/bin/m68k-elf-objcopy -O binary --only-section=.kernel_code --only-section=.rodata micro-kernel.elf micro-kernel.bin
/opt/crosschain/bin/m68k-elf-gcc -c -fomit-frame-pointer -std=gnu99 -mcpu=68332 read-kernel.c -o read-kernel.o
/opt/crosschain/bin/m68k-elf-ld  -T read-kernel.ld main.o read-kernel.o -o read-kernel.elf
/opt/crosschain/bin/m68k-elf-objcopy -O binary --only-section=.kernel_ram read-kernel.elf read-kernel.bin

$ ls -l *.bin *.elf
-rwxr-xr-x. 1 antus antus   810 Dec  8 15:34 micro-kernel.bin
-rwxr-xr-x. 1 antus antus 17508 Dec  8 15:34 micro-kernel.elf
-rwxr-xr-x. 1 antus antus   889 Dec  8 15:34 read-kernel.bin
-rwxr-xr-x. 1 antus antus  9576 Dec  8 15:34 read-kernel.elf

