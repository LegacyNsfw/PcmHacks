copy ..\bin2hex\bin\debug\bin2hex.exe .
copy ..\hex2bin\bin\debug\hex2bin.exe .

bin2hex.exe bin2hex.exe
hex2bin bin2hex.exe.hex.txt
fc /b bin2hex.exe bin2hex.exe.hex.txt.bin
