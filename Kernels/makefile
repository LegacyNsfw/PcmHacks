# Use toolchain from: https://github.com/haarer/toolchain68k
# Configure for m68k-elf target and install to default location
# Tested on EL6 and Fedora 28
#
pcm ?= P01
address ?= FF8000

PREFIX = /opt/crosschain/bin/m68k-elf-

CC = $(PREFIX)gcc
LD = $(PREFIX)ld
OBJCOPY = $(PREFIX)objcopy
OBJDUMP = $(PREFIX)objdump

CCFLAGS = -c -fomit-frame-pointer -std=gnu99 -mcpu=68332 -D$(pcm)
LDFLAGS =
DUMPFLAGS = -d -S
COPYFLAGS = -O binary

# PCM specific C files list
FILE_LIST = CFiles-$(pcm).list
_CFILES = $(shell col -b < ${FILE_LIST})

# PCM specific object file list from CFiles-$(pcm).list
OFILES = $(_CFILES:.c=.o)

# PCM specific Linker Script (.ld).
PCM_LDSCRIPT = SECTIONS { .text \(0x12340000\) : { main.o } .kernel_code :	{ Kernel-$(pcm).o \(.kernelstart\) \* \(.text\) } .kernel_data : { \* \(.kerneldata\) }}

all: Kernel-$(pcm).bin

%.o: %.c
	$(CC) $(CCFLAGS) $< -o $@

%.asm: %.bin
	$(OBJDUMP) $(DUMPFLAGS) -S $< -o $@

Kernel-$(pcm).elf: Kernel-$(pcm).o $(OFILES)
	@echo $(PCM_LDSCRIPT) > LinkerScript.tmp
	$(LD) $(LDFLAGS) --section-start .kernel_code=0x$(address) -T LinkerScript.tmp -o $@ Kernel-$(pcm).o $(OFILES)
	-@rm -f LinkerScript.tmp
	-$(OBJDUMP) $(DUMPFLAGS) $@ > $@.disassembly

Kernel-$(pcm).bin: Kernel-$(pcm).elf
	$(OBJCOPY) $(COPYFLAGS) --only-section=.kernel_code --only-section=.rodata Kernel-$(pcm).elf Kernel-$(pcm).bin

clean:
	@rm -f *.bin *.o *.elf *.asm *.disassembly *.tmp

