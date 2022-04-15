using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PcmHacking
{
    public class FlashChip
    {
        /// <summary>
        /// Flash chip ID discovered by the kernel.
        /// </summary>
        public UInt32 ChipId { get; private set; }

        /// <summary>
        /// Flash chip size (512kb or 1mb).
        /// </summary>
        public UInt32 Size { get; private set; }

        /// <summary>
        /// Memory ranges for erasing and rewriting.
        /// </summary>
        public ICollection<MemoryRange> MemoryRanges { get; private set; }

        /// <summary>
        /// Flash chip description (manufacturer, model, size).
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Constructor. Just stores data, all of the interesting stuff is in the factory method.
        /// </summary>
        protected FlashChip(UInt32 chipId, string description, UInt32 size, ICollection<MemoryRange> memoryRanges)
        {
            this.ChipId = chipId;
            this.Description = description;
            this.Size = size;
            this.MemoryRanges = memoryRanges;
        }

        /// <summary>
        /// Returns the chip description.
        /// </summary>
        public override string ToString()
        {
            return this.Description;
        }

        /// <summary>
        /// Factory method. Selects the memory configuration, size, and description.
        /// </summary>
        public static FlashChip Create(UInt32 chipId, ILogger logger)
        {
            IList<MemoryRange> memoryRanges = null;
            string description;
            UInt32 size;

            switch (chipId)
            {
                // This is only here as a warning to anyone adding ranges for another chip.
                // Please read the comments carefully. See case 0x00894471 for the real data.
                case 0xFFFF4471:
                    var unused = new MemoryRange[]
                    {
                        // These numbers and descriptions are straight from the intel 28F400B data sheet.
                        // Notice that if you convert the 16 bit word sizes to decimal, they're all
                        // half as big as the description here, in bytes, indicates.
                        new MemoryRange(0x30000, 0x10000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x20000, 0x10000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x10000, 0x10000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x04000, 0x0C000, BlockType.Calibration), //  96kb main block
                        new MemoryRange(0x03000, 0x01000, BlockType.Parameter), //   8kb parameter block
                        new MemoryRange(0x02000, 0x01000, BlockType.Parameter), //   8kb parameter block
                        new MemoryRange(0x00000, 0x02000, BlockType.Boot), //  16kb boot block
                    };
                    throw new InvalidOperationException("This flash chip ID was not supposed to exist in the wild.");

                // Intel 28F800B
                case 0x0089889D:
                    size = 1024 * 1024;
                    description = "Intel 28F800B, 1mb";
                    memoryRanges = new MemoryRange[]
                    {
                        // These addresses are for a bottom fill chip (B) in byte mode (not word)
                        new MemoryRange(0xE0000, 0x20000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0xC0000, 0x20000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0xA0000, 0x20000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x80000, 0x20000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x60000, 0x20000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x40000, 0x20000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x20000, 0x20000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x08000, 0x18000, BlockType.Calibration), //  96kb main block 
                        new MemoryRange(0x06000, 0x02000, BlockType.Parameter), //   8kb parameter block
                        new MemoryRange(0x04000, 0x02000, BlockType.Parameter), //   8kb parameter block
                        new MemoryRange(0x00000, 0x04000, BlockType.Boot), //  16kb boot block
                    };
                    break;

                // Intel 28F400B
                case 0x00894471:
                    size = 512 * 1024;
                    description = "Intel 28F400B, 512kb";
                    memoryRanges = new MemoryRange[]
                    {
                        // These addresses are for a bottom fill chip (B) in byte mode (not word)
                        new MemoryRange(0x60000, 0x20000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x40000, 0x20000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x20000, 0x20000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x08000, 0x18000, BlockType.Calibration), //  96kb main block 
                        new MemoryRange(0x06000, 0x02000, BlockType.Parameter), //   8kb parameter block
                        new MemoryRange(0x04000, 0x02000, BlockType.Parameter), //   8kb parameter block
                        new MemoryRange(0x00000, 0x04000, BlockType.Boot), //  16kb boot block
                    };
                    break;

                // AM29BL162C
                case 0x00012203:
                    size = 2048 * 1024;
                    description = "AMD AM29BL162C, 2mb";
                    memoryRanges = new MemoryRange[]
                    {           // Start address, Size in Bytes
                        new MemoryRange(0x1C0000, 0x40000, BlockType.OperatingSystem), // 256kb main block
                        new MemoryRange(0x180000, 0x40000, BlockType.OperatingSystem), // 256kb main block
                        new MemoryRange(0x140000, 0x40000, BlockType.OperatingSystem), // 256kb main block
                        new MemoryRange(0x100000, 0x40000, BlockType.OperatingSystem), // 256kb main block
                        new MemoryRange( 0xC0000, 0x40000, BlockType.OperatingSystem), // 256kb main block
                        new MemoryRange( 0x80000, 0x40000, BlockType.OperatingSystem), // 256kb main block
                        new MemoryRange( 0x40000, 0x40000, BlockType.OperatingSystem), // 256kb main block
                        new MemoryRange( 0x08000, 0x38000, BlockType.Calibration),     // 229kb Calibration block
                        new MemoryRange( 0x06000, 0x02000, BlockType.Parameter),       //   8kb parameter block
                        new MemoryRange( 0x04000, 0x02000, BlockType.Parameter),       //   8kb parameter block
                        new MemoryRange( 0x00000, 0x04000, BlockType.Boot),            //  16kb boot block
                    };
                    break;

                // AM29F800BB   
                case 0x00012258:
                    size = 1024 * 1024;
                    description = "AMD AM29F800BB, 1mb";
                    memoryRanges = new MemoryRange[]
                    {
                        new MemoryRange(0xF0000, 0x10000, BlockType.OperatingSystem), //  64kb main block
                        new MemoryRange(0xE0000, 0x10000, BlockType.OperatingSystem), //  64kb main block
                        new MemoryRange(0xD0000, 0x10000, BlockType.OperatingSystem), //  64kb main block
                        new MemoryRange(0xC0000, 0x10000, BlockType.OperatingSystem), //  64kb main block
                        new MemoryRange(0xB0000, 0x10000, BlockType.OperatingSystem), //  64kb main block
                        new MemoryRange(0xA0000, 0x10000, BlockType.OperatingSystem), //  64kb main block
                        new MemoryRange(0x90000, 0x10000, BlockType.OperatingSystem), //  64kb main block
                        new MemoryRange(0x80000, 0x10000, BlockType.OperatingSystem), //  64kb main block
                        new MemoryRange(0x70000, 0x10000, BlockType.OperatingSystem), //  64kb main block
                        new MemoryRange(0x60000, 0x10000, BlockType.OperatingSystem), //  64kb main block
                        new MemoryRange(0x50000, 0x10000, BlockType.OperatingSystem), //  64kb main block
                        new MemoryRange(0x40000, 0x10000, BlockType.OperatingSystem), //  64kb main block
                        new MemoryRange(0x30000, 0x10000, BlockType.OperatingSystem), //  64kb main block
                        new MemoryRange(0x20000, 0x10000, BlockType.OperatingSystem), //  64kb main block
                        new MemoryRange(0x10000, 0x10000, BlockType.Calibration), //  64kb calibration block
                        new MemoryRange(0x08000, 0x08000, BlockType.Calibration), //  32kb calibration block
                        new MemoryRange(0x06000, 0x02000, BlockType.Parameter), //  8kb parameter block
                        new MemoryRange(0x04000, 0x02000, BlockType.Parameter), //  8kb parameter block
                        new MemoryRange(0x00000, 0x04000, BlockType.Boot), //  16kb boot block
                    };
                    break;

                // AM29BL802C
                case 0x00012281:
                    size = 1024 * 1024;
                    description = "AMD AM29BL802C, 1mb";
                    memoryRanges = new MemoryRange[]
                    {          // Start address, Size in Bytes
                        new MemoryRange(0xC0000, 0x40000, BlockType.OperatingSystem), // 256kb main block
                        new MemoryRange(0x80000, 0x40000, BlockType.OperatingSystem), // 256kb main block
                        new MemoryRange(0x60000, 0x20000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x40000, 0x20000, BlockType.OperatingSystem), // 128kb main block
                        new MemoryRange(0x20000, 0x20000, BlockType.Calibration),     // 128kb calibration block
                        new MemoryRange(0x08000, 0x18000, BlockType.Calibration),     //  96kb calibration block
                        new MemoryRange(0x06000, 0x02000, BlockType.Parameter),       //   8kb parameter block
                        new MemoryRange(0x04000, 0x02000, BlockType.Parameter),       //   8kb parameter block
                        new MemoryRange(0x00000, 0x04000, BlockType.Boot),            //  16kb boot block
                    };
                    break;

                // AM29F400BB   
                case 0x000122AB:
                    size = 512 * 1024;
                    description = "AMD AM29F400BB, 512kb";
                    memoryRanges = new MemoryRange[]
                    {
                        new MemoryRange(0x70000, 0x10000, BlockType.OperatingSystem), //  64kb main block
                        new MemoryRange(0x60000, 0x10000, BlockType.OperatingSystem), //  64kb main block
                        new MemoryRange(0x50000, 0x10000, BlockType.OperatingSystem), //  64kb main block
                        new MemoryRange(0x40000, 0x10000, BlockType.OperatingSystem), //  64kb main block
                        new MemoryRange(0x30000, 0x10000, BlockType.OperatingSystem), //  64kb main block
                        new MemoryRange(0x20000, 0x10000, BlockType.OperatingSystem), //  64kb main block
                        new MemoryRange(0x10000, 0x10000, BlockType.Calibration), //  64kb calibration block
                        new MemoryRange(0x08000, 0x08000, BlockType.Calibration), //  32kb calibration block
                        new MemoryRange(0x06000, 0x02000, BlockType.Parameter), //  8kb parameter block
                        new MemoryRange(0x04000, 0x02000, BlockType.Parameter), //  8kb parameter block
                        new MemoryRange(0x00000, 0x04000, BlockType.Boot), //  16kb boot block
                    };
                    break;

                // Both of these have eight 8kb blocks at the low end, the rest are
                // 64kb. Not sure if they're actually used in any PCMs though.
                case 0x00898893: // Intel 2F008B3 
                case 0x008988C1: // Intel 2F800C3 
                default:
                    string manufacturer;

                    switch ((chipId >> 16))
                    {
                        case 0x0001:
                            manufacturer = "AMD";
                            break;

                        case 0x0089:
                            manufacturer = "Intel";
                            break;
                        default:
                            manufacturer = "Unknown";
                            break;
                    }

                    logger.AddUserMessage(
                        "Unsupported flash chip ID " + chipId.ToString("X8") + ". Manufacturer: " + manufacturer +
                        Environment.NewLine +
                        "The flash memory in this PCM is not supported by this version of PCM Hammer." +
                        Environment.NewLine +
                        "Please look for a thread about this at pcmhacking.net, or create one if necessary." +
                        Environment.NewLine +
                        "We do aim to add support for all flash chips eventually.");
                    throw new ApplicationException();
            }

            // Sanity check the memory ranges;
            UInt32 lastStart = UInt32.MaxValue;
            string chipIdString = chipId.ToString("X8");
            for (int index = 0; index < memoryRanges.Count; index++)
            {
                if (index == 0)
                {
                    UInt32 top = memoryRanges[index].Address + memoryRanges[index].Size;
                    if ((top != 512 * 1024) && (top != 1024 * 1024))
                    {
                        throw new InvalidOperationException(chipIdString + " - Upper end of memory range must be 512k or 1024k, is " + top.ToString("X8"));
                    }

                    if (size != top)
                    {
                        throw new InvalidOperationException(chipIdString + " - Size does not match upper memory block.");
                    }
                }

                if (index == memoryRanges.Count - 1)
                {
                    if (memoryRanges[index].Address != 0)
                    {
                        throw new InvalidOperationException(chipIdString + " - Memory ranges must start at zero.");
                    }
                }

                if (lastStart != UInt32.MaxValue)
                {
                    if (lastStart != memoryRanges[index].Address + memoryRanges[index].Size)
                    {
                        throw new InvalidDataException(chipIdString + " - Top of range " + index + " must match base of range above.");
                    }
                }

                lastStart = memoryRanges[index].Address;
            }
            
            return new FlashChip(chipId, description, size, memoryRanges);
        }
    }
}
