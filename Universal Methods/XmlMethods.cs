
using System.Xml.Linq;
using System.Xml.Serialization;

namespace UniversalMethods
{    
    public static class XmlMethods
    {
        public static readonly XDocument DummyXDocument = new();
        /// <summary>
        /// Write an XML document in a specified location
        /// </summary>
        /// <param name="documentPath">Path to save the XML to</param>
        /// <param name="newDocument">Object that should be serialized into an XML</param>
        /// <param name="originalDocument">XDocument to use when serializing</param>
        /// <param name="serializer">Serializer to determine what type of object should be serialized</param>
        public static void SaveXmlDocument(string documentPath, object newDocument, XmlSerializer serializer)
        {

            string newXml;
            using (var sw = new StringWriter())
            {
                serializer.Serialize(sw, newDocument);
                newXml = sw.ToString();
            }
            XDocument updatedDocument = XDocument.Parse(newXml);
            DummyXDocument.Root?.ReplaceWith(updatedDocument.Root);

            File.WriteAllText(documentPath, updatedDocument.ToString());
        }
    }
}