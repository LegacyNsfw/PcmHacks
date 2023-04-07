To build the Kernels you'll need the gcc-m68k toolchain for Windows available here: http://gnutoolchains.com/m68k-elf/

The gcc-m68k toolchain needs to be installed to the installation default location of C:\SysGCC\m68k-elf or you need to
point to the location of m68k-elf-gcc.exe with the -g command line parameter (See: Build.cmd -h), allowing it to add itself to
the PATH is your choice, Build.cmd does not require it.

Build.cmd encapsulates the options used to build the binaries on Windows.

See Build.cmd -h or BuildAll.cmd -h for help.
Typical single Kernel usage: Build.cmd -aFF8000 -pP01

Build.cmd <- .cmd is important!

BuildAll.cmd wraps Build.cmd in a loop building all supported Kernels, all options in Build.cmd are available in BuildAll.cmd.

To build all supported Kernels simple run: BuildAll.cmd

--

build.bat has been superseded by Build.cmd, use of build.bat is not recommended!

fixpath.bat is a system dependent batch file and part of build.bat and is not recommended for use.

gcc.bat was used for testing command line options.

test.cpp was used for crc testing and development, it is not part of the Kernels and unnecessary to build.

--

The Kernels can also be built on Unix/Linux using the gcc-m68k toolchain that can be built on most Unix/Linux
systems using: https://github.com/haarer/toolchain68k.

There are pre-built binaries for some distros available as gcc-m68k-linux-gnu.

If you do not use the default install location, PREFIX can be used to point to the location used.

You will need to move Kernels-*.bin to the PcmHammer directory.

$ cd Kernels
$ make clean

$ make pcm=P01 address=FF8000
$ make clean

$ make pcm=P04 address=FF9090
$ make clean

$ make pcm=P10 address=FFB800
$ make clean

$ make pcm=P12 address=FF2000
$ make clean

