using System.Xml.Linq;
using XflComponents;
using UniversalMethods;
using System.Text.Json.Nodes;
using SixLabors.ImageSharp;

namespace HelperFunctions.Functions.Packages
{
    public class ConvertNZXfl
    {
        public static void Function()
        {
            // Ask for path
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter the XFL you want to convert");
            var originalPath = UM.AskForDirectory(wantedFiles:["DOMDocument.xml"], wantedDirs:["LIBRARY"]);
            var prefix = AskForPrefix(originalPath.Split("\\")[^1]);
            var xflPath = Path.Join(Path.GetDirectoryName(originalPath), prefix);


            // Remove files that aren't in DOMDocument and adjust existing files
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Setting up directory and getting DOMDocument... ");

            if (Directory.Exists(xflPath)) Directory.Delete(xflPath, recursive: true); // Clear out directory if it already exists for safe measures
            UM.CopyFolder(originalPath, xflPath); // Copy over file details
            Directory.CreateDirectory(Path.Join(xflPath, "LIBRARY", "image"));
            Directory.CreateDirectory(Path.Join(xflPath, "LIBRARY", "sprite"));

            // Get DOMDocument
            var domdocumentPath = Path.Join(xflPath, "DOMDocument.xml");
            XDocument document = XDocument.Load(domdocumentPath);
            using var documentReader = document.CreateReader();
            var DOMDocumentObject = (DOMDocument?)DOMDocument.serializer.Deserialize(documentReader)!;

            // Remove files that aren't referenced in DOMDocument
            AdjustFiles(DOMDocumentObject, xflPath);
            ProgressChecker.WriteFinished();

            // Get old symbol names to reference later and clear out symbol references in DOMDocument
            var oldSymbolList = DOMDocumentObject.GetAllSymbolNames();
            DOMDocumentObject.SymbolItemList = [];

            // Make main symbol, add it to the DOMDocument
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Making main_sprite... ");
            var mainSymbol = MakeMainSymbol(DOMDocumentObject);
            ProgressChecker.WriteFinished();

            // Rename bitmaps, move to media folder
            Console.ForegroundColor = ConsoleColor.Green;
            var oldNewBitmapNames = AdjustBitmaps(DOMDocumentObject, xflPath, prefix);

            // Create new image symbols with new bitmap names along with correction symbols
            Console.ForegroundColor = ConsoleColor.Green;
            var correctionSymbolNames = MakeImageSymbols(DOMDocumentObject, xflPath, oldNewBitmapNames.Values.ToList());
            var oldBitmapNames = oldNewBitmapNames.Keys.ToList();
            for (int i = 0; i < oldNewBitmapNames.Count; i++)
            {
                var oldBitmapName = oldBitmapNames[i];
                oldNewBitmapNames[oldBitmapName] = correctionSymbolNames[i];
            }

            // Adjust all symbols to use the new image symbols
            Console.ForegroundColor = ConsoleColor.Green;
            AdjustSymbols(DOMDocumentObject, oldNewBitmapNames, oldSymbolList, xflPath, mainSymbol);

            // Make data.json
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Making data.json... ");
            MakeDataJson(xflPath, prefix, DOMDocumentObject.GetAllBitmapNames());
            ProgressChecker.WriteFinished();

            // Adjust DOMDocument
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Adjusting DOMDocument... ");

            AddInstanceLayer(DOMDocumentObject, mainSymbol.Timeline!.GetTotalLength());
            DOMDocumentObject.Timeline.name = "animation";
            DOMDocumentObject.width = "390";
            DOMDocumentObject.height = "390";
            RenameDOMDocumentLayers(DOMDocumentObject);
            FixActionFrames(DOMDocumentObject);


            UM.SaveXmlDocument(domdocumentPath, DOMDocumentObject, UM.DummyXDocument, DOMDocument.serializer);
            ProgressChecker.WriteFinished();

            // Ensure main.xfl is there
            File.WriteAllBytes(Path.Join(xflPath, "main.xfl"), [80, 82, 79, 88, 89, 45, 67, 83, 53]);

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write($"Wrote to ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(xflPath);
        }

        private static string AskForPrefix(string originalXfl)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter the prefix you want the XFL to have (ex. plant_peashooter)");
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                var userInput = Console.ReadLine()?.ToLower();
                if (string.IsNullOrWhiteSpace(userInput))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Enter a prefix to be used");
                    continue;
                }
                if (originalXfl == userInput)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Give a different prefix that is not shared with the converting XFL");
                    continue;
                }
                return userInput;
            }
        }

        private static void AdjustFiles(DOMDocument DOMDocument, string xflPath)
        {
            // Remove files that aren't the DOMDocument or xfl file
            var rootFileList = Directory.GetFiles(xflPath);
            List<string> wantedFiles = [Path.Join(xflPath, "DOMDocument.xml")];
            foreach (var file in rootFileList)
            {
                if (!wantedFiles.Contains(file))
                {
                    File.Delete(file);
                }
            }

            // Remove files in library that aren't listed in DOMDocument
            var DOMDocumentFileList = DOMDocument.GetAllSymbolBitmapNames(getFileEnding: true);
            for (int i = 0; i < DOMDocumentFileList.Count; i++)
            {
                DOMDocumentFileList[i] = Path.Join(xflPath, "LIBRARY", DOMDocumentFileList[i]);
            }
            var fileList = Directory.GetFiles(Path.Join(xflPath, "LIBRARY"), "*.*", SearchOption.AllDirectories);
            foreach (var file in fileList)
            {
                if (!DOMDocumentFileList.Contains(file))
                {
                    File.Delete(file);
                }
            }
        }

        private static SymbolItem MakeMainSymbol(DOMDocument DOMDocument)
        {
            List<AnimateLayer> mainLayers = [];
            List<AnimateLayer> toKeepLayers = [];
            foreach (var layer in DOMDocument.Timeline.Layers!)
            {
                if (layer.HasFrameElements())
                {
                    mainLayers.Add(layer);
                }
                else
                {
                    toKeepLayers.Add(layer);
                }
            }
            DOMDocument.Timeline.Layers = toKeepLayers;

            var mainTimeline = new SymbolTimeline()
            {
                name = "main_sprite",
                Layers = mainLayers
            };
            var mainSymbol = new SymbolItem()
            {
                name = "main_sprite",
                symbolType = "graphic",
                Timeline = mainTimeline
            };
            DOMDocument.AddNewSymbolItem("main_sprite");

            return mainSymbol;
        }

        private static Dictionary<string, string> AdjustBitmaps(DOMDocument DOMDocument, string xflPath, string prefix)
        {
            var bitmapList = DOMDocument.GetAllBitmapNames();
            DOMDocument.BitmapItemList = [];
            int currentNum = 1;
            Dictionary<string, string> oldNewBitmapNames = [];
            Directory.CreateDirectory(Path.Join(xflPath, "LIBRARY", "media"));

            Console.ForegroundColor = ConsoleColor.Green;
            var adjustBitmaps = new ProgressChecker("Moving bitmaps... ", bitmapList.Count);
            foreach (var oldBitmap in bitmapList)
            {
                var newName = $"media/{prefix}_{currentNum}";
                DOMDocument.AddNewBitmapItem(newName);

                var oldPath = Path.Join(xflPath, "LIBRARY", $"{oldBitmap}.png");
                var newPath = Path.Join(xflPath, "LIBRARY", $"{newName}.png");
                File.Move(oldPath, newPath);

                oldNewBitmapNames.Add(oldBitmap, newName);
                currentNum++;
                adjustBitmaps.AddOne();
            }

            return oldNewBitmapNames;
        }

        private static List<string> MakeImageSymbols(DOMDocument DOMDocument, string xflPath, List<string> newBitmapNames)
        {
            int correctionNum = 1;
            var correctionSymbolNames = new List<string>();

            var makeImageSymbols = new ProgressChecker("Making image and correction symbols... ", newBitmapNames.Count * 2);
            foreach (var bitmapName in newBitmapNames)
            {
                var newBitmap = bitmapName.Replace("media/", "");
                var newImageSymbol = SymbolItem.MakeSingleFrameSymbolItem(bitmapName, $"image/{newBitmap}", newBitmap, "BitmapInstance");
                var imageSymbolPath = Path.Join(xflPath, "library", "image", $"{newBitmap}.xml");
                var imageMatrix = new ElementMatrix
                {
                    a = 0.78125,
                    d = 0.78125
                };
                newImageSymbol.Timeline!.GetAllElements()[0].Matrix = imageMatrix;

                UM.SaveXmlDocument(imageSymbolPath, newImageSymbol, UM.DummyXDocument, SymbolItem.serializer);
                makeImageSymbols.AddOne();

                var correctionSymbolName = $"NZ correction {correctionNum}";
                var correctionSymbol = SymbolItem.MakeSingleFrameSymbolItem($"image/{newBitmap}", $"sprite/{correctionSymbolName}", correctionSymbolName);
                var correctionMatrix = new ElementMatrix
                {
                    a = 1.28,
                    d = 1.28
                };
                correctionSymbol.Timeline!.GetAllElements()[0].Matrix = correctionMatrix;
                var correctionSymbolPath = Path.Join(xflPath, "library", "sprite", $"{correctionSymbolName}.xml");
                UM.SaveXmlDocument(correctionSymbolPath, correctionSymbol, UM.DummyXDocument, SymbolItem.serializer);
                correctionNum++;

                DOMDocument.AddNewSymbolItem($"image/{newBitmap}");
                DOMDocument.AddNewSymbolItem($"sprite/{correctionSymbolName}");
                correctionSymbolNames.Add(correctionSymbolName);
                makeImageSymbols.AddOne();
            }

            return correctionSymbolNames;
        }

        private static void AdjustSymbols(DOMDocument DOMDocument, Dictionary<string, string> oldNewBitmapNames,
         List<string> oldSymbolList, string xflPath, SymbolItem mainSymbol)
        {
            int symbolNum = 1;
            Dictionary<string, string> oldNewSymbolNames = [];
            foreach (var oldSymbolName in oldSymbolList)
            {
                var newSymbolName = $"Symbol {symbolNum}";
                symbolNum++;
                oldNewSymbolNames.Add(oldSymbolName.Replace(".xml", ""), newSymbolName);
            }

            var libraryPath = Path.Join(xflPath, "LIBRARY");
            var adjustSymbols = new ProgressChecker("Adjusting sprite symbols... ", oldSymbolList.Count + 1);
            foreach (var oldSymbol in oldSymbolList)
            {
                var oldSymbolPath = Path.Join(libraryPath, $"{oldSymbol}");
                XDocument symbolDocument = XDocument.Load(oldSymbolPath);
                using var documentReader = symbolDocument.CreateReader();
                var symbol = (SymbolItem?)SymbolItem.serializer.Deserialize(documentReader)!;
                var layers = symbol.Timeline!.Layers!;

                FixElementReferences(symbol, oldNewBitmapNames, oldNewSymbolNames);
                symbol.Timeline!.Layers = SplitLayers(symbol.Timeline!.Layers!);
                symbol.Timeline!.Layers = RenameLayers(symbol.Timeline!.Layers!);

                var newSymbolName = oldNewSymbolNames[oldSymbol.Replace(".xml", "")];
                symbol.name = $"sprite/{newSymbolName}";
                symbol.Timeline!.name = newSymbolName;
                symbol.symbolType = "graphic";
                var newSymbolPath = Path.Join(libraryPath, "sprite", $"{newSymbolName}.xml");

                UM.SaveXmlDocument(newSymbolPath, symbol, UM.DummyXDocument, SymbolItem.serializer);
                File.Delete(oldSymbolPath);

                DOMDocument.RemoveSymbolItem(oldSymbol, includesEnd: true);
                DOMDocument.AddNewSymbolItem($"sprite/{newSymbolName}", includesEnd: false);

                adjustSymbols.AddOne();
            }

            FixElementReferences(mainSymbol, oldNewBitmapNames, oldNewSymbolNames);
            mainSymbol.Timeline!.Layers = SplitLayers(mainSymbol.Timeline!.Layers!);
            mainSymbol.Timeline!.Layers = RenameLayers(mainSymbol.Timeline!.Layers!);
            var mainSymbolPath = Path.Join(libraryPath, "main_sprite.xml");
            UM.SaveXmlDocument(mainSymbolPath, mainSymbol, UM.DummyXDocument, SymbolItem.serializer);
            adjustSymbols.AddOne();
        }

        private static void FixElementReferences(SymbolItem symbol, Dictionary<string, string> oldNewBitmapNames, Dictionary<string, string> oldNewSymbolNames)
        {
            var oldBitmapNames = oldNewBitmapNames.Keys.ToList();
            var oldSymbolNames = oldNewSymbolNames.Keys.ToList();

            foreach (var layer in symbol.Timeline!.Layers!)
            {
                foreach (var frame in layer.Frames!)
                {
                    var elements = frame.Elements;
                    for (int elementIndex = 0; elementIndex < frame.Elements.Count; elementIndex++)
                    {
                        var element = elements[elementIndex].ToSymbolInstance();
                        var oldLibraryItem = element.libraryItemName;
                        if (oldBitmapNames.Contains(oldLibraryItem))
                        {
                            element.libraryItemName = $"sprite/{oldNewBitmapNames[oldLibraryItem]}";
                        }
                        else if (oldSymbolNames.Contains(oldLibraryItem))
                        {
                            element.libraryItemName = $"sprite/{oldNewSymbolNames[oldLibraryItem]}";
                        }

                        element.symbolType = "graphic";
                        elements[elementIndex] = element;
                    }
                }
            }
        }

        private static List<AnimateLayer> SplitLayers(List<AnimateLayer> Layers)
        {
            List<AnimateLayer> LayersToReturn = [];
            foreach (AnimateLayer? layer in Layers!)
            {
                List<AnimateFrame>? frames = layer.Frames;

                // If there are no frames, the layer is null, or the layer has no library items, skip it
                if (frames is null || layer is null || layer.GetAllLibraryItems().Count == 0)
                {
                    continue;
                }

                // Remove beginning empty frames of layer
                var tempFrameElements = frames[0].Elements;
                while (tempFrameElements is null && frames.Count > 0)
                {
                    frames.RemoveAt(0);
                    if (frames.Count > 0)
                    {
                        tempFrameElements = frames[0].Elements;
                    }
                }
                if (frames.Count == 0) continue; // Skip layer if there are no remaining frames

                // If the amount of keyframes is 1, there is no need to check
                // for multiple symbols so we add it and move on
                if (frames.Count == 1)
                {
                    LayersToReturn.Add(layer);
                    continue;
                }

                var LayersToAdd = new List<AnimateLayer>(); // List of layers that will eventually be added to total layer list
                var currentFrames = new List<AnimateFrame>(); // Current set of frames that will be made
                string currentSymbol = ""; // Current symbol that will be expected
                int finalIndex = frames.Count; // This and current are used to make sure that the last frames are added
                int currentIndex = 0;

                foreach (AnimateFrame frame in frames)
                {
                    currentIndex++;
                    string mainLibraryItem = frame.GetMainLibraryItem(); // Main library item being used to check

                    // If an empty frame is found and there is a current symbol, make new layer
                    if (mainLibraryItem == "" && currentSymbol != "")
                    {
                        AnimateLayer newLayer = layer.MakeCopy();
                        newLayer.Frames = currentFrames;
                        LayersToAdd.Add(newLayer);

                        currentFrames = [];
                        currentSymbol = "";
                    }

                    // If there is no current symbol and a new symbol is found, make it the new symbol
                    else if (currentSymbol == "")
                    {
                        if (frame.GetMainLibraryItem() != "")
                        {
                            currentSymbol = mainLibraryItem;
                            currentFrames.Add(frame);
                        }

                        if (finalIndex == currentIndex)
                        {
                            AnimateLayer newLayer = layer.MakeCopy();
                            newLayer.Frames = currentFrames;
                            LayersToAdd.Add(newLayer);
                        }
                    }

                    // If a different symbol is found, make a new layer and make the new symbol the current symbol
                    else if (currentSymbol != mainLibraryItem)
                    {
                        currentSymbol = mainLibraryItem;

                        if (currentFrames.Count > 0)
                        {
                            AnimateLayer newLayer = layer.MakeCopy();
                            newLayer.Frames = currentFrames;
                            LayersToAdd.Add(newLayer);
                            currentFrames = [frame];
                        }
                        if (finalIndex == currentIndex)
                        {
                            AnimateLayer newLayer = layer.MakeCopy();
                            newLayer.Frames = currentFrames;
                            LayersToAdd.Add(newLayer);
                        }
                    }

                    // If a symbol that is consistent with the current is found, add it
                    else if (currentSymbol == mainLibraryItem)
                    {
                        currentFrames.Add(frame);

                        if (finalIndex == currentIndex)
                        {
                            AnimateLayer newLayer = layer.MakeCopy();
                            newLayer.Frames = currentFrames;
                            LayersToAdd.Add(newLayer);
                        }
                    }
                }
                LayersToReturn.AddRange(LayersToAdd);
            }

            
            foreach (AnimateLayer layer in LayersToReturn)
            {
                var frames = layer.Frames;

                AnimateFrame firstFrame = frames![0];
                if (firstFrame.index > 0)
                {
                    AnimateFrame emptyFrame = AnimateFrame.GetEmptyFrame(0, firstFrame.index);
                    frames.Insert(0, emptyFrame);
                }
            }
            return LayersToReturn;
        }

        private static List<AnimateLayer> RenameLayers(List<AnimateLayer> Layers)
        {
            int currentNum = 1;
            foreach (var layer in Layers)
            {
                layer.name = $"{currentNum}";
                currentNum++;
                layer.color = "#4F80FF";
            }
            return Layers;
        }

        private static void MakeDataJson(string xflPath, string prefix, List<string> bitmapNames)
        {
            var datajsonPath = Path.Join(xflPath, "data.json");
            string dataJsonString = "  {\"version\": 6, \"resolution\": 1536, \"position\": {\"x\": 0,\"y\": 0},\"image\":{},\"sprite\": {}}";
            var datajson = JsonNode.Parse(dataJsonString)!;
            var imageCollection = datajson["image"]!.AsObject();
            foreach (var fullBitmapName in bitmapNames)
            {
                var bitmapName = fullBitmapName.Replace("media/", "");
                var spriteId = $"{prefix.ToUpper()}_{bitmapName.ToUpper()}";
                var bitmapPath = Path.Join(xflPath, "LIBRARY", $"{fullBitmapName}.png");
                using Image image = Image.Load(bitmapPath);
                var width = (int)((image.Width * (1200.0 / 1536.0)) + 0.25);
                var height = (int)((image.Height * (1200.0 / 1536.0)) + 0.25);


                var toParse = $"{{\"id\": \"{spriteId}\",   \"dimension\": {{\"width\": {width}, \"height\": {height}}},   \"additional\": null}}";
                var toAddJsonNode = JsonNode.Parse(toParse)!;
                KeyValuePair<string, JsonNode?> toAddPair = new($"{bitmapName}", toAddJsonNode);
                imageCollection.Add(toAddPair);
            }
            datajson["image"] = imageCollection;

            UM.WriteJsonFile(datajsonPath, datajson);
        }

        private static void AddInstanceLayer(DOMDocument DOMDocument, int mainSpriteLength)
        {
            var layers = DOMDocument.Timeline.Layers!;

            var instanceFrame = AnimateFrame.GetSingleKeyframe(0, mainSpriteLength, "main_sprite");
            var instanceLayer = new AnimateLayer()
            {
                Frames = [instanceFrame],
                name = "instance",
                color = "#4F4FFF"
            };
            layers.Add(instanceLayer);
        }

        private static void RenameDOMDocumentLayers(DOMDocument DOMDocument)
        {
            bool renamedLabelLayer = false;
            bool renamedActionLayer = false;
            foreach (var layer in DOMDocument.Timeline.Layers!)
            {
                layer.color = "#4F80FF";
                if (layer.HasLabels() && !renamedLabelLayer)
                {
                    layer.name = "label";
                }
                else if (layer.HasActions() && !renamedActionLayer)
                {
                    layer.name = "action";
                }
                if (renamedLabelLayer && renamedActionLayer) break;
            }
        }
    
        private static void FixActionFrames(DOMDocument DOMDocument)
        {
            var actionLayer = DOMDocument.Timeline.GetLayerByName("action");
            if (actionLayer is null) return;
            foreach (var frame in actionLayer.Frames!)
            {
                var actionscript = frame.Actionscript;
                if (actionscript is null) continue;
                var cdataScripts = actionscript.scripts;
                if (cdataScripts is null) continue;

                for (int i = 0; i < cdataScripts.Count; i++)
                {
                    var script = cdataScripts[i];
                    var text = script.Text!;
                    if (!text.EndsWith("();") && text.EndsWith("\");"))
                    {
                        text = text.Replace("\");", "\", \"\");");
                        cdataScripts[i].Text = text;
                    }
                }
            }
        }
    }
}