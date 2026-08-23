using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using XflComponents;
using UniversalMethods;
using System.Text.Json;

public class XMLTester
{
    public static void Function()
    {
        Console.WriteLine("~~~~~~~~~~~~~~~~~~`");
        string documentPath = @"C:\Users\zacha\Documents\main.675.com.ea.game.pvz2_aub.obb.bundle\packages\ZombieTypes.json";
        var rawText = File.ReadAllText(documentPath);
        using (JsonDocument document = JsonDocument.Parse(rawText))
        {
            Console.WriteLine(document.RootElement.GetProperty("version"));
        }

    }
}