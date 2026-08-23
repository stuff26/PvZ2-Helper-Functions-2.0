using XflComponents;
using UniversalMethods;

namespace HelperFunctions.Functions.Packages
{
    public static class OffsetSpritePositions
    {
        public static void Function()
        {
            // Introduction, ask for necessary details from user
            UM.PrintColoredText(ConsoleColor.DarkCyan, "Enter how much you want to shift the X coordinate by", separateLines:true);
            double xChange = UserPrompts.AskForDouble();
            UM.PrintColoredText(ConsoleColor.DarkCyan, "Enter how much you want to shift the Y coordinate by", separateLines:true);
            double yChange = UserPrompts.AskForDouble();
            UM.PrintColoredText(ConsoleColor.DarkCyan, "Enter an XFL or an individual sprite", separateLines:true);
            var (symbolPaths, symbols) = PromptForSymbols();

            string prefix = "Editing symbols... ";
            ProgressChecker? editSymbols = null;
            if (symbols.Count > 1)
            {
                editSymbols = new ProgressChecker(prefix, symbols.Count);
            }
            else
            {
                UM.PrintColoredText(ConsoleColor.Green, prefix);
            }

            // Loop through, edit positions
            foreach (SymbolItem symbol in symbols)
            {
                symbol.Timeline.GetAllElements().ForEach(e => e.EditPositions(xChange, yChange));
                editSymbols?.AddOne();
            }

            if (editSymbols is null)
            {
                ProgressChecker.WriteFinished();
            }

            // Save document
            prefix = "Writing back files... ";
            ProgressChecker? writeSymbols = null;
            if (editSymbols is not null)
            {
                writeSymbols = new ProgressChecker(prefix, symbols.Count);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(prefix);
            }
            for (int i = 0; i < symbols.Count; i++)
            {
                XmlMethods.SaveXmlDocument(symbolPaths[i], symbols[i], SymbolItem.serializer);
                writeSymbols?.AddOne();
            }
            if (writeSymbols is null)
                ProgressChecker.WriteFinished();
        }


        private static (List<string> symbolPaths, List<SymbolItem> symbols) PromptForSymbols()
        {
            XFLInitOptions options = new()
            {
                GetSymbols = true,
                GetBitmaps = false,
                CheckProgress = true
            };

            while (true)
            {
                var (userPath, isFile) = UserPrompts.AskForPath();

                if (!isFile) // If the path is for an XFL
                {
                    var xfl = XFL.GetXFLSafely(userPath, options);
                    if (xfl is null) continue;

                    return (
                        xfl.GetAllSymbolNames().Select(p => Path.Join(xfl.XflPath, XFL.LibraryDirName, Path.ChangeExtension(p, ".xml"))).ToList(),
                        xfl.Symbols
                    );
                }

                else // If the path is for a symbol
                {
                    try
                    {
                        var symbol = UM.GetSymbol(userPath);
                        return (
                        [userPath],
                        [symbol]
                    );
                    }
                    catch (FileNotFoundException)
                    {
                        UM.PrintColoredText(ConsoleColor.Red, "Could not find file, enter again", separateLines:true);
                        continue;
                    }
                    catch (InvalidOperationException)
                    {
                        UM.PrintColoredText(ConsoleColor.Red, "Error reading symbol file, enter again", separateLines:true);
                        continue;
                    }
                }
            }
        }
    }
}