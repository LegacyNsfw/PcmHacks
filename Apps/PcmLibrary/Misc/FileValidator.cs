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
        private readonly string[] segmentNames =
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
            else
            {
                this.logger.AddUserMessage(
                    string.Format(
                        "Files must be 512k or 1024k. This file is {0} / {1:X} bytes long.",
                        this.image.Length,
                        this.image.Length));
                return false;
            }

            bool success = true;
            PcmType type = this.ValidateSignatures();
            if (type == PcmType.Undefined) success = false;

            if (type != PcmType.P12)
            {
                success &= this.ValidateChecksums();
            }
            else
            {
                this.logger.AddUserMessage("TODO: Add support for P10/P12 file checksum validation");
            }
            return success;
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
        /// Get the OSID from the file that the user wants to flash.
        /// </summary>
        public uint GetOsidFromImage()
        {
            int osid = 0;
            
            if (image.Length == 512 * 1024 || image.Length == 1024 * 1024) // bin valid sizes
            {
                PcmType type = this.ValidateSignatures();
                if (type == PcmType.P12) // P10/P12
                { 
                    osid += image[0x8004] << 24;
                    osid += image[0x8005] << 16;
                    osid += image[0x8006] << 8;
                    osid += image[0x8007] << 0;
                }
                else
                {
                    osid += image[0x504] << 24;
                    osid += image[0x505] << 16;
                    osid += image[0x506] << 8;
                    osid += image[0x507] << 0;
                }
            }

            return (uint)osid;
        }

        /// <summary>
        /// Validate a 512k image.
        /// </summary>
        private bool ValidateChecksums()
        {
            bool success = true;
            UInt32 tableAddress = 0x50C;

            this.logger.AddUserMessage("\tStart\tEnd\tStored\tNeeded\tVerdict\tSegment Name");

            for (UInt32 segment = 0; segment < 8; segment++)
            {
                UInt32 startAddressLocation = tableAddress + (segment * 8);
                UInt32 endAddressLocation = startAddressLocation + 4;
                
                UInt32 startAddress = ReadUnsigned(image, startAddressLocation);
                UInt32 endAddress = ReadUnsigned(image, endAddressLocation);

                // For most segments, the first two bytes are the checksum, so they're not counted in the computation.
                // For the overall "segment" the checkum is in the middle.
                UInt32 checksumAddress = startAddress == 0 ? 0x500 : startAddress;
                if (startAddress != 0)
                {
                    startAddress += 2;
                }

                string segmentName = segmentNames[segment];

                if ((startAddress >= image.Length) || (endAddress >= image.Length) || (checksumAddress >= image.Length))
                {
                    this.logger.AddUserMessage("Checksum table is corrupt.");
                    return false;
                }

                success &= this.ValidateRange(startAddress, endAddress, checksumAddress, segmentName);
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
            // P10 / P12 type
            if (image.Length == 1024 * 1024)
            {
                this.logger.AddDebugMessage("Trying P10/P12 1Mb");
                if ((image[0xFFFF8] == 0xAA) && (image[0xFFFF9] == 0x55))
                {
                    this.logger.AddDebugMessage("Signature found at 0xFFFF8");
                    return PcmType.P12; // also used for P10, but we dont know which we have yet.
                }
            }

            this.logger.AddDebugMessage("Trying P01/P59");
            if ((image[0x1FFFE] != 0x4A) || (image[0x01FFFF] != 0xFC))
            {
                this.logger.AddUserMessage("This file does not contain the expected signature at 0x1FFFE.");
                return PcmType.Undefined;
            }

            if (image.Length == 512 * 1024)
            {
                if ((image[0x7FFFE] != 0x4A) || (image[0x07FFFF] != 0xFC))
                {
                    this.logger.AddUserMessage("This file does not contain the expected signature at 0x7FFFE.");
                    return PcmType.Undefined;
                }
            }
            else if (image.Length == 1024 * 1024)
            {
                if ((image[0xFFFFE] != 0x4A) || (image[0x0FFFFF] != 0xFC))
                {
                    this.logger.AddUserMessage("This file does not contain the expected signature at 0xFFFFE.");
                    return PcmType.Undefined;
                }
            }
            else
            {
                this.logger.AddUserMessage("Files of size " + image.Length.ToString("X8") + " are not supported.");
                return PcmType.Undefined;
            }

            return PcmType.P01_P59;
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
        private bool ValidateRange(UInt32 start, UInt32 end, UInt32 storage, string description)
        {
            UInt16 storedChecksum = (UInt16)((this.image[storage] << 8) + this.image[storage + 1]);
            UInt16 computedChecksum = 0;
            
            for(UInt32 address = start; address <= end; address+=2)
            {
                if (address == 0x500)
                {
                    address = 0x502;
                }

                if (address == 0x4000)
                {
                    address = 0x20000;
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
    }
}