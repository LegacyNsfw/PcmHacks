using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace PcmHacking
{
    public class LogProfileWriter
    {
        public static void Write(LogProfile profile, string path)
        {
            using (XmlWriter writer = XmlWriter.Create(path))
            {
                XDocument document = new XDocument();
                XElement top = new XElement("LogProfile");
                document.Add(top);

                XElement pidParameters = new XElement("PidParameters");
                top.Add(pidParameters);

                foreach(ProfileParameter parameter in profile.Parameters)
                {
                    if (parameter.Parameter is PidParameter)
                    {
                        XElement element = new XElement("PidParameter");
                        element.SetAttributeValue("id", parameter.Parameter.Id);
                        element.SetAttributeValue("units", parameter.Conversion.Units);
                        pidParameters.Add(element);
                    }
                }

                XElement ramParameters = new XElement("RamParameters");
                top.Add(ramParameters);

                foreach (ProfileParameter parameter in profile.Parameters)
                {
                    if (parameter.Parameter is RamParameter)
                    {
                        XElement element = new XElement("RamParameter");
                        element.SetAttributeValue("id", parameter.Parameter.Id);
                        element.SetAttributeValue("units", parameter.Conversion.Units);
                        ramParameters.Add(element);
                    }
                }

                XElement mathParameters = new XElement("MathParameters");
                top.Add(mathParameters);

                foreach (ProfileParameter parameter in profile.Parameters)
                {
                    if (parameter.Parameter is MathParameter)
                    {
                        XElement element = new XElement("MatnParameter");
                        element.SetAttributeValue("id", parameter.Parameter.Id);
                        element.SetAttributeValue("units", parameter.Conversion.Units);
                        mathParameters.Add(element);
                    }
                }
            }
        }
    }
}
