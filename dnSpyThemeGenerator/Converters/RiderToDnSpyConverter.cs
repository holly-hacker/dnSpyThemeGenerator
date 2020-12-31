using System;
using System.Collections.Generic;
using System.Diagnostics;
using dnSpyThemeGenerator.Themes;

namespace dnSpyThemeGenerator.Converters
{
    internal class RiderToDnSpyConverter : IThemeConverter<RiderTheme, DnSpyTheme>
    {
        private static readonly Dictionary<string, string> AttributeMap = new()
        {
            {"defaulttext", "TEXT"},
            {"operator", "DEFAULT_OPERATION_SIGN"},
            {"punctuation", "DEFAULT_DOT"},
            {"number", "DEFAULT_NUMBER"},
            {"comment", "DEFAULT_LINE_COMMENT"},
            {"keyword", "DEFAULT_KEYWORD"},
            {"string", "DEFAULT_STRING"},
            {"verbatimstring", "DEFAULT_STRING"},
            {"char", "DEFAULT_STRING"},
            {"namespace", "ReSharper.NAMESPACE_IDENTIFIER"},
            {"type", "DEFAULT_CLASS_NAME"},
            // TODO: sealedtype, statictype
            {"statictype", "ReSharper.STATIC_CLASS_IDENTIFIER"},
            {"delegate", "ReSharper.DELEGATE_IDENTIFIER"},
            {"enum", "ReSharper.ENUM_IDENTIFIER"},
            {"interface", "DEFAULT_INTERFACE_NAME"},
            {"valuetype", "ReSharper.STRUCT_IDENTIFIER"},
            // TODO: module
            {"typegenericparameter", "ReSharper.TYPE_PARAMETER_IDENTIFIER"},
            {"methodgenericparameter", "ReSharper.TYPE_PARAMETER_IDENTIFIER"},
            {"instancemethod", "DEFAULT_INSTANCE_METHOD"},
            {"staticmethod", "DEFAULT_STATIC_METHOD"},
            {"extensionmethod", "ReSharper.EXTENSION_METHOD_IDENTIFIER"},
            {"instancefield", "DEFAULT_INSTANCE_FIELD"},
            {"instanceevent", "DEFAULT_INSTANCE_FIELD"},
            {"instanceproperty", "DEFAULT_INSTANCE_FIELD"},
            {"enumfield", "DEFAULT_INSTANCE_FIELD"},
            {"literalfield", "DEFAULT_INSTANCE_FIELD"},
            {"staticfield", "DEFAULT_STATIC_FIELD"},
            {"staticevent", "DEFAULT_STATIC_FIELD"},
            {"staticproperty", "DEFAULT_STATIC_FIELD"},
            {"local", "DEFAULT_LOCAL_VARIABLE"},
            {"parameter", "DEFAULT_PARAMETER"},
            {"preprocessorkeyword", "DEFAULT_KEYWORD"},
            // TODO: preprocessortext
            {"label", "DEFAULT_LABEL"},
            {"opcode", "DEFAULT_KEYWORD"},
            // TODO: ...
        };

        private static readonly Dictionary<string, (string riderKey, string dnSpyAttribute)> ColorMap = new()
        {
            {"linenumber", ("LINE_NUMBERS_COLOR", "fg")},
            {"selectedtext", ("SELECTION_BACKGROUND", "bg")},
            {"inactiveselectedtext", ("SELECTION_BACKGROUND", "bg")},
            // {"tooltipbackgroun(d", (, "bg")"TOLTIP"},
            
            {"environmentscrollbarthumbbackground", ("ScrollBar.thumbColor", "bg")},
            {"environmentscrollbarthumbmouseoverbackground", ("ScrollBar.hoverThumbColor", "bg")},
            {"environmentscrollbarbackground", ("ScrollBar.trackColor", "bg")},
            {"environmentscrollbararrowbackground", ("ScrollBar.trackColor", "bg")},
            {"environmentscrollbararrowdisabledbackground", ("ScrollBar.trackColor", "bg")},

            {"treeview", ("PROMOTION_PANE", "bg")},
            {"glyphmargin", ("GUTTER_BACKGROUND", "bg")},
        };

        private static readonly Dictionary<string, (string color, string attr)> HardcodedColors = new()
        {
            {"treeviewitemselected", ("#1FFFFFFF", "bg")},
            {"treeviewitemmouseover", ("#3FFFFFFF", "bg")},
        };

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
                if (HardcodedColors.TryGetValue(dnSpyColor, out var result))
                {
                    dnSpyAttributes[result.attr] = result.color;
                }
                else if (AttributeMap.TryGetValue(dnSpyColor, out string riderAttributeKey))
                {
                    foreach ((string dnSpyAttributeName, _) in dnSpyAttributes)
                    {
                        var riderAttributeName = MapAttributeName(dnSpyAttributeName);
                        if (riderAttributeName is null)
                        {
                            if (dnSpyAttributeName != "name")
                                Debug.WriteLine("Skipping unknown attribute " + dnSpyAttributeName);
                        }
                        else if (!source.Attributes.TryGetValue(riderAttributeKey, out var riderAttributes))
                        {
                            Debug.WriteLine("Couldn't resolve rider attribute " + dnSpyAttributeName);
                        }
                        else if (!riderAttributes.TryGetValue(riderAttributeName, out var riderValue))
                        {
                            Debug.WriteLine("Couldn't find attribute in rider attributes: " + riderAttributeName);
                        }
                        else
                        {
                            dnSpyAttributes[dnSpyAttributeName] = ConvertColor(riderValue);
                            Console.WriteLine($"Mapping attribute {dnSpyColor}.{dnSpyAttributeName}");
                        }
                    }
                }
                else if (ColorMap.TryGetValue(dnSpyColor, out var riderColorTuple))
                {
                    if (!source.Colors.TryGetValue(riderColorTuple.riderKey, out var riderColor))
                    {
                        Debug.WriteLine($"couldn't find color {riderColor} in rider theme");
                    }
                    else
                    {
                        dnSpyAttributes[riderColorTuple.dnSpyAttribute] = ConvertColor(riderColor);
                        Console.WriteLine($"Mapping color {dnSpyColor}.{riderColorTuple.dnSpyAttribute}");
                    }
                }
                else
                {
                    Debug.WriteLine("Skipping unknown key " + dnSpyColor);
                }
            }

            /*
            var bgColor = donor.Colors["defaulttext"]["bg"];
            donor.Colors["dialogwindow"]["bg"] = bgColor;
            donor.Colors["dialogwindowactivecaption"]["bg"] = bgColor;
            donor.Colors["dialogwindowinactivecaption"]["bg"] = bgColor;
            
            donor.Colors["environmentbackground"]["fg"] = bgColor;
            donor.Colors["environmentbackground"]["bg"] = bgColor;
            donor.Colors["environmentbackground"]["color3"] = bgColor;
            donor.Colors["environmentbackground"]["color4"] = bgColor;
            
            donor.Colors["environmentmainwindowactivecaption"]["bg"] = bgColor;
            donor.Colors["environmentmainwindowinactivecaption"]["bg"] = bgColor;
            */
        }

        private static string ConvertColor(string color)
        {
            return "#" + color.PadLeft(6, '0');
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