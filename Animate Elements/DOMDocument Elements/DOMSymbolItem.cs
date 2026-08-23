using System.Xml.Serialization;
using System.Xml;

namespace XflComponents
{
    [XmlRoot("Include", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class DOMSymbolItem()
    {
        [XmlAttribute("href")]
        public string Href { get; set; } = string.Empty;
        [XmlAttribute("itemIcon")]
        public string? ItemIcon { get; set; }
        [XmlAttribute("loadImmediate")]
        public string? LoadImmediate { get; set; }
        [XmlAttribute("itemID")]
        public string? ItemID { get; set; }
        [XmlAttribute("lastModified")]
        public string? LastModified { get; set; }

        public bool ShouldSerializeHref() => !string.IsNullOrWhiteSpace(Href);

        /// <summary>
        /// Get the root name of the symbol file used
        /// </summary>
        /// <returns>The root symbol file name</returns>
        public string? GetEndSymbolFile() => Path.GetFileNameWithoutExtension(Href) ?? string.Empty;

        public override string ToString()
        {
            return $"DOMSymbolItem named {Href}";
        }
    }
}