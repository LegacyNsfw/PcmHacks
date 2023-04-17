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

To build an Assembly Kernel.
Build.cmd -x -aFF8000 -pP01
Will build the P01 Kernel for loading at address FF8000 and not copy it anywhere, Clean.cmd will remove it ...
The dash x tells the build system to build the assembly version.

To build a Loader and Kernel
Build.cmd -x -aFF9090 -lFF9890 -pP04
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
Kernel.S              The Kernel
Kernel.ld             Linker Script specific to the Assembly Kernel
Loader.S              The Loader
Loader.ld             Linker Script specific to the Assembly Loader
Common-Assembly.h     Common elements, could not use Common.h due to conflict with the C Kernel
Readme-Assembly.txt   Readme
