@setlocal
@echo off

goto beginning
* Create a non parsed area for header, notes and routines (call :label).
**********************************************************************
*
* Name         : CreateCKernelPCMSpecificLinkerScript.cmd
* Description  : Creates a PCM specific Linker Script file (.ld), not meant for manual use!
* Author       : Gampy <pcmhacking.net>
* Authored Date: 03/25/2023
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
**********************************************************************
* Here we'll collect the routines (call :label / goto label).
*
**
*
*************************************** Beginning
* Let us get to it!
:beginning

if "%1"=="" goto :EOF
echo SECTIONS {	.text (0x12340000) :	{	main.o	}	.kernel_code :	{	Kernel-%1.o (.kernelstart)	* (.text)	}	.kernel_data :	{	* (.kerneldata)	}} > LinkerScript.tmp

