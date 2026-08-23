using XflComponents;
using UniversalMethods;

namespace HelperFunctions.Functions.Packages
{
    public static class RemoveEmptyLayers
    {
        public static void Function()
        {
            UM.PrintColoredText(ConsoleColor.DarkCyan, "Enter an XFL or symbol you want to edit", separateLines:true);
            var symbols = GetSymbols();
            int symbolCount = symbols.Count;
            if (symbols.Count == 0)
            {
                UM.PrintColoredText(ConsoleColor.Red, "No symbols found");
                return;
            }

            var editedLayers = new ProgressChecker("Editing Symbols... ", symbolCount);

            foreach (var (symbol, path) in symbols)
            {
                symbol.Timeline.RemoveEmptyLayers();
                symbol.Timeline.RemoveTrailingFrames();
                XmlMethods.SaveXmlDocument(path, symbol, SymbolItem.serializer);
                editedLayers.AddOne();
            }
        }
        
        private static List<(SymbolItem symbol, string path)> GetSymbols()
        {
            XFLInitOptions options = new()
            {
                GetSymbols = true,
                GetBitmaps = false,
                CheckProgress = true
            };
            while (true)
            {
                var (path, isFile) = UserPrompts.AskForPath(["DOMDocument.xml"]);
                if (isFile)
                {
                    return [(UM.GetSymbol(path), path)];
                }
                else
                {
                    var xfl = XFL.GetXFLSafely(path, options);
                    if (xfl is null) continue;

                    return xfl.Symbols.Select(s => (s, Path.Join(path,
                                                       XFL.LibraryDirName,
                                                       Path.ChangeExtension(s.Name, ".xml"))))
                                                       .ToList();
                }
            }
        }
    }
}