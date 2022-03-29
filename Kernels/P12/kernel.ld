SECTIONS
{
	.text (0x12340000) :
	{
		main.o
	}

	.kernel_code :
	{
		write-kernel.o (.kernelstart)
		write-kernel.o (.text)
		crc.o (.text)
		common.o (.text)
		common-readwrite.o (.text)
		flash-intel.o (.text)
		flash-amd.o (.text)
	}

	.kernel_data :
	{
		write-kernel.o (.kerneldata)
		common.o (.kerneldata)
		crc.o (.kerneldata)
	}
}
