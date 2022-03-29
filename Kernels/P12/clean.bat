@echo off
for %%A in (
  *.o
  *.disassembly
  *.out
  *.S
  *.bin
  *.elf
  *.log
  *.exe
  *.map
  ) do if exist %%A echo   Deleting %%A & del "%%A"

