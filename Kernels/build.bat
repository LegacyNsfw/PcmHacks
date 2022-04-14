@echo off
: -fomit-frame-pointer = remove the boilerplate linkw/unlk instructions
: -c    = compile but do not link
: -O1   = optimization level
: -g    = include debug information - not using this because the
:         disassembly is either corrupt or just incomprehensible

if -%KernelBuild%- == -- call fixpath.bat

call clean.cmd

rem * Please See: Build.cmd -h
call BuildAll.cmd

c:\mingw\bin\g++ -o test.exe test.cpp crc.c

dir *.bin
pause

