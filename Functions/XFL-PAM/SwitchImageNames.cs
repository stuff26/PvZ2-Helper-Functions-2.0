using UniversalMethods;
using System.Xml.Linq;
using System.Text;
using XflComponents;

namespace HelperFunctions.Functions.Packages
{
    public class SwitchImageNames
    {
        /*
        CHECKLIST
        - Account for possibility of duplicate media or image names
        - Add progress checker stuff
        */
        public static void Function()
        {
            // Get XFL and DOMDocument
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter the XFL you want to edit");
            var xflPath = UM.AskForDirectory(["DOMDocument.xml"]);
            string domdocumentPath = Path.Join(xflPath, "DOMDocument.xml");
            XDocument document = XDocument.Load(domdocumentPath);
            using var documentReader = document.CreateReader();
            DOMDocument DOMDocumentObject = (DOMDocument?)DOMDocument.serializer.Deserialize(documentReader)!;

            // Ask to what kind of switch to do
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter type of switch you want to do:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[1] Change media names to names used in image symbols");
            Console.WriteLine("[2] Change image names to names used in media");
            var switchType = UM.AskForInt(1, 2);
            
            // Get image symbol names and objects
            var imageSymbols = GetImageSymbolNames(DOMDocumentObject);
            var imageSymbolObjects = GetImageSymbolObjects(imageSymbols, xflPath);
            var mismatchSymbols = GetMismatchSymbols(imageSymbolObjects);
            if (mismatchSymbols.Count == 0) // If there are no mismatched symbols, end process immediately
            {
                Console.WriteLine("No mismatched symbols found");
                return;
            }
            if (switchType == 1)
            {
                ChangeMediaNames(DOMDocumentObject, mismatchSymbols, imageSymbolObjects, xflPath);
            }
            else if (switchType == 2)
            {
                var imageSymbolSwaps = ChangeImageNames(DOMDocumentObject, mismatchSymbols, imageSymbolObjects, xflPath);
                AdjustSymbols(imageSymbolSwaps, xflPath, DOMDocumentObject);
            }

            UM.SaveXmlDocument(domdocumentPath, DOMDocumentObject, UM.DummyXDocument, DOMDocument.serializer);
        }

        public static List<string> GetImageSymbolNames(DOMDocument DOMDocumentObject)
        {
            List<string> imageSymbolNames = [];
            foreach (var symbolName in DOMDocumentObject.GetAllSymbolNames())
            {
                if (symbolName.StartsWith("image/"))
                {
                    imageSymbolNames.Add(symbolName.Replace(".xml", ""));
                }
            }
            return imageSymbolNames;
        }

        public static Dictionary<string, SymbolItem> GetImageSymbolObjects(List<string> imageSymbolNames, string xflPath)
        {
            Dictionary<string, SymbolItem> imageSymbolObjects = [];
            foreach (var symbolName in imageSymbolNames)
            {
                var symbolPath = Path.Join(xflPath, "library", $"{symbolName}.xml");
                XDocument symbolDocument = XDocument.Load(symbolPath);
                using var symbolReader = symbolDocument.CreateReader();
                SymbolItem symbol = (SymbolItem)SymbolItem.serializer.Deserialize(symbolReader)!;
                imageSymbolObjects.Add(symbolName, symbol);
            }
            return imageSymbolObjects;
        }
    
        public static List<string> GetMismatchSymbols(Dictionary<string, SymbolItem> imageSymbolObjects)
        {
            List<string> mismatchSymbols = [];
            foreach (var imageSymbolPair in imageSymbolObjects)
            {
                var symbolName = imageSymbolPair.Key.Replace("image/", "");
                var symbol = imageSymbolPair.Value;
                var libraryItems = symbol.Timeline!.GetAllLibraryItems(unique:true);
                if (libraryItems.Count != 1) continue;

                var mediaName = libraryItems[0]!.Replace("media/", "");
                if (symbolName != mediaName)
                {
                    mismatchSymbols.Add($"image/{symbolName}");
                }
            }
            return mismatchSymbols;
        }
    
