using System.Text;
using System.Xml.Linq;
using XflComponents;
using UniversalMethods;

namespace HelperFunctions.Functions.Packages
{
    public class CheckXflErrors
    {
        public static void Function()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter the XFL you want to scan");
            var results = AskForSymbolItem();
            List<SymbolItem> SymbolList = results.SymbolList;
            string domdocumentPath = Path.Join(results.xflPath, "DOMDocument.xml");
            XDocument document = XDocument.Load(domdocumentPath);
            using var documentReader = document.CreateReader();
            DOMDocument DOMDocumentObject = (DOMDocument?)DOMDocument.serializer.Deserialize(documentReader)!;

            // Method to use to find errors and internal name
            var ErrorCheckList = new List<(Func<SymbolItem, string> errorChecker, string errorType)>()
            {
                (CheckMultipleElements, "multipleElements"),
                (CheckMultipleLibraryItemTypes, "inconsistentLibraryItem"),
                (CheckEmptyKeyframeGaps, "emptyKeyframeGaps"),
                (CheckTweens, "hasTweens"),
                (CheckIncorrectSymbolTypes, "incorrectSymbolTypes"),
                (CheckWrongLayerTypes, "wrongLayerTypes"),
                (CheckWrongBitmapProperties, "wrongBitmapProperties")
            };

            Console.ForegroundColor = ConsoleColor.Green;
            string prefix = "Scanning symbols... ";
            var scanSymbols = new ProgressChecker(prefix, SymbolList.Count);

