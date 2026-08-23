using System.Xml.Serialization;
using System.Xml;

namespace XflComponents
{
    [XmlRoot("DOMFolderItem", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class DOMFolder
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;
        [XmlAttribute("itemID")]
        public string? ItemID { get; set; }
        [XmlAttribute("isExpanded")]
        public string? IsExpanded { get; set; }

        public bool ShouldSerializeName() => !string.IsNullOrWhiteSpace(Name);
        
        public override string ToString()
        {
            return $"DOMFolder named {Name}";
        }
    }
}