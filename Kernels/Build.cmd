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
* Revision Date: 03/01/2023 - Gampy <pcmhacking.net> Merged P04, swapped -p & -t, removed -r, reworked
*                                                    for ease of adding new kernels, see NOTES:.
* Revision Date: 03/25/2023 - Gampy <pcmhacking.net> Added Assembly Kernel and Kernel Loader.
*                                                    Added -l Kernel Loader Address.
*                                                    Added -x Build Assembly Kernel and or Loader.
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
* To add a new C Kernel,
*   1. Create the entry point C file named as "Kernel-<PCM>.c".
*      It must contain at least, the following:
*        int __attribute__((section(".kernelstart")))
*        KernelStart(void)
*        {
*          // Create main loop here.
*        }
*   2. Create a plain text "CFiles-<PCM>.list" for ancillary C files.
*      One dot c filename per line, no spaces, do not include Kernel-<PCM>.c.
*      Must contain at least main.c.
*   3. Add the new PCM to BuildAll.cmd.
*   4. Add the new PCM to .github/workflows/CheckBuild.yml (See note in CheckBuild.yml).
*
**********************************************************************
* Here we'll collect the routines (call :label / goto label).
*
**
* Help message
**
:Usage
  echo.
  echo   %0 -a^<address^> -c -d -g^<path^> -m -p^<pcm^> -t^<path^> -x
  echo.
  echo     -a^<address^>
  echo       Set base address for the kernel. (no space, in hex, no 0x)
  echo       Value: 0x%BASE_ADDRESS%
  echo.
  echo     -c
  echo       Set flag to copy Kernel-^<PCM^>.bin to PcmHammer's build directory or not.
  if defined COPY_BIN (
    echo       Value: True
  ) else (
    echo       Value: False
  )
  echo.
  echo     -d
  echo       Set flag to dump Kernel-^<PCM^>.elf ^> Kernel-^<PCM^>.disassembly or not.
  if defined DUMP_ELF (
    echo       Value: True
  ) else (
    echo       Value: False
  )
  echo.
  echo     -g^<path^>
  echo       Set path to GNU m68k bin directory. (no space)
  echo       Value: %GCC_LOCATION%
  echo.
  echo     -l^<address^>
  echo       Set base address for the Loader. (no space, in hex, no 0x)
  echo       Value: 0x%LOADER_ADDRESS%
  echo.
  echo     -m
  echo       Set flag to dump Kernel-^<PCM^>.map or not.
  if defined DUMP_MAP (
    echo       Value: True
  ) else (
    echo       Value: False
  )
  echo.
  echo     -p^<pcm^>
  echo       Set the PCM (P01 (P01 includes P59), P04, P10, P12, Micro, Read, Test). (no space)
  echo       Value: %PCM%
  echo.
  echo     -t^<path^>
  echo       Set target ^<path^> where to copy Kernel-^<PCM^>.bin. (no space)
  echo       Value: %BIN_LOCATION%
  echo.
  echo     -x
  echo       Set flag to build an assembly Kernel and or Kernel Loader.
  if defined ASSEMBLY_KERNEL (
    echo       Value: True
  ) else (
    echo       Value: False
  )
  echo.
  echo     /h
  echo     -h
  echo     --help
  echo       Display the Help Menu
  echo       TIP: Following your arguments with the help argument (as the last argument), it will show your values in help.
  echo.
  echo.
  echo     Examples:
  echo     %0
  echo     %0 -a%BASE_ADDRESS% -p%PCM%
  echo     %0 -a%BASE_ADDRESS% -p%PCM% -g%GCC_LOCATION% -t%BIN_LOCATION%
  echo.
  echo     P01 is default, thus all others require at least example 2
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

rem * -c Set default to copy Kernel-<PCM>.bin to PcmHammer's build directory.
set COPY_BIN=True

rem * -d Set default to not dump Kernel-<PCM>.elf > Kernel-<PCM>.disassembly
set DUMP_ELF=

rem * -g Set default path to m68k bin directory.
set GCC_LOCATION=C:\SysGCC\m68k-elf\bin\

rem * -l Set to build the Loader kernel (Non defined is default, do not modify).
set LOADER_ADDRESS=

rem * -m Set default to not dump memory map Kernel-<PCM>.map.
set DUMP_MAP=

rem * -p Set default PCM.
set PCM=P01

rem * -t Set default target <path> where to copy Kernel-<PCM>.bin.
set BIN_LOCATION=..\Apps\PcmHammer\bin\Debug\

rem * -x Set default not to build an assembly Kernel and or Kernel Loader.
set ASSEMBLY_KERNEL=


rem * Handle command line options.
(
  setlocal enabledelayedexpansion
  for %%A in (%*) do (
    set VAR=%%A
    if /i "!VAR:~0,2!" == "-a" set "BASE_ADDRESS=!VAR:~2!"
    if /i "!VAR!" == "-c"      set COPY_BIN=
    if /i "!VAR!" == "-d"      set DUMP_ELF=True
    if /i "!VAR:~0,2!" == "-g" set "GCC_LOCATION=!VAR:~2!"
    if /i "!VAR:~0,2!" == "-l" set "LOADER_ADDRESS=!VAR:~2!"
    if /i "!VAR!" == "-m"      set DUMP_MAP=True
    if /i "!VAR:~0,2!" == "-p" set "PCM=!VAR:~2!"
    if /i "!VAR:~0,2!" == "-t" set "BIN_LOCATION=!VAR:~2!"
    if /i "!VAR!" == "-x"      set ASSEMBLY_KERNEL=True
    if /i "!VAR!" == "/h"      goto Usage
    if /i "!VAR!" == "-h"      goto Usage
    if /i "!VAR!" == "--help"  goto Usage
  )
  setlocal disabledelayedexpansion
)


