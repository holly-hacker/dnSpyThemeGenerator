using System;
using System.Diagnostics;
using dnSpyThemeGenerator.Themes;

namespace dnSpyThemeGenerator.Converters
{
    internal class RiderToDnSpyConverter : IThemeConverter<RiderTheme, DnSpyTheme>
    {
        public void CopyTo(RiderTheme source, DnSpyTheme donor)
        {
            donor.Name = source.Name.ToLower().Replace(" ", "_");
            donor.MenuName = source.Name;
            var bytes = new byte[16];
            new Random(source.Name.GetHashCode()).NextBytes(bytes);
            donor.Guid = new Guid(bytes);
            donor.Order = 9001;

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
                    }
                    else if (!source.Attributes.TryGetValue(riderColor, out var riderAttributes))
                    {
                        Debug.WriteLine("Couldn't resolve rider attribute " + dnSpyAttributeName);
                    }
                    else if (riderAttributes.TryGetValue(riderAttributeName, out var riderValue))
                    {
                        dnSpyAttributes[dnSpyAttributeName] = "#" + riderValue.PadLeft(6, '0');
                        Console.WriteLine($"Mapping {dnSpyColor}.{dnSpyAttributeName}");
                    }
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
                "opcode" => "DEFAULT_KEYWORD",
                // TODO: ...
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