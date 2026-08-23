using System.Text;
using XflComponents;
using UniversalMethods;

namespace HelperFunctions.Functions.Packages
{
    public class CheckXflErrors
    {
        public static void Function()
        {
            // Get XFL
            UM.PrintColoredText(
            [
                (ConsoleColor.DarkCyan, "Enter the "),
                (ConsoleColor.Green, "XFL"),
                (ConsoleColor.DarkCyan, " you wish to scan\n")
            ]);
            var xfl = XFL.AskForXFL(new()
            {
                GetSymbols = true,
                GetBitmaps = false,
                CheckProgress = true
            });

            // Methods to use to find errors and internal name of errors
            var errorCheckList = new List<(Func<SymbolItem, string> errorChecker, string errorType)>()
            {
                (CheckMultipleElements, "multipleElements"),
                (CheckMultipleLibraryItemTypes, "inconsistentLibraryItem"),
                (CheckEmptyKeyframeGaps, "emptyKeyframeGaps"),
                (CheckTweens, "hasTweens"),
                (CheckIncorrectSymbolTypes, "incorrectSymbolTypes"),
                (CheckWrongLayerTypes, "wrongLayerTypes"),
                (CheckWrongBitmapProperties, "wrongBitmapProperties"),
                (CheckEmptyLayers, "hasEmptyLayers")
            };

            var scanSymbols = new ProgressChecker("Scanning symbols... ", xfl.Symbols.Count);

            // 1st => symbol name
            // 2nd => internal error name
            // 3rd => error message
            var symbolErrorTracker = new Dictionary<string, Dictionary<string, string>>();
            foreach (SymbolItem symbol in xfl.Symbols)
            {
                string symbolName = symbol.Name;
                symbolErrorTracker.Add(symbolName, []);

                // Loop through error check list
                foreach (var (errorChecker, errorType) in errorCheckList)
                {
                    // Error message
                    var currentError = errorChecker(symbol);

                    // If the error message is not "", add to error tracker
                    if (currentError != string.Empty)
                    {
                        symbolErrorTracker[symbolName].Add(errorType, currentError);
                    }
                }
                scanSymbols.AddOne();
            }

            // Check for errors in DOMDocument
            UM.PrintColoredText(ConsoleColor.Green, "Scanning DOMDocument... ");
            var domDocumentLayers = new Dictionary<string, AnimateLayer?>()
            {
                {DOMDocument.LabelLayer, xfl.DOMDocument.GetLabelLayer()},
                {DOMDocument.ActionLayer, xfl.DOMDocument.GetActionLayer()},
                {DOMDocument.InstanceLayer, xfl.DOMDocument.GetInstanceLayer()},
            };

            // Make a list of DOMDocument errors
            List<string> domDocumentErrors = [
                ..CheckGeneralDOMDocument(xfl.DOMDocument),
                ..CheckLabelLayer(domDocumentLayers[DOMDocument.LabelLayer]),
                ..CheckActionLayer(domDocumentLayers[DOMDocument.ActionLayer]),
                ..CheckInstanceLayer(domDocumentLayers[DOMDocument.InstanceLayer], xfl)
            ];
            ProgressChecker.WriteFinished();

            // If there are no DOMDocument errors and there are no issues with any symbols, print success message
            if (domDocumentErrors.Count == 0 && !symbolErrorTracker.Values.Any(v => v.Count > 0))
            {
                UM.PrintColoredText(ConsoleColor.Yellow, "No errors found", separateLines:true);
            }

            // Otherwise, make error message and print it
            else
            {
                UM.PrintColoredText(ConsoleColor.Green, "Writing error message... ");
                string errorMessage = MakeErrorMessage(symbolErrorTracker, domDocumentErrors);
                ProgressChecker.WriteFinished();
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(errorMessage);
            }
        }

