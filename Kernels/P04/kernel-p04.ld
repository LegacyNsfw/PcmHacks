SECTIONS
{
	.text (0x12340000) :
	{
		main.o	
	}
	
	.kernel_code (0xFF9090) :
	{
		kernel-p04.o (.kernelstart)
		kernel-p04.o (.text)
	}

	.kernel_data (0xFF9A00) :
	{
		kernel-p04.o (.kerneldata)
	}
}
