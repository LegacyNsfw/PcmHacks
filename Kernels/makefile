# Use toolchain from: https://github.com/haarer/toolchain68k
# Configure for m68k-elf target and install to default location
# Tested on EL6 and Fedora 28
PREFIX = /opt/crosschain/bin/m68k-elf-

CC = $(PREFIX)gcc
LD = $(PREFIX)ld
OBJCOPY = $(PREFIX)objcopy
OBJDUMP = $(PREFIX)objdump

CCFLAGS = -c -fomit-frame-pointer -std=gnu99 -mcpu=68332
LDFLAGS =
DUMPFLAGS = -d -S
COPYFLAGS = -O binary

all: micro-kernel.bin read-kernel.bin

%.o: %.c
	$(CC) $(CCFLAGS) $< -o $@

%.asm: %.bin
	$(OBJDUMP) $(DUMPFLAGS) -S $< -o $@

micro-kernel.elf: micro-kernel.o main.o common.o
	$(LD) $(LDFLAGS) -T micro-kernel.ld main.o micro-kernel.o -o $@
	-$(OBJDUMP) $(DUMPFLAGS) $@ > $@.disassembly

micro-kernel.bin: micro-kernel.elf
	$(OBJCOPY) $(COPYFLAGS) --only-section=.kernel_code --only-section=.rodata micro-kernel.elf micro-kernel.bin
	cp micro-kernel.bin ../Apps/PcmHammer/bin/Debug/

read-kernel.elf: read-kernel.o main.o common.o
	$(LD) $(LDFLAGS) -T read-kernel.ld main.o read-kernel.o -o $@
	-$(OBJDUMP) $(DUMPFLAGS) $@ > $@.disassembly

read-kernel.bin: read-kernel.elf
	$(OBJCOPY) $(COPYFLAGS) --only-section=.kernel_code --only-section=.rodata read-kernel.elf read-kernel.bin
	cp read-kernel.bin ../Apps/PcmHammer/bin/Debug/

clean:
	rm -f *.bin *.o *.elf *.asm
