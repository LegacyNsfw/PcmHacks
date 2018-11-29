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

It might be possible to get gcc for m68k for Linux using the instructions
below, but I haven't tried that approach. They're from here:
http://daveho.github.io/2012/10/26/m68k-elf-cross-compiler.html

set -e
export M68KPREFIX=/home/dhovemey/linux/m68k-elf
export PATH=$M68KPREFIX/bin:$PATH
mkdir crossgcc
wget http://ftp.gnu.org/gnu/binutils/binutils-2.23.tar.gz
gunzip -c binutils-2.23.tar.gz | tar xvf -
wget http://ftp.gnu.org/gnu/gcc/gcc-4.4.2/gcc-core-4.4.2.tar.bz2
bunzip2 -c gcc-core-4.4.2.tar.bz2 | tar xvf -
mkdir build
cd build
mkdir binutils
mkdir gcc
cd binutils
../../binutils-2.23/configure --target=m68k-unknown-elf --prefix=$M68KPREFIX
make -j 3
make install
cd ../gcc
../../gcc-4.4.2/configure --target=m68k-unknown-elf --disable-libssp \
  --prefix=$M68KPREFIX
make -j 3
make install