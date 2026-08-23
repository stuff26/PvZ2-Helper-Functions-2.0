using System.Xml.Serialization;
using System.Xml;

namespace XflComponents
{
    /// <summary>
    /// Contains details for a motion tween
    /// </summary>
    [XmlRoot("AnimationCore", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class AnimationCore
    {
        [XmlAttribute]
        public string? TimeScale { get; set; }
        [XmlAttribute]
        public string? Version { get; set; }
        [XmlAttribute("duration")]
        public string? Duration { get; set; }

        [XmlElement]
        public TimeMap? TimeMap { get; set; }
        [XmlElement("metadata")]
        public MetaData? Metadata { get; set; }
        [XmlElement("PropertyContainer")]
        public PropertyContainer? PropertyContainer { get; set; }
    }

    public sealed class TimeMap
    {
        [XmlAttribute("strength")]
        public string? Strength { get; set; }
        [XmlAttribute("type")]
        public string? Type { get; set; }
    }

    public sealed class MetaData
    {
        [XmlArray("names", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("name", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<Name> Names { get; set; } = [];

        [XmlElement]
        public Settings? Settings { get; set; }
    }

    [XmlRoot("name", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class Name
    {
        [XmlAttribute("langID")]
        public string? LangID { get; set; }
        [XmlAttribute("value")]
        public string? Value { get; set; }
    }

    public sealed class Settings
    {
        [XmlAttribute("orientToPath")]
        public string? OrientToPath { get; set; }
        [XmlAttribute("xformPtXOffsetPct")]
        public string? XformPtXOffsetPct { get; set; }
        [XmlAttribute("xformPtYOffsetPct")]
        public string? XformPtYOffsetPct { get; set; }
        [XmlAttribute("xformPtZOffsetPixels")]
        public string? XformPtZOffsetPixels { get; set; }
    }

    public sealed class PropertyContainer
    {
        [XmlAttribute("id")]
        public string? Id { get; set; }

        [XmlElement("Property", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<Property> Properties { get; set; } = [];

        [XmlElement("PropertyContainer", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<PropertyContainer> PropertyContainers { get; set; } = [];
    }

    public sealed class Property
    {
        [XmlAttribute("enabled")]
        public string? Enabled { get; set; }
        [XmlAttribute("id")]
        public string? Id { get; set; }
        [XmlAttribute("ignoreTimeMap")]
        public string? IgnoreTimeMap { get; set; }
        [XmlAttribute("readonly")]
        public string? ReadOnly { get; set; }
        [XmlAttribute("visible")]
        public string? Visible { get; set; }

        [XmlElement("Keyframe", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<TweenKeyframe>? Keyframes { get; set; }
    }

    public sealed class TweenKeyframe
    {
        [XmlAttribute("anchor")]
        public string? Anchor { get; set; }
        [XmlAttribute("next")]
        public string? Next { get; set; }
        [XmlAttribute("previous")]
        public string? Previous { get; set; }
        [XmlAttribute("roving")]
        public string? Roving { get; set; }
        [XmlAttribute("timevalue")]
        public string? Timevalue { get; set; }
    }
}