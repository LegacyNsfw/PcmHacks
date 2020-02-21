PCM Hammer supports reading and writing the Operating System (OS) and Calibration of General Motors P01 and P59 Powertrain Control Modules (PCMs). The 12200411 is best known variant, but there are several and we aim to support all of them.

PCM Logger supports logging from the same PCMs. 

VPW Explorer is intended for developers rather than for end users. It's basically just a sandbox for testing new ideas.

PcmLibrary contains core logic for the applications. While the applications currently only run on Windows, this probably should work on any operating system that has a .Net Core implementation (Mac, Linux, Android).

PcmLibraryWindowsForms contains Windows-specific functionality like serial ports and J2534 support. 
