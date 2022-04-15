@setlocal
@echo off

goto beginning
* Create a non parsed area for header, notes and routines (call :label).
**********************************************************************
*
* Name         : Build.cmd
* Description  : Build PcmHammer's kernel with options, most specifically the kernel base address.
* Author       : Gampy <pcmhacking.net>
* Authored Date: 11/16/2018
* Revision Date: 05/19/2020 - Gampy <pcmhacking.net> Cleanup for publication.
* Revision Date: 04/01/2022 - Gampy <pcmhacking.net> Added -t<PCM Type>, added -r dump kernel RAM map.
* Revision Date: 04/14/2022 - Gampy <pcmhacking.net> Fixed ld map dump.
*
* Authors disclaimer
*   It is what it is, you can do with it as you please. (with respect)
*
*   Just don't blame me if it teaches your computer to smoke!
*
*   -Enjoy
*
*
* NOTES:
*
*
**********************************************************************
* Here we'll collect the routines (call :label / goto label).
*
**
* Help message
**
:Usage
  echo.
  echo   %0 -a^<address^> -c -d -g^<path^> -m -p^<path^> -r -t^<pcm type^>
  echo.
  echo     -a^<address^>
  echo       Set base address for the kernel. (no space, in hex, no 0x)
  echo       Value: 0x%BASE_ADDRESS%
  echo.
  echo     -c
  echo       Set flag to copy kernel.bin to PcmHammer's build directory or not.
  if defined COPY_BIN (
    echo       Value: True
  ) else (
    echo       Value: False
  )
  echo.
  echo     -d
  echo       Set flag to dump kernel.elf ^> kernel.disassembly or not.
  if defined DUMP_ELF (
    echo       Value: True
  ) else (
    echo       Value: False
  )
  echo.
  echo     -g^<path^>
  echo       Set path to GNU m68k bin dir. (no space)
  echo       Value: %GCC_LOCATION%
  echo.
  echo     -m
  echo       Set flag to dump kernel Map or not.
  if defined DUMP_MAP (
    echo       Value: True
  ) else (
    echo       Value: False
  )
  echo.
  echo     -p^<path^>
  echo       Set path\^<filename^> where to copy kernel.bin. (no space)
  echo       Value: %BIN_LOCATION%
  echo.
  echo     -r
  echo       Set flag to dump kernel Ram or not.
  if defined DUMP_RAM (
    echo       Value: True
  ) else (
    echo       Value: False
  )
  echo.
  echo     -t^<pcm type^>
  echo       Set the PCM type (P01 (P01 includes P59), P10, P12). (no space)
  echo       Value: %PCMTYPE%
  echo.
  echo     /h
  echo     -h
  echo     --help
  echo       Display the Help Menu
  echo       TIP: Following your arguments with the help argument (as the last argument), it will show your values in help.
  echo.
  echo.
  echo     Example:
  echo     %0 -a%BASE_ADDRESS% -g%GCC_LOCATION% -p%BIN_LOCATION% -t%PCMTYPE%
  echo.
  goto :EOF
*
*
**
* Removes trailing slash if one exists.
**
:Detrailslash in out
  set A=%~1
  if %A:~-1%==\ (
    set %2=%A:~0,-1%
  ) else (
    set %2=%A%
  )
  goto :EOF
*
*
*************************************** Beginning
* Let us get to it!
:beginning

rem * Set option defaults here ...

rem * -a Set default base address for the kernel.
set BASE_ADDRESS=FF8000

rem * -c Set default to copy kernel.bin to PcmHammer's build directory.
set COPY_BIN=True

rem * -d Set default to NOT dump kernel.elf ^> kernel.disassembly
set DUMP_ELF=

rem * -g Set default path to m68k bin dir.
set GCC_LOCATION=C:\SysGCC\m68k-elf\bin\

rem * -m Set flag to dump kernel Map or not.
set DUMP_MAP=

rem * -p Set default path\^<filename^> where to copy kernel.bin.
set BIN_LOCATION=..\Apps\PcmHammer\bin\debug\

rem * -r Set flag to dump kernel Map or not.
set DUMP_RAM=

rem * -t Set default PCM type.
set PCMTYPE=P01


rem * Handle command line options.
(
  setlocal enabledelayedexpansion
  for %%A in (%*) do (
    set VAR=%%A
    if /i "!VAR:~0,2!" == "-a" set "BASE_ADDRESS=!VAR:~2!"
    if /i "!VAR!" == "-c"      set COPY_BIN=
    if /i "!VAR!" == "-d"      set DUMP_ELF=True
    if /i "!VAR:~0,2!" == "-g" set "GCC_LOCATION=!VAR:~2!"
    if /i "!VAR!" == "-m"      set DUMP_MAP=True
    if /i "!VAR:~0,2!" == "-p" set "BIN_LOCATION=!VAR:~2!"
    if /i "!VAR!" == "-r"      set DUMP_RAM=True
    if /i "!VAR:~0,2!" == "-t" set "PCMTYPE=!VAR:~2!"
    if /i "!VAR!" == "/h"      goto Usage
    if /i "!VAR!" == "-h"      goto Usage
    if /i "!VAR!" == "--help"  goto Usage
  )
  setlocal disabledelayedexpansion
)

rem * Setup linker map dump
if defined DUMP_MAP (set "DUMPMAP=-Map Kernel-%PCMTYPE%.map")

rem * Ensure we have no trailing slash.
call :Detrailslash "%GCC_LOCATION%" GCC_LOCATION
call :Detrailslash "%BIN_LOCATION%" BIN_LOCATION

rem *** All that for this ...
"%GCC_LOCATION%\m68k-elf-gcc.exe" -c -D=%PCMTYPE% -fomit-frame-pointer -std=gnu99 -mcpu=68332 -O0 main.c write-kernel.c crc.c common.c common-readwrite.c flash-intel.c flash-amd.c
if %errorlevel% neq 0 goto :EOF

"%GCC_LOCATION%\m68k-elf-ld.exe" --section-start .kernel_code=0x%BASE_ADDRESS% -T kernel.ld %DUMPMAP% -o Kernel-%PCMTYPE%.elf main.o write-kernel.o crc.o common.o common-readwrite.o flash-intel.o flash-amd.o
if %errorlevel% neq 0 goto :EOF

"%GCC_LOCATION%\m68k-elf-objcopy.exe" -O binary --only-section=.kernel_code --only-section=.rodata Kernel-%PCMTYPE%.elf Kernel-%PCMTYPE%.bin
if %errorlevel% neq 0 goto :EOF

if defined DUMP_ELF (
  "%GCC_LOCATION%\m68k-elf-objdump.exe" -d -S Kernel-%PCMTYPE%.elf > Kernel-%PCMTYPE%.disassembly
  if %errorlevel% neq 0 goto :EOF
)

if defined DUMP_RAM (
  "%GCC_LOCATION%\m68k-elf-objdump.exe" -s -j .kernel_data Kernel-%PCMTYPE%.elf > Kernel-%PCMTYPE%.ram
  if %errorlevel% neq 0 goto :EOF
)

if defined COPY_BIN (
  echo %BIN_LOCATION%\Kernel-%PCMTYPE%.bin
  copy Kernel-%PCMTYPE%.bin "%BIN_LOCATION%"
)
