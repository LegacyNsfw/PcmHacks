
cd kernels
call build.bat
cd ..

copy Apps\PcmLogger\bin\Debug\PcmLogger.exe Apps\PcmHammer\bin\Debug
copy Apps\PcmLogger\bin\Debug\*.profile Apps\PcmHammer\bin\Debug
copy Apps\PcmLogger\bin\Debug\*.dll Apps\PcmHammer\bin\Debug