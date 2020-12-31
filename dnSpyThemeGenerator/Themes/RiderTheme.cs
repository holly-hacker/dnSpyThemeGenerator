using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace dnSpyThemeGenerator.Themes
{
    internal class RiderTheme
    {
        public string Name { get; init; }
        public Dictionary<string, string> Colors { get; } = new();
        public Dictionary<string, Dictionary<string, string>> Attributes { get; } = new();

        private RiderTheme()
        {
        }

        public static RiderTheme ReadFromStream(Stream stream)
        {
            var doc = XDocument.Load(stream);
            var root = doc.Root;

            var theme = new RiderTheme
            {
                Name = root.Attribute("name").Value,
            };

            foreach (var xNode in root.Nodes())
            {
                if (xNode is XElement xElement)
                {
                    switch (xElement.Name.LocalName)
                    {
                        case "colors":
                        {
                            foreach (var option in xElement.Nodes().Cast<XElement>())
                            {
                                var name = option.Attribute("name").Value;
                                var value = option.Attribute("value").Value;
                                theme.Colors[name] = value;
                            }

                            break;
                        }
                        case "attributes":
                        {
                            foreach (var attributeOption in xElement.Nodes().Cast<XElement>())
                            {
                                var attributeName = attributeOption.Attribute("name").Value;
                                var parsed = ParseAttributeOption(attributeOption);
                                if (parsed is not null)
                                    theme.Attributes[attributeName] = parsed;
                            }

                            break;
                        }
                    }
                }
            }

            return theme;
        }

        private static Dictionary<string, string> ParseAttributeOption(XElement attributeOption)
        {
            if (attributeOption.Attribute("baseAttributes") is { } baseAttr)
            {
                var baseName = baseAttr.Value;
                var newElement = attributeOption.Parent
                    .Nodes()
                    .OfType<XElement>()
                    .SingleOrDefault(x => x.Attribute("name").Value == baseName);


                return newElement is null ? null : ParseAttributeOption(newElement);
            }

            var attributeValue = (XElement) attributeOption.Nodes().Single();
            Dictionary<string, string> dic = new();
            foreach (var option in attributeValue.Nodes().Cast<XElement>())
            {
                var name = option.Attribute("name").Value;
                var value = option.Attribute("value").Value;
                dic[name] = value;
            }

            return dic;
        }
    }
}