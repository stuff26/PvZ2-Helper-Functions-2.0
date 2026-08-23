using UniversalMethods;
using XflComponents;

namespace HelperFunctions.Functions.Packages
{
    public static class ConvertNZXfl
    {
        private const int Resolution = 1536;
        
        public static void Function()
        {
            // Ask for path
            UM.PrintColoredText(ConsoleColor.DarkCyan, "Enter the XFL you want to convert", separateLines:true);
            var xfl = XFL.AskForXFL(new XFLInitOptions()
            {
                GetSymbols = true,
                GetBitmaps = true,
                CheckProgress = true,
                FixResolution = Resolution
            });
            
            var originalPath = xfl.XflPath;
            var prefix = AskForPrefix(Path.GetFileNameWithoutExtension(originalPath));
            UM.PrintColoredText([
                (ConsoleColor.DarkCyan, "Put XFL as split labels? "),
                (ConsoleColor.Yellow, "(Y/N)\n")
            ]
                );
            bool splitLabels = UserPrompts.AskYesOrNo();
            var xflPath = Path.Join(Path.GetDirectoryName(originalPath), prefix);
            xfl.XflPath = xflPath;

            // Get old symbol names to reference later and clear out symbol references in DOMDocument
            var oldSymbolList = xfl.GetAllSymbolNames();
            xfl.ClearDOMDocumentSymbolItems();

            // Make main symbol, add it to the DOMDocument
            UM.PrintColoredText(ConsoleColor.Green, "Making main_sprite... ");
            var mainSymbol = MakeMainSymbol(xfl.DOMDocument);
            xfl.AddSymbol(mainSymbol);
            ProgressChecker.WriteFinished();

            // Rename bitmaps, move to media folder
            Console.ForegroundColor = ConsoleColor.Green;
            var oldNewBitmapNames = AdjustBitmaps(xfl, prefix);

            // Create new image symbols with new bitmap names along with correction symbols
            var correctionSymbolNames = MakeImageSymbols(xfl);
            var oldBitmapNames = oldNewBitmapNames.Keys.ToList();
            for (int i = 0; i < oldNewBitmapNames.Count; i++)
            {
                var oldBitmapName = oldBitmapNames[i];
                oldNewBitmapNames[oldBitmapName] = correctionSymbolNames[i];
            }

            // Adjust all symbols to use the new image symbols
            AdjustSymbols(xfl, oldNewBitmapNames, oldSymbolList.ToList());

            // Adjust DOMDocument
            UM.PrintColoredText(ConsoleColor.Green, "Adjusting DOMDocument... ");

            var DOMDocumentObject = xfl.DOMDocument;
            AddInstanceLayer(DOMDocumentObject, mainSymbol.Timeline.GetTotalLength());
            DOMDocumentObject.Timeline.Name = DOMDocument.TimelineName;
            DOMDocumentObject.Width = DOMDocument.DefaultSize;
            DOMDocumentObject.Height = DOMDocument.DefaultSize;
            RenameDOMDocumentLayers(DOMDocumentObject);
            FixActionFrames(DOMDocumentObject);
            ProgressChecker.WriteFinished();

            if (splitLabels)
            {
                xfl.ConvertToSplitLabels(checkProgress:true);
            }
            
            UM.PrintColoredText(ConsoleColor.Green, "Saving XFL... ");
            xfl.UpdateAllItemReferences();
            xfl.SaveXfl();
            ProgressChecker.WriteFinished();

            UM.PrintColoredText([
                (ConsoleColor.DarkCyan, "Wrote to "),
                (ConsoleColor.Green, $"{xflPath}\n")
            ]);
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
                if (userInput.Contains('/') || userInput.Contains('\\'))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Enter a prefix without \"/\" or \"\\\"");
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

        private static SymbolItem MakeMainSymbol(DOMDocument DOMDocument)
        {
            List<AnimateLayer> mainLayers = [];
            List<AnimateLayer> toKeepLayers = [];
            foreach (var layer in DOMDocument.Timeline.Layers)
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
                Name = XFL.MainSprite,
                Layers = mainLayers
            };
            var mainSymbol = new SymbolItem()
            {
                Name = XFL.MainSprite,
                Timeline = mainTimeline
            };

