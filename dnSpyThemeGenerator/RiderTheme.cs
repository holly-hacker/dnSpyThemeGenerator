using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            foreach ((string dnSpyColor, var dnSpyAttributes) in donor.Colors)
            {
                var riderColor = MapColorKey(dnSpyColor);
                if (riderColor is null)
                {
                    Debug.WriteLine("Skipping unknown key " + dnSpyColor);
                    continue;
                }

                foreach ((string dnSpyAttributeName, _) in dnSpyAttributes)
                {
                    var riderAttributeName = MapAttributeName(dnSpyAttributeName);
                    if (riderAttributeName is null)
                    {
                        if (dnSpyAttributeName != "name")
                            Debug.WriteLine("Skipping unknown attribute " + dnSpyAttributeName);
                        continue;
                    }

                    dnSpyAttributes[dnSpyAttributeName] = "#" + _attributes[riderColor][riderAttributeName];
                    Console.WriteLine($"Mapping {dnSpyColor}.{dnSpyAttributeName}");
                }
            }
        }

        private static string MapColorKey(string dnSpyName)
        {
            return dnSpyName switch
            {
                "defaulttext" => "TEXT",
                "operator" => "DEFAULT_OPERATION_SIGN",
                "punctuation" => "DEFAULT_DOT",
                "number" => "DEFAULT_NUMBER",
                "comment" => "DEFAULT_LINE_COMMENT",
                "keyword" => "DEFAULT_KEYWORD",
                "string" => "DEFAULT_STRING",
                "verbatimstring" => "DEFAULT_STRING",
                "char" => "DEFAULT_STRING",
                "namespace" => "ReSharper.NAMESPACE_IDENTIFIER",
                "type" => "DEFAULT_CLASS_NAME", // TODO
                // TODO: sealedtype, statictype
                "statictype" => "ReSharper.STATIC_CLASS_IDENTIFIER",
                "delegate" => "ReSharper.DELEGATE_IDENTIFIER",
                "enum" => "ReSharper.ENUM_IDENTIFIER",
                "interface" => "DEFAULT_INTERFACE_NAME",
                "valuetype" => "ReSharper.STRUCT_IDENTIFIER",
                // TODO: module
                "typegenericparameter" => "ReSharper.TYPE_PARAMETER_IDENTIFIER",
                "methodgenericparameter" => "ReSharper.TYPE_PARAMETER_IDENTIFIER",
                "instancemethod" => "DEFAULT_INSTANCE_METHOD",
                "staticmethod" => "DEFAULT_STATIC_METHOD",
                "extensionmethod" => "ReSharper.EXTENSION_METHOD_IDENTIFIER",
                "instancefield" or "instanceevent" or "instanceproperty" => "DEFAULT_INSTANCE_FIELD",
                "enumfield" or "literalfield" => "DEFAULT_INSTANCE_FIELD",
                "staticfield" or "staticevent" or "staticproperty" => "DEFAULT_STATIC_FIELD",
                "local" => "DEFAULT_LOCAL_VARIABLE",
                "parameter" => "DEFAULT_PARAMETER",
                "preprocessorkeyword" => "DEFAULT_KEYWORD",
                // TODO: preprocessortext
                "label" => "DEFAULT_LABEL",
                _ => null,
            };
        }

        private static string MapAttributeName(string dnSpyName)
        {
            return dnSpyName switch
            {
                "fg" => "FOREGROUND",
                "bg" => "BACKGROUND",
                _ => null,
            };
        }
    }
}