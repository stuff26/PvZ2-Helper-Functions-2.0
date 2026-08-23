using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace XflComponents
{
    [XmlRoot("Actionscript", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class Actionscript
    {
        [XmlElement("script", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<CDataScript> Scripts { get; set; } = [];

        public bool ShouldSerializeScripts() => Scripts.Count > 0;

        public List<string> GetScripts()
        {
            var foundScripts = new List<string>();
            if (Scripts is null) return foundScripts;
            foreach (var dataScript in Scripts)
            {
                foundScripts.Add(dataScript.Text!);
            }
            return foundScripts;
        }
    }

    public sealed class CDataScript() : IXmlSerializable
    {
        [XmlText]
        public string? Text { get; set; }

        public XmlSchema? GetSchema() => null;
        
        public void ReadXml(XmlReader reader)
        {
            Text = reader.ReadElementContentAsString();
        }

        public void WriteXml(XmlWriter writer)
        {
            if (!string.IsNullOrEmpty(Text))
            {
                writer.WriteCData(Text);
            }
        }
    }
}