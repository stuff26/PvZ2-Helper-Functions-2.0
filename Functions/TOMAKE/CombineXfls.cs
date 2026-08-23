using XflComponents;

namespace HelperFunctions.Functions.Packages
{
    public static class CombineXfls
    {
        public static void Function()
        {
            // Get the base XFL
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter the first XFL to combine with");
            Console.WriteLine("(This will act as the base for the combined XFL)");
            var baseXflDir = @"C:\Users\zacha\Documents\main.675.com.ea.game.pvz2_aub.obb.bundle\packet\VaseBreakerFeastivusGroup.package\resource\images\full\vasebreaker\vase_feastivus_brown";
            //UserPrompts.AskForDirectory(["DOMDocument.xml"]);
            var baseXfl = new XFL(baseXflDir, new XFLInitOptions(){GetSymbols = true, GetBitmaps = true});

            // Get additional XFLs to combine
            int numAddedXfls = 0;
            List<string> xflsToCombinePaths = [];
            while (true)
            {
                numAddedXfls++;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"[{numAddedXfls}] ");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("Enter another XFL or enter nothing when you are finished");
                var userInput = @"C:\Users\zacha\Documents\main.675.com.ea.game.pvz2_aub.obb.bundle\packet\VaseBreakerFeastivusGroup.package\resource\images\full\vasebreaker\vase_feastivus_gargantuar";//UserPrompts.AskForDirectory(["DOMDocument.xml"], allowNoAnswer:true);
                if (userInput == string.Empty) break;
                xflsToCombinePaths.Add(userInput);
                numAddedXfls = 2; break; //////
            }

            // Get the new folder name of the XFL
            var baseDirectoryName = Path.GetFileName(baseXflDir);
            var parentFolder = Path.GetDirectoryName(baseXflDir);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter the folder name you wish to have for the resulting XFL");
            string newXflPath;
            while (true)
            {
                var newXflName = "testing";//Console.ReadLine()?.Trim();
                Console.ForegroundColor = ConsoleColor.Red;
                if (string.IsNullOrWhiteSpace(newXflName))
                {
                    Console.WriteLine("Enter a valid folder name");
                    continue;
                }
                if (newXflName == baseDirectoryName)
                {
                    Console.WriteLine("Can't override base XFL, enter again");
                    continue;
                }
                newXflPath = Path.Join(parentFolder, newXflName);
                if (xflsToCombinePaths.Contains(newXflPath))
                {
                    Console.WriteLine("Can't override a given XFL, enter again");
                }
                Directory.Delete(newXflPath);
                //UM.CopyFolder(baseXflDir, newXflPath);
                baseXflDir = newXflPath;
                break;
            }

            // If there are no additional XFLs given, just copy the base XFL to a new directory
            if (numAddedXfls == 1)
            {
                ProgressChecker.WriteFinished();
                return;
            }

            // Get XFL objects
            List<XFL> xflsToCombine = [];
            foreach (var xflPath in xflsToCombinePaths)
            {
                XFL? newXfl = new(xflPath, new XFLInitOptions(){GetSymbols = true, GetBitmaps = true});
                xflsToCombine.Add(newXfl);
            }
            
            // Add together symbols, rename them depending on original xfl name
            int numSymbols = 0;
            foreach (var xfl in xflsToCombine)
            {
                numSymbols += xfl.GetNumSymbols();
            }
            ProgressChecker addSymbols = new("Adding symbols...", numSymbols);

            // Rename symbols in base XFL
            var prefixToAdd = "basexfl_";
            baseXfl.AddPrefixToSymbols(prefixToAdd);
            baseXfl.AddPrefixToBitmaps(prefixToAdd);

            int xflNum = 1;
            foreach (var xfl in xflsToCombine)
            {
                // Add symbols to XFL folder, rename them, fix references to other symbols
                AddSymbolsToXfl(xfl, xflNum, baseXflDir, addSymbols);

                // Add new symbol names to DOMDocument and XFL object
                var symbolNames = xfl.GetAllSymbolNames();
                baseXfl.DOMDocument.AddNewSymbolItemRange(symbolNames);
                baseXfl.Symbols.AddRange(xfl.Symbols);

                xflNum++;
            }

            // Ditto, but for bitmaps
            int numBitmaps = 0;
            foreach (var xfl in xflsToCombine)
            {
                numBitmaps += xfl.GetNumBitmaps();
            }
            ProgressChecker addBitmaps = new("Adding bitmaps...", numBitmaps);

            xflNum = 1;
            foreach (var xfl in xflsToCombine)
            {
                // Add bitmaps to XFL folder, rename them, fix references to bitmaps
                AddBitmapsToXfl(xfl, xflNum, baseXflDir, addBitmaps);

                // Add new bitmaps names to DOMDocument and XFL object
                var bitmapNames = xfl.GetAllBitmapNames();
                baseXfl.DOMDocument.AddNewBitmapItemRange(bitmapNames);
                baseXfl.Bitmaps.AddRange(xfl.Bitmaps);

                xflNum++;
            }

            // Combine labels in DOMDocument
            
            // Save files
            baseXfl.SaveXfl(baseXflDir);
        }

        public static void AddSymbolsToXfl(XFL xfl, int xflNum, string baseXflDir, ProgressChecker addSymbols)
        {
            // Change symbol name internally
            Dictionary<string, string> oldNewSymbolDict = [];
            foreach (var symbol in xfl.Symbols)
            {
                var symbolPath = symbol.Name!;
                var symbolName = Path.GetFileName(symbolPath)!;
                var symbolParentDir = Path.GetDirectoryName(symbolPath);
                var newSymbolName = $"{symbolParentDir}/xfl{xflNum}_{symbolName}";
                symbol.ChangeName(newSymbolName);
                oldNewSymbolDict.Add(symbolPath, newSymbolName);
            }

            foreach (var symbol in xfl.Symbols)
            {
                // Change references in symbols
                foreach (var element in symbol.Timeline!.GetAllElements())
                {
                    var elementLibraryItem = element.LibraryItemName;
                    if (elementLibraryItem is null || !element.GetType().Equals(typeof(SymbolInstance))) continue;
                    element.LibraryItemName = oldNewSymbolDict[elementLibraryItem];
                }

                addSymbols.AddOne();
            }
        }

        public static void AddBitmapsToXfl(XFL xfl, int xflNum, string baseXflDir, ProgressChecker addBitmaps)
        {
            // Change symbol name internally
            Dictionary<string, string> oldNewBitmapDict = [];
            var mediaDir = Path.Join(baseXflDir, XFL.LibraryDirName, XFL.MediaFolder);
            foreach (var bitmap in xfl.Bitmaps)
            {
                // Make new bitmap name
                var bitmapName = bitmap.Name;
                var newBitmapName = bitmap.AddPrefix($"xfl{xflNum}_");
                oldNewBitmapDict.Add(bitmapName, newBitmapName);
            }

            foreach (var symbol in xfl.Symbols)
            {
                // Change references in symbols
                foreach (var element in symbol.Timeline!.GetAllElements())
                {
                    var elementLibraryItem = element.LibraryItemName;
                    if (elementLibraryItem is null || !element.GetType().Equals(typeof(BitmapInstance))) continue;
                    element.LibraryItemName = oldNewBitmapDict[elementLibraryItem];
                }

                addBitmaps.AddOne();
            }
        }
    }
}