using System.Xml.Serialization;
using System.Xml;

namespace XflComponents
{
    [XmlRoot("Include", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public class DOMSymbolItem
    {
        [XmlAttribute]
        public string? href { get; set; }
        [XmlAttribute]
        public string? itemIcon { get; set; }
        [XmlAttribute]
        public string? loadImmediate { get; set; }
        [XmlAttribute]
        public string? itemID { get; set; }
        [XmlAttribute]
        public string? lastModified { get; set; }

        /// <summary>
        /// Get the root name of the symbol file used
        /// </summary>
        /// <returns>The root symbol file name</returns>
        public string? GetEndSymbolFile()
        {
            if (href is null) return null;
            string[] splitString = href.Split("/");
            string tempString = splitString[^1];
            return tempString.Replace(".xml", "");
        }
    }
}