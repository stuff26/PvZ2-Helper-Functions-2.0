using System.Xml.Serialization;
using System.Xml;

namespace XflComponents
{
    /// <summary>
    /// Contains details for a motion tween
    /// </summary>
    [XmlRoot("AnimationCore", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public class AnimationCore
    {
        [XmlAttribute]
        public string? TimeScale { get; set; }
        [XmlAttribute]
        public string? Version { get; set; }
        [XmlAttribute]
        public string? duration { get; set; }

        [XmlElement]
        public TimeMap? TimeMap { get; set; }
        [XmlElement]
        public MetaData? metadata { get; set; }
        [XmlElement("PropertyContainer")]
        public PropertyContainer? PropertyContainer { get; set; }
    }

    public class TimeMap
    {
        [XmlAttribute]
        public string? strength { get; set; }
        [XmlAttribute]
        public string? type { get; set; }
    }

    public class MetaData
    {
        [XmlArray("names", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("name", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<Name>? Names { get; set; }

        [XmlElement]
        public Settings? Settings { get; set; }
    }

    [XmlRoot("name", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public class Name
    {
        [XmlAttribute]
        public string? langID { get; set; }
        [XmlAttribute]
        public string? value { get; set; }
    }

    public class Settings
    {
        [XmlAttribute]
        public string? orientToPath { get; set; }
        [XmlAttribute]
        public string? xformPtXOffsetPct { get; set; }
        [XmlAttribute]
        public string? xformPtYOffsetPct { get; set; }
        [XmlAttribute]
        public string? xformPtZOffsetPixels { get; set; }
    }

    public class PropertyContainer
    {
        [XmlAttribute]
        public string? id { get; set; }

        [XmlElement("Property", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<Property>? Properties { get; set; }

        [XmlElement("PropertyContainer", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<PropertyContainer>? PropertyContainers { get; set; }
    }

    public class Property
    {
        [XmlAttribute]
        public string? enabled { get; set; }
        [XmlAttribute]
        public string? id { get; set; }
        [XmlAttribute]
        public string? ignoreTimeMap { get; set; }
        [XmlAttribute("readonly")]
        public string? ReadOnly { get; set; }
        [XmlAttribute]
        public string? visible { get; set; }

        [XmlElement("Keyframe", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<TweenKeyframe>? Keyframes { get; set; }
    }

    public class TweenKeyframe
    {
        [XmlAttribute]
        public string? anchor { get; set; }
        [XmlAttribute]
        public string? next { get; set; }
        [XmlAttribute]
        public string? previous { get; set; }
        [XmlAttribute]
        public string? roving { get; set; }
        [XmlAttribute]
        public string? timevalue { get; set; }
    }
}