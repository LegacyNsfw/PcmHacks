SECTIONS
{
    .text :
    {
      *(.text)
    }

    .data :
    {
      *(.data)
    }

    .bss :
    {
      *(.bss)
    }

    .kernel_data :
    {
      Kernel.o (.kerneldata)
    }
}