        public static void ChangeMediaNames(DOMDocument DOMDocumentObject, List<string> mismatchSymbols,
        Dictionary<string, SymbolItem> imageSymbolObjects, string xflPath)
        {
            var tempPaths = new List<string>();
            foreach (var mismatchSymbol in mismatchSymbols)
            {
                var imageSymbol = imageSymbolObjects[mismatchSymbol];
                var oldMediaName = imageSymbol.Timeline!.GetAllLibraryItems()[0]!;
                var oldMediaPath = Path.Join(xflPath, "library", $"{oldMediaName}.png");
                var tempMediaPath = Path.Join(xflPath, "library", "media", $"{mismatchSymbol.Replace("image/", "")}.png.TEMP");
                tempPaths.Add(tempMediaPath);
                //File.Move(oldMediaPath, tempMediaPath);

                // Adjust DOMDocument
                DOMDocumentObject.RemoveBitmapItem(oldMediaName);
                DOMDocumentObject.AddNewBitmapItem($"media/{mismatchSymbol.Replace("image/", "")}");
            }
            foreach (var tempPath in tempPaths)
            {
                var newMediaPath = tempPath.Replace(".TEMP", "");
                Console.WriteLine(tempPath);
                Console.WriteLine(newMediaPath);
                //File.Move(tempPath, newMediaPath);
            }
        }

        public static Dictionary<string, string> ChangeImageNames(DOMDocument DOMDocumentObject, List<string> mismatchSymbols,
        Dictionary<string, SymbolItem> imageSymbolObjects, string xflPath)
        {
            var tempPaths = new List<string>();
            var imageSymbolSwaps = new Dictionary<string, string>();
            foreach (var mismatchSymbol in mismatchSymbols)
            {
                var imageSymbol = imageSymbolObjects[mismatchSymbol];
                var intendedMediaName = imageSymbol.Timeline!.GetAllLibraryItems()[0]!.Replace("media/", "");
                imageSymbol.name = $"image/{intendedMediaName}";
                imageSymbol.Timeline.name = intendedMediaName;

                var currentImageSymbolPath = Path.Join(xflPath, "library", $"{mismatchSymbol}.xml");
                var tempImageSymbolPath = Path.Join(xflPath, "library", "image", $"{intendedMediaName}.xml.TEMP");
                tempPaths.Add(tempImageSymbolPath);
                File.Move(currentImageSymbolPath, tempImageSymbolPath);

                DOMDocumentObject.RemoveSymbolItem(mismatchSymbol);
                DOMDocumentObject.AddNewSymbolItem($"image/{intendedMediaName}");
                UM.SaveXmlDocument(tempImageSymbolPath, imageSymbol, UM.DummyXDocument, SymbolItem.serializer);
                imageSymbolSwaps.Add(mismatchSymbol, $"image/{intendedMediaName}");
            }

            foreach (var tempPath in tempPaths)
            {
                var newPath = tempPath.Replace(".TEMP", "");
                File.Move(tempPath, newPath);
            }
            return imageSymbolSwaps;
        }
    
        public static void AdjustSymbols(Dictionary<string, string> imageSymbolSwaps, string xflPath,
        DOMDocument DOMDocumentObject)
        {
            var allSymbols = DOMDocumentObject.GetAllSymbolNames(getFileEnding:true);
            foreach (var symbolName in allSymbols)
            {
                if (symbolName.StartsWith("image/")) continue;

                var symbolPath = Path.Join(xflPath, "library", symbolName);
                XDocument symbolDocument = XDocument.Load(symbolPath);
                using var symbolReader = symbolDocument.CreateReader();
                SymbolItem symbol = (SymbolItem)SymbolItem.serializer.Deserialize(symbolReader)!;

                var allElements = symbol.Timeline!.GetAllElements();
                bool editedSymbol = false;
                foreach (var element in allElements)
                {
                    var libraryItem = element.libraryItemName;
                    if (imageSymbolSwaps.ContainsKey(libraryItem))
                    {
                        element.libraryItemName = imageSymbolSwaps[libraryItem];
                        editedSymbol = true;
                    }
                }
                if (editedSymbol)
                {
                    UM.SaveXmlDocument(symbolPath, symbol, UM.DummyXDocument, SymbolItem.serializer);
                }
            }
        }
    }
}