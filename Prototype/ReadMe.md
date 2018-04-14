This tool will support reading and writing the Operating System (OS) and Calibration GM 12200411 PCM and other GM PCMs with compatible hardware.

Note that editing the calibration will require another tool, such as [TunerPro](http://www.tunerpro.net/) or [Tactrix EcuFlash](http://www.tactrix.com/). And there is still work to be done on the definition files that will tell the editing software where the various tables and constants are in the firmware for each version of the several operating systems that GM used on the 411 PCM. To use tunerpro you will need an XDF definistion for your PCMs Operating System, often referred to as OSID. 

The most common 01/02 OSID is 12202088 the best current public XDF is http://www.gearhead-efi.com/Fuel-Injection/showthread.php?6086-TunerPro-comprehensive-XDF-for-0411-PCM-with-Checksums
The most common 03/04 OSID is 12225074 the best current public XDF is https://pcmhacking.net/forums/viewtopic.php?f=3&t=3845 More items can be requested in the thread
If you want to and can develop XDFs for other OSIDs please join us!



We are currently focusing on four OBD2 interfaces:

* [AVT-852](http://www.avt-hq.com/852_hw.htm) - This is the most expensive option, but this was one of the first interfaces to support J1850 VPW, and can be used today with the LS1 reading utility available at https://pcmhacking.net/forums/viewtopic.php?f=3&t=3111 Antus is migrating a lot of code from that tool to this with the help of NSFW and releasing it under the GPL.

* [ObdDiag AllPro](http://www.obddiag.net/products.html) - This is quite inexpensive, and will require a firmware update, but is likely to be the most popular choice.

* Any J2534-compatible interface that supports J1850 VPW should work. There are a few of those on the market, and we cannot test them all, but if you have one, give it a try. As time goes by we'll list the ones that are proven to work. The genuine GM MDIs are one device which is known to work, though it is not cheap. Cheaper options will be identified in the future.

* [Scantool MX or SX](https://www.scantool.net/obdlink-sx/) - Inexpensive, does not support 4x (high speed) mode, and not yet proven, but this will probably work. 2018-04-14 it has been found to be as functional as the other options.

Updating the firmware on the AllPro adapter requires the [FlashMagic](http://www.flashmagictool.com/download.html) tool for now. Future versions may come with the required firmware installed. The manufacturer has expressed interest in incorporating the changes into the build that gets flashed to new devices before they are sold, but that hasn't happened yet. We will send a pull request after this app is working and we have been able to test it end to end. The latest firmware source code is available [here](https://github.com/antuspcm/allpro/tree/4xj1850) and binary versions will be available soon.

