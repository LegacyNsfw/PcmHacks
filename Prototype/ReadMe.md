This tool will support reading and writing the firmware of the GM 12200411 EEPROM.  

Note that editing the firmware will require another tool, such as [TunerPro](http://www.tunerpro.net/) or [Tactrix EcuFlash](http://www.tactrix.com/). And there is still work to be done on the definition files that will tell the editing software where the various tables and constants are in the firmware for each version of the several operating systems that GM used on the 411 PCM.

We are currently focusing on four OBD2 interfaces:

* [AVT-852](http://www.avt-hq.com/852_hw.htm) - This is the most expensive option, but this was one of the first interfaces to support J1850 VPW, and can be used today with the LS1 reading utility available at http://pcmhacking.net/.

* [ObdDiag AllPro](http://www.obddiag.net/products.html) - This is quite inexpensive, and will require a firmware update, but is likely to be the most popular choice.

* Any J2534-compatible interface that supports J1850 VPW should work. There are a few of those on the market, and we cannot test them all, but if you have one, give it a try. As time goes by we'll list the ones that are proven to work.

* [Scantool MX or SX](https://www.scantool.net/obdlink-sx/) - Inexpensive, does not support 4x (high speed) mode, and not yet proven, but this will probably work.

Updating the firmware on the AllPro adapter requires the [FlashMagic](http://www.flashmagictool.com/download.html) tool for now. Future versions may come with the required firmware installed. The manufacturer has expressed interest in incorporating the changes into the build that gets flashed to new devices before they are sold, but that hasn't happened yet. The latest firmware source code is available [here](https://github.com/antuspcm/allpro/tree/4xj1850) and binary versions will be available soon. 

