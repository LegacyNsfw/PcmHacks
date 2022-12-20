using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{
    /// Options for commandline parameters
    /// using parser from:
    /// https://github.com/commandlineparser/commandline
    public class CommandLineOptions
    {
        [Option("writecalibration", Required = false, HelpText = "Write calibration from file")]
        public string BinFilePath { get; set; }
        [Option("version", Required = false, HelpText = "Display version information")]
        public bool ShowVersion { get; set; }
        [Option('r', Required = false, HelpText = "Reset device configuration")]
        public bool ResetDeviceConfiguration { get; set; }
    }
}
