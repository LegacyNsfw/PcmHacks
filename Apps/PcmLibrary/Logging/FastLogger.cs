//#define FAST_LOGGING

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PcmHacking
{   
    /// <summary>
    /// Extends the base Logger class with an implemenation of "fast" logging.
    /// </summary>
    /// <remarks>
    /// If you send this...
    /// 6C - priority
    /// 10 - destination = PCM
    /// F0 - source = tool
    /// 2A - mode = send DPID
    /// 24 - submode = streaming
    /// FE
    /// FD
    /// FC
    /// FB
    /// ...the PCM should respond with a stream of DPID payloads.
    /// </remarks>
    public class FastLogger : Logger
    {
        private DateTime lastNotificationTime;

        /// <summary>
        /// Constructor.
        /// </summary>
        public FastLogger(
            Vehicle vehicle,
            uint osid,
            DpidConfiguration dpidConfiguration,
            MathValueProcessor mathValueProcessor,
            ILogger uiLogger)
            : base(
                  vehicle,
                  osid,
                  dpidConfiguration,
                  mathValueProcessor,
                  uiLogger)
        {
        }

        /// <summary>
        /// Invoke this once to begin a logging session.
        /// </summary>
        protected override async Task<bool> StartLoggingInternal()
        {
            await this.Vehicle.SetDeviceTimeout(TimeoutScenario.Minimum);

            if (!await this.Vehicle.RequestDpids(this.Dpids, 0x24))
            {
                return false;
            }

            await this.Vehicle.SetDeviceTimeout(TimeoutScenario.DataLoggingStreaming);


            this.lastNotificationTime = DateTime.Now;
            return true;
        }

        /// <summary>
        /// Invoke this repeatedly to get each row of data from the PCM.
        /// </summary>
        /// <returns></returns>
        protected override async Task GetNextRowInternal(LogRowParser row)
        {
            for (int count = 0; count < 6 && !row.IsComplete; count++)
            {
                RawLogData rawData = await this.Vehicle.ReadLogData();
                if (rawData != null)
                {
                    this.UILogger.add
                    row.ParseData(rawData);
                }
            }

            if (DateTime.Now.Subtract(lastNotificationTime) > TimeSpan.FromSeconds(2))
            {
                this.lastNotificationTime = DateTime.Now;
                await this.Vehicle.ForceSendToolPresentNotification();
            }
        }
    }
}
