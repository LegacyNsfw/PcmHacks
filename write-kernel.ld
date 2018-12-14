SECTIONS
{
	.text (0x12340000) :
	{
		main.o	
	}

	.kernel_data (0xFFC000) :
	{
		write-kernel.o (.kerneldata)
		common.o (.kerneldata)
	}

	.kernel_code (0xFF8000) :
	{
		write-kernel.o (.kernelstart)
		write-kernel.o (.text)
		common.o (.text)
		common-readwrite.o (.text)
	}
}