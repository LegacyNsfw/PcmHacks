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
    public class ParameterNotSupportedException : Exception
    {
        public ParameterNotSupportedException(string message) : base(message)
        { }

        public ParameterNotSupportedException(Parameter parameter) : base(
            $"Parameter \"{parameter.Name}\" is not supported by this PCM.")
        { }
    }

    /// <summary>
    /// Requests log data from the Vehicle.
    /// </summary>
    /// <remarks>
    /// In theory we can send a single message to command the PCM to spew data
    /// continuously. In practice, I haven't been able to get that to work.
    /// 
    /// But I still have hope, so the code for that is in the FAST_LOGGING
    /// sections. We'll get faster data rates if we can figure this out.
    /// </remarks>
    public class Logger
    {
        private readonly Vehicle vehicle;
        private readonly uint osid;
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
        private Logger(Vehicle vehicle, uint osid, DpidConfiguration dpidConfiguration, MathValueProcessor mathValueProcessor)
        {
            this.vehicle = vehicle;
            this.osid = osid;
            this.dpidConfiguration = dpidConfiguration;
            this.mathValueProcessor = mathValueProcessor;
        }

        /// <summary>
        /// The factory method converts the list of columns to a DPID configuration and math-value processor.
        /// </summary>
        public static Logger Create(Vehicle vehicle, uint osid, IEnumerable<LogColumn> columns)
        {
            DpidConfiguration dpidConfiguration = new DpidConfiguration();

            List<LogColumn> singleByteColumns = new List<LogColumn>();
            List<LogColumn> mathColumns = new List<LogColumn>();
            List<LogColumn> pcmColumns = new List<LogColumn>();
            List<MathColumnAndDependencies> dependencies = new List<MathColumnAndDependencies>();

            // Separate PCM columns from Math columns
            foreach (LogColumn column in columns)
            {
                PcmParameter pcmParameter = column.Parameter as PcmParameter;
                if (pcmParameter != null)
                {
                    pcmColumns.Add(column);
                    continue;
                }

                MathParameter mathParameter = column.Parameter as MathParameter;
                if (mathParameter != null)
                {
                    mathColumns.Add(column);
                    continue;
                }                
            }

            // Ensure all math columns have their dependencies
            foreach (LogColumn column in mathColumns)
            {
                MathParameter mathParameter = (MathParameter)column.Parameter;

                LogColumn xColumn = pcmColumns.Where(x => x.Parameter.Id == mathParameter.XColumn.Parameter.Id).FirstOrDefault();

                if (xColumn == null)
                {
                    xColumn = new LogColumn(mathParameter.XColumn.Parameter, mathParameter.XColumn.Conversion);
                    pcmColumns.Add(xColumn);
                }

                LogColumn yColumn = pcmColumns.Where(y => y.Parameter.Id == mathParameter.YColumn.Parameter.Id).FirstOrDefault();
                if (yColumn == null)
                {
                    yColumn = new LogColumn(mathParameter.YColumn.Parameter, mathParameter.YColumn.Conversion);
                    pcmColumns.Add(yColumn);
                }

                MathColumnAndDependencies map = new MathColumnAndDependencies(column, xColumn, yColumn);
                dependencies.Add(map);
            }

            // Populate DPIDs with two-byte values
            byte groupId = 0xFE;
            ParameterGroup group = new ParameterGroup(groupId);
            foreach (LogColumn column in pcmColumns)
            {
                PcmParameter pcmParameter = column.Parameter as PcmParameter;
                if (pcmParameter == null)
                {
                    continue;
                }

                if (pcmParameter.ByteCount == 1)
                {
                    singleByteColumns.Add(column);
                    continue;
                }

                if (!group.TryAddLogColumn(column))
                {
                    throw new ParameterNotSupportedException(
                        $"Parameter \"{column.Parameter.Name}\" is not supported by this PCM.");
                }

                if (group.TotalBytes == ParameterGroup.MaxBytes)
                {
                    dpidConfiguration.ParameterGroups.Add(group);
                    groupId--;
                    group = new ParameterGroup(groupId);
                }
            }

            // Add the remaining one-byte values
            foreach (LogColumn column in singleByteColumns)
            {
                if (!group.TryAddLogColumn(column))
                {
                    throw new ParameterNotSupportedException(
                        $"Parameter \"{column.Parameter.Name}\" is not supported by this PCM.");
                }

                if (group.TotalBytes == ParameterGroup.MaxBytes)
                {
                    dpidConfiguration.ParameterGroups.Add(group);
                    groupId--;
                    group = new ParameterGroup(groupId);
                }
            }

            // Add the last DPID group
            if (group.LogColumns.Count > 0)
            {
                dpidConfiguration.ParameterGroups.Add(group);
                group = null;
            }

            return new Logger(
                vehicle,
                osid,
                dpidConfiguration,
                new MathValueProcessor(
                    dpidConfiguration,
                    dependencies));
        }

        public IEnumerable<string> GetColumnNames()
        {
            return this.dpidConfiguration.GetParameterNames().Concat(this.mathValueProcessor.GetHeaderNames());
        }

        /// <summary>
        /// Invoke this once to begin a logging session.
        /// </summary>
        public async Task<bool> StartLogging()
        {
            this.dpids = await this.vehicle.ConfigureDpids(this.dpidConfiguration, this.osid);

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
