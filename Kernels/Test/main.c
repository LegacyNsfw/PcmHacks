void Test();

int 
__attribute__((section (".kernelram")))
_start(void)
{
    Test();
}

void 
__attribute__((section (".kernelram")))
Test()
{
}
