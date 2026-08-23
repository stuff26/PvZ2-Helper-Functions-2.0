using UniversalMethods;
using XflComponents;

namespace HelperFunctions.Functions.Packages
{
    public static class ReorganizeLabelOrder
    {
        public static void Function()
        {
            // Get XFL to edit
            UM.PrintColoredText(ConsoleColor.DarkCyan, "Enter the XFL you want to edit", separateLines:true);
            var xfl = XFL.AskForXFL(new()
            {
                GetSymbols = true,
                GetBitmaps = false,
                CheckProgress = true
            },
            new()
            {
                {xfl => xfl.DOMDocument.ContainsInstanceLayer(), $"DOMDocument does not contains {DOMDocument.InstanceLayer}"},
                {xfl => xfl.DOMDocument.ContainsActionLayer(), $"DOMDocument does not contains {DOMDocument.ActionLayer}"},
                {xfl => xfl.DOMDocument.ContainsLabelLayer(), $"DOMDocument does not contains {DOMDocument.LabelLayer}"},
                {xfl => xfl.GetMainSpriteSymbol() is null || (bool)!xfl.IsSplitLabelsType()!, "XFL needs to be a split labels type"},
                {xfl => !xfl.DOMDocument.ContainsDuplicateLabels(), $"DOMDocument contains duplicate labels in {DOMDocument.LabelLayer}"}
            });
            xfl.XflPath = UserPrompts.OverwriteXFLPrompt(xfl.XflPath);

            // Ask user how to organize labels
            int organizeType = PromptOrganizeType();

            // Get all label details
            var labelDetailList = GetLabelDetails(xfl);

            // Organize according to user input
            labelDetailList = OrganizeLabelDetails(labelDetailList, organizeType);

            // Insert into DOMDocument
            InsertOrganizedLabels(xfl.DOMDocument, labelDetailList);

            // Save files
            xfl.SaveXfl();
        }
        private static int PromptOrganizeType()
        {
            List<string> organizeOptions = [
                "Alphabetical order",
                "Reverse alphabetical order",
                "Length order (longest labels first)",
                "Reverse length order",
                "Manual order",
            ];
            UM.PrintColoredText(ConsoleColor.DarkCyan, "Enter how to organize the labels", separateLines:true);
            int optionNum = 1;
            foreach (var organizeOption in organizeOptions)
            {
                UM.PrintColoredText(ConsoleColor.Green, $"[{optionNum}]");
                UM.PrintColoredText(ConsoleColor.White, " - ");
                UM.PrintColoredText(ConsoleColor.DarkCyan, organizeOption, separateLines:true);
                optionNum++;
            }
            
            return UserPrompts.AskForInt(1, organizeOptions.Count);
        }

        private static List<LabelDetails> GetLabelDetails(XFL xfl)
        {
            var labelDetailList = new List<LabelDetails>();
            var labelSymbols = xfl.GetLabelSymbols();
            var labelLayer = xfl.DOMDocument.GetLabelLayer()!;
            var actionLayer = xfl.DOMDocument.GetActionLayer()!;
            foreach (var symbol in labelSymbols)
            {
                var symbolFileName = symbol.GetFileName();
                var symbolLength = symbol.Timeline.GetTotalLength();
                int startingIndex = -1;
                int endingIndex = -1;
                foreach (var frame in labelLayer.Frames)
                {
                    if (frame.Name == symbolFileName)
                    {
                        startingIndex = frame.Index;
                    }
                    else if (startingIndex != -1 && endingIndex == -1 && frame.Name != string.Empty)
                    {
                        endingIndex = frame.Index - 1;
                    }
                }

                // If the label was not found in the DOMDocument
                if (startingIndex == -1)
                {
                    List<AnimateFrame> cutActionLayers = [new AnimateFrame(1, symbolLength - 1),
                                                          AnimateFrame.GetSingleStopActionKeyframe(symbolLength)];
                    labelDetailList.Add(new(symbolFileName, symbol, cutActionLayers));
                    continue;
                }

                // If the label is found
                AnimateLayer cutActionLayer = actionLayer.CutLayer(startingIndex, endingIndex);
                cutActionLayer.FixFramePositions();
                /*
                cutActionLayer.Frames = cutActionLayer.Frames.Where(f => !f.GetActionScripts().Contains(AnimateFrame.StopAction)).ToList(); // Remove stop actions

                var lengthDifference = cutActionLayer.GetLayerLength() - symbolLength;
                if (lengthDifference < 0)
                {
                    cutActionLayer.Frames.Add(new AnimateFrame(startingIndex, -1 * lengthDifference));
                }
                else if (lengthDifference > 0)
                {
                    cutActionLayer = cutActionLayer.CutLayer(0, symbolLength);
                }

                cutActionLayer.Frames.Last().Duration--;
                cutActionLayer.Frames.Add(AnimateFrame.GetSingleStopActionKeyframe(symbolLength - 1));
                */
                labelDetailList.Add(new(symbolFileName, symbol, cutActionLayer.Frames));
            }

            return labelDetailList;
        }
        
