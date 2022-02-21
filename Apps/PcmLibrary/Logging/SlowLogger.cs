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
    /// Extends the logger class with a slow implementation.
    /// </summary>
    /// <remarks>
    /// This implementation sends a new request for each row of log data.
    /// </remarks>
    public class SlowLogger : Logger
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public SlowLogger(
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
            int scenario = ((int)TimeoutScenario.DataLogging1 - 1);
            scenario += this.DpidConfiguration.ParameterGroups.Count;
            await this.Vehicle.SetDeviceTimeout((TimeoutScenario)scenario);

            return true;
        }

        /// <summary>
        /// Invoke this repeatedly to get each row of data from the PCM.
        /// </summary>
        protected override async Task GetNextRowInternal(LogRowParser row)
        {
            try
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                if (!await this.Vehicle.RequestDpids(this.Dpids, false))
                {
                    return;
                }

                while (!row.IsComplete)
                {
                    RawLogData rawData = await this.Vehicle.ReadLogData();
                    if (rawData == null)
                    {
                        return;
                    }

                    row.ParseData(rawData);
                }

            }
            finally
            {
                Thread.CurrentThread.Priority = ThreadPriority.Normal;
            }
        }
    }
}
