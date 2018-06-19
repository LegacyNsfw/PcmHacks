copy ..\byteswap\bin\debug\byteswap.exe .

byteswap.exe byteswap.exe
byteswap.exe byteswap.exe.swap
fc /b byteswap.exe byteswap.exe.swap.swap
