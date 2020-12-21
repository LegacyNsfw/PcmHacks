using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace PcmHacking
{
    // TODO: support multiple conversions
    public class MathValue
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Units { get; set; }

        [XmlAttribute]
        public string XParameter { get; set; }

        [XmlAttribute]
        public string XConversion { get; set; }

        [XmlAttribute]
        public string YParameter { get; set; }

        [XmlAttribute]
        public string YConversion { get; set; }

        [XmlAttribute]
        public string Formula { get; set; }

        [XmlAttribute]
        public string Format { get; set; }
    }

    public class MathValueConfiguration
    {
        [XmlElement("MathValue")]
        public List<MathValue> MathValues;
    }

    public class MathValueConfigurationLoader
    {
        private readonly ILogger logger;

        public MathValueConfiguration Configuration { get; private set; }

        public MathValueConfigurationLoader(ILogger logger)
        {
            this.logger = logger;
        }

        public bool Initialize()
        {
            try
            {
                using (Stream stream = File.OpenRead("MathValues.configuration"))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(MathValueConfiguration));
                    this.Configuration = (MathValueConfiguration)serializer.Deserialize(stream);
                    return true;
                }
            }
            catch (Exception exception)
            {
                this.logger.AddUserMessage("Unable to load math-value configuration.");
                this.logger.AddDebugMessage(exception.ToString());
                return false;
            }
        }
    }
}
