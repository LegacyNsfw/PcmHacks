SECTIONS
{
	.text (0x12340000) :
	{
		main.o	
	}

	.kernel_data (0xFFA000) :
	{
		test-kernel.o (.kerneldata)
		common.o (.kerneldata)
	}

	.kernel_code (0xFF8000) :
	{
		test-kernel.o (.kernelstart)
		test-kernel.o (.text)
		common.o (.text)
	}
}