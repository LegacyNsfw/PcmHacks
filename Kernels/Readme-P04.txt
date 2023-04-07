This P04 kernel is a work in progress.
It's currently NOT able to read or write a P04 PCM.
It can _almost_ read. The DLC code sometimes drops packets from the PC.
It sometimes truncates transmitted packets and/or crashes the PCM.
On Antus's P04 it reads until the around the 420kb mark and crashes during this packet, perhaps because of so many 5s in it which are 01010101 in binary?

[11:31:57:246]  RX: 6D F0 10 36 01 04 00 07 60 00 02 B8 02 B8 02 B8 02 B8 02 B8 02 B8 02 B8 02 B8 02 B8 02 B8 05 71 05 71 05 71 08 29 08 29 08 29 10 00 10 00 10 00 10 00 10 00 10 00 10 00 10 00 10 00 00 00 01 33 00 00 07 B3 40 00 00 00 40 00 00 00 40 00 00 00 40 00 00 00 40 00 00 00 40 00 00 00 40 00 00 00 40 00 00 00 40 00 00 00 40 00 00 00 00 60 41 89 00 0A 3D 71 00 60 41 89 00 0A 3D 71 02 00 41 99 99 9A 3E 66 66 66 50 00 00 00 2C CC CC CD 09 99 99 9A 03 20 00 10 62 43 4E 14 7A E1 4E 14 7A E1 2D 70 A3 D7 2D 70 A3 D7 1F 00 18 00 05 00 03 00 80 00 80 00 13 00 10 00 01 00 00 66 FA 00 FA 00 06 00 03 00 00 CD 00 66 02 80 01 80 00 00 00 00 00 00 00 00 00 00 08 00 12 C0 2D 70 A3 D7 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 00 01 89 37 01 FF 00 14 00 14 00 14 00 14 00 14 00 08 00 08 00 08 00 06 00 06 00 06 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 2F AE 2F AE 2F AE 2F AE 2F AE 2F AE 2F AE 2F AE 2F AE 2F AE 2F AE 00 14 00 12 00 10 00 0E 00 0C 00 0A 00 08 00 06 00 06 00 06 00 04 00 04 00 04 00 04 00 04 00 04 00 04 00 04 00 04 00 42 00 42 10 00 10 00 00 10 0D E6 70 B2 6A EF 65 2D 5A A5 52 14 4A F2 43 C8 3C 0C 24 81 1F AB 1A 23 16 76 12 E2 10 20 0E 1F 0C 53 0A F1 09 E1 08 E9 08 0F 07 49 06 84 05 C3 05 11 04 AD 04 1F 03 B3 03 73 04 00 04 80 05 00 05 33 05 4D 05 80 05 80 05 80 05 80 40 00 2A AB 2A AB 2A AB 2A AB 28 00 25 55 22 AB 20 00 1D 55 1A AB 18 00 15 55 15 55 15 55 15 55 15 55 15 55 15 55 15 55 15 55 15 55 15 55 15 55 15 55 15 55 15 55 15 55 15 55 15 55 15 55 15 55 15 55 15 55 15 55 15 55 15 55 15 55 15

This Kernel comes from initial work by Gampy@PCMHacking, who was able to make the pcmhammer micro kernel respond with a version string on a P04.
Antus merged code shared by the other PCMs, with modifications until it was able to send a block transfer on the P04.
The P04 code may end up working on the other PCMs but at the moment its different enough that it needs to stay out of the main tree.

There are still significant challenges that will need to be solved before writing a P04 is possible, even after this kernel can reliably read.
If you have the skills please tweak and test, and send us a pull request with any progress you can make.

- Antus@pcmhacking.net 2022-20-25
