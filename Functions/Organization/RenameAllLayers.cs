using XflComponents;
using UniversalMethods;

namespace HelperFunctions.Functions.Packages
{
    public static class RenameAllLayers
    {
        private static readonly Dictionary<string, char> ShortenedDirNames = new(){
            {XFL.SpriteFolder, 's'},
            {XFL.MediaFolder, 'm'},
            {XFL.ImageFolder, 'i'}
        };
        
        public static void Function()
        {
            // Introduction, ask for necessary details from user
            int renameMethod = AskForRenameMethod();
            UM.PrintColoredText(ConsoleColor.DarkCyan, "Enter an XFL or an individual sprite", separateLines:true);
            var result = AskForSymbolItem();

            // Process results
            List<string> AllSymbolPaths = result.SymbolPathList;
            List<SymbolItem> SymbolList = result.SymbolList;

            // Loop through, edit positions
            var renameLayers = new ProgressChecker("Renaming layers...", SymbolList.Count);
            foreach (SymbolItem symbol in SymbolList)
            {
                if (renameMethod == 1 || renameMethod == 2)
                {
                    int currentNum = 0;
                    int numToChangeBy = 0;
                    if (renameMethod == 1)
                    {
                        currentNum = 1;
                        numToChangeBy = 1;
                    }
                    else if (renameMethod == 2)
                    {
                        currentNum = symbol.Timeline.GetLayerCount();
                        numToChangeBy = -1;
                    }
                    foreach (AnimateLayer? layer in symbol.Timeline.Layers)
                    {
                        if (layer is null || layer.Name is null) continue;
                        layer.Name = $"{currentNum}";
                        currentNum += numToChangeBy;
                    }
                }
                else if (renameMethod == 3)
                {
                    var libraryItemCount = new Dictionary<string, int>();
                    foreach (var layer in symbol.Timeline.Layers)
                    {
                        if (layer is null || layer.Name is null) continue;
                        var mainLibraryItem = layer.GetMainLibraryItem();
                        if (mainLibraryItem is null)
                        {
                            mainLibraryItem = "Empty";
                        }
                        else if (mainLibraryItem.Contains('/'))
                        {
                            string[] splitName = mainLibraryItem.Split("/");
                            string rootName = mainLibraryItem.Split("/")[^1];
                            char folderPrefix = ShortenedDirNames.GetValueOrDefault(splitName[0], '?');
                            mainLibraryItem = $"{folderPrefix}/{rootName}";
                        }

                        if (!libraryItemCount.ContainsKey(mainLibraryItem))
                        {
                            libraryItemCount.Add(mainLibraryItem, 1);
                            layer.Name = mainLibraryItem;
                        }
                        else
                        {
                            libraryItemCount[mainLibraryItem]++;
                            layer.Name = $"{mainLibraryItem} {libraryItemCount[mainLibraryItem]}";
                        }
                    }
                }
                renameLayers.AddOne();
            }

            // Save document
            var writeFiles = new ProgressChecker("Writing back files...", SymbolList.Count);
            for (int i = 0; i < SymbolList.Count; i++)
            {
                XmlMethods.SaveXmlDocument(AllSymbolPaths[i], SymbolList[i], SymbolItem.serializer);
                writeFiles.AddOne();
            }
        }

        private static int AskForRenameMethod()
        {
            UM.PrintColoredText([
                (ConsoleColor.DarkCyan, "How do you want to rename the layers?\n"),
                (ConsoleColor.Green, "[1]"),
                (ConsoleColor.White, " - "),
                (ConsoleColor.DarkCyan, "Rename by number, top layers first\n"),
                (ConsoleColor.Green, "[2]"),
                (ConsoleColor.White, " - "),
                (ConsoleColor.DarkCyan, "Rename by number, bottom layers first\n"),
                (ConsoleColor.Green, "[3]"),
                (ConsoleColor.White, " - "),
                (ConsoleColor.DarkCyan, "Rename by used sprite\n")
            ]);
            return UserPrompts.AskForInt(1, 3);
        }
        private static (List<string> SymbolPathList, List<SymbolItem> SymbolList) AskForSymbolItem()
        {
            while (true)
            {
                // Get input from user
                var (pathInput, isSymbol) = UserPrompts.AskForPath(["DOMDocument.xml"]);

                // If directory is a folder, check the contents to see if it is an xfl
                List<string>? symbolDirectories;
                Console.ForegroundColor = ConsoleColor.Red;
                if (!isSymbol)
                {
                    symbolDirectories = XFL.GetAllSymbolDirectories(pathInput);
                    pathInput = UserPrompts.OverwriteXFLPrompt(pathInput);
                }
                else
                {
                    symbolDirectories = [pathInput];
                }

                // Open document to check inside, check for errors while at it
                List<string> symbolPaths = [];
                List<SymbolItem> symbolList = [];

                var processSymbols = new ProgressChecker("Processing symbols... ", symbolDirectories.Count);
                Console.ForegroundColor = ConsoleColor.Red;

                foreach (string symbolPath in symbolDirectories)
                {
                    SymbolItem symbol;
                    try
                    {
                        symbol = UM.GetSymbol(symbolPath);
                    }
                    catch (System.Xml.XmlException)
                    {
                        Console.WriteLine($"The symbol {Path.GetFileName(symbolPath)} is not valid, will be ignored");
                        processSymbols.AddOne();
                        continue;
                    }

                    symbolPaths.Add(symbolPath);
                    symbolList.Add(symbol);

                    processSymbols.AddOne();
                }

                // Return
                var toReturn = (symbolPaths, symbolList);
                return toReturn;
            }
        }
    }
}