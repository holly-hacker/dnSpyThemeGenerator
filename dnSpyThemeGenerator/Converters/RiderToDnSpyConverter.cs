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

        private static readonly Dictionary<(string key, string attribute), string> ColorMap = new()
        {
            {("linenumber", "fg"), "LINE_NUMBERS_COLOR"},
            {("selectedtext", "bg"), "SELECTION_BACKGROUND"},
            {("inactiveselectedtext", "bg"), "SELECTION_BACKGROUND"},

            {("environmentscrollbarthumbbackground", "bg"), "ScrollBar.thumbColor"},
            {("environmentscrollbarthumbmouseoverbackground", "bg"), "ScrollBar.hoverThumbColor"},
            {("environmentscrollbarbackground", "bg"), "ScrollBar.trackColor"},
            {("environmentscrollbararrowbackground", "bg"), "ScrollBar.trackColor"},
            {("environmentscrollbararrowdisabledbackground", "bg"), "ScrollBar.trackColor"},

            {("treeview", "bg"), "PROMOTION_PANE"},
            {("glyphmargin", "bg"), "GUTTER_BACKGROUND"},
        };

        private static readonly Dictionary<(string key, string attribute), string> HardcodedColors = new()
        {
            {("treeviewitemselected", "bg"), "#1FFFFFFF"},
            {("treeviewitemmouseover", "bg"), "#3FFFFFFF"},
        };

        public void CopyTo(RiderTheme source, DnSpyTheme donor)
        {
            donor.Name = source.Name.ToLower().Replace(" ", "_");
            donor.MenuName = source.Name;
            var bytes = new byte[16];
            new Random(source.Name.GetHashCode()).NextBytes(bytes);
            donor.Guid = new Guid(bytes);
            donor.Order = 9001;

            foreach ((string dnSpyColorName, var dnSpyAttributes) in donor.Colors)
            {
                foreach ((string dnSpyAttributeName, _) in dnSpyAttributes)
                {
                    if (HardcodedColors.TryGetValue((dnSpyColorName, dnSpyAttributeName), out var result))
                    {
                        dnSpyAttributes[dnSpyAttributeName] = result;
                    }
                    else if (AttributeMap.TryGetValue(dnSpyColorName, out string riderAttributeKey))
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
                            Console.WriteLine($"Mapping attribute {dnSpyColorName}.{dnSpyAttributeName}");
                        }
                    }
                    else if (ColorMap.TryGetValue((dnSpyColorName, dnSpyAttributeName), out var riderColorName))
                    {
                        if (!source.Colors.TryGetValue(riderColorName, out var riderColor))
                        {
                            Debug.WriteLine($"couldn't find color {riderColor} in rider theme");
                        }
                        else
                        {
                            dnSpyAttributes[dnSpyAttributeName] = ConvertColor(riderColor);
                            Console.WriteLine($"Mapping color {dnSpyColorName}.{dnSpyAttributeName}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Skipping unknown key " + dnSpyColorName);
                    }
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