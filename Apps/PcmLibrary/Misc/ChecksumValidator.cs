using System;
using System.Collections.Generic;
using System.Text;

namespace PcmHacking
{
    /// <summary>
    /// Try to detect corrupted firmware images.
    /// </summary>
    public class ChecksumValidator
    {
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
        public ChecksumValidator(byte[] image, ILogger logger)
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
                return this.Validate512();
            }
            else if (this.image.Length == 1024 * 1024)
            {
                this.logger.AddUserMessage("Validating 1024k file.");
                return this.Validate1024();
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
        }

        /// <summary>
        /// Validate a 512k image.
        /// </summary>
        private bool Validate512()
        {
            bool success = true;

            success &= ValidateRange(      0, 0x7FFFD,   0x500, "Operating system");
            success &= ValidateRange( 0x8002, 0x13FFF,  0x8000, "Engine calibration");
            success &= ValidateRange(0x14002, 0x16DFF, 0x14000, "Engine diagnostics.");
            success &= ValidateRange(0x16E02, 0x1BDFF, 0x16E00, "Transmission calibration");
            success &= ValidateRange(0x1BE02, 0x1C7FF, 0x1BE00, "Transmission diagnostics");
            success &= ValidateRange(0x1C802, 0x1E51F, 0x1C800, "Fuel system");
            success &= ValidateRange(0x1E522, 0x1EE9F, 0x1E520, "System");
            success &= ValidateRange(0x1EEA2, 0x1EF9F, 0x1EEA0, "Speedometer");

            return success;
        }

        /// <summary>
        /// Validate a 1024k image.
        /// </summary>
        /// <returns></returns>
        private bool Validate1024()
        {
            this.logger.AddUserMessage("Validate is not yet implemented for 1024k files.");
            return false;
        }

        /// <summary>
        /// Validate a range.
        /// </summary>
        private bool ValidateRange(UInt32 start, UInt32 end, UInt32 storage, string description)
        {
            string range = string.Format("{0:X} - {1:X} ({2})", start, end, description);

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

            if (storedChecksum == computedChecksum)
            {
                this.logger.AddUserMessage(range + " Valid");
                return true;
            }

            string error = string.Format(" Image: {0:X4}, Computed: {1:X4}", storedChecksum, computedChecksum);
            this.logger.AddUserMessage(range + error);
            return false;
        }
    }
}