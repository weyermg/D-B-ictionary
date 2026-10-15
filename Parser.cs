using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Elastic.Clients.Elasticsearch;

namespace Ingester
{
    public class Parser
    {  
        private static readonly ImmutableHashSet<string> PARTS_OF_SPEECH = ImmutableHashSet.Create(
            "Adjective", "Adverb", "Ambiposition", "Article", "Circumposition",
            "Classifier","Conjunction","Contraction","Counter","Determiner","Ideophone","Interjection",
            "Noun","Numeral","Participle","Particle","Postposition","Preposition","Pronoun","Proper noun","Verb",
            "Circumfix","Combining form","Infix","Interfix","Prefix","Root","Suffix",
            "Diacritical mark", "Letter", "Ligature", "Number", "Punctuation Mark",
            "Syllable","Symbol","Phrase","Proverb","Prepositional phrase","Han character",
            "Hanzi", "Kanji", "Hanja","Romanization","Logogram","Determinative"
        );
        public static void Main()
        {
            Skim();
        }
        public static void Skim()
        {
            string path = "../enwiktionary.xml";

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
                        if (nsValue == "-11")
                        {
                            continue;
                        }
                        if (int.Parse(nsValue) == 0)
                        {
                            string page = GetText(rawPageXml);
                            var word = ParsePage(page);
                            PrintWord(word);
                        }
                        return;

                        /*if (pageCount % 1000 == 0)
                        {
                            double pct = (double)fs.Position / totalBytes * 100;
                            long currentMB = fs.Position / 1024 / 1024;
                            long totalMB = totalBytes / 1024 / 1024;
                            Console.WriteLine($"[Processed {pageCount:N0} pages] Current Namespace Routed:  | Progress: {currentMB:N0}MB / {totalMB:N0}MB ({pct:F2}%)");
                        }*/
                    }
                }
            }
        }
        public static void PrintWord( Dictionary<string, List<Dictionary<string, List<string>>>> word)
        {
            foreach (var language in word)
            {
                Console.WriteLine($"Language: {language.Key}");
                for (int i = 0; i < language.Value.Count; i++)
                {
                    Console.WriteLine($"    Etymology {i + 1}");
                    foreach (var pos in language.Value[i])
                    {
                        Console.WriteLine($"        {pos.Key}:");
                        foreach (string definition in pos.Value)
                        {
                            Console.WriteLine($"            {definition}");
                        }
                    }
                }
            }
        }

        private static string ProcessDefinition(string definition, int mainNumber)
        {
            List<string> result = [];
            string[] lines = definition.Split('\n');
            bool skippingQuote = false;

            foreach (string rawline in lines)
            {
                string line = rawline.TrimEnd();
                if (line.Contains("#*"))
                {
                    skippingQuote = true;
                    continue;
                }
                if (skippingQuote)
                {
                    if (Regex.IsMatch(line, @"^#+(?![:*])\s"))
                    {
                        skippingQuote = false;
                    } else
                    {
                        continue;
                    }
                }

                if (Regex.IsMatch(line, @"^#+[:*]")) continue;
                Match match = Regex.Match(line, @"^(?<hashes>#+)\s*(?<text>.*)$");
                if (!match.Success)
                {
                    if (!string.IsNullOrWhiteSpace(line)) result.Add(line);
                    continue;
                }
                int depth = match.Groups["hashes"].Length;
                string text = CleanDefinition(match.Groups["text"].Value.Trim());
                if (depth == 1)
                {
                    result.Add($"{mainNumber}. {text}");
                } else
                {
                    int indentation = ((depth - 1) * 3) + 12;
                    result.Add(new string(' ', indentation) + "- " + text);
                }
            }
            return string.Join(Environment.NewLine, result);
        }

        private static string CleanDefinition(string definition)
        {
            var result = new StringBuilder();
            int i = 0;
            while (i < definition.Length)
            {
                //Parse {{<wikitext data>}}
                if (i + 1 < definition.Length &&
                    definition[i] == '{' &&
                    definition[i + 1] == '{')
                {
                    int end = FindTemplateEnd(definition, i);

                    if (end != -1)
                    {
                        string template = definition.Substring(i + 2, end - (i + 2));
                        List<string> args = SplitTemplateArgs(template);
                        result.Append(ReplaceTemplate(args));
                        i = end + 2;
                        continue;
                    }
                }
                result.Append(definition[i]);
                i++;
            }
            
            

            //Replace [[page|display text]] with display text
            definition = Regex.Replace(result.ToString(), @"\[\[(?:[^|\]]*\|)?([^\]]+)\]\]", "$1");

            return definition;
        }

        private static string ReplaceTemplate(List<string> args)
        {
            if (args.Count == 0) return "";
            switch (args[0])
            {
                case "lb":
                case "lbl":
                case "label":
                case "tlb":
                case "term-label":
                    return args.Count > 2 ? "(" + string.Join(" ", args.GetRange(2, args.Count - 2)).Replace(" _", "") + ")": "";
                case "ant":
                case "syn":
                    return args.Count > 2 ? string.Join(", ", args.GetRange(2, args.Count - 2)) : "";
                case "ux":
                    return args.Count > 2 ? args[2] : "";
                case "l":
                case "link":
                    return args.Count >= 4 ? args[3] : (args.Count >= 3 ? args[2] : "");
                default:
                    if (args[0].Trim().EndsWith(" of", StringComparison.OrdinalIgnoreCase) && args.Count >= 2)
                    {
                        if (args.Count >= 3) return args[0] + " " + args[2];
                        return args[0] + " " + args[1];
                    }
                    
                    return "";
                
            }
        }

        private static List<string> SplitTemplateArgs(string template)
        {
            List<string> args = [];
            StringBuilder current = new();
            int depth = 0;

            for (int i = 0; i < template.Length; i++)
            {
                if (i + 1 < template.Length && template[i] == '{' && template[i+1] == '{')
                {
                    depth++;
                    current.Append("{{");
                } else if (i + 1 < template.Length && template[i] == '}' && template[i + 1] == '}')
                {
                    depth--;
                    current.Append("}}");
                    i++;
                } else if (template[i] == '|' && depth == 0)
                {
                    args.Add(current.ToString().Trim());
                    current.Clear();
                } else
                {
                    current.Append(template[i]);
                }
            }
            args.Add(current.ToString().Trim());
            return args;
        }

        private static int FindTemplateEnd(string text, int start)
        {
            int depth = 1;
            for (int i = start + 2; i < text.Length - 1; i++)
            {
                if (text[i] == '{' && text[i + 1] == '{')
                {
                    depth++;
                    i++;
                }
                else if (text[i] == '}' && text[i+1] == '}')
                {
                    depth--;
                    if (depth == 0) return i;
                    i++;
                }
            }
            return -1;
        }

        private static Dictionary<string,List<Dictionary<string,List<string>>>> ParsePage(string page)
        {
            string[] lines = page.Replace("\r\n", "\n").Split('\n');

            Regex headingRegex = new(@"^(={2,6})[ \t]*(.+?)[ \t]*\1[ \t]*$");
            Regex defRegex = new(@"^(?<hashes>#+)(?=\s|$)");

            Dictionary<string,List<Dictionary<string,List<string>>>> word = [];

            string? currentLanguage = null;
            string? currentPos = null;
            
            List<Dictionary<string,List<string>>> etymologies = [];
            Dictionary<string,List<string>> pos = [];
            List<string> currentDefs = [];
            StringBuilder currentDef = new();
            int defNumber = 0;

            void FlushDefinition()
            {
                if (currentDef.Length == 0) return;
                defNumber++;
                currentDefs.Add(ProcessDefinition(currentDef.ToString().TrimEnd(), defNumber));
                currentDef.Clear();
            }

            void FlushPos()
            {
                FlushDefinition();
                if (currentPos != null && currentDefs.Count > 0)
                    pos[currentPos] = currentDefs;
                    currentDefs = [];
                    currentPos = null;
                    defNumber = 0;
            }

            void FlushEtymology()
            {
                FlushPos();
                if (currentLanguage == null) return;
                if (pos.Count > 0) etymologies.Add(pos);
                pos = [];
            }

            void FlushLanguage()
            {
                FlushEtymology();
                if (currentLanguage != null)
                    word[currentLanguage] = etymologies;
                etymologies = [];
            }

            foreach (string line in lines)
            {
                Match heading = headingRegex.Match(line);

                if (heading.Success)
                {
                    string title = heading.Groups[2].Value.Trim();
                    int level = heading.Groups[1].Length;

                    FlushPos();

                    if (level == 2)
                    {
                        FlushLanguage();
                        currentLanguage = title;
                        continue;
                    }

                    Match etymNum = Regex.Match(title, @"^Etymology(?:\s+(\d+))?$");
                    if (etymNum.Success)
                    {
                        FlushEtymology();
                        continue;
                    }

                    if (PARTS_OF_SPEECH.Contains(title))
                    {
                        currentPos = title;
                        etymologies.Add(pos);
                    } 
                    continue;
                }

                if (currentPos == null) continue;
                
                Match definition = defRegex.Match(line);
                if (definition.Success)
                {
                    string hashes = definition.Groups["hashes"].Value;
                    int depth = hashes.Length;

                    if (depth == 1)
                    {
                        FlushDefinition();
                        currentDef.AppendLine(line);
                    } else if (currentDef.Length > 0)
                    {
                        currentDef.AppendLine(line);
                    }
                } else if (currentDef.Length > 0)
                {
                    currentDef.AppendLine(line);
                }
            }
            FlushLanguage();
            return word;
        }

        public static string GetText(string pageXml)
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
                        if (subReader.NodeType == XmlNodeType.Element && subReader.Name == "text")
                        {
                            return subReader.ReadElementContentAsString();
                        }
                    }
                }
            } catch
            {
                return null;
            } 
            return null;
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
                        if (subReader.NodeType == XmlNodeType.Element && subReader.Name == "title")
                        {
                            if (!subReader.ReadElementContentAsString().Equals("free"))
                            {
                                return "-11";
                            }
                        }
                        if (subReader.NodeType == XmlNodeType.Element && subReader.Name == "ns")
                        {
                            return subReader.ReadElementContentAsString();
                        }
                    }
                }
            } catch
            {
                
            }
            return "-10";
        }
    }
}