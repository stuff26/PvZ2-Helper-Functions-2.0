using UniversalMethods;
using XflComponents;

namespace HelperFunctions.Functions.Packages
{
    public static class AddMissingLabels
    {
        public static void Function()
        {
            // Ask user for XFL
            UM.PrintColoredText(ConsoleColor.DarkCyan, "Enter the XFL you want to edit", separateLines:true);
            var xfl = XFL.AskForXFL(
                new()
                {
                    GetSymbols = true,
                    GetBitmaps = false,
                    CheckProgress = true 
                },
                new()
                {
                    {xfl => xfl.DOMDocument.HasLabel(DOMDocument.InstanceLayer), "Could not find instance layer in DOMDocument"},
                    {xfl => xfl.DOMDocument.HasLabel(DOMDocument.LabelLayer), "Could not find label layer in DOMDocument"},
                    {xfl => xfl.DOMDocument.HasLabel(DOMDocument.ActionLayer), "Could not find action layer in DOMDocument"},
                    {xfl => XFL.IsSplitLabelsType(xfl) is not null, "Could not determine type of XFL, enter again"}
                });
            xfl.XflPath = UserPrompts.OverwriteXFLPrompt(xfl.XflPath);

            // Check for type of XFL
            var isSplitLabelsType = (bool)xfl.IsSplitLabelsType()!;

            // If main_sprite is used in instance layer
            bool shouldSaveFiles;
            if (!isSplitLabelsType)
            {
                shouldSaveFiles = AddMissingLabelsNoSplitLabels(xfl);
            }
            else
            {
                shouldSaveFiles = AddMissingLabelsSplitLabels(xfl);
            }

            // Save DOMDocument
            if (shouldSaveFiles)
            {
                UM.PrintColoredText(ConsoleColor.Green, "Saving DOMDocument... ");
                xfl.WriteDOMDocument(addFile:true);
                ProgressChecker.WriteFinished();
            }
        }

        private static void AppendLabelToDOMDocument(string labelName, int length, DOMDocument domDocument)
        {
            var index = domDocument.Timeline.GetTotalLength();

            // Add to label layer
            var labelLayer = domDocument.Timeline.GetLayerByName(DOMDocument.LabelLayer)!;
            var newLabelFrame = AnimateFrame.GetLabelFrame(index, labelName, length);
            labelLayer.Frames.Add(newLabelFrame);

            // Add to instance layer
            var instanceLayer = domDocument.Timeline.GetLayerByName(DOMDocument.InstanceLayer)!;
            var newInstanceFrame = AnimateFrame.GetSingleKeyframe(index, length, $"{DOMDocument.LabelLayer}/{labelName}", elementType:"SymbolInstance");
            instanceLayer.Frames.Add(newInstanceFrame);
            
            // Add to action layer
            var actionLayer = domDocument.Timeline.GetLayerByName(DOMDocument.ActionLayer)!;
            var emptyFrame = new AnimateFrame(index, length - 1);
            actionLayer.Frames.Add(emptyFrame);
            var stopActionFrame = AnimateFrame.GetSingleStopActionKeyframe(index:index + length);
            actionLayer.Frames.Add(stopActionFrame);
        }
    
        private static bool AddMissingLabelsNoSplitLabels(XFL xfl)
        {                
            // Potentially add manual insertion for different label types??
            var mainSprite = xfl.GetSymbolByName(XFL.MainSprite)!;
                
            // Get length of DOMDocument and main_sprite
            int domDocumentLength = xfl.DOMDocument.Timeline.GetTotalLength();
            int mainSpriteLength = mainSprite.Timeline.GetTotalLength();
            int missingFrames = mainSpriteLength - domDocumentLength;

            // If the length of the DOMDocument greater than or equal to length of main_sprite, exit out and print success message
            if (missingFrames <= 0)
            {
                UM.PrintColoredText(ConsoleColor.Yellow,
                "main_sprite length is already less than or equal to DOMDocument length, no further edits needed",
                separateLines:true);
                return false;
            }

            // Extend DOMDocument length, add stop actions
            AppendLabelToDOMDocument("new_label", missingFrames, xfl.DOMDocument);

            return true;
        }
    
        private static bool AddMissingLabelsSplitLabels(XFL xfl)
        {
            // If the type is split labels, get all the labels in the DOMDocument and all label symbols
            var domDocumentLabels = xfl.DOMDocument.GetAllLabels();
            var labelSymbols = xfl.GetLabelSymbols(); // Get all symbols in label folder
            var libraryLabels = labelSymbols.Select(s => Path.GetFileName(s.Name));

            // Get labels in library but not in DOMDocument
            var missingLabels = libraryLabels.Except(domDocumentLabels).ToList();
            var numMissingLabels = missingLabels.Count;
            if (numMissingLabels == 0) // If no missing labels are found, print success message and exit out
            {
                UM.PrintColoredText(ConsoleColor.Yellow,
                                    "No missing labels in DOMDocument found, no further edits needed",
                                    separateLines:true);
                return false;
            }

            // Add missing labels to DOMDocument
            UM.PrintColoredText([
                (ConsoleColor.DarkCyan, "Found "),
                (ConsoleColor.Green, $"{numMissingLabels}"),
                (ConsoleColor.DarkCyan, " missing labels in "),
                (ConsoleColor.Green, $"DOMDocument\n"),
            ]);
            var addLabels = new ProgressChecker("Adding missing labels... ", numMissingLabels);
            foreach (var missingLabel in missingLabels)
            {
                var labelSymbolName = $"label/{missingLabel}";
                var labelSymbol = xfl.GetSymbolByName(labelSymbolName)!;
                var labelSymbolLength = labelSymbol.Timeline.GetTotalLength();
                AppendLabelToDOMDocument(missingLabel, labelSymbolLength, xfl.DOMDocument);
                addLabels.AddOne();
            }

            return true;
        }
    }
}