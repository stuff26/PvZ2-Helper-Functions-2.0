using XflComponents;
using UniversalMethods;

namespace HelperFunctions.Functions.Packages
{
    public static class RenameAllMedia
    {
        public static void Function()
        {
            // Ask for XFL
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            UM.PrintColoredText(ConsoleColor.DarkCyan, "Enter an XFL or an individual sprite", separateLines:true);
            var xfl = XFL.AskForXFL(new()
            {
                GetSymbols = true,
                GetBitmaps = true,
                CheckProgress = true,
                GetDataJsonData = true
            },
            new(){
                {CheckForNoMainSymbol, "Could not determine main symbol used by image symbols"},
                {CheckForMediaReuse, "Image symbols reuse the same bitmaps, fix then enter again"}
            });

            // Ask for prefix to use for new media, ask to overwrite existing XFL
            var prefix = AskForPrefix();
            int renameType = AskForRenameType();
            xfl.XflPath = UserPrompts.OverwriteXFLPrompt(xfl.XflPath);

            // Get all image symbols
            var imageBitmaps = CompileImageBitmaps(xfl);
            var oldNewImageDict = RenameFiles(imageBitmaps, xfl, renameType, prefix);

            // Adjust references to image symbols in sprite and label symbols
            AdjustExistingSymbols(oldNewImageDict, xfl);

            xfl.UpdateAllItemReferences();
            UM.PrintColoredText(ConsoleColor.Green, "Saving XFL... ");
            xfl.SaveXfl();
            ProgressChecker.WriteFinished();
        }

        private static string AskForPrefix()
        {
            UM.PrintColoredText([
                (ConsoleColor.DarkCyan, "Enter the prefix you want "),
                (ConsoleColor.Yellow, "(ex: "),
                (ConsoleColor.Green, "zombie_tutorial"),
                (ConsoleColor.Yellow, ")\n")
            ]);
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

                return userInput;
            }
        }

        private static int AskForRenameType()
        {
            UM.PrintColoredText([
                (ConsoleColor.DarkCyan, "Enter the way you want to rename the files\n"),
                (ConsoleColor.Green, "[1]"),
                (ConsoleColor.White, " - "),
                (ConsoleColor.DarkCyan, "By number, with smallest images being first\n"),
                (ConsoleColor.Green, "[2]"),
                (ConsoleColor.White, " - "),
                (ConsoleColor.DarkCyan, "By 1200 size of bitmaps (PopCap style)\n")
            ]);
            return UserPrompts.AskForInt(1, 2);
        }

        private static bool CheckForNoMainSymbol(XFL xfl)
        {
            return !xfl.GetImageSymbols()
                      .Any(s => s.Timeline.GetAllLibraryItems().Count != 1);
        }

        private static bool CheckForMediaReuse(XFL xfl)
        {
            var imageSymbols = xfl.GetImageSymbols().Where(s => s.Timeline.Layers.Count > 0);
            var mediaTracker = new HashSet<string>();
            return !imageSymbols.Select(s => s.Timeline.Layers[0].GetMainLibraryItem())
                                .Where(libItem => libItem is not null)
                                .Any(libItem => !mediaTracker.Add(libItem!));
        }

        private static List<(SymbolItem symbol, Bitmap bitmap)> CompileImageBitmaps(XFL xfl)
        {
            var symbols = xfl.GetImageSymbols();
            List<(SymbolItem symbol, Bitmap bitmap)> imageBitmaps = [];

            foreach (var symbol in symbols)
            {
                var element = symbol.Timeline.GetAllElements()[0];
                var media = element.LibraryItemName;
                
                var bitmap = xfl.GetBitmapByName(media)!;
                imageBitmaps.Add((symbol, bitmap));
            }
            
            // Sort image and bitmaps by bitmap's width, then height, then return
            imageBitmaps = imageBitmaps.OrderBy(sbp => xfl.CalculateDataJsonSize(sbp.bitmap.Width)) // Calculate with 1200 size to
                               .ThenBy(sbp => xfl.CalculateDataJsonSize(sbp.bitmap.Height)) // prevent rounding errors 
                               .ThenBy(sbp => sbp.bitmap.GetFileSize())                     // from causing issues
                               .ToList();
            xfl.Bitmaps = imageBitmaps.Select(kvp => kvp.bitmap).ToList();

            return imageBitmaps;
        }

        private static Dictionary<string, string> RenameFiles(List<(SymbolItem symbol, Bitmap bitmap)> imageBitmaps,
                                        XFL xfl, int renameType, string prefix)
        {
            // Get the bitmap sizes, if rename type 2
            Dictionary<string, (int width, int height)>? bitmapSizes = null;
            if (renameType == 2)
            {
                bitmapSizes = [];
                foreach (var bitmapSize in xfl.GetBitmapSizeDictionary())
                {
                    var (name, (width, height)) = bitmapSize;
                    width = xfl.CalculateDataJsonSize(width);
                    height = xfl.CalculateDataJsonSize(height);
                    bitmapSizes.Add(name, (width, height));
                }
            }
            
            // Make counter variable, used if rename type 1
            int symbolNum = 1;

            // Make a dictionary to keep track of old and new image symbol names
            Dictionary<string, string> oldNewImageNames = [];

            // Make a dictionary to keep track of existing media names for type 2
            Dictionary<string, int> symbolNameTracker = [];

            // Loop through all symbol and bitmap pairs
            ProgressChecker renameImageSymbols = new("Renaming files...", imageBitmaps.Count);
            foreach (var (symbol, bitmap) in imageBitmaps)
            {
                // Get new name for symbol and bitmap
                var newName = string.Empty;
                if (renameType == 1)
                {
                    newName = $"{prefix}_{symbolNum}";
                }
                else if (renameType == 2)
                {
                    var bitmapName = bitmap.Name;
                    var (width, height) = bitmapSizes![bitmapName];
                    newName = $"{prefix}_{width}x{height}";
                    if (symbolNameTracker.TryGetValue(newName, out int num))
                    {
                        symbolNameTracker[newName] += 1;
                        newName += $"_{num+1}";
                    }
                    else
                    {
                        symbolNameTracker.Add(newName, 1);
                    }
                }

                // Make image and media name
                var newSymbolName = $"image/{newName}";
                var newMediaName = $"media/{newName}";

                // Add old and new image symbol names to dictionary
                var oldSymbolName = symbol.Name;
                oldNewImageNames.Add(oldSymbolName, newSymbolName);

                // Change symbol name
                symbol.ChangeName(newSymbolName);

                // Change media reference in symbol
                var element = symbol.Timeline.GetAllElements()[0];
                element.LibraryItemName = newMediaName;

                // Rename bitmap
                bitmap.Name = newMediaName;
                
                // Counters
                symbolNum++;
                renameImageSymbols.AddOne();
            }

            return oldNewImageNames;
        }

        private static void AdjustExistingSymbols(Dictionary<string, string> oldNewImageDict, XFL xfl)
        {
            // Get non image symbols
            var symbolList = xfl.Symbols.Where(s => Path.GetDirectoryName(s.Name) != XFL.ImageFolder).ToList();
            ProgressChecker renameSymbolInternals = new("Adjusting used symbol names...", symbolList.Count);

            foreach (var symbol in symbolList)
            {
                var elements = symbol.Timeline.GetAllElements();
                foreach (var element in elements)
                {
                    var oldLibraryItem = element.LibraryItemName;
                    if (oldNewImageDict.TryGetValue(oldLibraryItem, out var newLibraryItem))
                    {
                        element.LibraryItemName = newLibraryItem;
                    }
                }
                renameSymbolInternals.AddOne();
            }
        }
    }
}