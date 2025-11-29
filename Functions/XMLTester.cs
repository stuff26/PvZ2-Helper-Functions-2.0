using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using XflComponents;
using UniversalMethods;

public class XMLTester
{
    public static void Function()
    {
        Console.WriteLine("~~~~~~~~~~~~~~~~~~`");
        string documentPath = @"C:\Users\zacha\Downloads\zombie_dark_wizard_4\zombie_dark_wizard_4\DOMDocument.xml";
        XDocument document = XDocument.Load(documentPath);

        using var documentReader = document.CreateReader();
        DOMDocument? DOMDocumentTest = (DOMDocument?)DOMDocument.serializer.Deserialize(documentReader);


        UM.SaveXmlDocument(documentPath, DOMDocumentTest!, document, DOMDocument.serializer);

    }
}