SECTIONS
{
	.text (0x12340000) :
	{
		main.o	
	}

	.kernel_ram (0xFF9000) :
	{
		read-kernel.o (.kernelstart)
		read-kernel.o (.text)
		common.o
	}
}