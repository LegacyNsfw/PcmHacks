using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace PcmHacking
{
    public class LogProfileXmlWriter
    {
        private Stream stream;

        public LogProfileXmlWriter(Stream stream)
        {
            this.stream = stream;
        }

        public void Write(LogProfile profile)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(LogProfile));
            serializer.Serialize(stream, profile);
        }
    }
}