        private static string CheckMultipleElements(SymbolItem symbol)
        {
            // Setup
            var errorIndexes = new Dictionary<int, List<int>>();
            for (int layerIndex = 0; layerIndex < symbol.Timeline.Layers.Count; layerIndex++)
            {
                // Get layer and frames objects, if no frames are found then skip to next layer
                var layer = symbol.Timeline.Layers[layerIndex]; // Get layer
                var frames = layer.Frames; // Get list of frames
                if (frames is null || frames.Count == 0) continue;

                // Loop through each frame
                var FoundErrors = new List<int>();
                for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
                {
                    var frame = frames[frameIndex];
                    // If multiple elements are found in the frame, add to list of errors
                    if (frame.HasMultipleElements())
                    {
                        FoundErrors.Add(frame.Index);
                    }
                }
                if (FoundErrors.Count > 0)
                {
                    errorIndexes.Add(layerIndex + 1, FoundErrors);
                }
            }

            // Make error message
            var errorMessage = new StringBuilder();
            foreach (var layerError in errorIndexes)
            {
                var layerIndex = layerError.Key;
                var layer = symbol.Timeline.Layers[layerIndex];
                string layerName = layer.Name;
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
            var layers = symbol.Timeline.Layers;

            // First int is the layer index
            // Second int is the frame number
            // Third string is the found symbol item
            var foundErrors = new Dictionary<int, Dictionary<int, string>>();
            List<string?> mainLibraryItems = [];
            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                // Get layer, frames, and main library item, continue to next layer if there are no frames
                var layer = layers[layerIndex];
                var frames = layer.Frames;
                var mainLibraryItem = layer.GetMainLibraryItem();
                mainLibraryItems.Add(mainLibraryItem);
                if (frames.Count == 0 || mainLibraryItem is null) continue;

                // Loop through all frames
                var foundLayerErrors = new Dictionary<int, string>();
                for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
                {
                    // Get current frame and its library item, skip if no library item is found
                    var currentFrame = frames[frameIndex];
                    var currentLibraryItem = currentFrame.GetMainLibraryItem();
                    if (currentLibraryItem is null) continue;

                    // If current library item is different from main library item, add to loop
                    if (currentLibraryItem != mainLibraryItem)
                    {
                        foundLayerErrors.Add(currentFrame.Index, currentLibraryItem);
                    }
                }

                // Add to list of errors
                if (foundLayerErrors.Count > 0)
                {
                    foundErrors.Add(layerIndex, foundLayerErrors);
                }
            }

            // Process error message
            var errorMessage = new StringBuilder();
            foreach (var LayerErrorPair in foundErrors)
            {
                int layerIndex = LayerErrorPair.Key;
                string layerName = layers[layerIndex].Name;
                string mainLibraryItem = mainLibraryItems[layerIndex]!;
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
            var layers = symbol.Timeline.Layers;
            var foundErrors = new Dictionary<int, List<int>>();
            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                var layer = layers[layerIndex];
                var frames = layer.Frames;
                if (layer is null || frames is null) continue;

                // 0 = empty, 1 = element
                int shouldExpect = 0;
                var unexpectedEmptyKeyframes = new List<int>();
                for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
                {
                    var frame = frames[frameIndex];
                    var elements = frame.Elements;
                    if (shouldExpect == 0 && elements.Count > 0)
                    {
                        shouldExpect = 1;
                    }
                    if (shouldExpect == 1 && elements.Count == 0)
                    {
                        unexpectedEmptyKeyframes.Add(frame.Index);
                    }
                }
                if (unexpectedEmptyKeyframes.Count > 0)
                {
                    foundErrors.Add(layerIndex, unexpectedEmptyKeyframes);
                }
            }

            var errorMessage = new StringBuilder();
            foreach (var ErrorKeyPair in foundErrors)
            {
                int layerIndex = ErrorKeyPair.Key;
                string layerName = layers[layerIndex].Name;
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
            var layers = symbol.Timeline.Layers;
            var foundErrors = new Dictionary<int, List<int>>();
            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                var layer = layers[layerIndex];
                var frames = layer.Frames;
                if (frames is null || frames.Count == 0) continue;

                var errorFrames = new List<int>();
                for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
                {
                    var frame = frames[frameIndex];
                    if (frame.Duration >= 2 && frame.HasTweens())
                    {
                        errorFrames.Add(frame.Index);
                    }
                }
                if (errorFrames.Count > 0)
                {
                    foundErrors.Add(layerIndex, errorFrames);
                }
            }

            var errorMessage = new StringBuilder();

            foreach (var ErrorKeyPair in foundErrors)
            {
                int layerIndex = ErrorKeyPair.Key;
                string layerName = layers[layerIndex].Name;
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
            Type expectedSymbolType = symbolType == XFL.ImageFolder
                                      ? new BitmapInstance().GetType()
                                      : new SymbolInstance().GetType();

            var errorTracker = new Dictionary<int, List<int>>();
            var layers = symbol.Timeline.Layers;
            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                var layer = layers[layerIndex];
                var frames = layer.Frames;
                if (layer is null || frames is null) continue;

                var errorFrames = new List<int>();
                for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
                {
                    var frame = frames[frameIndex];
                    var elements = frame.Elements;
                    if (elements is null) continue;

                    foreach (var element in elements)
                    {
                        if (!element.GetType().Equals(expectedSymbolType))
                        {
                            errorFrames.Add(frame.Index);
                            break;
                        }
                    }
                }
                if (errorFrames.Count > 0)
                {
                    errorTracker.Add(layerIndex, errorFrames);
                }
            }

            var errorMessage = new StringBuilder();
            foreach (var ErrorKeyPair in errorTracker)
            {
                int layerIndex = ErrorKeyPair.Key;
                string layerName = layers[layerIndex].Name;
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
            var layers = symbol.Timeline.Layers;
            var errorCheckList = new List<int>();
            string[] notAllowedLayers = ["folder", "mask"];
            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                var layer = layers[layerIndex];
                var layerType = layer.LayerType;
                if (notAllowedLayers.Contains(layerType))
                {
                    errorCheckList.Add(layerIndex);
                }
            }

            var errorMessage = new StringBuilder();
            foreach (int layerError in errorCheckList)
            {
                var layerName = layers[layerError].Name;
                errorMessage.Append($"\n\t\tLayer \"{layerName}\", index {layerError + 1}");
            }

            return errorMessage.ToString();
        }
        private static string CheckWrongBitmapProperties(SymbolItem symbol)
        {
            string folderName = symbol.GetFolder();
            if (folderName != XFL.ImageFolder) return string.Empty;

            var errorTracker = new List<string>();
            var layers = symbol.Timeline.Layers;
            if (layers.Count > 1)
            {
                errorTracker.Add("multipleLayers");
            }

            if (layers is not null)
                foreach (var layer in layers)
                {
                    int layerLength = layer.GetLayerLength();
                    if (layerLength > 1 && !errorTracker.Contains("longLayerLength"))
                    {
                        errorTracker.Add("longLayerLength");
                    }

                    var elements = layer.GetAllFrameElements();
                    if (!errorTracker.Contains("incorrectScaling"))
                    {
                        foreach (var element in elements)
                        {
                            if ((element.Matrix.B != 0)
                            || (element.Matrix.C != 0)
                            || element.Matrix.A != element.Matrix.D)
                            {
                                errorTracker.Add("incorrectScaling");
                                break;
                            }
                        }
                    }
                }

            var errorMessageConverter = new Dictionary<string, string>()
        {
            {"multipleLayers", "More than one layers are found, ensure there is only one layer" },
            {"longLayerLength", "Layer(s) with a length longer than 1 frame are found, ensure the length is exactly one"},
            {"incorrectScaling", "Some frames are either rotated or have unequal scaling"}
        };

            var errorMessage = new StringBuilder();
            foreach (var error in errorTracker)
            {
                string toAddMessage = errorMessageConverter[error];
                errorMessage.Append($"\n\t\t{toAddMessage}");
            }

            return errorMessage.ToString();
        }
        private static string CheckEmptyLayers(SymbolItem symbol)
        {
            int layerNum = 0;
            var errorLayers = new List<int>();
            foreach (var layer in symbol.Timeline.Layers)
            {
                if (layer.IsEmpty())
                {
                    errorLayers.Add(layerNum);
                }
                layerNum++;
            }
            if (errorLayers.Count == 0) return "";
            
            var errorMessage = new StringBuilder("\n");
            foreach (var errorNum in errorLayers)
            {
                var layer = symbol.Timeline.Layers[errorNum];
                errorMessage.AppendLine($"\t\tLayer {layer.Name}, index {errorNum + 1}");
            }

            return errorMessage.ToString();
        }

