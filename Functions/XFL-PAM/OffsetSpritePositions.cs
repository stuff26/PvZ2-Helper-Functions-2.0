using System.Xml.Linq;
using XflComponents;
using UniversalMethods;

namespace HelperFunctions.Functions.Packages
{
    public class OffsetSpritePositions
    {
        public static void Function()
        {
            // Introduction, ask for necessary details from user
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter how much you want to shift the X coordinate by");
            double xChange = UM.AskForDouble();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter how much you want to shift the Y coordinate by");
            double yChange = UM.AskForDouble();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter an XFL or an individual sprite");
            var result = AskForSymbolItem();


            // Process results
            List<string> AllSymbolPaths = result.SymbolPathList;
            List<SymbolItem> SymbolList = result.SymbolList;

            Console.ForegroundColor = ConsoleColor.Green;
            string prefix = "Editing symbols... ";
            ProgressChecker? editSymbols = null;
            if (SymbolList.Count > 1)
            {
                editSymbols = new ProgressChecker(prefix, SymbolList.Count);
            }
            else
            {
                Console.Write(prefix);
            }

            // Loop through, edit positions
            foreach (SymbolItem symbol in SymbolList)
            {
                foreach (FrameElements element in symbol.Timeline!.GetAllElements())
                {
                    element.EditPositions(xChange, yChange);
                }
                editSymbols?.AddOne();
            }
            if (editSymbols is null)
            {
                ProgressChecker.WriteFinished();
            }

            // Save document
            Console.ForegroundColor = ConsoleColor.Green;
            prefix = "Writing back files... ";
            ProgressChecker? writeSymbols = null;
            if (editSymbols is not null)
                writeSymbols = new ProgressChecker(prefix, SymbolList.Count);
            else
                Console.Write(prefix);
            for (int i = 0; i < SymbolList.Count; i++)
            {
                UM.SaveXmlDocument(AllSymbolPaths[i], SymbolList[i], UM.DummyXDocument, SymbolItem.serializer);
                writeSymbols?.AddOne();
            }
            if (writeSymbols is null)
                ProgressChecker.WriteFinished();
        }


        private static (List<string> SymbolPathList, List<SymbolItem> SymbolList) AskForSymbolItem()
        {
            while (true)
            {
                // Get input from user
                var (pathInput, isFile) = UM.AskForPath(["DOMDocument.xml"]);
                Console.ForegroundColor = ConsoleColor.Red;

                // If directory is a folder, check the contents to see if it is an xfl
                List<string>? SymbolDirectories;
                if (!isFile)
                {
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

                ProgressChecker? retrieveSymbols = null;
                if (SymbolDirectories.Count > 1)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    retrieveSymbols = new("Loading symbols... ", SymbolDirectories.Count);
                }

                foreach (string symbolPath in SymbolDirectories)
                {
                    SymbolItem? symbol;
                    
                    XDocument symbolDocument = XDocument.Load(symbolPath);
                    using var documentReader = symbolDocument.CreateReader();
                    symbol = (SymbolItem?)SymbolItem.serializer.Deserialize(documentReader);

                    AllSymbolPaths.Add(symbolPath);
                    SymbolList.Add(symbol!);
                    retrieveSymbols?.AddOne();
                }

                // Return
                retrieveSymbols?.FixCursorPosition();
                var toReturn = (AllSymbolPaths, SymbolList);
                return toReturn;
            }
        }
    }
}