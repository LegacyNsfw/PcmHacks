///////////////////////////////////////////////////////////////////////////////
// Shared flash code.
///////////////////////////////////////////////////////////////////////////////

#include "common.h"
#include "flash.h"

///////////////////////////////////////////////////////////////////////////////
// Unlock the flash memory.
//
// We tried having separate commands for lock and unlock, however the PCM's
// VPW signal becomes noisy while the flash is unlocked. The AVT was able to
// deal with that, but the ScanTool and AllPro interfaces couldn't read the
// signal at all.
//
// So instead of VPW commands to unlock and re-lock, we just unlock during the
// erase and write operations, and re-lock when the operation is complete.
//
// These commands remain supported, just in case we find a way to use them.
///////////////////////////////////////////////////////////////////////////////
void FlashUnlock()
{
	SIM_CSBARBT = 0x0006;
	SIM_CSORBT = 0x6820;
	SIM_CSBAR0 = 0x0006;
	SIM_CSOR0 = 0x7060;

	// TODO: can we just |= HARDWAREIO?
	unsigned short hardwareFlags = HARDWARE_IO;
	WasteTime();
	hardwareFlags |= 0x0001;
	WasteTime();
	HARDWARE_IO = hardwareFlags;

	VariableSleep(0x50);
}

///////////////////////////////////////////////////////////////////////////////
// Lock the flash memory.
//
// See notes above.
///////////////////////////////////////////////////////////////////////////////
void FlashLock()
{
	SIM_CSBARBT = 0x0006;
	SIM_CSORBT = 0x6820;
	SIM_CSBAR0 = 0x0006;
	SIM_CSOR0 = 0x1060;

	unsigned short hardwareFlags = HARDWARE_IO;
	hardwareFlags &= 0xFFFE;
	WasteTime();
	WasteTime();
	HARDWARE_IO = hardwareFlags;

	VariableSleep(0x50);
}

