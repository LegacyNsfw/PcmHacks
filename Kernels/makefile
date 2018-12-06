# Work in progress - untested until I can get a 68k toolchain working. Probably that means https://github.com/haarer/toolchain68k
PREFIX = ./freescale-coldfire-2011.09/bin/m68k-linux-gnu-

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

micro-kernel.elf: micro-kernel.o main.o
	$(LD) $(LDFLAGS) -T micro-kernel.ld main.o micro-kernel.o -o $@

micro-kernel.bin: micro-kernel.elf
	$(OBJCOPY) $(COPYFLAGS) --only-section=.kernel_code --only-section=.rodata micro-kernel.elf micro-kernel.bin

read-kernel.elf: read-kernel.o main.o
	$(LD) $(LDFLAGS) -T read-kernel.ld main.o read-kernel.o -o $@

read-kernel.bin: read-kernel.elf
	$(OBJCOPY) $(COPYFLAGS) --only-section=.kernel_ram read-kernel.elf read-kernel.bin

clean:
	rm -f *.bin *.o *.elf *.asm
