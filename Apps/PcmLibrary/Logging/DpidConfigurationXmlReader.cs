using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PcmHacking
{
    public class DpidConfigurationXmlReader
    {
        private Stream stream;

        public DpidConfigurationXmlReader(Stream stream)
        {
            this.stream = stream;
        }

        public DpidConfiguration Read()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DpidConfiguration));
            return (DpidConfiguration) serializer.Deserialize(this.stream);
        }
    }
}
