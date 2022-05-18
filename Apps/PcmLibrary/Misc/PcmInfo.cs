using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml.Linq;

namespace PcmHacking
{
    public enum PcmType
    {
        Undefined = 0, // required for failed osid test on binary file
        P01_P59,
        P04,
        P10,
        P12,
        LB7,
        LLY,
        BLACKBOX
    }

    /// <summary>
    /// Combines various metadata about compatiable PCM's.
    /// </summary>
    public class PcmInfo
    {
        /// <summary>
        /// Operating system ID.
        /// </summary>
        public uint OSID { get; private set; }

        /// <summary>
        /// Descriptive text.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Indicates whether this PCM is supported by the app.
        /// </summary>
        public bool IsSupported { get; private set; }

        /// <summary>
        /// Indicates how to validate files.
        /// </summary>
        public PcmType ValidationMethod { get; private set; }

        /// <summary>
        /// What type of hardware it is
        /// </summary>
        public PcmType HardwareType { get; private set; }

        /// <summary>
        /// Name of the kernel file to use.
        /// </summary>
        public string KernelFileName { get; private set; }

        /// <summary>
        /// Base address to begin writing the kernel to.
        /// </summary>
        public int KernelBaseAddress { get; private set; }

        /// <summary>
        /// Base address to begin reading or writing the ROM contents.
        /// </summary>
        public int ImageBaseAddress { get; private set; }

        /// <summary>
        /// Size of the Image not necessarily the size of the ROM.
        /// </summary>
        public int ImageSize { get; private set; }

        /// <summary>
        /// Size of the RAM.
        /// </summary>
        public int RAMSize { get; private set; }

        /// <summary>
        /// Which key algorithm to use to unlock the PCM.
        /// </summary>
        public int KeyAlgorithm { get; private set; }