rem * Setup linker map dump
if defined DUMP_MAP (set "DUMPMAP=-Map Kernel-%PCM%.map")

rem * Ensure we have no trailing slash.
call :Detrailslash "%GCC_LOCATION%" GCC_LOCATION
call :Detrailslash "%BIN_LOCATION%" BIN_LOCATION

rem * Create PCM specific object file list from CFiles-<PCM>.list
(
  setlocal enabledelayedexpansion
  if exist CFiles-%PCM%.list (
    for /f %%A in (CFiles-%PCM%.list) do (
      set "OLIST=!OLIST! %%~nA.o"
    )
  )
  setlocal disabledelayedexpansion
)

rem *** All that for this ...

if not defined ASSEMBLY_KERNEL (
  rem ***
  rem *** C Kernel
  rem ***
  "%GCC_LOCATION%\m68k-elf-gcc.exe" -c -D=%PCM% -fomit-frame-pointer -std=gnu99 -mcpu=68332 -O0 Kernel-%PCM%.c @CFiles-%PCM%.list
  if %errorlevel% neq 0 goto :EOF

  rem * Create PCM specific Linker Script (.ld).
  rem *** WARNING! Redirections (>) break inside if statements and do ugly things to the environment!
  rem *** If you get brave and test the above mentioned issue, be sure to use a new command interpreter each time, for it does corrupt it!
  rem *** To get around this, we'll do it in another environment, thus no if statement, thus no issue.
  call CreateCKernelPCMSpecificLinkerScript.cmd %PCM%

  "%GCC_LOCATION%\m68k-elf-ld.exe" --section-start .kernel_code=0x%BASE_ADDRESS% -T LinkerScript.tmp %DUMPMAP% -o Kernel-%PCM%.elf Kernel-%PCM%.o %OLIST%
  if %errorlevel% neq 0 goto :EOF

  del /f LinkerScript.tmp

  "%GCC_LOCATION%\m68k-elf-objcopy.exe" -O binary --only-section=.kernel_code --only-section=.rodata Kernel-%PCM%.elf Kernel-%PCM%.bin
  if %errorlevel% neq 0 goto :EOF

  if defined DUMP_ELF (
    "%GCC_LOCATION%\m68k-elf-objdump.exe" -d -S Kernel-%PCM%.elf > Kernel-%PCM%.disassembly
    if %errorlevel% neq 0 goto :EOF
  )

  if defined COPY_BIN (
    echo Copying Kernel-%PCM%.bin -^> %BIN_LOCATION%\Kernel-%PCM%.bin
    copy Kernel-%PCM%.bin "%BIN_LOCATION%" 1>nul
  )
) else (

  rem ***
  rem *** Assembly Kernel and or Loader
  rem ***

  rem *** Builds the Assembly kernel
  "%GCC_LOCATION%\m68k-elf-gcc.exe" -c -D=%PCM% -fomit-frame-pointer -std=gnu99 -mcpu=68332 -O0 Kernel.S
  if %errorlevel% neq 0 goto :EOF

  "%GCC_LOCATION%\m68k-elf-ld.exe" --section-start .text=0x%BASE_ADDRESS% -T Kernel.ld %DUMPMAP% -o Kernel-%PCM%.elf Kernel.o
  if %errorlevel% neq 0 goto :EOF

  "%GCC_LOCATION%\m68k-elf-objcopy.exe" -O binary --only-section=.text --only-section=.data Kernel-%PCM%.elf Kernel-%PCM%.bin
  if %errorlevel% neq 0 goto :EOF

  if defined DUMP_ELF (
    "%GCC_LOCATION%\m68k-elf-objdump.exe" -d -S Kernel-%PCM%.elf > Kernel-%PCM%.disassembly
    if %errorlevel% neq 0 goto :EOF
  )

  if defined COPY_BIN (
    echo Copying Kernel-%PCM%.bin -^> %BIN_LOCATION%\Kernel-%PCM%.bin
    copy Kernel-%PCM%.bin "%BIN_LOCATION%" 1>nul
  )

  rem *** Handle the Kernel Loader
  if defined LOADER_ADDRESS (
    rem *** All that for this ...
    "%GCC_LOCATION%\m68k-elf-gcc.exe" -c -D=%PCM% -fomit-frame-pointer -std=gnu99 -mcpu=68332 -O0 Loader.S
    if %errorlevel% neq 0 goto :EOF

    "%GCC_LOCATION%\m68k-elf-ld.exe" --section-start .text=0x%LOADER_ADDRESS% -T Loader.ld %DUMPMAP% -o Loader-%PCM%.elf Loader.o
    if %errorlevel% neq 0 goto :EOF

    "%GCC_LOCATION%\m68k-elf-objcopy.exe" -O binary --only-section=.text --only-section=.data Loader-%PCM%.elf Loader-%PCM%.bin
    if %errorlevel% neq 0 goto :EOF

    if defined DUMP_ELF (
      "%GCC_LOCATION%\m68k-elf-objdump.exe" -d -S Loader-%PCM%.elf > Loader-%PCM%.disassembly
      if %errorlevel% neq 0 goto :EOF
    )

    if defined COPY_BIN (
      echo Copying Loader-%PCM%.bin -^> %BIN_LOCATION%\Loader-%PCM%.bin
      copy Loader-%PCM%.bin "%BIN_LOCATION%" 1>nul
    )
  )
)