        private static List<LabelDetails> OrganizeLabelDetails(List<LabelDetails> labelDetailList, int organizeType)
        {
            if (organizeType == 1)
            {
                return labelDetailList.OrderBy(l => l.name).ToList();
            }
            if (organizeType == 2)
            {
                return labelDetailList.OrderByDescending(l => l.name).ToList();
            }
            if (organizeType == 3)
            {
                return labelDetailList.OrderByDescending(l => l.Length).ToList();
            }
            if (organizeType == 4)
            {
                return labelDetailList.OrderBy(l => l.Length).ToList();
            }
            if (organizeType == 5)
            {
                return ManualOrganizeLabelDetails(labelDetailList);
            }

            return labelDetailList;
        }

        private static List<LabelDetails> ManualOrganizeLabelDetails(List<LabelDetails> labelDetailList)
        {
            var newLabelDetailList = new List<LabelDetails>();
            labelDetailList = labelDetailList.OrderBy(l => l.name).ToList();
            UM.PrintColoredText(ConsoleColor.Green, "Enter labels in the order you want to add", separateLines:true);
            while (labelDetailList.Count > 1)
            {
                var labelNum = 0;
                foreach (var label in labelDetailList)
                {
                    labelNum++;
                    UM.PrintColoredText(
                    [
                        (ConsoleColor.Green, $"[{labelNum}]"),
                        (ConsoleColor.White, " - "),
                        (ConsoleColor.Green, $"{label.name}"),
                        (ConsoleColor.DarkCyan, $", Length: {label.Length}\n"),
                    ]);
                }
                var userInput = UserPrompts.AskForInt(1, labelNum) - 1;
                var selectedLabel = labelDetailList[userInput];
                labelDetailList.RemoveAt(userInput);
                newLabelDetailList.Add(selectedLabel);
            }

            newLabelDetailList.Add(labelDetailList[0]); // Automatically add last label
            return newLabelDetailList;
        }
    
        private static void InsertOrganizedLabels(DOMDocument domDocument, List<LabelDetails> labelDetailList)
        {
            domDocument.Timeline = DOMDocument.MakeNewDOMDocumentTimeline();
            var (instanceLayer, actionLayer, labelLayer) = domDocument.GetEssentialLayers();
            var currentIndex = 0;
            foreach (var label in labelDetailList)
            {
                var labelFrame = AnimateFrame.GetLabelFrame(currentIndex, label.name, wantedDuration:label.Length);
                labelLayer!.Frames.AddRange(labelFrame);

                label.actionFrames.ForEach(f => f.Index += currentIndex);
                actionLayer!.Frames.AddRange(label.actionFrames);

                var labelSymbol = $"label/{label.name}";
                var instanceFrame = AnimateFrame.GetSingleKeyframe(currentIndex, label.Length, labelSymbol, "SymbolInstance");
                instanceLayer!.Frames.Add(instanceFrame);

                currentIndex += label.Length;
            }
        }
    }

    public class LabelDetails(string Name, SymbolItem symbol, List<AnimateFrame> actionFrames)
    {
        public string name = Name;
        public SymbolItem symbol = symbol;
        public List<AnimateFrame> actionFrames = actionFrames;

        public int Length => symbol.Timeline.GetTotalLength();
    }
}