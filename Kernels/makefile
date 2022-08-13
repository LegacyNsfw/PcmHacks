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

pcm?=P01

CCFLAGS += -D$(pcm)

all: kernel.bin

%.o: %.c
	$(CC) $(CCFLAGS) $< -o $@

%.asm: %.bin
	$(OBJDUMP) $(DUMPFLAGS) -S $< -o $@

kernel.elf: main.o write-kernel.o crc.o common.o common-readwrite.o flash-intel.o flash-amd.o
	$(LD) $(LDFLAGS) -T kernel.ld main.o write-kernel.o crc.o common.o common-readwrite.o flash-intel.o flash-amd.o -o $@
	-$(OBJDUMP) $(DUMPFLAGS) $@ > $@.disassembly

kernel.bin: kernel.elf
	$(OBJCOPY) $(COPYFLAGS) --only-section=.kernel_code --only-section=.rodata kernel.elf kernel.bin
	mv kernel.bin kernel-$(pcm).bin

clean:
	rm -f *.bin *.o *.elf *.asm