        /// <summary>
        /// For reporting progress and success/fail.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Populate this object based on the given OSID.
        /// </summary>
        public PcmInfo(uint osid, ILogger logger)
        {
            this.OSID = osid;
            this.logger = logger;

            ushort uniqueId = 0;
            const string filename = @"Xml\OsIDs.xml";

            try
            {
                XDocument doc = XDocument.Load(filename);
                string result = doc.Descendants("UniqueID")
                    .Where(i => Convert.ToUInt32(i.Attribute("osid").Value, 10) == OSID)
                    .Select(i => i.Value).FirstOrDefault();
                uniqueId = Convert.ToUInt16(result, 10);
            }
            catch (Exception e)
            {
                this.logger.AddUserMessage("OsID Lookup failure.");
                this.logger.AddDebugMessage(e.Message);
            }


            // These defaults work for P01 and P59 hardware.
            // They will need to be overwritten for others.
            this.KernelFileName = "Kernel-P01.bin";
            this.KernelBaseAddress = 0xFF8000;
            this.RAMSize = 0x4DFF;
            this.ValidationMethod = PcmType.P01_P59;
            this.HardwareType = PcmType.P01_P59;

            // This will be overwritten for known-to-be-unsupported operating systems.
            this.IsSupported = true;

            switch (uniqueId)
            {
                // LB7 Duramax EFI Live COS
                case 1:
                    this.KeyAlgorithm = 2;
                    this.Description = "LB7 EFILive COS";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    this.HardwareType = PcmType.LB7;
                    break;

                // LB7 Duramax service no 9388505
                case 2:
                    this.KeyAlgorithm = 2;
                    this.Description = "LB7 9388505";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    this.HardwareType = PcmType.LB7;
                    break;

                // LB7 Duramax service no 12210729
                case 3:
                    this.KeyAlgorithm = 2;
                    this.Description = "LB7 12210729";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    this.HardwareType = PcmType.LB7;
                    break;

                // LLY Duramax service no 12244189 - 1mbyte?
                case 4:
                    this.IsSupported = false;
                    this.KeyAlgorithm = 2;
                    this.Description = "LLY 12244189";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 1024 * 1024;
                    this.HardwareType = PcmType.LLY;
                    break;

                // LLY Duramax EFI Live Cos
                case 5:
                    this.IsSupported = false;
                    this.KeyAlgorithm = 2;
                    this.Description = "LLY EFILive COS";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 1024 * 1024;
                    this.HardwareType = PcmType.LLY;
                    break;

                // VCM Suite COS
                case 6:
                    this.KeyAlgorithm = 3;
                    this.Description = "VCM Suite 2 Bar";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;

                // VCM Suite COS
                case 7:
                    this.KeyAlgorithm = 4;
                    this.Description = "VCM Suite 3 Bar";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;

                // VCM Suite COS
                case 8:
                    this.KeyAlgorithm = 5;
                    this.Description = "VCM Suite Mafless";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;

                // VCM Suite COS
                case 9:
                    this.KeyAlgorithm = 6;
                    this.Description = "VCM Suite MAF RTT";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;

                // VCM Suite COS
                case 10:
                    this.KeyAlgorithm = 8;
                    this.Description = "VCM Suite 2 Bar";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;

                // VCM Suite COS
                case 11:
                    this.KeyAlgorithm = 9;
                    this.Description = "VCM Suite MAF RTT";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;

                // VCM Suite COS
                case 12:
                    this.KeyAlgorithm = 7;
                    this.Description = "VCM Suite Mafless";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;

                // VCM Suite COS
                case 13:
                    this.KeyAlgorithm = 10;
                    this.Description = "VCM Suite 3 Bar";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;

                // VCM Suite COS
                case 14:
                    this.KeyAlgorithm = 12;
                    this.Description = "VCM Suite 2 Bar";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;

                // VCM Suite COS
                case 15:
                    this.KeyAlgorithm = 13;
                    this.Description = "VCM Suite 3 Bar";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;

                // VCM Suite COS
                case 16:
                    this.KeyAlgorithm = 11;
                    this.Description = "VCM Suite Mafless";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;

                // VCM Suite COS
                case 17:
                    this.KeyAlgorithm = 14;
                    this.Description = "VCM Suite MAF RTT";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;

                //------- HPT COS -----------
                case 18:
                    this.Description = "Unknown VCM Suite COS";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;

                //-------------------------

                // 9354896
                case 19:
                    this.KeyAlgorithm = 40;
                    this.Description = "9354896";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;

                // 12200411
                case 20:
                    this.KeyAlgorithm = 40;
                    this.Description = "12200411";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;

                // EFI Live COS
                case 21:
                    this.KeyAlgorithm = 40;
                    string type = osid.ToString();
                    switch (Convert.ToInt32(type, 10))
                    {
                        case 1:
                            this.Description = "EFI Live COS1";
                            break;

                        case 2:
                            this.Description = "EFI Live COS2";
                            break;

                        case 3:
                            this.Description = "EFI Live COS3";
                            break;

                        case 5:
                            this.Description = "EFI Live COS5";
                            break;

                        default:
                            this.Description = "EFI Live COS";
                            break;
                    }
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;

                // 1Mb pcms
                // GMC Sierra service number 12589463
                case 22:
                    this.KeyAlgorithm = 40;
                    this.Description = "12589463";
                    this.ImageSize = 1024 * 1024;
                    break;

                // Service number 12586242
                case 23:
                    this.KeyAlgorithm = 40;
                    this.Description = "12586242";
                    this.ImageSize = 1024 * 1024;
                    break;

                // usa 12586243
                case 24:
                    this.KeyAlgorithm = 40;
                    this.Description = "12586243";
                    this.ImageSize = 1024 * 1024;
                    break;

                // not sure 12582605
                case 25:
                    this.KeyAlgorithm = 40;
                    this.Description = "12582605";
                    this.ImageSize = 1024 * 1024;
                    break;

                // not sure 12582811
                case 26:
                    this.KeyAlgorithm = 40;
                    this.Description = "12582811";
                    this.ImageSize = 1024 * 1024;
                    break;

                // not sure 12602802
                case 27:
                    this.KeyAlgorithm = 40;
                    this.Description = "12602802";
                    this.ImageSize = 1024 * 1024;
                    break;

                // Blackbox
                case 28:
                    this.IsSupported = false;
                    this.KeyAlgorithm = 15;
                    this.Description = "'Black Box' 9366810";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    this.HardwareType = PcmType.BLACKBOX;
                    break;

                // Hardware 9380717 V6 P04
                // including HWID 9380717
                case 29:
                    this.KeyAlgorithm = 14;
                    this.Description = "1998-2005 V6";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    this.IsSupported = false;
                    this.KernelBaseAddress = 0xFF9090;
                    this.ValidationMethod = PcmType.P04;
                    this.HardwareType = PcmType.P04;
                    break;

                // P10
                case 30:
                    this.KernelFileName = "Kernel-P10.bin";
                    this.KernelBaseAddress = 0xFFB800; // Or FFA000? https://pcmhacking.net/forums/viewtopic.php?f=42&t=7742&start=450#p115622
                    this.RAMSize = 0x2800; // or 4000?
                    this.IsSupported = true;
                    this.KeyAlgorithm = 66;
                    this.Description = "P10";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    this.ValidationMethod = PcmType.P10;
                    this.HardwareType = PcmType.P10;
                    break;

                // P12 1m
                case 31:
                    this.KernelFileName = "Kernel-P12.bin";
                    this.KernelBaseAddress = 0xFF2000; // or FF0000? https://pcmhacking.net/forums/viewtopic.php?f=42&t=7742&start=450#p115622
                    this.RAMSize= 0x6000;
                    this.IsSupported = true;
                    this.KeyAlgorithm = 91;
                    this.Description = "P12 1m (Atlas I4/I5/I6)";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 1024 * 1024;
                    this.ValidationMethod = PcmType.P12;
                    this.HardwareType = PcmType.P12;
                    break;

                // P12b 2m - See: https://pcmhacking.net/forums/viewtopic.php?f=42&t=7742&start=470#p115747
                case 32:
                    this.KernelFileName = "Kernel-P12.bin";
                    this.KernelBaseAddress = 0xFF2000; // or FF0000? https://pcmhacking.net/forums/viewtopic.php?f=42&t=7742&start=450#p115622
                    this.RAMSize = 0x6000;
                    this.IsSupported = true;
                    this.KeyAlgorithm = 91;
                    this.Description = "P12b 2m (Atlas I4/I5/I6)";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 2048 * 1024;
                    this.ValidationMethod = PcmType.P12;
                    this.HardwareType = PcmType.P12;
                    break;

                default:
                    this.IsSupported = false;
                    this.KeyAlgorithm = 40;
                    this.Description = "Unknown";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 0;
                    break;
            }
        }
    }
}
