using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Ingester
{
    public class Parser
    {  
        public static void Skim()
        {
            string path = "../enwiktionary.xml";
            string output = "../sorted";

            if (!Directory.Exists(output))
            {
                Directory.CreateDirectory(output);
            }

            XmlReaderSettings settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                Async = false
            };

            using (FileStream fs = File.OpenRead(path))
            using (XmlReader r = XmlReader.Create(fs, settings))
            {
                long totalBytes = fs.Length;
                long pageCount = 0;
                while (r.Read())
                {
                    if (r.NodeType == XmlNodeType.Element && r.Name == "page")
                    {
                        pageCount++;
                        string rawPageXml = r.ReadOuterXml();
                        string nsValue = ExtractNamespace(rawPageXml);
                        string target = Path.Combine(output, $"ns_{nsValue}.xml");
                        File.AppendAllText(target, rawPageXml + Environment.NewLine, Encoding.UTF8);

                        if (pageCount % 1000 == 0)
                        {
                            double pct = (double)fs.Position / totalBytes * 100;
                            long currentMB = fs.Position / 1024 / 1024;
                            long totalMB = totalBytes / 1024 / 1024;
                            Console.WriteLine($"[Processed {pageCount:N0} pages] Current Namespace Routed: {nsValue} | Progress: {currentMB:N0}MB / {totalMB:N0}MB ({pct:F2}%)");
                        }
                    }
                }
            }
        }

        private static string ExtractNamespace(string pageXml)
        {
            try
            {
                XmlReaderSettings subSettings = new XmlReaderSettings
                {
                    ConformanceLevel = ConformanceLevel.Fragment
                };
                using (StringReader sr = new StringReader(pageXml))
                using (XmlReader subReader = XmlReader.Create(sr, subSettings))
                {
                    while (subReader.Read())
                    {
                        if (subReader.NodeType == XmlNodeType.Element && subReader.Name == "ns")
                        {
                            return subReader.ReadElementContentAsString();
                        }
                    }
                }
            } catch
            {
                
            }
            return "0";
        }
    }
}