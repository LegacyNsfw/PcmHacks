This tool supports reading and writing the Operating System (OS) and Calibration of General Motors P01 and P59 Powertrain Control Modules (PCMs). The 12200411 is best known variant, but there are several and we aim to support all of them.

We are currently focusing on four OBD2 interfaces:

* [AVT-852](http://www.avt-hq.com/852_hw.htm) - This is the most expensive option, but this was one of the first interfaces to support J1850 VPW, and can be used today with the LS1 reading utility available at https://pcmhacking.net/forums/viewtopic.php?f=3&t=3111 Antus is migrating a lot of code from that tool to this with the help of NSFW and releasing it under the GPL.

* [ObdDiag AllPro](http://www.obddiag.net/products.html) - Only the USB and dev-board versions are supported (not the bluetooth version, unfortunately). The AllPro devices were quite inexpensive, but unfortunately they are no longer in production.

	(ObdDiag has been a great partner in this project, incorporating changes and responding to feedback. However this means that AllPro devices purchased before early June 2018 will need a firmware update to version 1.21 or later. Updating the firmware on the AllPro adapter requires the [FlashMagic](http://www.flashmagictool.com/download.html) tool. Instructions are [here](http://www.obddiag.net/allpro_prog.html) and a link to the firmware can be found [here](http://www.obddiag.net/allpro.html).)

* Any J2534-compatible interface that supports J1850 VPW should work. There are a few of those on the market, and we cannot test them all, but if you have one, give it a try. As time goes by we'll list the ones that are proven to work. The genuine GM MDIs are one device which is known to work, though it is not cheap. Cheaper options will be identified in the future.

* [Scantool MX or SX](https://www.scantool.net/obdlink-sx/) - These are inexpensive and superior to ELM knock-off devices, however they do not support 4x (high speed) mode, so a full read of a 512kb PCM takes a little over 20 minutes. 

    Other devices based on Scantool's interface chip will probably work just as well (and just as slowly) but we have not tested any ourselves.

Note that ELM-based devices are not likely to work with this app. That is partly because most ELM-based devices on the market today are poor-quality imitations from shady manufacturers and resellers. If you have one, and it does not work, we recommend choosing a device from the list above instead.

If you have a device not listed above, feel free to try it. If it works, please let us know the specific manufacturer and model number, so that we can add it to the list. (Thanks in advance!)

What about editing?  

Editing the calibration will require another tool, such as [TunerPro](http://www.tunerpro.net/) or [Tactrix EcuFlash](http://www.tactrix.com/). There is still work to be done on the definition files that will tell the editing software where the various tables and constants are located with the firmware for each version of the several operating systems that GM used on the 411 PCM. 

To use tunerpro you will need an XDF definition for your PCM's Operating System, often referred to by its OSID.  To use EcuFlash you will have to convert an existing XDF to EcuFlash's XML format. (There are no tools for this yet, but maybe soon...)

* The most common OSID for 2001 and 2002 PCMs is 12202088, and the best current public XDF is available here: http://www.gearhead-efi.com/Fuel-Injection/showthread.php?6086-TunerPro-comprehensive-XDF-for-0411-PCM-with-Checksums  

* Some 01/02 vehicles were reflashed to OSID 12593358 after purchase. There is not yet an XDF for this OSID but we're working on it.  

* The most common OSID for 2003 and 2004 model years is 12225074, and the best current public XDF is available here: https://pcmhacking.net/forums/viewtopic.php?f=3&t=3845 More items can be requested in the thread. 

If you want to develop XDFs for other OSIDs please join us!  



