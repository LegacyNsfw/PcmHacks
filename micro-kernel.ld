SECTIONS
{
	.text (0x12340000) :
	{
		main.o	
	}

	.kernel_data (0xFFA000) :
	{
		micro-kernel.o (.kerneldata)
	}

	.kernel_code (0xFF8000) :
	{
		micro-kernel.o (.kernelstart)
		micro-kernel.o (.text)
	}
}