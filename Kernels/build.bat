@echo off
: -fomit-frame-pointer = remove the boilerplate linkw/unlk instructions
: -c    = compile but do not link
: -O1   = optimization level
: -g    = include debug information - not using this because the
:         disassembly is either corrupt or just incomprehensible

if -%KernelBuild%- == -- call fixpath.bat

call clean.bat

rem * Please See: Build.cmd -h
rem * Build.cmd defaults to P01 (and P59).
rem * Example P01: Build.cmd
rem * Example P10: Build.cmd -aFFB800 -tP10
rem * Example P12: Build.cmd -aFF2000 -tP12
call Build.cmd -m -d

c:\mingw\bin\g++ -o test.exe test.cpp crc.c

dir *.bin

