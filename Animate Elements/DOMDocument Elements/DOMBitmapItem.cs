using System.Xml.Serialization;
using System.Xml;

namespace XflComponents
{
    [XmlRoot("DOMBitmapItem", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class DOMBitmapItem()
    {
        // Strings
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty; // In general, does not end with .png
        [XmlAttribute("itemID")]
        public string? ItemID { get; set; }
        [XmlAttribute("sourceExternalFilepath")]
        public string? SourceExternalFilepath { get; set; }
        [XmlAttribute("sourceLastImported")]
        public string? SourceLastImported { get; set; }
        [XmlAttribute("externalFileCRC32")]
        public string? ExternalFileCRC32 { get; set; }
        [XmlAttribute("externalFileSize")]
        public string? ExternalFileSize { get; set; }
        [XmlAttribute("allowSmoothing")]
        public string? AllowSmoothing { get; set; }
        [XmlAttribute("useImportedJPEGData")]
        public string? UseImportedJPEGData { get; set; }
        [XmlAttribute("compressionType")]
        public string? CompressionType { get; set; }
        [XmlAttribute("originalCompressionType")]
        public string? OriginalCompressionType { get; set; }
        [XmlAttribute("quality")]
        public string? Quality { get; set; }
        [XmlAttribute("href")]
        public string Href { get; set; } = string.Empty; // In general, ends with .png
        [XmlAttribute("bitmapDataHRef")]
        public string? BitmapDataHRef { get; set; }
        [XmlAttribute("frameRight")]
        public string? FrameRight { get; set; }
        [XmlAttribute("frameBottom")]
        public string? FrameBottom { get; set; }

        public bool ShouldSerializeName() => !string.IsNullOrWhiteSpace(Name);
        public bool ShouldSerializeHref() => !string.IsNullOrWhiteSpace(Href);

        /// <summary>
        /// Get the root name of the bitmap file used
        /// </summary>
        /// <returns>The root bitmap file name</returns>
        public string GetEndBitmapFile() => Path.GetFileName(Name) ?? string.Empty;

        /// <summary>
        /// Change the name of the DOM bitmap item to something new
        /// </summary>
        /// <param name="newName">New name to replace itself with</param>
        /// <param name="endsInPng">If false, ".png" will be added at the end for the href</param>
        public void ChangeName(string newName)
        {
            Name = Path.ChangeExtension(newName, null);
            Name = Path.ChangeExtension(newName, "png");
        }

        public override string ToString()
        {
            return $"DOMBitmapItem named {Href}";
        }
    }
}