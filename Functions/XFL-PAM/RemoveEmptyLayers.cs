using System.Xml.Linq;
using XflComponents;
using UniversalMethods;

namespace HelperFunctions.Functions.Packages
{
    public class RemoveEmptyLayers
    {
        public static void Function()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter an XFL or symbol you want to edit");
            var symbols = GetSymbols();
            int symbolCount = symbols.Count;
            if (symbols.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No symbols are found");
                return;
            }

            ProgressChecker? editedLayers = null;
            string prefix = "Editing Symbols... ";
            if (symbols.Count >= 2)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                editedLayers = new(prefix, symbolCount);
            }
            else
            {
                Console.Write(prefix);
            }

            foreach (var (symbol, path) in symbols)
            {
                symbol.Timeline!.RemoveEmptyLayers();
                symbol.Timeline!.RemoveTrailingFrames();
                UM.SaveXmlDocument(path, symbol, UM.DummyXDocument, SymbolItem.serializer);
                editedLayers?.AddOne();
            }
            if (editedLayers is null) ProgressChecker.WriteFinished();
        }
        
        private static List<(SymbolItem symbol, string path)> GetSymbols()
        {
            var (path, isFile) = UM.AskForPath(["DOMDocument.xml"]);
            if (isFile)
            {
                var symbolDocument = XDocument.Load(path);
                using var documentReader = symbolDocument.CreateReader();
                return [((SymbolItem)SymbolItem.serializer.Deserialize(documentReader)!, path)];
            }
            else
            {
                var symbolDirs = UM.GetAllSymbolDirectories(path);
                List<(SymbolItem symbol, string path)> symbolItems = [];
                foreach (var symbolPath in symbolDirs)
                {
                    var symbolDocument = XDocument.Load(symbolPath);
                    using var documentReader = symbolDocument.CreateReader();
                    symbolItems.Add(((SymbolItem)SymbolItem.serializer.Deserialize(documentReader)!, symbolPath));
                }
                return symbolItems;
            }
        }
    }
}