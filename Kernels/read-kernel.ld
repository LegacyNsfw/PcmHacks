SECTIONS
{
	.text (0x12340000) :
	{
		main.o	
	}

	.kernel_data (0xFFC000) :
	{
		read-kernel.o (.kerneldata)
		common.o (.kerneldata)
	}

	.kernel_code (0xFF9000) :
	{
		read-kernel.o (.kernelstart)
		read-kernel.o (.text)
		common.o (.text)
	}
}