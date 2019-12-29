using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PcmHacking
{
    public class LogProfileXmlReader
    {
        private Stream stream;

        public LogProfileXmlReader(Stream stream)
        {
            this.stream = stream;
        }

        public LogProfile Read()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(LogProfile));
            return (LogProfile) serializer.Deserialize(this.stream);
        }
    }
}
