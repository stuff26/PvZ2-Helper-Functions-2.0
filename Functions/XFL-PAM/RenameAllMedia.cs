using System.Xml.Linq;
using XflComponents;
using UniversalMethods;
using System.Text.Json.Nodes;
using SixLabors.ImageSharp;

namespace HelperFunctions.Functions.Packages
{
    public class RenameAllMedia
    {
        public static void Function()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter an XFL or an individual sprite");
            Console.WriteLine("(ensure there are no images that reuse the same media)");
            var xflPath = UM.AskForDirectory(["DOMDocument.xml", "data.json"]);
            //var xflPath = @"C:\Users\zacha\Documents\Coding Stuff\Helper Functions Remake\HelperFunctions\zombie_modern_allstar";

            string newPrefix = AskForPrefix();

            string domdocumentPath = Path.Join(xflPath, "DOMDocument.xml");
            XDocument document = XDocument.Load(domdocumentPath);
            using var documentReader = document.CreateReader();
            DOMDocument DOMDocumentObject = (DOMDocument?)DOMDocument.serializer.Deserialize(documentReader)!;

            List<string> imageSymbolList = [];
            foreach (var symbolName in DOMDocumentObject.GetAllSymbolNames(getFileEnding:true))
            {
                if (symbolName.StartsWith("image/"))
                {
                    imageSymbolList.Add(symbolName);
                }
            }
            Dictionary<string, string> oldNewImageDict = [];
            for (int i = 0; i < imageSymbolList.Count; i++)
            {
                var oldSymbolName = imageSymbolList[i];
                oldNewImageDict.Add(oldSymbolName, $"image/{newPrefix}{i+1}.xml");
            }
            RenameImageSymbolFiles(oldNewImageDict, xflPath);

            var imageMediaDict = RenameImageSymbolInternal(oldNewImageDict, xflPath);

            RenameMedia(imageMediaDict, xflPath);

            AdjustDOMDocument(DOMDocumentObject, oldNewImageDict);
            UM.SaveXmlDocument(domdocumentPath, DOMDocumentObject, UM.DummyXDocument, DOMDocument.serializer);

            AdjustExistingSymbols(DOMDocumentObject, oldNewImageDict, xflPath);

            MakeDataJson(imageMediaDict.Keys.ToList(), xflPath, newPrefix);

            Console.ForegroundColor = ConsoleColor.DarkCyan;
        }

        public static string AskForPrefix()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter the prefix you want (ex zombie_tutorial)");
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                var userInput = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Red;
                if (string.IsNullOrWhiteSpace(userInput))
                {
                    Console.WriteLine("Enter a prefix");
                    continue;
                }
                if (userInput.Contains('/') || userInput.Contains('\\'))
                {
                    Console.WriteLine("Enter a prefix without \"/\" or \"\\\"");
                    continue;
                }
                if (!userInput!.EndsWith('_'))
                {
                    userInput= $"{userInput}_";
                }