        private static List<string> CheckGeneralDOMDocument(DOMDocument domDocument)
        {
            var layers = domDocument.Timeline.Layers;
            var foundErrors = new List<string>();

            // Check for extra layers, missing intended layers, and duplicate needed layers
            List<string> intendedLayerNames = [DOMDocument.InstanceLayer, DOMDocument.ActionLayer, DOMDocument.LabelLayer];
            var neededLayerNames = intendedLayerNames.ToList(); // Make a copy
            foreach (var layer in layers)
            {
                var layerName = layer.Name;
                if (neededLayerNames.Count > 0 && neededLayerNames.Contains(layerName))
                {
                    neededLayerNames.Remove(layerName);
                }
                else if (intendedLayerNames.Contains(layerName) && !foundErrors.Contains("multipleNeededLayers"))
                {
                    foundErrors.Add("multipleNeededLayers");
                }
                else if (!foundErrors.Contains("extraLayers"))
                {
                    foundErrors.Add("extraLayers");
                }
            }
            if (neededLayerNames.Count > 0)
            {
                foundErrors.Add("missingNeededLayers");
            }
            var frameRate = domDocument.FrameRate;
            if (frameRate > 30)
            {
                foundErrors.Add("framerateTooHigh");
            }

            foundErrors.Sort();
            return foundErrors;
        }
        private static List<string> CheckLabelLayer(AnimateLayer? layer)
        {
            var foundErrors = new List<string>();
            if (layer is null) return foundErrors;

            if (!foundErrors.Contains("labelHasElements") && layer.HasFrameElements())
            {
                foundErrors.Add("labelHasElements");
            }
            if (!foundErrors.Contains("labelHasActions") && layer.HasActions())
            {
                foundErrors.Add("labelHasActions");
            }
            if (!foundErrors.Contains("labelHasNoLabels") && !layer.HasLabels())
            {
                foundErrors.Add("labelHasNoLabels");
            }
            if (!foundErrors.Contains("duplicateLabels") && layer.HasDuplicateLabels())
            {
                foundErrors.Add("duplicateLabels");
            }

            foundErrors.Sort();
            return foundErrors;
        }
        private static List<string> CheckActionLayer(AnimateLayer? layer)
        {
            var foundErrors = new List<string>();
            if (layer is null) return foundErrors;

            if (layer.HasFrameElements())
            {
                foundErrors.Add("actionHasElements");
            }
            if (layer.HasLabels())
            {
                foundErrors.Add("actionHasLabels");
            }
            List<string> allowedScripts = ["stop", "fscommand"];
            foreach (var script in layer.GetActions())
            {
                string scriptBeginning = script.Split("(")[0];
                if (!allowedScripts.Contains(scriptBeginning))
                {
                    foundErrors.Add("actionHasWrongActions");
                    break;
                }
            }

            foundErrors.Sort();
            return foundErrors;
        }
        private static List<string> CheckInstanceLayer(AnimateLayer? layer, XFL xfl)
        {
            var foundErrors = new List<string>();
            if (layer is null) return foundErrors;

            if (!layer.HasFrameElements())
            {
                foundErrors.Add("instanceHasNoElements");
            }
            if (layer.HasLabels())
            {
                foundErrors.Add("instanceHasLabels");
            }
            if (layer.HasActions())
            {
                foundErrors.Add("instanceHasActions");
            }

            if (layer.GetAllLibraryItems().Any(li => !(li.StartsWith(XFL.LabelFolder) || li == XFL.MainSprite)))
                foundErrors.Add("instanceNotUsingLabelSymbols");
            if (layer.Frames.Any(f => f.HasTransformations()))
                foundErrors.Add("instanceHasTransformations");
            if (layer.Frames.Any(f => f.Elements[0].Matrix.XPosition != 0.0 || f.Elements[0].Matrix.YPosition != 0.0))
                foundErrors.Add("instanceNotAtZeroZero");
            
            foreach (var frame in layer.Frames)
            {
                var labelSymbolName = frame.GetMainLibraryItem();
                if (labelSymbolName is null) continue;
                var labelSymbol = xfl.GetSymbolByName(labelSymbolName);

                var frameDuration = frame.Duration;
                var labelDuration = labelSymbol?.Timeline.GetTotalLength();
                if (labelDuration is not null && frameDuration != labelDuration)
                {
                    foundErrors.Add("mismatchLabelSymbolLength");
                    break;
                }
            }

            foundErrors.Sort();
            return foundErrors;
        }


