using System.Xml.Linq;
using XflComponents;
using UniversalMethods;

namespace HelperFunctions.Functions.Packages
{
    public class RenameAllLayers
    {
        public static void Function()
        {
            // Introduction, ask for necessary details from user
            int renameMethod = AskForRenameMethod();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter an XFL or an individual sprite");
            var result = AskForSymbolItem();


            // Process results
            List<string> AllSymbolPaths = result.SymbolPathList;
            List<SymbolItem> SymbolList = result.SymbolList;

            // Loop through, edit positions
            Console.ForegroundColor = ConsoleColor.Green;
            string prefix = "Renaming layers... ";
            var renameLayers = new ProgressChecker(prefix, SymbolList.Count);
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
                        currentNum = symbol.Timeline!.GetLayerCount();
                        numToChangeBy = -1;
                    }
                    foreach (AnimateLayer? layer in symbol.Timeline!.Layers!)
                    {
                        if (layer is null || layer.name is null) continue;
                        layer.name = $"{currentNum}";
                        currentNum += numToChangeBy;
                    }
                }
                else if (renameMethod == 3)
                {
                    var LibraryItemCount = new Dictionary<string, int>();
                    foreach (AnimateLayer? layer in symbol.Timeline!.Layers!)
                    {
                        if (layer is null || layer.name is null) continue;
                        string mainLibraryItem = layer.GetMainLibraryItem();
                        if (string.IsNullOrWhiteSpace(mainLibraryItem))
                        {
                            mainLibraryItem = "Empty";
                        }
                        else if (mainLibraryItem.Contains('/'))
                        {
                            string[] splitName = mainLibraryItem.Split("/");
                            string rootName = mainLibraryItem.Split("/")[^1];
                            if (splitName[0] == "sprite")
                            {
                                mainLibraryItem = $"s/{rootName}";
                            }
                            else if (splitName[0] == "image")
                            {
                                mainLibraryItem = $"i/{rootName}";
                            }
                            else if (splitName[0] == "media")
                            {
                                mainLibraryItem = $"m/{rootName}";
                            }
                        }

                        if (!LibraryItemCount.ContainsKey(mainLibraryItem))
                        {
                            LibraryItemCount.Add(mainLibraryItem, 1);
                            layer.name = mainLibraryItem;
                        }
                        else
                        {
                            LibraryItemCount[mainLibraryItem]++;
                            layer.name = $"{mainLibraryItem} {LibraryItemCount[mainLibraryItem]}";
                        }
                    }
                }
                renameLayers.AddOne();
            }

            // Save document
            Console.ForegroundColor = ConsoleColor.Green;
            prefix = "Writing back files... ";
            var writeFiles = new ProgressChecker(prefix, SymbolList.Count);
            for (int i = 0; i < SymbolList.Count; i++)
            {
                UM.SaveXmlDocument(AllSymbolPaths[i], SymbolList[i], UM.DummyXDocument, SymbolItem.serializer);
                writeFiles.AddOne();
            }
        }

        private static int AskForRenameMethod()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("How do you want to rename the layers?");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[1] Rename by number, top layers first");
            Console.WriteLine("[2] Rename by number, bottom layers first");
            Console.WriteLine("[3] Rename by used sprite");
            return UM.AskForInt(1, 3);
        }
        private static (List<string> SymbolPathList, List<SymbolItem> SymbolList) AskForSymbolItem()
        {
            while (true)
            {
                // Get input from user
                var (pathInput, isSymbol) = UM.AskForPath(["DOMDocument.xml"]);

                // If directory is a folder, check the contents to see if it is an xfl
                List<string>? SymbolDirectories;
                Console.ForegroundColor = ConsoleColor.Red;
                if (!isSymbol)
                {
                    var fileList = Directory.GetFiles(pathInput).ToList();
                    if (!fileList.Contains(Path.Join(pathInput, "DOMDocument.xml")))
                    {
                        Console.WriteLine("Folder is not an XFL, enter again");
                        continue;
                    }
                    SymbolDirectories = UM.GetAllSymbolDirectories(pathInput);
                    if (SymbolDirectories is null)
                    {
                        Console.WriteLine("Error reading DOMDocument, could not access symbol list, enter again");
                        continue;
                    }
                }
                else
                {
                    SymbolDirectories = [pathInput];
                }

                // Open document to check inside, check for errors while at it
                List<string> AllSymbolPaths = [];
                List<SymbolItem> SymbolList = [];

                Console.ForegroundColor = ConsoleColor.Green;
                string prefix = "Processing symbols... ";
                var processSymbols = new ProgressChecker(prefix, SymbolDirectories.Count);
                Console.ForegroundColor = ConsoleColor.Red;

                foreach (string symbolPath in SymbolDirectories)
                {
                    XDocument symbolDocument;
                    SymbolItem? symbol;
                    try
                    {
                        symbolDocument = XDocument.Load(symbolPath);
                        using var documentReader = symbolDocument.CreateReader();
                        symbol = (SymbolItem?)SymbolItem.serializer.Deserialize(documentReader);
                        if (symbol is null || symbol.Timeline is null)
                        {
                            throw new System.Xml.XmlException();
                        }
                    }
                    catch (System.Xml.XmlException)
                    {
                        Console.WriteLine($"The symbol {symbolPath} doesn't seem to be valid, will be ignored");
                        continue;
                    }

                    // Check if the symbol itself and the timeline is null
                    if (symbol is null || symbol.Timeline is null)
                    {
                        Console.WriteLine("Could not find properly elements in XML, enter again");
                        continue;
                    }

                    AllSymbolPaths.Add(symbolPath);
                    SymbolList.Add(symbol);

                    processSymbols.AddOne();
                }

                // Return
                var toReturn = (AllSymbolPaths, SymbolList);
                return toReturn;
            }
        }
    }
}