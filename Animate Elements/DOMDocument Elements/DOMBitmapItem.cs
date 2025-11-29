using System.Xml.Serialization;
using System.Xml;

namespace XflComponents
{
    [XmlRoot("DOMBitmapItem", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public class DOMBitmapItem
    {
        // Strings
        [XmlAttribute]
        public string? name { get; set; } // In general, does not end with .png
        [XmlAttribute]
        public string? itemID { get; set; }
        [XmlAttribute]
        public string? sourceExternalFilepath { get; set; }
        [XmlAttribute]
        public string? sourceLastImported { get; set; }
        [XmlAttribute]
        public string? externalFileCRC32 { get; set; }
        [XmlAttribute]
        public string? externalFileSize { get; set; }
        [XmlAttribute]
        public string? allowSmoothing { get; set; }
        [XmlAttribute]
        public string? useImportedJPEGData { get; set; }
        [XmlAttribute]
        public string? compressionType { get; set; }
        [XmlAttribute]
        public string? originalCompressionType { get; set; }
        [XmlAttribute]
        public string? quality { get; set; }
        [XmlAttribute]
        public string? href { get; set; } // In general, ends with .png
        [XmlAttribute]
        public string? bitmapDataHRef { get; set; }
        [XmlAttribute]
        public string? frameRight { get; set; }
        [XmlAttribute]
        public string? frameBottom { get; set; }

        /// <summary>
        /// Get the root name of the bitmap file used
        /// </summary>
        /// <returns>The root bitmap file name</returns>
        public string? GetEndBitmapFile()
        {
            if (name is null) return null;
            string[] splitString = name.Split("/");
            string tempString = splitString[^1];
            return tempString;
        }
    }
}