            return mainSymbol;
        }

        private static Dictionary<string, string> AdjustBitmaps(XFL xfl, string prefix)
        {
            xfl.ClearDOMDocumentBitmapItems();
            Dictionary<string, string> oldNewBitmapNames = [];

            var adjustBitmaps = new ProgressChecker("Moving bitmaps... ", xfl.Bitmaps.Count);
            int currentNum = 1;
            foreach (var oldBitmap in xfl.Bitmaps)
            {
                var oldName = oldBitmap.Name;
                var newName = $"media/{prefix}_{currentNum}";
                oldBitmap.Name = newName;
                oldNewBitmapNames.Add(oldName, newName);

                currentNum++;
                adjustBitmaps.AddOne();
            }

            return oldNewBitmapNames;
        }

        private static List<string> MakeImageSymbols(XFL xfl)
        {
            int correctionNum = 1;
            var correctionSymbolNames = new List<string>();

            var bitmaps = xfl.Bitmaps;
            var imageScale = Math.Round((double)XFL.ReferenceResolution / Resolution, 5);
            var correctionScale = Math.Round(1 / imageScale);

            var makeImageSymbols = new ProgressChecker("Making image and correction symbols... ", 2 * bitmaps.Count);
            foreach (var bitmap in bitmaps)
            {   
                var bitmapName = bitmap.Name;
                var fileBitmapName = Path.GetFileName(bitmapName);
                var newImageSymbol = SymbolItem.MakeSingleFrameSymbolItem(bitmapName, $"image/{fileBitmapName}", "BitmapInstance");
                var imageMatrix = new ElementMatrix
                {
                    A = imageScale,
                    D = imageScale
                };
                newImageSymbol.Timeline.GetAllElements()[0].Matrix = imageMatrix;
                xfl.AddSymbol(newImageSymbol);

                makeImageSymbols.AddOne();

                var correctionSymbolName = $"NZ Correction {correctionNum}";
                var correctionSymbol = SymbolItem.MakeSingleFrameSymbolItem($"image/{fileBitmapName}", $"sprite/{correctionSymbolName}");
                var correctionMatrix = new ElementMatrix
                {
                    A = correctionScale,
                    D = correctionScale
                };
                correctionSymbol.Timeline.GetAllElements()[0].Matrix = correctionMatrix;
                xfl.AddSymbol(correctionSymbol);
                correctionNum++;

                correctionSymbolNames.Add(correctionSymbolName);
                makeImageSymbols.AddOne();
            }

            return correctionSymbolNames;
        }

        private static void AdjustSymbols(XFL xfl, Dictionary<string, string> oldNewBitmapNames, List<string> oldSymbolList)
        {
            int symbolNum = 1;
            Dictionary<string, string> oldNewSymbolNames = [];
            foreach (var oldSymbolName in oldSymbolList)
            {
                var newSymbolName = $"Symbol {symbolNum}";
                symbolNum++;
                oldNewSymbolNames.Add(Path.GetFileNameWithoutExtension(oldSymbolName), newSymbolName);
            }

            var symbols = xfl.Symbols;
            var adjustSymbols = new ProgressChecker("Adjusting sprite symbols... ", symbols.Count - (oldNewBitmapNames.Count * 2));
            foreach (var symbol in symbols)
            {
                if (symbol.Name.StartsWith("image/") || symbol.Name.StartsWith("sprite/NZ Correction ")) continue;

                FixElementReferences(symbol, oldNewBitmapNames, oldNewSymbolNames);
                symbol.Timeline.Layers = SplitLayers(symbol.Timeline.Layers);
                symbol.Timeline.Layers = RenameLayers(symbol.Timeline.Layers);

                if (symbol.Name != XFL.MainSprite)
                {
                    var originalName = Path.GetFileName(symbol.Name);
                    var newName = oldNewSymbolNames[originalName];
                    symbol.ChangeName($"sprite/{newName}");
                }

                adjustSymbols.AddOne();
            }
        }

