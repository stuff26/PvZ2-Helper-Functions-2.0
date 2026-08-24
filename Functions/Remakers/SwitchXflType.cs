using XflComponents;
using UniversalMethods;

namespace HelperFunctions.Functions.Packages
{
    public static class SwitchXflType
    {
        public static void Function()
        {   
            // Get the XFL that will be converted
            UM.PrintColoredText(ConsoleColor.DarkCyan, "Enter the XFL you want to convert", separateLines:true);
            var xfl = XFL.AskForXFL(new()
            {
                GetSymbols = true,
                GetBitmaps = true,
                CheckProgress = true
            },
            new()
            {
                {xfl => xfl.DOMDocument.HasEssentialLayers(), "DOMDocument does not have essential layers, enter again"},
                {xfl => xfl.IsSplitLabelsType() is not null, "Could not determine type of XFL, enter again"},
                {xfl => !xfl.DOMDocument.ContainsDuplicateLabels(), "XFL can't have duplicated labels, enter again"},
                {xfl => xfl.HasMainSpriteSymbol() || xfl.GetLabelsWithNoSymbol().Count == 0, "Some labels are missing label symbols, enter again"},
                {xfl => xfl.HasMainSpriteSymbol() || xfl.LabelSymbolsMatchLabelSize(), "Labels in DOMDocument don't match label symbol size"},
                {xfl => !xfl.HasMainSpriteSymbol() || xfl.DOMDocument.GetInstanceLayer()!.GetLayerLength() 
                        == xfl.GetMainSpriteSymbol()?.Timeline.GetTotalLength(), "Instance layer and main sprite don't have the same size"}
            });
            var isSplitLabels = xfl.IsSplitLabelsType();
            xfl.XflPath = UserPrompts.OverwriteXFLPrompt(xfl.XflPath);

            // If split labels --> nonsplit labels
            if (isSplitLabels == true)
            {
                SplitToNonSplitLabels(xfl);
            }

            // If nonsplit labels => split labels
            else if (isSplitLabels == false)
            {
                NonSplitToSplitLabels(xfl);
            }
            
            // Save XFL after edits
            UM.PrintColoredText(ConsoleColor.Green, "Saving XFL... ");
            xfl.SaveXfl();
            ProgressChecker.WriteFinished();
        }

        // Used for split => nonsplit labels
        private static void SplitToNonSplitLabels(XFL xfl)
        {
            
            UM.PrintColoredText(ConsoleColor.DarkCyan, "Converting to non split labels", separateLines:true);

            // Get a dictionary containing every label and its corresponding symbol
            UM.PrintColoredText(ConsoleColor.Green, "Retrieving symbol details... ");
            var labelSymbolDict = GetLabelSymbolDict(xfl); // Label name as key, corresponding symbol item as value
            var labelDurations = xfl.DOMDocument.GetLabelLengths()!; // Label name as key, label length as value
            ProgressChecker.WriteFinished();

            // Create main_sprite symbol object
            UM.PrintColoredText(ConsoleColor.Green, "Creating main_sprite... ");
            var mainSymbolItem = new SymbolItem(XFL.MainSprite);
            mainSymbolItem.Timeline.Layers = CombineLayers(labelSymbolDict, labelDurations, xfl);
            ProgressChecker.WriteFinished();

            // Edit DOMDocument references and add symbol
            UM.PrintColoredText(ConsoleColor.Green, "Editing DOMDocument... ");
            xfl.DOMDocument.AddNewSymbolItem(XFL.MainSprite);
            xfl.DOMDocument.RemoveSymbolItem(labelSymbolDict.Keys.ToList(), XFL.LabelFolder, includesEnd:false);

            // Edit XFL object references
            xfl.Symbols.Add(mainSymbolItem);
            var labelSymbols = labelSymbolDict.Values.ToList();
            labelSymbols.ForEach(l => xfl.Symbols.Remove(l));

            // Add symbol to DOMDocument instance layer
            var instanceLayer = xfl.DOMDocument.GetInstanceLayer()!;
            var mainSpriteLength = mainSymbolItem.Timeline.GetTotalLength();
            var mainSpriteFrame = AnimateFrame.GetSingleKeyframe(0, mainSpriteLength, XFL.MainSprite);
            instanceLayer.Frames = [mainSpriteFrame];

            // Remove label symbols

            // Remove label folder if there are no symbols present
            if (!xfl.Symbols.Any(s => s.GetFolder() == XFL.LabelFolder))
                xfl.DOMDocument.RemoveFolderItem(XFL.LabelFolder);

            ProgressChecker.WriteFinished();
        }

