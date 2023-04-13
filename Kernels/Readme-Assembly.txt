An Assembly Kernel for J1850VPW speaking PCM's that use a Motorola 68k Processor.

Currently read only.

Supported PCM's.
P01
P04
P10
P12
P59
E54

How to build the Assembly Kernels

To build a Kernel.
Build.cmd -x -aFF8000 -pP01
Will build the P01 Kernel for loading at address FF8000 and not copy it anywhere, Clean.cmd will remove it ...
The dash x tells the build system to build the assembly version.

To build a Loader kernel
Build.cmd -x -aFF9090 -lFF9890 -pP01
Will build the P04 Loader and Kernel.

If you want Build.cmd to copy the Kernel someplace
Build.cmd -x -c -tC:\Directory\Where\You\Want\It -aFF8000 -pP01

See Build.cmd -h for help and or other options ...

Load addresses
    -aFF8000 -pP01 (Includes P59)
    -aFF9090 -lFF9890 -pP04
    -aFFB800 -pP10
    -aFF2000 -pP12
    -aFF8F50 -pE54

Assembly kernel filelist
Loader.ld             Specific to the Assembly Loader
Kernel.S              The Kernel
Loader.S              The Loader
Readme-Assembly.txt   Readme
Common-Assembly.h     Common element, could not use Common.h due to conflict with the C Kernel
Kernel.ld             Specific to the Assembly Kernel
