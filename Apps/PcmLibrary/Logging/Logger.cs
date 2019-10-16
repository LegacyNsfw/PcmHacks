using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcmHacking
{    
    /// <summary>
    /// Requests log data from the Vehicle.
    /// </summary>
    public class Logger
    {
        private Vehicle vehicle;
        private LogProfile profile;
        private DpidCollection dpids;

#if FAST_LOGGING
        private DateTime lastRequestTime;
#endif

        /// <summary>
        /// Constructor.
        /// </summary>
        public Logger(Vehicle vehicle, LogProfile profile)
        {
            this.vehicle = vehicle;
            this.profile = profile;
        }

        /// <summary>
        /// Invoke this once to begin a logging session.
        /// </summary>
        public async Task<bool> StartLogging()
        {
            this.dpids = await this.vehicle.ConfigureDpids(this.profile);

            if (this.dpids == null)
            {
                return false;
            }

            await this.vehicle.SetDeviceTimeout(TimeoutScenario.ReadProperty);

#if FAST_LOGGING
            if (!await this.vehicle.RequestDpids(this.dpids))
            {
                return null;
            }

            this.lastRequestTime = DateTime.Now;
#endif
            return true;
        }

        /// <summary>
        /// Invoke this repeatedly to get each row of data from the PCM.
        /// </summary>
        /// <returns></returns>
        public async Task<string[]> GetNextRow()
        {
            LogRowParser row = new LogRowParser(this.profile);

#if FAST_LOGGING
            if (DateTime.Now.Subtract(lastRequestTime) > TimeSpan.FromSeconds(2))
            {
                await this.vehicle.ForceSendToolPresentNotification();
            }
#endif
#if !FAST_LOGGING
            if (!await this.vehicle.RequestDpids(this.dpids))
            {
                return null;
            }
#endif

            while (!row.IsComplete)
            {

                RawLogData rawData = await this.vehicle.ReadLogData();
                if (rawData == null)
                {
                    return null;
                }

                row.ParseData(rawData);
            }

            return row.Evaluate();
        }
    }
}
