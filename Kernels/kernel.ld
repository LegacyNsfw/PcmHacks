SECTIONS
{
	.text (0x12340000) :
	{
		main.o
	}

	.kernel_data (0xFFA800) :
	{
		write-kernel.o (.kerneldata)
		common.o (.kerneldata)
		crc.o (.kerneldata)
	}

	.kernel_code (0xFF8000) :
	{
		write-kernel.o (.kernelstart)
		write-kernel.o (.text)
		crc.o (.text)
		common.o (.text)
		common-readwrite.o (.text)
		flash.o (.text)
		flash-intel.o (.text)
		flash-amd.o (.text)
	}
}
