@echo off
for %%A in (
  *.o
  *.disassembly
  *.out
  *.ram
  *.bin
  *.elf
  *.log
  *.exe
  *.map
  ) do if exist %%A echo   Deleting %%A & del "%%A"

