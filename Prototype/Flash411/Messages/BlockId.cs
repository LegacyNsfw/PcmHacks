using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    class BlockId
    {
        public const byte Vin1               = 0x01;
        public const byte Vin2               = 0x02;
        public const byte Vin3               = 0x03;
        public const byte Serial1            = 0x04;
        public const byte Serial2            = 0x05;
        public const byte Serial3            = 0x06;
        public const byte Serial4            = 0x07;
        public const byte CalibrationID      = 0x08;
        public const byte OSID               = 0x0A; // Operating System ID
        public const byte EngineCalID        = 0x0B; // Engine Segment Calibration ID
        public const byte EngineDiagCalID    = 0x0C; // Engine Diagnostic Calibration ID
        public const byte TransCalID         = 0x0D; // Transmission Segment Calibration ID
        public const byte TransDiagID        = 0x0E; // Transmission Diagnostic Calibration ID
        public const byte FuelCalID          = 0x0F; // Fuel Segment Calibration ID
        public const byte SystemCalID        = 0x10; // System Segment Calibration ID
        public const byte SpeedCalID         = 0x11; // Speed Calibration ID
        public const byte BCC                = 0x14; // Broad Cast Code
        public const byte MEC                = 0xA0; // Manufacturers Enable Counter
    }
}
