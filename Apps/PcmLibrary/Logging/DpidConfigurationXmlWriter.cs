using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace PcmHacking
{
    public class DpidConfigurationXmlWriter
    {
        private Stream stream;

        public DpidConfigurationXmlWriter(Stream stream)
        {
            this.stream = stream;
        }

        public void Write(DpidConfiguration profile)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DpidConfiguration));
            serializer.Serialize(stream, profile);
        }
    }
}
