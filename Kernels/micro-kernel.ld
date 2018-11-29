SECTIONS
{
	.text (0x12340000) :
	{
		main.o	
	}

	.kernel_ram (0xFF9000) :
	{
		micro-kernel.o (.kernelstart)
		micro-kernel.o (.text)
	}
}