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

                WriteParameters<PidParameter>(profile, top);
                WriteParameters<RamParameter>(profile, top);
                WriteParameters<MathParameter>(profile, top);

                document.Save(writer);
            }
        }

        private static void WriteParameters<T>(LogProfile profile, XElement top) where T : Parameter
        {
            string parameterType = typeof(T).Name;

            XElement parameterListElement = new XElement(string.Format("{0}s", parameterType));

            top.Add(parameterListElement);

            foreach (LogColumn column in profile.Columns)
            {
                if (column.Parameter is T)
                {
                    XElement element = new XElement(parameterType);
                    element.SetAttributeValue("id", column.Parameter.Id);
                    element.SetAttributeValue("units", column.Conversion.Units);
                    parameterListElement.Add(element);
                }
            }
        }
    }
}