            // First string key is symbol name
            // Second string key is the error type
            // Third string are the error details
            var SymbolErrorTracker = new Dictionary<string, Dictionary<string, string>>();
            foreach (SymbolItem symbol in SymbolList)
            {
                string symbolName = symbol.name!;
                SymbolErrorTracker.Add(symbolName, []);

                foreach (var (errorChecker, errorType) in ErrorCheckList)
                {
                    var currentError = errorChecker(symbol);
                    if (currentError.Length != 0)
                    {
                        SymbolErrorTracker[symbolName].Add(errorType, currentError);
                    }
                }
                scanSymbols.AddOne();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Scanning DOMDocument... ");
            var DOMDocumentLayers = new Dictionary<string, AnimateLayer?>()
            {
                {"label", null},
                {"action", null},
                {"instance", null},
            };
            foreach (var layer in DOMDocumentObject.Timeline!.Layers!)
            {
                var layerName = layer.name;
                if (DOMDocumentLayers.ContainsKey(layerName) && DOMDocumentLayers[layerName] is null)
                {
                    DOMDocumentLayers[layerName] = layer;
                }
                // If two layers with one of the intended names is found, replace it with an empty layer object so it is skipped
                else if (DOMDocumentLayers.ContainsKey(layerName))
                {
                    DOMDocumentLayers[layerName] = new AnimateLayer();
                }
            }
            var DOMDocumentErrors = new List<string>();
            DOMDocumentErrors.AddRange(CheckGeneralDOMDocument(DOMDocumentObject));
            DOMDocumentErrors.AddRange(CheckLabelLayer(DOMDocumentLayers["label"]));
            DOMDocumentErrors.AddRange(CheckActionLayer(DOMDocumentLayers["action"]));
            DOMDocumentErrors.AddRange(CheckInstanceLayer(DOMDocumentLayers["instance"]));
            ProgressChecker.WriteFinished();


            if (DOMDocumentErrors.Count == 0 && UM.DictionaryContainsOnlyEmptyLists(SymbolErrorTracker))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No errors found");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Writing error message...");
                string errorMessage = MakeErrorMessage(SymbolErrorTracker, DOMDocumentErrors);
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(errorMessage);
            }
        }

        private static (List<SymbolItem> SymbolList, string xflPath) AskForSymbolItem()
        {
            while (true)
            {
                // Get input from user
                var pathInput = UM.AskForDirectory(["DOMDocument.xml"]);

                // Get a list of all symbols and check if DOMDocument is invalid while at it
                var SymbolDirectories = UM.GetAllSymbolDirectories(pathInput);
                if (SymbolDirectories is null)
                {
                    Console.WriteLine("Error reading DOMDocument, could not access symbol list, enter again");
                    continue;
                }

                // Open document to check inside, check for errors while at it
                List<string> AllSymbolPaths = [];
                List<SymbolItem> SymbolList = [];
                Console.ForegroundColor = ConsoleColor.Green;
                var processSymbols = new ProgressChecker("Processing files... ", SymbolDirectories.Count);
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
                        processSymbols.RemoveOne();
                        Console.Write($"The symbol {Path.GetFileName(symbolPath)} doesn't seem to be valid, will be ignored");
                        continue;
                    }

                    // Check if the symbol itself and the timeline is null
                    if (symbol is null || symbol.Timeline is null)
                    {
                        processSymbols.RemoveOne();
                        Console.WriteLine($"Could not find properly elements in symbol {Path.GetFileName(symbolPath)}, will be ignored");
                        continue;
                    }

                    AllSymbolPaths.Add(symbolPath);
                    SymbolList.Add(symbol);
                    processSymbols.AddOne();
                }

                // Return
                processSymbols.FixCursorPosition();
                return (SymbolList, pathInput);
            }
        }

        private static string CheckMultipleElements(SymbolItem symbol)
        {
            // Setup
            var ErrorIndexes = new Dictionary<int, List<int>>();
            for (int layerIndex = 0; layerIndex < symbol.Timeline!.Layers!.Count; layerIndex++)
            {
                // Get layer and frames objects, if no frames are found then skip to next layer
                var layer = symbol.Timeline!.Layers![layerIndex]; // Get layer
                var frames = layer.Frames; // Get list of frames
                if (frames is null || frames.Count == 0) continue;

                // Loop through each frame
                var FoundErrors = new List<int>();
                for (int frameIndex = 0; frameIndex < frames!.Count; frameIndex++)
                {
                    var frame = frames[frameIndex];
                    // If multiple elements are found in the frame, add to list of errors
                    if (frame.HasMultipleElements())
                    {
                        FoundErrors.Add(frameIndex);
                    }
                }
                if (FoundErrors.Count > 0)
                {
                    ErrorIndexes.Add(layerIndex, FoundErrors);
                }
            }

            // Make error message
            var errorMessage = new StringBuilder();
            foreach (var layerError in ErrorIndexes)
            {
                var layerIndex = layerError.Key;
                var layer = symbol.Timeline.Layers[layerIndex];
                string layerName = layer.name;
                errorMessage.Append($"\n\t\tLayer name \"{layerName}\", layer index {layerIndex + 1}:\n\t\t\tFrame Indexes: ");
                var indexRanges = UM.TurnIntoValueRange(layerError.Value);
                var tempMessage = new StringBuilder();
                foreach (List<int> frameIndex in indexRanges)
                {
                    if (frameIndex[0] == frameIndex[1])
                        tempMessage.Append("{" + (frameIndex[0] + 1) + "}, ");
                    else
                        tempMessage.Append("{" + (frameIndex[0] + 1) + "-" + (frameIndex[1] + 1) + "}, ");
                }
                errorMessage.Append(tempMessage.ToString()[..^2]); // Remove trailing comma
            }

            return errorMessage.ToString();
        }
        private static string CheckMultipleLibraryItemTypes(SymbolItem symbol)
        {
            var layers = symbol.Timeline!.Layers!;

            // First int is the layer index
            // Second int is the frame number
            // Third string is the found symbol item
            var FoundErrors = new Dictionary<int, Dictionary<int, string>>();
            List<string> MainLibraryItems = [];
            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                // Get layer, frames, and main library item, continue to next layer if there are no frames
                var layer = layers[layerIndex];
                var frames = layer.Frames;
                var mainLibraryItem = layer.GetMainLibraryItem();
                MainLibraryItems.Add(mainLibraryItem);
                if (frames is null
                || frames.Count == 0
                || mainLibraryItem == "") continue;

                // Loop through all frames
                var FoundLayerErrors = new Dictionary<int, string>();
                for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
                {
                    // Get current frame and its library item, skip if no library item is found
                    var currentFrame = frames[frameIndex];
                    var currentLibraryItem = currentFrame.GetMainLibraryItem();
                    if (currentLibraryItem == "") continue;

                    // If current library item is different from main library item, add to loop
                    if (currentLibraryItem != mainLibraryItem)
                    {
                        FoundLayerErrors.Add(currentFrame.index, currentLibraryItem);
                    }
                }

                // Add to list of errors
                if (FoundLayerErrors.Count > 0)
                {
                    FoundErrors.Add(layerIndex, FoundLayerErrors);
                }
            }

            // Process error message
            var errorMessage = new StringBuilder();
            foreach (var LayerErrorPair in FoundErrors)
            {
                int layerIndex = LayerErrorPair.Key;
                string layerName = layers[layerIndex].name;
                string mainLibraryItem = MainLibraryItems[layerIndex];
                errorMessage.Append($"\n\t\tLayer \"{layerName}\", index {layerIndex + 1}, has inconsistent library items, first item found is {mainLibraryItem}\n\t\t\tFrame Indexes:");

                foreach (var FrameErrorPair in LayerErrorPair.Value)
                {
                    int frameIndex = FrameErrorPair.Key;
                    var foundLibraryItem = FrameErrorPair.Value;
                    errorMessage.Append("\n\t\t\tFrame {" + (frameIndex + 1) + "}" + $" => {foundLibraryItem}");
                }
            }

            return errorMessage.ToString();
        }
        private static string CheckEmptyKeyframeGaps(SymbolItem symbol)
        {
            var layers = symbol.Timeline!.Layers!;
            var FoundErrors = new Dictionary<int, List<int>>();
            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                var layer = layers[layerIndex];
                var frames = layer?.Frames;
                if (layer is null || frames is null) continue;

                string shouldExpect = "empty";
                var unexpectedEmptyKeyframes = new List<int>();
                for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
                {
                    var frame = frames[frameIndex];
                    var elements = frame.Elements;
                    if (shouldExpect == "empty" && elements?.Count > 0)
                    {
                        shouldExpect = "element";
                    }
                    if (shouldExpect == "element" && (elements is null || elements.Count == 0))
                    {
                        unexpectedEmptyKeyframes.Add(frame.index);
                    }
                }
                if (unexpectedEmptyKeyframes.Count > 0)
                {
                    FoundErrors.Add(layerIndex, unexpectedEmptyKeyframes);
                }
            }

            var errorMessage = new StringBuilder();
            foreach (var ErrorKeyPair in FoundErrors)
            {
                int layerIndex = ErrorKeyPair.Key;
                string layerName = layers[layerIndex].name;
                errorMessage.Append($"\n\t\tLayer \"{layerName}\", index {layerIndex + 1}\n\t\t\tFrame indexes: ");
                var frameIndexes = UM.TurnIntoValueRange(ErrorKeyPair.Value);

                var tempMessage = new StringBuilder();
                foreach (var frameIndex in frameIndexes)
                {
                    if (frameIndex[0] == frameIndex[1])
                        tempMessage.Append("{" + (frameIndex[0] + 1) + "}, ");
                    else
                        tempMessage.Append("{" + (frameIndex[0] + 1) + "-" + (frameIndex[1] + 1) + "}, ");
                }
                errorMessage.Append(tempMessage.ToString()[..^2]);
            }
            return errorMessage.ToString();
        }
        private static string CheckTweens(SymbolItem symbol)
        {
            var layers = symbol.Timeline!.Layers!;
            var FoundErrors = new Dictionary<int, List<int>>();
            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                var layer = layers[layerIndex];
                var frames = layer.Frames;
                if (frames is null || frames.Count == 0) continue;

                var ErrorFrames = new List<int>();
                for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
                {
                    var frame = frames[frameIndex];
                    if (frame.HasTweens())
                    {
                        ErrorFrames.Add(frameIndex);
                    }
                }
                if (ErrorFrames.Count > 0)
                {
                    FoundErrors.Add(layerIndex, ErrorFrames);
                }
            }

            var errorMessage = new StringBuilder();

            foreach (var ErrorKeyPair in FoundErrors)
            {
                int layerIndex = ErrorKeyPair.Key;
                string layerName = layers[layerIndex].name;
                errorMessage.Append($"\n\t\tLayer \"{layerName}\", index {layerIndex + 1}\n\t\t\tFrame indexes: ");
                var frameIndexes = UM.TurnIntoValueRange(ErrorKeyPair.Value);

                var tempMessage = new StringBuilder();
                foreach (var frameIndex in frameIndexes)
                {
                    if (frameIndex[0] == frameIndex[1])
                        tempMessage.Append("{" + (frameIndex[0] + 1) + "}, ");
                    else
                        tempMessage.Append("{" + (frameIndex[0] + 1) + "-" + (frameIndex[1] + 1) + "}, ");
                }
                errorMessage.Append(tempMessage.ToString()[..^2]);
            }
            return errorMessage.ToString();
        }
        private static string CheckIncorrectSymbolTypes(SymbolItem symbol)
        {
            var symbolType = symbol.GetFolder();
            Type expectedSymbolType;
            if (symbolType == "image")
            {
                expectedSymbolType = new BitmapInstance().GetType();
            }
            else
            {
                expectedSymbolType = new SymbolInstance().GetType();
            }

            var ErrorTracker = new Dictionary<int, List<int>>();
            var layers = symbol.Timeline!.Layers;
            for (int layerIndex = 0; layerIndex < layers?.Count; layerIndex++)
            {
                var layer = layers[layerIndex];
                var frames = layer.Frames;
                if (layer is null || frames is null) continue;

                var ErrorFrames = new List<int>();
                for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
                {
                    var frame = frames[frameIndex];
                    var elements = frame.Elements;
                    if (elements is null) continue;

                    foreach (var element in elements)
                    {
                        if (!element.GetSymbolType().Equals(expectedSymbolType))
                        {
                            ErrorFrames.Add(frameIndex);
                            break;
                        }
                    }
                }
                if (ErrorFrames.Count > 0)
                {
                    ErrorTracker.Add(layerIndex, ErrorFrames);
                }
            }

            var errorMessage = new StringBuilder();
            foreach (var ErrorKeyPair in ErrorTracker)
            {
                int layerIndex = ErrorKeyPair.Key;
                string layerName = layers![layerIndex].name;
                errorMessage.Append($"\n\t\tLayer \"{layerName}\", index {layerIndex + 1}\n\t\t\tFrame indexes: ");
                var frameIndexes = UM.TurnIntoValueRange(ErrorKeyPair.Value);

                var tempMessage = new StringBuilder();
                foreach (var frameIndex in frameIndexes)
                {
                    if (frameIndex[0] == frameIndex[1])
                        tempMessage.Append("{" + (frameIndex[0] + 1) + "}, ");
                    else
                        tempMessage.Append("{" + (frameIndex[0] + 1) + "-" + (frameIndex[1] + 1) + "}, ");
                }
                errorMessage.Append(tempMessage.ToString()[..^2]);
            }

            return errorMessage.ToString();
        }
        private static string CheckWrongLayerTypes(SymbolItem symbol)
        {
            var layers = symbol.Timeline!.Layers!;
            var ErrorCheckList = new List<int>();
            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                var layer = layers[layerIndex];
                var layerType = layer.layerType;
                string[] notAllowedLayers = ["folder", "mask"];
                if (notAllowedLayers.Contains(layerType))
                {
                    ErrorCheckList.Add(layerIndex);
                }
            }

            var errorMessage = new StringBuilder();
            foreach (int layerError in ErrorCheckList)
            {
                var layerName = layers[layerError].name;
                errorMessage.Append($"\n\t\tLayer \"{layerName}\", index {layerError + 1}");
            }

            return errorMessage.ToString();
        }
        private static string CheckWrongBitmapProperties(SymbolItem symbol)
        {
            string folderName = symbol.GetFolder();
            if (folderName != "image") return "";

            var ErrorTracker = new List<string>();
            var layers = symbol.Timeline!.Layers;
            if (layers?.Count > 1)
            {
                ErrorTracker.Add("multipleLayers");
            }

            if (layers is not null)
                foreach (var layer in layers)
                {
                    int layerLength = layer.GetLayerLength();
                    if (layerLength > 1 && !ErrorTracker.Contains("longLayerLength"))
                    {
                        ErrorTracker.Add("longLayerLength");
                    }

                    var elements = layer.GetAllFrameElements();
                    if (!ErrorTracker.Contains("incorrectScaling"))
                    {
                        foreach (var element in elements)
                        {
                            if ((element?.Matrix?.b != 0 && element?.Matrix?.b is not null)
                            || (element?.Matrix?.c != 0 && element?.Matrix?.c is not null)
                            || element?.Matrix?.a != element?.Matrix?.d)
                            {
                                ErrorTracker.Add("incorrectScaling");
                                break;
                            }
                        }
                    }
                }

            var ErrorMessageConverter = new Dictionary<string, string>()
        {
            {"multipleLayers", "More than one layers are found, ensure there is only one layer" },
            {"longLayerLength", "Layer(s) with a length longer than 1 frame are found, ensure the length is exactly one"},
            {"incorrectScaling", "Some frames are either rotated or have unequal scaling"}
        };

            var errorMessage = new StringBuilder();
            foreach (var error in ErrorTracker)
            {
                string toAddMessage = ErrorMessageConverter[error];
                errorMessage.Append($"\n\t\t{toAddMessage}");
            }

            return errorMessage.ToString();
        }

        private static List<string> CheckGeneralDOMDocument(DOMDocument DOMDocument)
        {
            var layers = DOMDocument!.Timeline!.Layers!;
            var FoundErrors = new List<string>();

            // Check for extra layers, missing intended layers, and duplicate needed layers
            List<string> IntendedLayerNames = ["instance", "action", "label"];
            var NeededLayerNames = IntendedLayerNames.ToList(); // Make a copy
            foreach (var layer in layers)
            {
                var layerName = layer.name;
                if (NeededLayerNames.Count > 0 && NeededLayerNames.Contains(layerName))
                {
                    NeededLayerNames.Remove(layerName);
                }
                else if (IntendedLayerNames.Contains(layerName) && !FoundErrors.Contains("multipleNeededLayers"))
                {
                    FoundErrors.Add("multipleNeededLayers");
                }
                else if (!FoundErrors.Contains("extraLayers"))
                {
                    FoundErrors.Add("extraLayers");
                }
            }
            if (NeededLayerNames.Count > 0)
            {
                FoundErrors.Add("missingNeededLayers");
            }
            int? frameRate = DOMDocument.frameRate;
            if (frameRate is not null && frameRate > 30)
            {
                FoundErrors.Add("framerateTooHigh");
            }

            FoundErrors.Sort();
            return FoundErrors;
        }
        private static List<string> CheckLabelLayer(AnimateLayer? layer)
        {
            var FoundErrors = new List<string>();
            if (layer is null) return FoundErrors;

            if (!FoundErrors.Contains("labelHasElements") && layer.HasFrameElements())
            {
                FoundErrors.Add("labelHasElements");
            }
            if (!FoundErrors.Contains("labelHasActions") && layer.HasActions())
            {
                FoundErrors.Add("labelHasActions");
            }
            if (!FoundErrors.Contains("labelHasNoLabels") && !layer.HasLabels())
            {
                FoundErrors.Add("labelHasNoLabels");
            }

            FoundErrors.Sort();
            return FoundErrors;
        }
        private static List<string> CheckActionLayer(AnimateLayer? layer)
        {
            var FoundErrors = new List<string>();
            if (layer is null) return FoundErrors;

            if (layer.HasFrameElements())
            {
                FoundErrors.Add("actionHasElements");
            }
            if (layer.HasLabels())
            {
                FoundErrors.Add("actionHasLabels");
            }
            List<string> allowedScripts = ["stop", "fscommand"];
            foreach (var script in layer.GetActions())
            {
                string scriptBeginning = script.Split("(")[0];
                if (!allowedScripts.Contains(scriptBeginning))
                {
                    FoundErrors.Add("actionHasWrongActions");
                    break;
                }
            }

            FoundErrors.Sort();
            return FoundErrors;
        }
        private static List<string> CheckInstanceLayer(AnimateLayer? layer)
        {
            var FoundErrors = new List<string>();
            if (layer is null) return FoundErrors;

            if (!layer.HasFrameElements())
            {
                FoundErrors.Add("instanceHasNoElements");
            }
            if (layer.HasLabels())
            {
                FoundErrors.Add("instanceHasLabels");
            }
            if (layer.HasActions())
            {
                FoundErrors.Add("instanceHasActions");
            }
            foreach (string usedLibraryItem in layer.GetAllLibraryItems())
            {
                // Only allow symbols that start with "label" or is named "main_sprite"
                if (!(usedLibraryItem.StartsWith("label") || usedLibraryItem == "main_sprite"))
                {
                    FoundErrors.Add("instanceNotUsingLabelSymbols");
                    break;
                }
            }
            foreach (var frame in layer.Frames!)
            {
                if (frame.HasTransformations())
                {
                    FoundErrors.Add("instanceHasTransformations");
                    break;
                }
            }

            FoundErrors.Sort();
            return FoundErrors;
        }


        private static string MakeErrorMessage(Dictionary<string, Dictionary<string, string>> ErrorTracker, List<string> DOMDocumentErrors)
        {
            // Setup to convert internal message types into proper messages
            var errorMessage = new StringBuilder(); // Full error message that will be returned at the end
            var errorMessageTypes = new Dictionary<string, string>()
        {
            {"multipleElements", "More than one elements are found in some keyframes" },
            {"inconsistentLibraryItem", "Different types of library items are found in the same layers"},
            {"emptyKeyframeGaps", "Layers with empty keyframe gaps are found"},
            {"hasTweens", "Some unconverted tweens are found"},
            {"incorrectSymbolTypes", "Incorrect symbol types are found in certain symbol types"},
            {"wrongLayerTypes", "Some layers are folders or are other types that aren't allowed"},
            {"wrongBitmapProperties", "Symbol is an image symbol and has some properties it shouldn't have"},
            {"multipleNeededLayers", "Multiple layers that are named both \"layer\", \"action\", or \"instance\" are found"},
            {"extraLayers", "Layers that aren't named \"layer\", \"action\", or \"instance\" are found"},
            {"missingNeededLayers", "Layers named \"layer\", \"action\", or \"instance\" are missing"},
            {"framerateTooHigh", "The framerate of the document is above 30, ensure it is at or below 30"},
            {"labelHasElements", "Elements are found in the \"label\" layer, it should only contain keyframes with labels"},
            {"labelHasActions", "Action scripts are found in the \"label\" layer, it should only contain keyframes with labels"},
            {"labelHasNoLabels", "No labels are found in the \"label\" layer, there should be at least one label"},
            {"actionHasElements", "Elements are found in the \"action\" layer, it should only contain keyframes with action scripts"},
            {"actionHasLabels", "Labels are found in the \"action\" layer, it should only contain keyframes with action scripts"},
            {"actionHasWrongActions", "Action scripts that aren't \"stop\" or \"fscommand\" are found in the action layer"},
            {"instanceHasNoElements", "No elements are found in the \"instance\" layer, ensure it has at least one element"},
            {"instanceHasLabels", "Labels are found in the \"instance\" layer, it should only contain keyframes with action scripts"},
            {"instanceHasActions", "Action scripts are found in the \"instance\" layer, it should only contain keyframes with labels"},
            {"instanceNotUsingLabelSymbols", "Library items that aren't \"label\" symbols or named \"main_sprite\" are found in the \"instance\" layer"},
            {"instanceHasTransformations", "The element(s) in the \"instance\" layer are scaled or rotated, ensure it is not transformed"}
        };

            // Add errors found in DOMDocument
            if (DOMDocumentErrors.Count > 0)
            {
                errorMessage.Append("Errors in the main document are found\n");
                foreach (string errorType in DOMDocumentErrors)
                {
                    errorMessage.Append($"\t{errorMessageTypes[errorType]}\n");
                }
                errorMessage.Append("\n");
            }

            // Loop through the errors found in each symbol
            foreach (var SymbolErrorPair in ErrorTracker)
            {
                var errors = SymbolErrorPair.Value; // Error message type + full error message
                if (errors.Count == 0) continue; // If no errors are found in the symbol, continue to next one

                // Get symbol name and insert that the symbol has an error 
                var symbolName = SymbolErrorPair.Key;
                errorMessage.Append($"Symbol {symbolName} has errors");

                // Loop through the error types and error messages
                foreach (var ErrorDetails in errors)
                {
                    string errorType = ErrorDetails.Key;
                    errorType = "\t" + errorMessageTypes[errorType];
                    errorMessage.Append($"\n{errorType}{ErrorDetails.Value}");
                }
                errorMessage.Append("\n\n");
            }

            return errorMessage.ToString();
        }
    }
}