///////////////////////////////////////////////////////////////////////////////
// A kernel to read ROM data and send it to the application.
///////////////////////////////////////////////////////////////////////////////

#include "common.h"

void ProcessReadRequest()
{
	inputBuffer[0] = 0;
	inputBuffer[1] = 0;
	inputBuffer[2] = 0;
	inputBuffer[3] = 0;
}

int 
__attribute__((section(".kernelstart")))
KernelStart(void)
{
	Startup();

	for (int i = 0; i < 0x300000; i++)
	{
		int messageLength = ReadMessage();
		switch (inputBuffer[3])
		{
		case 0x35:
			ProcessReadRequest();
			break;

		case 0x20:
			if (ProcessResetRequest())
			{
				Reboot();
			}
			break;
		}
	}
}

