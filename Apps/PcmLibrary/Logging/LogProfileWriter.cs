using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace PcmHacking
{
    /// <summary>
    /// Writes a LogProfile to an XML file.
    /// </summary>
    public class LogProfileWriter
    {
        public static void Write(LogProfile profile, string path)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "    ";
            settings.NewLineChars = Environment.NewLine;
            settings.NewLineOnAttributes = false;
            settings.OmitXmlDeclaration = true;

            using (XmlWriter writer = XmlWriter.Create(path, settings))
            {    
                XDocument document = new XDocument();
                XElement top = new XElement("LogProfile");
                document.Add(top);

                XElement pidParameters = new XElement("PidParameters");
                top.Add(pidParameters);

                foreach(LogColumn column in profile.Columns)
                {
                    if (column.Parameter is PidParameter)
                    {
                        XElement element = new XElement("PidParameter");
                        element.SetAttributeValue("id", column.Parameter.Id);
                        element.SetAttributeValue("units", column.Conversion.Units);
                        pidParameters.Add(element);
                    }
                }

                XElement ramParameters = new XElement("RamParameters");
                top.Add(ramParameters);

                foreach (LogColumn column in profile.Columns)
                {
                    if (column.Parameter is RamParameter)
                    {
                        XElement element = new XElement("RamParameter");
                        element.SetAttributeValue("id", column.Parameter.Id);
                        element.SetAttributeValue("units", column.Conversion.Units);
                        ramParameters.Add(element);
                    }
                }

                XElement mathParameters = new XElement("MathParameters");
                top.Add(mathParameters);

                foreach (LogColumn column in profile.Columns)
                {
                    if (column.Parameter is MathParameter)
                    {
                        XElement element = new XElement("MathParameter");
                        element.SetAttributeValue("id", column.Parameter.Id);
                        element.SetAttributeValue("units", column.Conversion.Units);
                        mathParameters.Add(element);
                    }
                }

                document.Save(writer);
            }
        }
    }
}
