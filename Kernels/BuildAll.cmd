@setlocal
@echo off

goto beginning
* Create a non parsed area for header, notes and routines (call :label).
**********************************************************************
*
* Name         : BuildAll.cmd
* Description  : Build All of PcmHammer's kernels.
* Author       : Gampy <pcmhacking.net>
* Authored Date: 04/11/2022
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
**
*
*************************************** Beginning
* Let us get to it!
:beginning

rem * Handle command line options
rem * Block invalid command line arguments -a and -t, they cannot be used in this context.
rem * They would need to be changed below.
(
  setlocal enabledelayedexpansion
  for %%A in (%*) do (
    set VAR=%%A
    if /i "!VAR:~0,2!" == "-a" echo Invalid argument & goto :EOF
    if /i "!VAR:~0,2!" == "-t" echo Invalid argument & goto :EOF
  )
  setlocal disabledelayedexpansion
)


for %%A in (
  "-aFF8000 -tP01",
  "-aFFB800 -tP10",
  "-aFF2000 -tP12"
  ) do call Build.cmd %%~A %*

rem * Experimental alpha quality P04 read kernel
cd P04
call kernel-p04.bat
cd ..

