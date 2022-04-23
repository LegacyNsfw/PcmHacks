using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PcmHacking
{
    /// <summary>
    /// Try to detect corrupted firmware images.
    /// </summary>
    public class FileValidator
    {
        /// <summary>
        /// Names of segments in P01 and P59 operating systems.
        /// </summary>
        private readonly string[] segmentNames_P01_P59 =
        {
            "Operating system",
            "Engine calibration",
            "Engine diagnostics.",
            "Transmission calibration",
            "Transmission diagnostics",
            "Fuel system",
            "System",
            "Speedometer",
        };

        /// <summary>
        /// Names of segments in P10 operating systems.
        /// </summary>
        private readonly string[] segmentNames_P10 =
        {
            "Operating system",
            "Engine calibration",
            "Transmission calibration",
            "System",
            "Speedometer",
        };

        // Names of segments in P12 operating system are documented in the ValidateChecksums() function

        /// <summary>
        /// The contents of the firmware file that someone wants to flash.
        /// </summary>
        private readonly byte[] image;

        /// <summary>
        /// For reporting progress and success/fail.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        public FileValidator(byte[] image, ILogger logger)
        {
            this.image = image;
            this.logger = logger;
        }

        /// <summary>
        /// Indicate whether the image is valid or not.
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            if (this.image.Length == 512 * 1024)
            {
                this.logger.AddUserMessage("Validating 512k file.");
            }
            else if (this.image.Length == 1024 * 1024)
            {
                this.logger.AddUserMessage("Validating 1024k file.");
            }
            else if (this.image.Length == 2048 * 1024)
            {
                this.logger.AddUserMessage("Validating 2048k file.");
            }
            else
            {
                this.logger.AddUserMessage(
                    string.Format(
                        "Files must be 512k, 1024k or 2048k. This file is {0} / {1:X} bytes long.",
                        this.image.Length,
                        this.image.Length));
                return false;
            }

            return this.ValidateChecksums();
        }

        /// <summary>
        /// Compare the OS ID from the PCM with the OS ID from the file.
        /// </summary>
        public bool IsSameOperatingSystem(UInt32 pcmOsid)
        {
            UInt32 fileOsid = this.GetOsidFromImage();

            if (pcmOsid == fileOsid)
            {
                this.logger.AddUserMessage("PCM and file are both operating system " + pcmOsid);
                return true;
            }

            this.logger.AddUserMessage("Operating system IDs do not match.");
            this.logger.AddUserMessage("PCM operating system ID: " + pcmOsid);
            this.logger.AddUserMessage("File operating system ID: " + fileOsid);
            return false;
        }

        /// <summary>
        /// Compare the HW type from the PCM with the type determined from the file.
        /// </summary>
        public bool IsSameHardware(UInt32 pcmOsid)
        {
            UInt32 fileOsid = this.GetOsidFromImage();

            PcmInfo pcmInfo = new PcmInfo(pcmOsid);
            PcmInfo fileInfo = new PcmInfo(fileOsid);

            if (pcmInfo.HardwareType == fileInfo.HardwareType)
            {
                this.logger.AddUserMessage("PCM and file are both for the same Hardware " + fileInfo.HardwareType.ToString());
                return true;
            }

            this.logger.AddUserMessage("Hardware types do not match. This file is not compatible with this PCM");
            this.logger.AddUserMessage("PCM Hardware is: " + pcmInfo.HardwareType.ToString());
            this.logger.AddUserMessage("File requires: " + fileInfo.HardwareType.ToString());
            return false;
        }

        /// <summary>
        /// Get the OSID from the file that the user wants to flash.
        /// </summary>
        public uint GetOsidFromImage()
        {
            int osid = 0;

            if (image.Length == 512 * 1024 || image.Length == 1024 * 1024 || image.Length == 2048 * 1024) // bin valid sizes
            {
                PcmType type = this.ValidateSignatures();
                switch (type)
                {
                    case PcmType.P01_P59:
                        osid += image[0x504] << 24;
                        osid += image[0x505] << 16;
                        osid += image[0x506] << 8;
                        osid += image[0x507] << 0;
                        break;

                    case PcmType.P10:
                        osid += image[0x52E] << 24;
                        osid += image[0x52F] << 16;
                        osid += image[0x530] << 8;
                        osid += image[0x531] << 0;
                        break;

                    case PcmType.P12:
                        osid += image[0x8004] << 24;
                        osid += image[0x8005] << 16;
                        osid += image[0x8006] << 8;
                        osid += image[0x8007] << 0;
                        break;
                }
            }
            return (uint)osid;
        }

        /// <summary>
        /// Validate a binary image of a known type
        /// </summary>
        private bool ValidateChecksums()
        {
            bool success = true;
            UInt32 tableAddress;
            UInt32 segments = 0;

            PcmType type = this.ValidateSignatures();

            switch (type)
            {
                case PcmType.P01_P59:
                    tableAddress = 0x50C;
                    segments = 8;
                    break;

                case PcmType.P10:
                    tableAddress = 0x546;
                    segments = 5;
                    break;

                case PcmType.P12:
                    tableAddress = 0x0;
                    segments = 0;
                    break;

                case PcmType.Undefined:
                    return false;

                default:
                    this.logger.AddDebugMessage("TODO: Implement FileValidator::ValidateChecksums for a " + type.ToString());
                    return false;
            }

            this.logger.AddUserMessage("\tStart\tEnd\tStored\tNeeded\tVerdict\tSegment Name");

            switch (type)
            {
                // P12 is so wierd we hard code segments with a special function here
                case PcmType.P12:
                    success &= ValidateRangeP12(0x922, 0x900, 0x94A, 2, "Boot Block");
                    success &= ValidateRangeP12(0x8022, 0, 0x804A, 2, "OS");
                    success &= ValidateRangeP12(0x80C4, 0, 0x80E4, 2, "Engine Calibration");
                    success &= ValidateRangeP12(0x80F7, 0, 0x8117, 2, "Engine Diagnostics");
                    success &= ValidateRangeP12(0x812A, 0, 0x814A, 2, "Transmission Calibration");
                    success &= ValidateRangeP12(0x815D, 0, 0x817D, 2, "Transmission Diagnostics");
                    success &= ValidateRangeP12(0x805E, 0, 0x807E, 2, "Speedometer");
                    success &= ValidateRangeP12(0x8091, 0, 0x80B1, 2, "System");
                    break;

                // The rest can use the generic code
                default:
                    for (UInt32 segment = 0; segment < segments; segment++)
                    {
                        UInt32 startAddressLocation = tableAddress + (segment * 8);
                        UInt32 endAddressLocation = startAddressLocation + 4;

                        UInt32 startAddress = ReadUnsigned(image, startAddressLocation);
                        UInt32 endAddress = ReadUnsigned(image, endAddressLocation);

                        // For most segments, the first two bytes are the checksum, so they're not counted in the computation.
                        // For the overall "segment" in a P01/P59 the checkum is in the middle.
                        UInt32 checksumAddress;
                        switch (type)
                        {
                            case PcmType.P01_P59:
                                checksumAddress = startAddress == 0 ? 0x500 : startAddress;
                                break;

                            case PcmType.P10:
                                checksumAddress = startAddress == 0 ? 0x52A : startAddress;
                                break;

                            default:
                                checksumAddress = startAddress;
                                break;
                        }

                        if (startAddress != 0)
                        {
                            startAddress += 2;
                        }

                        string segmentName;
                        switch (type)
                        {
                            case PcmType.P10:
                                segmentName = segmentNames_P10[segment];
                                break;

                            default:
                                segmentName = segmentNames_P01_P59[segment];
                                break;
                        }

                        if ((startAddress >= image.Length) || (endAddress >= image.Length) || (checksumAddress >= image.Length))
                        {
                            this.logger.AddUserMessage("Checksum table is corrupt.");
                            return false;
                        }

                        success &= ValidateRange(type, startAddress, endAddress, checksumAddress, segmentName);
                    }
                    break;
            }
            return success;
        }

        /// <summary>
        /// 
        /// </summary>
        private UInt32 ReadUnsigned(byte[] image, UInt32 offset)
        {
            return BitConverter.ToUInt32(image.Skip((int)offset).Take(4).Reverse().ToArray(), 0);
        }

        /// <summary>
        /// Validate signatures
        /// </summary>
        private PcmType ValidateSignatures()
        {
            // All currently supported bins are 512Kb, 1Mb or 2Mb
            if ((image.Length != 512 * 1024) && (image.Length != 1024 * 1024) && (image.Length != 2048 * 1024))
            {
                this.logger.AddUserMessage("Files of size " + image.Length.ToString("X8") + " are not supported.");
                return PcmType.Undefined;
            }

            // 512Kb Types
            if (image.Length == 512 * 1024)
            {
                // P01 512Kb
                this.logger.AddDebugMessage("Trying P01 512Kb");
                if ((image[0x1FFFE] == 0x4A) && (image[0x01FFFF] == 0xFC))
                {
                    if ((image[0x7FFFE] == 0x4A) || (image[0x07FFFF] == 0xFC))
                    {
                        return PcmType.P01_P59;
                    }
                }

                this.logger.AddDebugMessage("Trying P10 512Kb");
                if ((image[0x17FFE] == 0x55) && (image[0x017FFF] == 0x55))
                {
                    if ((image[0x7FFFC] == 0xA5) && (image[0x07FFFD] == 0x5A) && (image[0x7FFFE] == 0xA5) && (image[0x07FFFF] == 0xA5))
                    {
                        return PcmType.P10;
                    }
                }
            }

            // 1Mb types
            if (image.Length == 1024 * 1024)
            {
                this.logger.AddDebugMessage("Trying P59 1Mb");
                if ((image[0x1FFFE] == 0x4A) && (image[0x01FFFF] == 0xFC))
                {
                    if ((image[0xFFFFE] == 0x4A) && (image[0x0FFFFF] == 0xFC))
                    {
                        return PcmType.P01_P59;
                    }
                }

                this.logger.AddDebugMessage("Trying P12 1Mb");
                if ((image[0xFFFF8] == 0xAA) && (image[0xFFFF9] == 0x55))
                {
                    return PcmType.P12;
                }
            }

            // 2Mb types
            if (image.Length == 2048 * 1024)
            {
                this.logger.AddDebugMessage("Trying P12 2Mb");
                if ((image[0x17FFF8] == 0xAA) && (image[0x17FFF9] == 0x55))
                {
                    return PcmType.P12;
                }
            }

            this.logger.AddDebugMessage("Unable to identify or validate bin image content");
            return PcmType.Undefined;
        }

        /// <summary>
        /// Print the header for the table of checksums.
        /// </summary>
        private void PrintHeader()
        {
            this.logger.AddUserMessage("\tStart\tEnd\tResult\tFile\tActual\tContent");
        }

        /// <summary>
        /// Validate a range.
        /// </summary>
        private bool ValidateRange(PcmType type, UInt32 start, UInt32 end, UInt32 storage, string description)
        {
            UInt16 storedChecksum = (UInt16)((this.image[storage] << 8) + this.image[storage + 1]);
            UInt16 computedChecksum = 0;

            for (UInt32 address = start; address <= end; address += 2)
            {
                switch (type)
                {
                    case PcmType.P10:
                        switch (address)
                        {
                            case 0x52A:
                                address = 0x52C;
                                break;

                            case 0x4000:
                                address = 0x20000;
                                break;

                            case 0x7FFFA:
                                end = 0x7FFFA; // A hacky way to short circuit the end
                                break;
                        }
                        break;

                    case PcmType.P01_P59:
                        if (address == 0x500)
                        {
                            address = 0x502;
                        }

                        if (address == 0x4000)
                        {
                            address = 0x20000;
                        }
                        break;
                }

                UInt16 value = (UInt16)(this.image[address] << 8);
                value |= this.image[address + 1];
                computedChecksum += value;
            }

            computedChecksum = (UInt16)((0 - computedChecksum));

            bool verdict = storedChecksum == computedChecksum;

            string error = string.Format(
                "\t{0:X5}\t{1:X5}\t{2:X4}\t{3:X4}\t{4:X4}\t{5}",
                start,
                end,
                storedChecksum,
                computedChecksum,
                verdict ? "Good" : "BAD",
                description);

            this.logger.AddUserMessage(error);
            return verdict;
        }

        /// <summary>
        /// Validate a range for P12
        /// Its so different it gets its own routine...
        /// </summary>
        private bool ValidateRangeP12(UInt32 segment, UInt32 offset, UInt32 index, UInt32 blocks, string description)
        {
            UInt16 storedChecksum = 0;
            UInt16 computedChecksum = 0;
            int first = 0;
            int start = 0;
            int end = 0;

            int sumaddr;
            sumaddr  = image[segment + 0] << 24;
            sumaddr += image[segment + 1] << 16;
            sumaddr += image[segment + 2] << 8;
            sumaddr += image[segment + 3];

            storedChecksum = (UInt16)((this.image[sumaddr + offset] << 8) + this.image[sumaddr + offset + 1]);

            // Lookup the start and end of each block, and add it to the sum
            for (UInt32 block = 0; block < blocks; block++)
            {
                start  = image[index + (block * 8) + 0] << 24;
                start += image[index + (block * 8) + 1] << 16;
                start += image[index + (block * 8) + 2] << 8;
                start += image[index + (block * 8) + 3];
                end    = image[index + (block * 8) + 4] << 24;
                end   += image[index + (block * 8) + 5] << 16;
                end   += image[index + (block * 8) + 6] << 8;
                end   += image[index + (block * 8) + 7];

                for (UInt32 address = (UInt32) start; address <= end; address += 2)
                {
                    UInt16 value = (UInt16)(this.image[address] << 8);
                    value |= this.image[address + 1];
                    computedChecksum += value;
                }

                if (block == 0)
                {
                    first = start;
                }
            }

            computedChecksum = (UInt16) ((0 - computedChecksum)); // 2s compliment

            bool verdict = storedChecksum == computedChecksum;

            string error = string.Format(
                "\t{0:X5}\t{1:X5}\t{2:X4}\t{3:X4}\t{4:X4}\t{5}",
                first, // The start of the first block
                end,   // The end of the last block
                storedChecksum,
                computedChecksum,
                verdict ? "Good" : "BAD",
                description);

            this.logger.AddUserMessage(error);
            return verdict;
        }
    }
}
