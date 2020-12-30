using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace dnSpyThemeGenerator
{
    public class RiderTheme
    {
        private string _name;
        private readonly Dictionary<string, string> _colors = new();
        private readonly Dictionary<string, Dictionary<string, string>> _attributes = new();

        private RiderTheme()
        {
        }

        public static RiderTheme ReadFromStream(Stream stream)
        {
            var doc = XDocument.Load(stream);
            var root = doc.Root;

            var theme = new RiderTheme
            {
                _name = root.Attribute("name").Value
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
                                theme._colors[name] = value;
                            }

                            break;
                        }
                        case "attributes":
                        {
                            foreach (var attributeOption in xElement.Nodes().Cast<XElement>())
                            {
                                var attributeName = attributeOption.Attribute("name").Value;
                                theme._attributes[attributeName] = ParseAttributeOption(attributeOption);
                            }

                            break;
                        }
                        default: throw new Exception("Unexpected attribute name: " + xElement.Name);
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
                    .Single(x => x.Attribute("name").Value == baseName);

                return ParseAttributeOption(newElement);
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

        public void CopyTo(DnSpyTheme donor)
        {
            donor.Name = _name.ToLower().Replace(" ", "_");
            donor.MenuName = _name;
            var bytes = new byte[16];
            new Random(_name.GetHashCode()).NextBytes(bytes);
            donor.Guid = new Guid(bytes);
            donor.Order = 9001;
            
            // TODO: colors
            donor.Colors["string"]["fg"] = "Orange";
        }
    }
}