///////////////////////////////////////////////////////////////////////////////
// Code that will be useful for more than one type of kernel.
///////////////////////////////////////////////////////////////////////////////

// Before everything else...
void Startup();

// Timing and watchdog
void ScratchWatchdog() __attribute__((optimize("-O0")));
void LongSleepWithWatchdog(int iterations);
void WasteTime();

// VPW send/receive
int ReadMessage();
extern char inputBuffer[];

void SendToolPresent();
void SendMessage(char* message, int length);

// Messages all kernels must handle
int ProcessResetRequest();

// After everything else...
void Reboot();


