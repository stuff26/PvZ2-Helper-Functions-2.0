using System.Xml.Serialization;
using System.Xml;

namespace XflComponents
{
    [XmlRoot("DOMFolderItem", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public class DOMFolder
    {
        [XmlAttribute]
        public string? name { get; set; }
        [XmlAttribute]
        public string? itemID { get; set; }
        [XmlAttribute]
        public string? isExpanded { get; set; }
    }
}