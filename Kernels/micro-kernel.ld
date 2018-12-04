SECTIONS
{
	.text (0x12340000) :
	{
		main.o	
	}

	.kernel_data (0xFFC000) :
	{
		micro-kernel.o (.kerneldata)
	}

	.kernel_code (0xFF9000) :
	{
		micro-kernel.o (.kernelstart)
		micro-kernel.o (.text)
	}
}