        private static void FixElementReferences(SymbolItem symbol, Dictionary<string, string> oldNewBitmapNames, Dictionary<string, string> oldNewSymbolNames)
        {
            foreach (var layer in symbol.Timeline.Layers)
            {
                foreach (var frame in layer.Frames)
                {
                    var elements = frame.Elements;
                    for (int elementIndex = 0; elementIndex < frame.Elements.Count; elementIndex++)
                    {
                        var element = elements[elementIndex].ToSymbolInstance();
                        var oldLibraryItem = element.LibraryItemName;
                        if (oldNewBitmapNames.TryGetValue(oldLibraryItem, out var newBitmapName))
                        {
                            element.LibraryItemName = $"sprite/{newBitmapName}";
                        }
                        else if (oldNewSymbolNames.TryGetValue(oldLibraryItem, out var newSymbolName))
                        {
                            element.LibraryItemName = $"sprite/{newSymbolName}";
                        }

                        element.SymbolType = FrameElements.DefaultSymbolType;
                        elements[elementIndex] = element;
                    }
                }
            }
        }

        private static List<AnimateLayer> SplitLayers(List<AnimateLayer> Layers)
        {
            List<AnimateLayer> LayersToReturn = [];
            foreach (var layer in Layers)
            {
                var frames = layer.Frames.ToList();

                // If there are no frames, the layer is null, or the layer has no library items, skip it
                if (layer.GetAllLibraryItems().Count == 0)
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
                var currentSymbol = string.Empty; // Current symbol that will be expected
                int finalIndex = frames.Count; // This and current are used to make sure that the last frames are added
                int currentIndex = 0;

                foreach (var frame in frames)
                {
                    currentIndex++;
                    var mainLibraryItem = frame.GetMainLibraryItem(); // Main library item being used to check

                    // If an empty frame is found and there is a current symbol, make new layer
                    if (mainLibraryItem is null && currentSymbol != string.Empty)
                    {
                        AnimateLayer newLayer = layer.MakeCopy();
                        newLayer.Frames = currentFrames;
                        LayersToAdd.Add(newLayer);

                        currentFrames = [];
                        currentSymbol = string.Empty;
                    }

                    // If there is no current symbol and a new symbol is found, make it the new symbol
                    else if (currentSymbol == string.Empty)
                    {
                        if (frame.GetMainLibraryItem() != string.Empty)
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

                AnimateFrame firstFrame = frames[0];
                if (firstFrame.Index > 0)
                {
                    AnimateFrame emptyFrame = new(0, firstFrame.Index);
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
                layer.Name = $"{currentNum}";
                currentNum++;
                layer.Color = AnimateLayer.DefaultColor;
            }
            return Layers;
        }

        private static void AddInstanceLayer(DOMDocument DOMDocument, int mainSpriteLength)
        {
            var layers = DOMDocument.Timeline.Layers;

            var instanceFrame = AnimateFrame.GetSingleKeyframe(0, mainSpriteLength, XFL.MainSprite);
            var instanceLayer = new AnimateLayer(DOMDocument.InstanceLayer, instanceFrame);
            layers.Add(instanceLayer);
        }

        private static void RenameDOMDocumentLayers(DOMDocument DOMDocument)
        {
            bool renamedLabelLayer = false;
            bool renamedActionLayer = false;
            foreach (var layer in DOMDocument.Timeline.Layers)
            {
                layer.Color = "#4F80FF";
                if (layer.HasLabels() && !renamedLabelLayer)
                {
                    layer.Name = DOMDocument.LabelLayer;
                }
                else if (layer.HasActions() && !renamedActionLayer)
                {
                    layer.Name = DOMDocument.ActionLayer;
                }
            }
        }
    
        private static void FixActionFrames(DOMDocument DOMDocument)
        {
            var actionLayer = DOMDocument.Timeline.GetLayerByName("action");
            if (actionLayer is null) return;
            foreach (var frame in actionLayer.Frames)
            {
                if (frame.Actionscript is null
                || frame.Actionscript.Scripts is null) continue;
                var cdataScripts = frame.Actionscript.Scripts;

                for (int i = 0; i < cdataScripts.Count; i++)
                {
                    var script = cdataScripts[i];
                    var text = script.Text;
                    if (text is null) continue;
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