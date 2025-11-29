using System.Xml.Linq;
using XflComponents;
using UniversalMethods;

namespace HelperFunctions.Functions.Packages
{
    public class SplitMultiFrameLayers()
    {
        public static void Function()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter an XFL or an individual sprite");
            var results = AskForSymbolItem();
            var SymbolPathList = results.SymbolPathList;
            var SymbolList = results.SymbolList;
            
            Console.ForegroundColor = ConsoleColor.Green;
            var editSymbols = new ProgressChecker("Editing symbols... ", SymbolList.Count);
            foreach (SymbolItem symbol in SymbolList)
            {
                List<AnimateLayer>? Layers = symbol?.Timeline!.Layers;
                var NewLayerList = SplitLayers(Layers!);
                NewLayerList = AddEmptyFrames(NewLayerList);
                symbol?.Timeline!.ReplaceLayers(NewLayerList);
                editSymbols.AddOne();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            var writeFiles = new ProgressChecker("Writing files... ", SymbolList.Count);
            for (int i = 0; i < SymbolPathList.Count; i++)
            {
                UM.SaveXmlDocument(SymbolPathList[i], SymbolList[i], UM.DummyXDocument, SymbolItem.serializer);
                writeFiles.AddOne();
            }
        }




        public static (List<string> SymbolPathList, List<SymbolItem> SymbolList) AskForSymbolItem()
        {
            while (true)
            {
                // Get input from user
                var (pathInput, isFile) = UM.AskForPath(["DOMDocument.xml"]);

                // If directory is a folder, check the contents to see if it is an xfl
                List<string>? SymbolDirectories;
                if (!isFile)
                {
                    var fileList = Directory.GetFiles(pathInput).ToList();
                    if (!fileList.Contains(Path.Join(pathInput, "DOMDocument.xml")))
                    {
                        Console.WriteLine("Folder is not an XFL, enter again");
                        continue;
                    }
                    SymbolDirectories = UM.GetAllSymbolDirectories(pathInput);
                    if (SymbolDirectories is null)
                    {
                        Console.WriteLine("Error reading DOMDocument, could not access symbol list, enter again");
                        continue;
                    }
                }
                else
                {
                    SymbolDirectories = [pathInput];
                }

                // Open document to check inside, check for errors while at it
                List<string> AllSymbolPaths = [];
                List<SymbolItem> SymbolList = [];
                Console.ForegroundColor = ConsoleColor.Green;
                var getSymbols = new ProgressChecker("Processing files... ", SymbolDirectories.Count);
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (string symbolPath in SymbolDirectories)
                {
                    SymbolItem? symbol;
                    try
                    {
                        XDocument symbolDocument = XDocument.Load(symbolPath);
                        using var documentReader = symbolDocument.CreateReader();
                        symbol = (SymbolItem?)SymbolItem.serializer.Deserialize(documentReader);
                        if (symbol is null || symbol.Timeline is null)
                        {
                            throw new System.Xml.XmlException();
                        }
                    }
                    catch (System.Xml.XmlException)
                    {
                        getSymbols.RemoveOne();
                        Console.WriteLine($"The symbol {Path.GetFileName(symbolPath)} doesn't seem to be valid, will be ignored");
                        continue;
                    }

                    // Check if the symbol itself and the timeline is null
                    if (symbol is null || symbol.Timeline is null)
                    {
                        getSymbols.RemoveOne();
                        Console.WriteLine($"The symbol {Path.GetFileName(symbolPath)} doesn't seem to be valid, will be ignored");
                        continue;
                    }

                    AllSymbolPaths.Add(symbolPath);
                    SymbolList.Add(symbol);
                    getSymbols.AddOne();
                }

                // Return
                getSymbols.FixCursorPosition();
                var toReturn = (AllSymbolPaths, SymbolList);
                return toReturn;
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

            return LayersToReturn;
        }
        private static List<AnimateLayer> AddEmptyFrames(List<AnimateLayer> NewLayerList)
        {
            foreach (AnimateLayer layer in NewLayerList)
            {
                var frames = layer.Frames;

                AnimateFrame firstFrame = frames![0];
                if (firstFrame.index > 0)
                {
                    AnimateFrame emptyFrame = AnimateFrame.GetEmptyFrame(0, firstFrame.index);
                    frames.Insert(0, emptyFrame);
                }
            }

            return NewLayerList;
        }
    }
}