        // Used for nonsplit => split labels
        private static void NonSplitToSplitLabels(XFL xfl)
        {
             
            UM.PrintColoredText(ConsoleColor.DarkCyan, "Converting to split labels", separateLines:true);

            // Get main symbol, check for possible errors
            UM.PrintColoredText(ConsoleColor.Green, "Retrieving main_sprite... ");
            var mainSpriteSymbol = xfl.GetMainSpriteSymbol()!;
            var labelIndexes = xfl.DOMDocument.GetLabelIndexes();
            ProgressChecker.WriteFinished();

            // Make label folder, add to DOMDocument
            var makeLabelSymbols = new ProgressChecker("Writing new label symbols...", labelIndexes.Count);

            foreach (var label in labelIndexes)
            {
                // Get values
                var labelName = label.Key;
                var (start, end) = label.Value;
                var labelSymbolName = $"{XFL.LabelFolder}/{labelName}";

                // Make label symbol
                var labelSymbolLayers = SymbolTimeline.CutLayers(mainSpriteSymbol.Timeline.Layers, start, end);
                var labelSymbol = new SymbolItem(labelSymbolName, labelSymbolLayers);

                // Add reference to DOMDocument and XFL object
                xfl.DOMDocument.AddNewSymbolItem(labelSymbolName);
                xfl.AddSymbol(labelSymbol);

                makeLabelSymbols.AddOne();
            }

            // Add instance layer to DOMDocument
            UM.PrintColoredText(ConsoleColor.Green, "Editing references... ");
            var instanceLayer = xfl.DOMDocument.GetInstanceLayer()!;
            instanceLayer.Frames = AnimateFrame.GetKeyframeSeries(labelIndexes);

            // Remove main_sprite
            xfl.DOMDocument.RemoveSymbolItem(XFL.MainSprite, includesEnd:false);
            xfl.Symbols.Remove(mainSpriteSymbol);

            // Add label folder
            if (!xfl.DOMDocument.FolderList.Any(f => f.Name == XFL.LabelFolder))
                xfl.DOMDocument.AddNewFolderItem(XFL.LabelFolder);

            ProgressChecker.WriteFinished();
        }

        private static Dictionary<string, SymbolItem> GetLabelSymbolDict(XFL xfl)
        => xfl.DOMDocument
              .GetAllLabels()
              .ToDictionary(l => l, l => xfl.GetSymbolByName($"{XFL.LabelFolder}/{l}")!);

        private static List<AnimateLayer> CombineLayers(Dictionary<string, SymbolItem> labelSymbolDict, Dictionary<string, int> labelDuration, XFL xfl)
        {
            // Find how many layers there are total and make list for layers
            var totalLayers = 0;
            labelSymbolDict.Values.ToList().ForEach(s => totalLayers += s.Timeline.GetTotalLength());
            List<AnimateLayer> mainLayers = new(totalLayers);

            int currentFramePosition = 0; // Keeps track of the first frame empty frame index to place new frames
            foreach (var labelSymbolName in xfl.DOMDocument.GetAllLabels())
            {
                var labelSymbol = labelSymbolDict[labelSymbolName];
                labelSymbol.Timeline.MoveFrames(currentFramePosition); // Move frames to their intended position
                var symbolLayers = labelSymbol.Timeline.Layers; // Get a list of layers from symbol
                mainLayers.AddRange(symbolLayers); // Combine layers to the list of main layers
                currentFramePosition += labelDuration[labelSymbolName]; // Shift amount current position
            }

            return mainLayers;
        }
    }
}