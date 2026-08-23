using UniversalMethods;
using XflComponents;

namespace HelperFunctions.Functions.Packages
{
    public static class RemoveUnusedItems
    {
        public static void Function()
        {
            // Ask for XFL
            UM.PrintColoredText(ConsoleColor.DarkCyan, "Enter the XFL you want to edit", separateLines:true);
            var xfl = XFL.AskForXFL(new()
            {
                GetSymbols = true,
                GetBitmaps = true,
                GetDataJsonData = true,
                CheckProgress = true
            });
            xfl.XflPath = UserPrompts.OverwriteXFLPrompt(xfl.XflPath);

            // Keep dictionaries to track which symbol is used where
            var items = xfl.GetAllSymbolNames().ToList();
            items.AddRange(xfl.GetAllBitmapNames());
            var itemUsage = items.ToDictionary(key => key, key => new List<string>());
            FillInItemUsage(xfl, itemUsage);
            var symbolObjectDict = xfl.GetSymbolObjectDict();
            var bitmapObjectdict = xfl.GetBitmapObjectDict();

            // Setup to get which items to not remove and get initial set of unused items
            var unremovableItems = xfl.DOMDocument.GetUsedSymbols().ToHashSet();
            var unusedItems = CheckUnusedItems(itemUsage, unremovableItems);

            // If no unused items are found, print success message and exit out
            if (unusedItems.Count == 0)
            {
                UM.PrintColoredText(ConsoleColor.Yellow, "No unused items found", separateLines:true);
                return;
            }

            // Keep looping through unused items until none are left
            UM.PrintColoredText(ConsoleColor.Green, "Removing unused items... ");
            while (unusedItems.Count > 0)
            {
                foreach (var item in unusedItems)
                {
                    RemoveUnusedItem(item, xfl, itemUsage, symbolObjectDict, bitmapObjectdict);
                }
                unusedItems = CheckUnusedItems(itemUsage, unremovableItems);
            }
            ProgressChecker.WriteFinished();
            
            // Save XFL
            UM.PrintColoredText(ConsoleColor.Green, "Saving XFL... ");
            xfl.SaveXfl();
            ProgressChecker.WriteFinished();
        }

        private static void FillInItemUsage(XFL xfl, Dictionary<string, List<string>> itemUsage)
        {
            // Loop through every symbol in XFL
            foreach (var symbol in xfl.Symbols)
            {
                var symbolName = symbol.Name;
                var symbolsUsed = symbol.Timeline.GetAllLibraryItems(); // Get list of used symbol items
                var usageLists = symbolsUsed.Select(key => itemUsage[key]).ToList(); // Get the usage lists for each symbol item
                usageLists.ForEach(l => l.Add(symbolName)); // Add this symbol to each usage list
            }
        }

        private static List<string> CheckUnusedItems(Dictionary<string, List<string>> itemUsage, HashSet<string> unremovableItems)
        => itemUsage.Where(kvp => kvp.Value.Count == 0)
                    .Select(kvp => kvp.Key)
                    .Where(item => !unremovableItems.Contains(item))
                    .ToList();

        private static void RemoveUnusedItem(string unusedItem, XFL xfl, Dictionary<string, List<string>> itemUsage,
        Dictionary<string, SymbolItem> symbolObjectDict, Dictionary<string, Bitmap> bitmapObjectDict)
        {
            // Remove item from list of used items
            itemUsage.Remove(unusedItem);

            // Get if this is a symbol or bitmap
            var itemType = xfl.IsSymbolOrBitmap(unusedItem)!;

            // Get the item that is not used, remove from XFL and remove reference to it in DOMDocument
            if (itemType.Equals(typeof(SymbolItem))) // If this item is a symbol
            {
                var unusedSymbol = symbolObjectDict[unusedItem];
                xfl.Symbols.Remove(unusedSymbol);
                xfl.DOMDocument.RemoveSymbolItem(unusedItem, includesEnd:false);
            }
            else // If this is a bitmap
            {
                var unusedBitmap = bitmapObjectDict[unusedItem];
                xfl.Bitmaps.Remove(unusedBitmap);
                xfl.DOMDocument.RemoveBitmapItem(unusedItem, includesEnd:false);
            }

            // Remove unused item from lists
            foreach (var usage in itemUsage.Values)
            {
                usage.Remove(unusedItem);
            }
        }
    }
}