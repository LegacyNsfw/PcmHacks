The goal here is to create PCM read and write kernels using C rather than assembly.

At this point I'm just trying to create a tool chain and validate the concept.

build.bat encapsulates the options needed to convert C code into kernel binaries.
(Perhaps this should use a makefile, but that would require installing make.)

gcc.bat is mostly just for experimenting with gcc options before
moving those options into the build.bat script.

disasm.bat can be run on .o files or a.out to inspect the contents.

The build script produces a .S file, and the SRecordToKernel.exe utility 
uses the .S file to create a .bin file which contains only the kernel, and
none of the boilerplate code for Windows/Linux executables.
The source code to the SRecordToKernel utility is in this same repository,
in the DevTools\SRecordToKernel directory.

--

The GCC-m68k toolchain for Windows is available here:
http://gnutoolchains.com/m68k-elf/

--

The gcc toolchain for linux can be built on most linux distros with https://github.com/haarer/toolchain68k

You will need to comment out avr in the build scripts then uncomment m68k-elf

You may also want to change the install path to /usr/local/bin/ otherwise update the prefix in the makefile to suit your location
