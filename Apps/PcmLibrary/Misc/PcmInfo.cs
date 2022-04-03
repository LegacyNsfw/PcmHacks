using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    /// This combines various metadata about whatever PCM we've connected to.
    /// </summary>
    /// <remarks>
    /// This was ported from the LS1 flashing utility originally posted at pcmhacking.net.
    /// </remarks>
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
        /// Indicates how to validate files before writing.
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
        /// Size of the ROM.
        /// </summary>
        public int ImageSize { get; private set; }

        /// <summary>
        /// Which key algorithm to use to unlock the PCM.
        /// </summary>
        public int KeyAlgorithm { get; private set; }

        /// <summary>
        /// Populate this object based on the given OSID.
        /// </summary>
        public PcmInfo(uint osid)
        {
            this.OSID = osid;

            // These defaults work for P01 and P59 hardware.
            // They will need to be overwriten for others.
            this.KernelFileName = "kernel.bin";
            this.KernelBaseAddress = 0xFF8000;
            this.ValidationMethod = PcmType.P01_P59;
            this.HardwareType = PcmType.P01_P59;

            // This will be overwritten for known-to-be-unsupported operating systems.
            this.IsSupported = true;

            switch (osid)
            {
                // LB7 Duramax EFI Live COS
                case 01337601:
                case 01710001:
                case 01887301:
                case 02444101:
                case 02600601:
                case 02685301:
                case 03904401:
                case 01337605:
                case 01710005:
                case 01887305:
                case 02444105:
                case 02600605:
                case 02685305:
                case 03904405:
                    this.KeyAlgorithm = 2;
                    this.Description = "LB7 EFILive COS";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    this.HardwareType = PcmType.LB7;
                    break;

                // LB7 Duramax service no 9388505
                case 15063376:
                case 15188873:
                case 15097100:
                    this.KeyAlgorithm = 2;
                    this.Description = "LB7 9388505";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    this.HardwareType = PcmType.LB7;
                    break;

                // LB7 Duramax service no 12210729
                case 15094441:
                case 15085499:
                case 15166853:
                case 15186006:
                case 15189044:
                    this.KeyAlgorithm = 2;
                    this.Description = "LB7 12210729";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    this.HardwareType = PcmType.LB7;
                    break;

                // LLY Duramax service no 12244189 - 1mbyte?
                case 15141668:
                case 15193885:
                case 15228758:
                case 15231599:
                case 15231600:
                case 15879103:
                case 15087230:
                    this.IsSupported = false;
                    this.KeyAlgorithm = 2;
                    this.Description = "LLY 12244189";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 1024 * 1024;
                    this.HardwareType = PcmType.LLY;
                    break;

                // LL7 Duramax EFI Live Cos
                case 04166801:
                case 04166805:
                case 05160001:
                case 05160005:
                case 05388501:
                case 05388505:
                case 05875801:
                case 05875805:
                    this.IsSupported = false;
                    this.KeyAlgorithm = 2;
                    this.Description = "LLY EFILive COS";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 1024 * 1024;
                    this.HardwareType = PcmType.LLY;
                    break;

                // VCM Suite COS
                case 1251001:
                    this.KeyAlgorithm = 3;
                    this.Description = "VCM Suite 2 Bar";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;
                case 1261001:
                    this.KeyAlgorithm = 4;
                    this.Description = "VCM Suite 3 Bar";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;
                case 1271001:
                    this.KeyAlgorithm = 5;
                    this.Description = "VCM Suite Mafless";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;
                case 1281001:
                    this.KeyAlgorithm = 6;
                    this.Description = "VCM Suite MAF RTT";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;
                case 1271002:
                    this.KeyAlgorithm = 7;
                    this.Description = "VCM Suite Mafless";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;
                case 1251002:
                    this.KeyAlgorithm = 8;
                    this.Description = "VCM Suite 2 Bar";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;
                case 1261002:
                    this.KeyAlgorithm = 9;
                    this.Description = "VCM Suite MAF RTT";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;
                case 1281002:
                    this.KeyAlgorithm = 10;
                    this.Description = "VCM Suite 3 Bar";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;
                case 1271003:
                    this.KeyAlgorithm = 11;
                    this.Description = "VCM Suite Mafless";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;
                case 1251003:
                    this.KeyAlgorithm = 12;
                    this.Description = "VCM Suite 2 Bar";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;
                case 1261003:
                    this.KeyAlgorithm = 13;
                    this.Description = "VCM Suite 3 Bar";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;
                case 1281003:
                    this.KeyAlgorithm = 14;
                    this.Description = "VCM Suite MAF RTT";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;
                //------- HPT COS -----------
                case 1250013:
                case 1250018:
                case 1251005:
                case 1251006:
                case 1251008:
                case 1251010:
                case 1251011:
                case 1251012:
                case 1251014:
                case 1251016:
                case 1251017:
                case 1260006:
                case 1260011:
                case 1261005:
                case 1261008:
                case 1261014:
                case 1261016:
                case 1270013:
                case 1270017:
                case 1271005:
                case 1271006:
                case 1271008:
                case 1271010:
                case 1271011:
                case 1271012:
                case 1271014:
                case 1271016:
                case 1271018:
                case 1281005:
                case 1281006:
                case 1281008:
                case 1281010:
                case 1281011:
                case 1281012:
                case 1281014:
                case 1281016:
                case 1281918:
                    this.Description = "Unknown VCM Suite COS";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;

                //-------------------------

                // 9354896
                case 9360360:
                case 9360361:
                case 9361140:
                case 9363996:
                case 9365637:
                case 9373372:
                case 9379910:
                case 9381344:
                case 12205612:
                case 12584929:
                case 12593359:
                case 12597506:
                case 16253027:
                    this.KeyAlgorithm = 40;
                    this.Description = "9354896";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;

                // 12200411
                case 12202088: // main 2000/2001 OS
                case 12206871:
                case 12208322:
                case 12209203:
                case 12212156:
                case 12216125:
                case 12221588:
                case 12225074: // main 2003/2004 OS
                case 12593358:
                    this.KeyAlgorithm = 40;
                    this.Description = "12200411";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;

                case 01250001:
                case 01290001:
                case 02020002:
                case 02040001:
                case 03150002:
                case 04072901:
                case 04073101:
                case 04110003:
                case 05120003:
                case 01250002:
                case 01290002:
                case 02020003:
                case 02040002:
                case 03150003:
                case 04072902:
                case 04073102:
                case 04140001:
                case 01250003:
                case 01290003:
                case 02020005:
                case 02040003:
                case 03170001:
                case 04072903:
                case 04073103:
                case 04140002:
                case 01270001:
                case 01290005:
                case 02030001:
                case 03110001:
                case 03190001:
                case 04073001:
                case 04080001:
                case 04140003:
                case 01270002:
                case 02010001:
                case 02030002:
                case 03130001:
                case 03190002:
                case 04073002:
                case 04110001:
                case 05120001:
                case 01270003:
                case 02020001:
                case 02030003:
                case 03150001:
                case 03190003:
                case 04073003:
                case 04110002:
                case 05120002:
                    this.KeyAlgorithm = 40;
                    string type = osid.ToString(); // Originally: PCMOSID.Text.Substring(PCMOSID.Text.Length - 2, 2);
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
                //GMC Sierra service number 12589463 
                case 12591725:
                case 12592618:
                case 12593555:
                case 12606961:
                case 12612115:
                    this.KeyAlgorithm = 40;
                    this.Description = "12589463";
                    this.ImageSize = 1024 * 1024;
                    break;

                //case 1250052:
                //case 1250058:
                //case 4073103:
                case 12564440:
                case 12585950:
                case 12588804:
                case 12592425:
                case 12592433: //Aussie
                case 12606960:
                case 12612114:
                    this.KeyAlgorithm = 40;
                    this.Description = "12586242";
                    this.ImageSize = 1024 * 1024;
                    break;
                // usa 12586243
                case 12587603:
                case 12587604:
                    this.KeyAlgorithm = 40;
                    this.Description = "12586243";
                    this.ImageSize = 1024 * 1024;
                    break;
                // not sure 12582605
                case 12578128:
                case 12579405:
                case 12580055:
                case 12593058:
                    this.KeyAlgorithm = 40;
                    this.Description = "12582605";
                    this.ImageSize = 1024 * 1024;
                    break;
                // not sure 12582811
                case 12587811:
                case 12605114:
                case 12606807:
                case 12608669:
                case 12613245:
                case 12613246:
                case 12613247:
                case 12619623:
                    this.KeyAlgorithm = 40;
                    this.Description = "12582811";
                    this.ImageSize = 1024 * 1024;
                    break;
                // not sure 12602802
                case 12597120:
                case 12613248:
                case 12619624:
                    this.KeyAlgorithm = 40;
                    this.Description = "12602802";
                    this.ImageSize = 1024 * 1024;
                    break;

                case 9355699:
                case 9365095:
                case 16263425: // 9366810 'black box'
                    this.IsSupported = false;
                    this.KeyAlgorithm = 15;
                    this.Description = "'Black Box' 9366810";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    this.HardwareType = PcmType.BLACKBOX;
                    break;

                //Hardware 9380717 V6 P04
                case 9354406:
                case 9356245:
                case 9356247:
                case 9356248:
                case 9356251:
                case 9356252:
                case 9356253:
                case 9356256:
                case 9356258:
                case 9363607:
                case 9377336:
                case 9380138:
                case 9380140:
                case 9380718:
                case 9381748:
                case 9381752:
                case 9381754:
                case 9381776:
                case 9381796:
                case 9381797:
                case 9381798:
                case 9382558:
                case 9382572:
                case 9384011:
                case 9384012:
                case 9384013:
                case 9384015:
                case 9384018:
                case 9384022:
                case 9384023:
                case 9384027:
                case 9384033:
                case 9384035:
                case 9384036:
                case 9384043:
                case 9384046:
                case 9384048:
                case 9384050:
                case 9384051:
                case 9384052:
                case 9384073:
                case 9384075:
                case 9384434:
                case 9384436:
                case 9384437:
                case 9384438:
                case 9384441:
                case 9384442:
                case 9384457:
                case 9384458:
                case 9384462:
                case 9384464:
                case 9384465:
                case 9384467:
                case 9384471:
                case 9384473:
                case 9384477:
                case 9386283:
                case 9386285:
                case 9386286:
                case 9386287:
                case 9386288:
                case 9386289:
                case 9387045:
                case 9387047:
                case 9387048:
                case 9387112:
                case 9389253:
                case 9389256:
                case 9389257:
                case 9389258:
                case 9389259:
                case 9389260:
                case 9389282:
                case 9389283:
                case 9389339:
                case 9389341:
                case 9389343:
                case 9389346:
                case 9389348:
                case 9389349:
                case 9389352:
                case 9389356:
                case 9389666:
                case 9389667:
                case 9389668:
                case 9389670:
                case 9389679:
                case 9389687:
                case 9389688:
                case 9389692:
                case 9389695:
                case 9389708:
                case 9389750:
                case 9389752:
                case 9389759:
                case 9389760:
                case 9389766:
                case 9389767:
                case 9389769:
                case 9389770:
                case 9389909:
                case 9390172:
                case 9390763:
                case 9390765:
                case 9392594:
                case 9392748:
                case 9392786:
                case 9392787:
                case 9392790:
                case 9392791:
                case 9392792:
                case 9392794:
                case 9392795:
                case 9392796:
                case 9392797:
                case 9392798:
                case 9392800:
                case 9392801:
                case 9392802:
                case 9392804:
                case 9392807:
                case 9393297:
                case 9393300:
                case 9393307:
                case 9393309:
                case 9393313:
                case 9393315:
                case 9393580:
                case 9393581:
                case 9393598:
                case 9393608:
                case 9393613:
                case 9393822:
                case 9393832:
                case 9393898:
                case 9393901:
                case 10384528:
                case 10384529:
                case 12201457:
                case 12201458:
                case 12201460:
                case 12201461:
                case 12201462:
                case 12201463:
                // P12s were here, are more of these IDs P12?
                case 12201465:
                case 12201466:
                case 12201467:
                case 12201468:
                case 12201687:
                case 12201772:
                case 12201779:
                case 12201782:
                case 12201783:
                case 12201785:
                case 12201786:
                case 12201787:
                case 12201788:
                case 12201791:
                case 12201792:
                case 12201793:
                case 12201795:
                case 12201796:
                case 12201797:
                case 12201803:
                case 12201822:
                case 12201829:
                case 12201830:
                case 12201850:
                case 12201862:
                case 12201863:
                case 12201865:
                case 12201866:
                case 12201867:
                case 12201868:
                case 12201875:
                case 12201885:
                case 12201886:
                case 12201887:
                case 12201888:
                case 12201889:
                case 12201891:
                case 12202127:
                case 12202129:
                case 12202133:
                case 12202135:
                case 12202155:
                case 12202881:
                case 12202882:
                case 12202885:
                case 12202941:
                case 12202942:
                case 12202945:
                case 12203016:
                case 12203657:
                case 12203659:
                case 12203661:
                case 12203792:
                case 12203793:
                case 12203795:
                case 12203796:
                case 12203797:
                case 12203798:
                case 12203799:
                case 12203800:
                case 12203801:
                case 12203802:
                case 12203803:
                case 12203805:
                case 12204282:
                case 12204287:
                case 12204288:
                case 12204437:
                case 12204438:
                case 12204439:
                case 12205378:
                case 12205379:
                case 12214055:
                case 12214056:
                case 12214057:
                case 12214058:
                case 12214381:
                case 12214391:
                case 12214436:
                case 12214710:
                case 12214711:
                case 12214712:
                case 12214713:
                case 12215038:
                case 12215040:
                case 12215321:
                case 12220113:
                case 12220115:
                case 12220117:
                case 12220118:
                case 12221087:
                case 12221090:
                case 12221092:
                case 12221096:
                case 12221098:
                case 12221101:
                case 12221111:
                case 12221112:
                case 12582150:
                case 12582151:
                case 12582152:
                case 12582153:
                case 12583164:
                case 16242202:
                case 16243034:
                case 16258875:
                    this.KeyAlgorithm = 14; // including HWID 9380717
                    this.Description = "1998-2005 V6";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    this.IsSupported = false;
                    this.KernelBaseAddress = 0xFF9090;
                    this.ValidationMethod = PcmType.P04;
                    this.HardwareType = PcmType.P04;
                    break;

                 // P10
                case 12584594:
                    this.IsSupported = true;
                    this.KeyAlgorithm = 66;
                    this.Description = "P10";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    this.KernelBaseAddress = 0xFFB800;
                    this.KernelFileName = "kernel-p10.bin";
                    this.ValidationMethod = PcmType.P10;
                    this.HardwareType = PcmType.P10;
                    break;
                //LL8 - Atlas I6 (4200) P12
                case 12604440:
                case 12606400:
                    this.KernelFileName = "kernel-p12.bin";
                    this.KernelBaseAddress = 0xFF2000;
                    this.ValidationMethod = PcmType.P12;
                    this.IsSupported = true;
                    this.KeyAlgorithm = 91;
                    this.Description = "LL8 Atlas P12";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 1024 * 1024;
                    this.ValidationMethod = PcmType.P12;
                    this.HardwareType = PcmType.P12;
                    break;

                //L52 - Atlas I5 (3500) P12
                case 12606374:
                case 12606375:
                    this.KernelFileName = "kernel-p12.bin";
                    this.KernelBaseAddress = 0xFF2000;
                    this.IsSupported = true;
                    this.KeyAlgorithm = 91;
                    this.Description = "L52 Atlas P12";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 1024 * 1024;
                    this.ValidationMethod = PcmType.P12;
                    this.HardwareType = PcmType.P12;
                    break;
                //LK5 - Atlas I4 (2800) P12
                case 12627883:
                    this.KernelFileName = "kernel-p12.bin";
                    this.KernelBaseAddress = 0xFF2000;
                    this.IsSupported = true;
                    this.KeyAlgorithm = 91;
                    this.Description = "LK5 Atlas P12";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 1024 * 1024;
                    this.ValidationMethod = PcmType.P12;
                    this.HardwareType = PcmType.P12;
                    break;

                default:
                    // this.IsSupported = false; // Not sure what the default should be...
                    this.KeyAlgorithm = 40;
                    this.Description = "Unknown";
                    this.ImageBaseAddress = 0x0;
                    this.ImageSize = 512 * 1024;
                    break;
            }
        }
    }
}