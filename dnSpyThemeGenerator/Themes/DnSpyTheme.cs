using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace dnSpyThemeGenerator.Themes
{
    internal class DnSpyTheme
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string MenuName { get; set; }
        public int Order { get; set; }
        public bool IsDark { get; set; }
        public bool IsHighContrast { get; set; }
        public Dictionary<string, Dictionary<string, string>> Colors { get; } = new();

        private DnSpyTheme()
        {
        }

        public static DnSpyTheme ReadFromStream(Stream stream)
        {
            var doc = XDocument.Load(stream);
            var root = doc.Root;

            var theme = new DnSpyTheme
            {
                Guid = Guid.Parse(root.Attribute("guid").Value),
                Name = root.Attribute("name").Value,
                MenuName = root.Attribute("menu-name").Value,
                Order = int.Parse(root.Attribute("order").Value),
                IsDark = bool.Parse(root.Attribute("is-dark").Value),
                IsHighContrast = bool.Parse(root.Attribute("is-high-contrast").Value),
            };

            var colors = (XElement) root.FirstNode;
            foreach (var color in colors.Nodes().Cast<XElement>())
            {
                var pairs = color.Attributes().Select(x => new KeyValuePair<string, string>(x.Name.LocalName, x.Value));
                var dic = new Dictionary<string, string>(pairs);
                theme.Colors.Add(dic["name"], dic);
            }

            return theme;
        }

        public void WriteToStream(Stream stream)
        {
            var colorElements = Colors.Values.Select(c =>
            {
                var e = new XElement("color");
                foreach ((string key, string value) in c)
                    e.SetAttributeValue(key, value);
                return (object) e;
            }).ToArray();
            var colors = new XElement("colors", colorElements);

            var root = new XElement("theme", colors);
            root.SetAttributeValue("guid", Guid.ToString());
            root.SetAttributeValue("name", Name);
            root.SetAttributeValue("menu-name", MenuName);
            root.SetAttributeValue("order", Order.ToString());
            root.SetAttributeValue("is-dark", IsDark.ToString());
            root.SetAttributeValue("is-high-contrast", IsHighContrast.ToString());

            var doc = new XDocument(root);

            using var writer = XmlWriter.Create(stream, new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true,
                IndentChars = "    ",
            });
            doc.WriteTo(writer);
        }
    }
}