//#define FAST_LOGGING

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
        private readonly Vehicle vehicle;
        private readonly DpidsAndMath profileAndMath;
        private DpidCollection dpids;

#if FAST_LOGGING
        private DateTime lastRequestTime;
#endif

        /// <summary>
        /// Constructor.
        /// </summary>
        public Logger(Vehicle vehicle, DpidsAndMath profileAndMath, MathValueConfiguration mathValueConfiguration)
        {
            this.vehicle = vehicle;
            this.profileAndMath = profileAndMath;
        }

        /// <summary>
        /// Invoke this once to begin a logging session.
        /// </summary>
        public async Task<bool> StartLogging()
        {
            this.dpids = await this.vehicle.ConfigureDpids(this.profileAndMath.Profile);

            if (this.dpids == null)
            {
                return false;
            }

            int scenario = ((int)TimeoutScenario.DataLogging1 - 1);
            scenario += this.profileAndMath.Profile.ParameterGroups.Count;
            await this.vehicle.SetDeviceTimeout((TimeoutScenario)scenario);

#if FAST_LOGGING
            if (!await this.vehicle.RequestDpids(this.dpids))
            {
                return false;
            }

            this.lastRequestTime = DateTime.Now;
#endif
            return true;
        }

        /// <summary>
        /// Invoke this repeatedly to get each row of data from the PCM.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetNextRow()
        {
            LogRowParser row = new LogRowParser(this.profileAndMath.Profile);

#if FAST_LOGGING
//            if (DateTime.Now.Subtract(lastRequestTime) > TimeSpan.FromSeconds(2))
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

            DpidValues dpidValues = row.Evaluate();

            IEnumerable<string> mathValues = this.profileAndMath.MathValueProcessor.GetMathValues(dpidValues);

            return dpidValues
                    .Select(x => x.Value.ValueAsString)
                    .Concat(mathValues)
                    .ToArray();
        }
    }
}
