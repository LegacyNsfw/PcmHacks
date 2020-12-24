using System;
using System.Collections.Generic;
using System.Text;

namespace PcmHacking
{
    public class LoggerConfigurationFactory
    {
        private IEnumerable<ProfileParameter> parameters;
        private ILogger logger;

        private static MathValueConfigurationLoader loader;

        public LoggerConfigurationFactory(IEnumerable<ProfileParameter> parameters, ILogger logger)
        {
            this.parameters = parameters;
            this.logger = logger;
        }

        public LoggerConfiguration CreateLoggerConfiguration()
        {
            DpidConfiguration dpids = new DpidConfiguration();

            List<ProfileParameter> singleByteParameters = new List<ProfileParameter>();

            byte groupId = 0xFE;
            ParameterGroup group = new ParameterGroup(groupId);
            foreach (ProfileParameter parameter in this.parameters)
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
                    dpids.ParameterGroups.Add(group);
                    groupId--;
                    group = new ParameterGroup(groupId);
                }
            }

            foreach (ProfileParameter parameter in singleByteParameters)
            {
                group.TryAddParameter(parameter);
                if (group.TotalBytes == ParameterGroup.MaxBytes)
                {
                    dpids.ParameterGroups.Add(group);
                    groupId--;
                    group = new ParameterGroup(groupId);
                }
            }

            if (group.Parameters.Count > 0)
            {
                dpids.ParameterGroups.Add(group);
                group = null;
            }

            // TODO: use ParameterDatabase and MathParameters isntead
            if (loader == null)
            {
                loader = new MathValueConfigurationLoader(this.logger);
                loader.Initialize();
            }

            return new LoggerConfiguration(dpids, loader.Configuration);
        }
    }
}
