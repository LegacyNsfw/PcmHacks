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
    /// Requests log data from the Vehicle.
    /// </summary>
    public class Logger
    {
        private readonly Vehicle vehicle;
        private readonly DpidConfiguration dpidConfiguration;
        private readonly MathValueProcessor mathValueProcessor;

        private DpidCollection dpids;
#if FAST_LOGGING
        private DateTime lastRequestTime;
#endif

        public DpidConfiguration DpidConfiguration {  get { return this.dpidConfiguration; } }
        public MathValueProcessor MathValueProcessor {  get { return this.mathValueProcessor; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        private Logger(Vehicle vehicle, DpidConfiguration dpidConfiguration, MathValueProcessor mathValueProcessor)
        {
            this.vehicle = vehicle;
            this.dpidConfiguration = dpidConfiguration;
            this.mathValueProcessor = mathValueProcessor;
        }

        public static Logger Create(Vehicle vehicle, IEnumerable<ProfileParameter> parameters)
        {
            DpidConfiguration dpidConfiguration = new DpidConfiguration();

            List<ProfileParameter> singleByteParameters = new List<ProfileParameter>();

            byte groupId = 0xFE;
            ParameterGroup group = new ParameterGroup(groupId);
            foreach (ProfileParameter parameter in parameters)
            {
                PcmParameter pcmParameter = parameter.Parameter as PcmParameter;
                if (pcmParameter == null)
                {
                    continue;
                }

                if (pcmParameter.ByteCount == 1)
                {
                    singleByteParameters.Add(parameter);
                    continue;
                }

                group.TryAddParameter(parameter);
                if (group.TotalBytes == ParameterGroup.MaxBytes)
                {
                    dpidConfiguration.ParameterGroups.Add(group);
                    groupId--;
                    group = new ParameterGroup(groupId);
                }
            }

            foreach (ProfileParameter parameter in singleByteParameters)
            {
                group.TryAddParameter(parameter);
                if (group.TotalBytes == ParameterGroup.MaxBytes)
                {
                    dpidConfiguration.ParameterGroups.Add(group);
                    groupId--;
                    group = new ParameterGroup(groupId);
                }
            }

            if (group.Parameters.Count > 0)
            {
                dpidConfiguration.ParameterGroups.Add(group);
                group = null;
            }

            // TODO: MathValueProcessor should be initialized from ParameterDatabase and passed-in math parameters 
            return new Logger(
                vehicle, 
                dpidConfiguration, 
                new MathValueProcessor(
                    dpidConfiguration, 
                    new MathValueConfiguration()));
        }

        public IEnumerable<string> GetColumnNames()
        {
            return this.dpidConfiguration.GetParameterNames().Concat(this.mathValueProcessor.GetHeaders());
        }

        /// <summary>
        /// Invoke this once to begin a logging session.
        /// </summary>
        public async Task<bool> StartLogging()
        {
            this.dpids = await this.vehicle.ConfigureDpids(this.dpidConfiguration);

            if (this.dpids == null)
            {
                return false;
            }

            int scenario = ((int)TimeoutScenario.DataLogging1 - 1);
            scenario += this.dpidConfiguration.ParameterGroups.Count;
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
            LogRowParser row = new LogRowParser(this.dpidConfiguration);

            try
            {
#if FAST_LOGGING
//          if (DateTime.Now.Subtract(lastRequestTime) > TimeSpan.FromSeconds(2))
            {
                await this.vehicle.ForceSendToolPresentNotification();
            }
#endif
#if !FAST_LOGGING
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
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

            }
            finally
            {
#if !FAST_LOGGING
                Thread.CurrentThread.Priority = ThreadPriority.Normal;
#endif
            }

            PcmParameterValues dpidValues = row.Evaluate();

            IEnumerable<string> mathValues = this.mathValueProcessor.GetMathValues(dpidValues);

            return dpidValues
                    .Select(x => x.Value.ValueAsString)
                    .Concat(mathValues)
                    .ToArray();
        }
    }
}
