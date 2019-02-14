﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PcmHacking
{
    public static class FlashChips
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chipId"></param>
        /// <returns></returns>
        public static IList<MemoryRange> GetMemoryRanges(UInt32 chipId, ILogger logger)
        {
            IList<MemoryRange> result = null;

            switch (chipId)
            {
                // This is only here as a warning to anyone adding ranges for another chip.
                // Please read the comments carefully. See case 0x00894471 for the real deal.
                case 0xFFFF4471:
                    var unused = new MemoryRange[]
                    {
                        // These numbers and descriptions are straight from the data sheet. 
                        // Notice that if you convert the hex sizes to decimal, they're all
                        // half as big as the description indicates. That's wrong. It doesn't
                        // work that way in the PCM, so this would only compare 256 kb.
                        new MemoryRange(0x30000, 0x10000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x20000, 0x10000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x10000, 0x10000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x04000, 0x0C000, BlockType.Calibration), //  96kb main block 
                        new MemoryRange(0x03000, 0x01000, BlockType.Parameter), //   8kb parameter block
                        new MemoryRange(0x02000, 0x01000, BlockType.Parameter), //   8kb parameter block
                        new MemoryRange(0x00000, 0x02000, BlockType.Boot), //  16kb boot block                        
                    };
                    return null;

                // Intel 28F400B
                case 0x00894471:
                    result = new MemoryRange[]
                    {
                        // All of these addresses and sizes are all 2x what's listed 
                        // in the data sheet, because the data sheet table assumes that
                        // "bytes" are 16 bits wide. Which means they're not bytes. But
                        // the data sheet calls them bytes.
                        new MemoryRange(0x60000, 0x20000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x40000, 0x20000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x20000, 0x20000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x08000, 0x18000, BlockType.Calibration), //  96kb main block 
                        new MemoryRange(0x06000, 0x02000, BlockType.Parameter), //   8kb parameter block
                        new MemoryRange(0x04000, 0x02000, BlockType.Parameter), //   8kb parameter block
                        new MemoryRange(0x00000, 0x04000, BlockType.Boot), //  16kb boot block
                    };
                    break;

                default:
                    logger.AddUserMessage(
                        "Unsupported flash chip ID " + chipId.ToString("X8") + ". " +
                        Environment.NewLine +
                        "The flash memory in this PCM is not supported by this version of PCM Hammer." +
                        Environment.NewLine +
                        "Please look for a thread about this at pcmhacking.net, or create one if necessary." +
                        Environment.NewLine +
                        "We do aim to support for all flash chips over time.");
                    return null;
            }

            // Sanity check the memory ranges
            UInt32 lastStart = UInt32.MaxValue;
            for (int index = 0; index < result.Count; index++)
            {
                if (index == 0)
                {
                    UInt32 top = result[index].Address + result[index].Size;
                    if ((top != 512 * 1024) && (top != 1024 * 1024))
                    {
                        throw new InvalidOperationException("Upper end of memory range must be 512k or 1024k.");
                    }
                }

                if (index == result.Count - 1)
                {
                    if (result[index].Address != 0)
                    {
                        throw new InvalidOperationException("Memory ranges must start at zero.");
                    }
                }

                if (lastStart != UInt32.MaxValue)
                {
                    if (lastStart != result[index].Address + result[index].Size)
                    {
                        throw new InvalidDataException("Top of this range must match base of range above.");
                    }
                }

                lastStart = result[index].Address;
            }

            return result;
        }
    }
}