        private static string MakeErrorMessage(Dictionary<string, Dictionary<string, string>> errorTracker, List<string> domDocumentErrors)
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
            {"hasEmptyLayers", "Symbol has layers with no frames in them"},
            {"multipleNeededLayers", "Multiple layers that are named both \"layer\", \"action\", or \"instance\" are found"},
            {"extraLayers", "Layers that aren't named \"layer\", \"action\", or \"instance\" are found"},
            {"missingNeededLayers", "Layers named \"layer\", \"action\", or \"instance\" are missing"},
            {"framerateTooHigh", "The framerate of the document is above 30, ensure it is at or below 30"},
            {"labelHasElements", "Elements are found in the \"label\" layer, it should only contain keyframes with labels"},
            {"labelHasActions", "Action scripts are found in the \"label\" layer, it should only contain keyframes with labels"},
            {"labelHasNoLabels", "No labels are found in the \"label\" layer, there should be at least one label"},
            {"duplicateLabels", "Duplicate labels are found in \"label\" layer, every label should be unique"},
            {"actionHasElements", "Elements are found in the \"action\" layer, it should only contain keyframes with action scripts"},
            {"actionHasLabels", "Labels are found in the \"action\" layer, it should only contain keyframes with action scripts"},
            {"actionHasWrongActions", "Action scripts that aren't \"stop\" or \"fscommand\" are found in the action layer"},
            {"instanceHasNoElements", "No elements are found in the \"instance\" layer, ensure it has at least one element"},
            {"instanceHasLabels", "Labels are found in the \"instance\" layer, it should only contain keyframes with action scripts"},
            {"instanceHasActions", "Action scripts are found in the \"instance\" layer, it should only contain keyframes with labels"},
            {"instanceNotUsingLabelSymbols", "Library items that aren't \"label\" symbols or named \"main_sprite\" are found in the \"instance\" layer"},
            {"instanceHasTransformations", "The element(s) in the \"instance\" layer are scaled or rotated ensure it is not modified"},
            {"instanceNotAtZeroZero", "The element(s) in the \"instance\" layer are not positioned at (0, 0)"},
            {"mismatchLabelSymbolLength", "Some label symbols don't match their length in instance layer"}
        };

            // Add errors found in DOMDocument
            if (domDocumentErrors.Count > 0)
            {
                errorMessage.Append("Errors in the main document are found\n");
                foreach (string errorType in domDocumentErrors)
                {
                    errorMessage.Append($"\t{errorMessageTypes[errorType]}\n");
                }
                errorMessage.Append('\n');
            }

            // Loop through the errors found in each symbol
            foreach (var symbolErrorPair in errorTracker)
            {
                var errors = symbolErrorPair.Value; // Error message type + full error message
                if (errors.Count == 0) continue; // If no errors are found in the symbol, continue to next one

                // Get symbol name and insert that the symbol has an error 
                var symbolName = symbolErrorPair.Key;
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