                return userInput;
            }
        }

        public static void RenameImageSymbolFiles(Dictionary<string, string> oldNewImageDict, string xflPath)
        {
            List<string> tempPathNames = [];
            Console.ForegroundColor = ConsoleColor.Green;
            ProgressChecker renameImageSymbols = new("Renaming image files... ", oldNewImageDict.Count);
            foreach (var oldNewImagePair in oldNewImageDict)
            {
                var oldSymbolName = oldNewImagePair.Key;
                var newSymbolName = oldNewImagePair.Value;
                var oldSymbolPath = Path.Join(xflPath, "library", oldSymbolName);
                var tempSymbolPath = Path.Join(xflPath, "library", $"{newSymbolName}.TEMP");
                tempPathNames.Add(tempSymbolPath);

                File.Move(oldSymbolPath, tempSymbolPath);
                renameImageSymbols.AddOne();
            }

            foreach (var tempPath in tempPathNames)
            {
                var newSymbolPath = tempPath.Replace(".TEMP", "");
                File.Move(tempPath, newSymbolPath);
            }
        }
    
        public static Dictionary<string, string> RenameImageSymbolInternal(Dictionary<string, string> oldNewImageDict, string xflPath)
        {
            Dictionary<string, string> imageMediaDict= [];
            Console.ForegroundColor = ConsoleColor.Green;
            ProgressChecker renameImageSymbolInternals = new("Renaming image symbol internals... ", oldNewImageDict.Count);
            foreach (var symbolName in oldNewImageDict.Values)
            {
                var symbolPath = Path.Join(xflPath, "library", symbolName);
                XDocument symbolDocument = XDocument.Load(symbolPath);
                using var symbolReader = symbolDocument.CreateReader();
                SymbolItem symbol = (SymbolItem)SymbolItem.serializer.Deserialize(symbolReader)!;

                var shortSymbolName = symbolName.Replace(".xml", "");
                symbol.name = shortSymbolName;
                symbol.Timeline!.name = shortSymbolName.Replace("image/", "");
                
                var elements = symbol.Timeline.GetAllElements();
                var mediaName = elements[0].libraryItemName;
                imageMediaDict.Add(shortSymbolName, mediaName);

                elements[0].libraryItemName = $"media/{shortSymbolName.Replace(".xml", "").Replace("image/", "")}";
                UM.SaveXmlDocument(symbolPath, symbol, UM.DummyXDocument, SymbolItem.serializer);
                renameImageSymbolInternals.AddOne();
            }

            return imageMediaDict;
        }
    
        public static void RenameMedia(Dictionary<string, string> imageMediaDict, string xflPath)
        {
            List<string> tempPathNames = [];
            Console.ForegroundColor = ConsoleColor.Green;
            ProgressChecker renameMedia = new("Renaming media files... ", imageMediaDict.Count);
            foreach (var imageMediaPair in imageMediaDict)
            {
                var newMediaName = imageMediaPair.Key.Replace("image/", "");
                var oldMediaName = imageMediaPair.Value;
                var newMediaPath = Path.Join(xflPath, "library", "media", $"{newMediaName}.png.TEMP");
                var oldMediaPath = Path.Join(xflPath, "library", $"{oldMediaName}.png");

                File.Move(oldMediaPath, newMediaPath);
                tempPathNames.Add(newMediaPath);
                renameMedia.AddOne();
            }

            foreach (var tempPath in tempPathNames)
            {
                var newPath = tempPath.Replace(".TEMP", "");
                File.Move(tempPath, newPath);
            }
        }
    
        public static void AdjustDOMDocument(DOMDocument DOMDocumentObject, Dictionary<string, string> oldNewImageDict)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            ProgressChecker adjustDOMDocument = new("Adjusting DOMDocument... ", oldNewImageDict.Count);
            foreach (var oldNewMediaPair in oldNewImageDict)
            {
                DOMDocumentObject.RemoveSymbolItem(oldNewMediaPair.Key.Replace(".xml", ""));
                DOMDocumentObject.AddNewSymbolItem(oldNewMediaPair.Value, includesEnd:true);
                
                var mediaName = oldNewMediaPair.Value.Replace("image/", "media/").Replace(".xml", "");
                DOMDocumentObject.RemoveBitmapItem($"media/{oldNewMediaPair.Key.Replace("image/", "").Replace(".xml", "")}");
                DOMDocumentObject.AddNewBitmapItem(mediaName);
                adjustDOMDocument.AddOne();
            }
        }
    
        public static void AdjustExistingSymbols(DOMDocument DOMDocumentObject, Dictionary<string, string> oldNewImageDict, string xflPath)
        {
            List<string> symbolList = [];
            foreach (var symbolName in DOMDocumentObject.GetAllSymbolNames())
            {
                if (!symbolName.StartsWith("image/"))
                {
                    symbolList.Add(symbolName);
                }
            }
            
            Dictionary<string, string> newOldImageDict = [];
            foreach (var pair in oldNewImageDict)
            {
                newOldImageDict.Add(pair.Key.Replace(".xml", ""), pair.Value.Replace(".xml", ""));
            }

            Console.ForegroundColor = ConsoleColor.Green;
            ProgressChecker renameSymbolInternals = new("Adjusting used symbol names... ", symbolList.Count);
            foreach (var symbolName in symbolList)
            {
                var symbolPath = Path.Join(xflPath, "library", symbolName);
                XDocument symbolDocument = XDocument.Load(symbolPath);
                using var symbolReader = symbolDocument.CreateReader();
                SymbolItem symbol = (SymbolItem)SymbolItem.serializer.Deserialize(symbolReader)!;

                var elements = symbol.Timeline!.GetAllElements();
                foreach (var element in elements)
                {
                    var libraryItem = element.libraryItemName;
                    if (newOldImageDict.ContainsKey(libraryItem))
                    {
                        element.libraryItemName = newOldImageDict[libraryItem];
                    }
                }

                UM.SaveXmlDocument(symbolPath, symbol, UM.DummyXDocument, SymbolItem.serializer);
                renameSymbolInternals.AddOne();
            }
        }
    
        public static void MakeDataJson(List<string> mediaNames, string xflPath, string prefix)
        {
            var dataJsonPath = Path.Join(xflPath, "data.json");
            var datajson = UM.GetJsonFile(dataJsonPath)!;
            var newImages = new JsonObject();

            Console.ForegroundColor = ConsoleColor.Green;
            ProgressChecker writeDataJson = new("Writing data.json... ", mediaNames.Count);
            foreach (var mediaName in mediaNames)
            {
                var newMediaName = mediaName.Replace("image/", "media/");
                var shortMediaName = newMediaName.Replace("media/", "");
                var spriteID = $"IMAGE_{prefix.ToUpper()}{shortMediaName.ToUpper()}";

                var bitmapPath = Path.Join(xflPath, "library", $"{newMediaName}.png");
                using Image image = Image.Load(bitmapPath);
                var width = (int)((image.Width * (1200.0 / 1536.0)) + 0.25);
                var height = (int)((image.Height * (1200.0 / 1536.0)) + 0.25);

                var toAddJsonNode = new JsonObject
                    {
                        ["id"] = spriteID,
                        ["dimension"] = new JsonObject
                        {
                            ["width"] = width,
                            ["height"] = height
                        },
                        ["additional"] = null
                    };
                KeyValuePair<string, JsonNode?> toAddPair = new($"{shortMediaName}", toAddJsonNode);
                newImages.Add(toAddPair);
                writeDataJson.AddOne();
            }

            datajson["image"] = newImages;
            UM.WriteJsonFile(dataJsonPath, datajson);
        }
    }
}