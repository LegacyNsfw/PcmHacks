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
* Revision Date: 03/01/2023 - Merged P04
* Revision Date: 03/25/2023 - Gampy <pcmhacking.net> Updated for new Assembly Kernels and Loaders.
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
rem * Block invalid command line arguments -a, -l and -p, they cannot be used in this context.
rem * They would need to be changed below.
(
  setlocal enabledelayedexpansion
  for %%A in (%*) do (
    set VAR=%%A
    if /i "!VAR:~0,2!" == "-a" echo Invalid argument & goto :EOF
    if /i "!VAR:~0,2!" == "-l" echo Invalid argument & goto :EOF
    if /i "!VAR:~0,2!" == "-p" echo Invalid argument & goto :EOF
    if /i "!VAR!" == "/h"      Build.cmd %*
    if /i "!VAR!" == "-h"      Build.cmd %*
    if /i "!VAR!" == "--help"  Build.cmd %*
  )
  setlocal disabledelayedexpansion
)


for %%A in (
  "-pP01 -aFF8000",
  "-pP04 -aFF9090 -lFF9890 -x"
  "-pP10 -aFFB800",
  "-pP12 -aFF2000",
  "-pE54 -aFF8F50 -x"
  ) do call Build.cmd %%~A %*

