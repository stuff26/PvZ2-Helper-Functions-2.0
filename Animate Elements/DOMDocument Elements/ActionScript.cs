using System.Xml.Serialization;
using System.Xml;


namespace XflComponents
{
    [XmlRoot("Actionscript", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public class Actionscript
    {
        [XmlElement("script", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<CDataScript>? scripts { get; set; }

        public List<string> GetScripts()
        {
            var foundScripts = new List<string>();
            if (scripts is null) return foundScripts;
            foreach (var dataScript in scripts)
            {
                foundScripts.Add(dataScript.Text!);
            }
            return foundScripts;
        }
    }

    public class CDataScript : IXmlSerializable
    {
        [XmlText]
        public string? Text { get; set; }

        public System.Xml.Schema.XmlSchema? GetSchema() => null;

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