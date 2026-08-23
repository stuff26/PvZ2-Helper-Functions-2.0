using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using XflComponents;

namespace UniversalMethods
{
    public static class UserPrompts
    {
        private static readonly HashSet<char?> AcceptYes = ['y', 'Y', '1'];
        private static readonly HashSet<char?> AcceptNo = ['n', 'N', '0'];
        /// <summary>
        /// Ask the user for yes or no (Y or N, or other acceptable equivalents from 'AcceptYes' and 'AcceptNo') safely and return result
        /// </summary>
        /// <returns>True if the user answers with Y, false if the user answers with N</returns>
        public static bool AskYesOrNo()
        {
            while (true)
            {
                var userInput = Console.ReadLine()?[0];
                if (AcceptYes.Contains(userInput)) return true;
                if (AcceptNo.Contains(userInput)) return false;
                else UM.PrintColoredText(ConsoleColor.Red, "Could not read input, enter again", separateLines:true);
            }
        }

        /// <summary>
        /// Ask the user for any double and check through to make sure the input is a double
        /// </summary>
        /// <returns>A double that user provides</returns>
        public static double AskForDouble()
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                string? userInput = Console.ReadLine()?.Trim();
                Console.ForegroundColor = ConsoleColor.Red;
                if (string.IsNullOrEmpty(userInput))
                {
                    Console.WriteLine("Enter a number");
                    continue;
                }

                if (double.TryParse(userInput, out double toReturnDouble))
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    return toReturnDouble;
                }
                else
                {
                    Console.WriteLine("Enter a number without additional text");
                    continue;
                }
            }
        }

        /// <summary>
        /// Ask for a double between certain values
        /// </summary>
        /// <param name="min">Minimum value that should be provided by the user, no lower bound exists if not specified</param>
        /// <param name="max">Maximum value that should be provided by the user, no upper bound exists if not specified</param>
        /// <returns></returns>
        public static double AskForDouble(double min = double.MinValue, double max = double.MaxValue)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                string? userInput = Console.ReadLine()?.Trim();
                Console.ForegroundColor = ConsoleColor.Red;
                if (string.IsNullOrEmpty(userInput))
                {
                    Console.WriteLine("Enter a number");
                    continue;
                }

                if (double.TryParse(userInput, out double toReturnDouble))
                {
                    if (toReturnDouble < min || toReturnDouble > max)
                    {
                        if (max != double.MaxValue)
                            Console.WriteLine($"Enter a number within {min} and {max}");
                        else
                            Console.WriteLine($"Enter a number of at least {min}");
                        continue;
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    return toReturnDouble;
                }

                else
                {
                    Console.WriteLine("Enter a number without additional text");
                    continue;
                }
            }
        }

        /// <summary>
        /// Ask the user for any int and check through to make sure the input is a int
        /// </summary>
        /// <returns>An int that user provides</returns>
        public static int AskForInt()
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                string? userInput = Console.ReadLine()?.Trim();
                Console.ForegroundColor = ConsoleColor.Red;
                if (string.IsNullOrEmpty(userInput))
                {
                    Console.WriteLine("Enter a number");
                    continue;
                }

                if (int.TryParse(userInput, out int toReturnInt))
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    return toReturnInt;
                }
                else
                {
                    Console.WriteLine("Enter an integer without additional text");
                    continue;
                }
            }
        }

        /// <summary>
        /// Ask for a int between certain values
        /// </summary>
        /// <param name="min">Minimum value that should be provided by the user, no lower bound exists if not specified</param>
        /// <param name="max">Maximum value that should be provided by the user, no upper bound exists if not specified</param>
        /// <returns></returns>
        public static int AskForInt(int min, int max = int.MaxValue, bool receiveNoInput = false, int? noInput = null)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                string? userInput = Console.ReadLine()?.Trim();
                if (receiveNoInput && userInput == string.Empty)
                    if (noInput is null) return min - 1;
                    else return (int)noInput;

                Console.ForegroundColor = ConsoleColor.Red;
                if (string.IsNullOrEmpty(userInput))
                {
                    Console.WriteLine("Enter a number");
                    continue;
                }

                if (int.TryParse(userInput, out int toReturnInt))
                {
                    if (toReturnInt < min || toReturnInt > max)
                    {
                        if (max != int.MaxValue)
                            Console.WriteLine($"Enter a number within {min} and {max}");
                        else
                            Console.WriteLine($"Enter a number of at least {min}");
                        continue;
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    return toReturnInt;
                }

                else
                {
                    Console.WriteLine("Enter a number without additional text");
                    continue;
                }
            }
        }

        /// <summary>
        /// Ask the user for a path to a DOMDocument
        /// </summary>
        /// <returns>A value tuple of the XDocument, path to DOMDocument, and object DOMDocument</returns>
        public static (XDocument document, string documentPath, DOMDocument domdocument) AskForDomDocument()
        {
            while (true)
            {
                // Get input from user
                Console.ForegroundColor = ConsoleColor.Magenta;
                string? pathInput = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Red;

                // Check if pathInput is invalid
                if (string.IsNullOrWhiteSpace(pathInput) || pathInput is null)
                {
                    Console.WriteLine("Enter a directory");
                    continue;
                }

                // Check if the file that pathInput directs to exists
                if (!File.Exists(pathInput))
                {
                    Console.WriteLine($"Could not find {pathInput}, enter again");
                    continue;
                }

                // Open document to check inside, check for errors while at it
                XDocument document;
                DOMDocument? symbol;
                try
                {
                    document = XDocument.Load(pathInput);
                    using var documentReader = document.CreateReader();
                    symbol = (DOMDocument?)DOMDocument.serializer.Deserialize(documentReader);
                }
                catch (System.Xml.XmlException)
                {
                    Console.WriteLine("The XML document doesn't seem to be valid, enter again");
                    continue;
                }

                // Check if the symbol itself and the timeline is null
                if (symbol is null || symbol.Timeline is null)
                {
                    Console.WriteLine("Could not find properly elements in XML, enter again");
                    continue;
                }

                // Return
                Console.ForegroundColor = ConsoleColor.White;
                var toReturn = (document, pathInput, symbol);
                return toReturn;
            }
        }
        

        /// <summary>
        /// Ask for a directory from the user and ensure the directory exists
        /// </summary>
        /// <param name="wantedFiles">Set of files that should be checked to find</param>
        /// <returns>The directory entered by the user</returns>
        public static string AskForDirectory(string[]? wantedFiles = null, string[]? wantedDirs = null, bool allowNoAnswer = false)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                var userInput = Console.ReadLine()?.Trim().Replace("\"", string.Empty);
                if (allowNoAnswer && userInput == string.Empty)
                {
                    return string.Empty;
                }
                Console.ForegroundColor = ConsoleColor.Red;
                if (string.IsNullOrWhiteSpace(userInput))
                {
                    Console.WriteLine("Enter a directory");
                    continue;
                }
                if (File.Exists(userInput))
                {
                    Console.WriteLine("Entered path is a file instead of a directory, enter again");
                    continue;
                }
                if (!Directory.Exists(userInput))
                {
                    Console.WriteLine("Entered path could not be found, enter again");
                    continue;
                }

                if (wantedFiles is not null)
                {
                    var foundFiles = Directory.GetFiles(userInput, "*.*", SearchOption.TopDirectoryOnly).ToList();
                    bool didFindFiles = true;
                    var missingFiles = new List<string>();
                    foreach (var wantedFile in wantedFiles)
                    {
                        var fullPath = Path.Join(userInput, wantedFile);
                        if (!foundFiles.Any(s => s.Equals(fullPath, StringComparison.OrdinalIgnoreCase)))
                        {
                            if (didFindFiles) didFindFiles = false;
                            missingFiles.Add(wantedFile);
                        }
                    }
                    if (!didFindFiles)
                    {
                        Console.WriteLine("Could not find the following files in the directory, enter again");
                        foreach (var missingFile in missingFiles)
                        {
                            Console.WriteLine(missingFile);
                        }
                        continue;
                    }
                }
                if (wantedDirs is not null)
                {
                    var foundDirs = Directory.GetDirectories(userInput).ToList();
                    bool didFindDirs = true;
                    var missingDirs = new List<string>();
                    foreach (var wantedDir in wantedDirs)
                    {
                        var fullPath = Path.Join(userInput, wantedDir);
                        if (!foundDirs.Any(s => s.Equals(fullPath, StringComparison.OrdinalIgnoreCase)))
                        {
                            if (didFindDirs) didFindDirs = false;
                            missingDirs.Add(wantedDir);
                        }
                    }
                    if (!didFindDirs)
                    {
                        Console.WriteLine("Could not find the following directories in the directory, enter again");
                        foreach (var missingDir in missingDirs)
                        {
                            Console.WriteLine(missingDir);
                        }
                        continue;
                    }
                }

                Console.ForegroundColor = ConsoleColor.White;
                return userInput;
            }
        }

        public static (string path, bool isFile) AskForPath(string[]? wantedFiles = null)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                var userInput = Console.ReadLine()?.Trim().Replace("\"", string.Empty);
                Console.ForegroundColor = ConsoleColor.Red;
                if (string.IsNullOrEmpty(userInput))
                {
                    Console.WriteLine("Enter a directory or file");
                    continue;
                }

                if (File.Exists(userInput))
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    return (userInput, true);
                }
                else if (Directory.Exists(userInput))
                {
                    if (wantedFiles is not null)
                    {
                        var existingFiles = Directory.GetFiles(userInput, "*.*", SearchOption.TopDirectoryOnly).ToList();
                        bool missingFiles = false;
                        int i = 0;
                        foreach (var wantedFile in wantedFiles)
                        {
                            var fullPath = Path.Join(userInput, wantedFile);
                            if (!existingFiles.Contains(fullPath))
                            {
                                missingFiles = true;
                                break;
                            }
                            i++;
                        }
                        if (missingFiles)
                        {
                            Console.WriteLine($"Could not find file {wantedFiles[i]}, enter again");
                            continue;
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    return (userInput, false);
                }
                else
                {
                    Console.WriteLine("Could not find file or directory, enter again");
                }
            }
        }

        /// <summary>
        /// Ask the user for a path to a symbol item
        /// </summary>
        /// <returns>A value tuple of the XDocument, path to symbol item, and object symbol item</returns>
        public static (string documentPath, SymbolItem symbol) AskForSymbolItem()
        {
            while (true)
            {
                // Get input from user
                Console.ForegroundColor = ConsoleColor.Magenta;
                string? pathInput = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Red;

                // Check if pathInput is invalid
                if (string.IsNullOrWhiteSpace(pathInput) || pathInput is null)
                {
                    Console.WriteLine("Enter a directory");
                    continue;
                }

                // Check if the file that pathInput directs to exists
                if (!File.Exists(pathInput))
                {
                    Console.WriteLine($"Could not find {pathInput}, enter again");
                    continue;
                }

                // Open document to check inside, check for errors while at it
                XDocument document;
                SymbolItem? symbol;
                try
                {
                    document = XDocument.Load(pathInput);
                    using var documentReader = document.CreateReader();
                    symbol = (SymbolItem?)SymbolItem.serializer.Deserialize(documentReader);
                }
                catch (System.Xml.XmlException)
                {
                    Console.WriteLine("The XML document doesn't seem to be valid, enter again");
                    continue;
                }

                // Check if the symbol itself and the timeline is null
                if (symbol is null || symbol.Timeline is null)
                {
                    Console.WriteLine("Could not find properly elements in XML, enter again");
                    continue;
                }

                // Return
                Console.ForegroundColor = ConsoleColor.White;
                var toReturn = (pathInput, symbol);
                return toReturn;
            }
        }
        
        /// <summary>
        /// Ask the user for a JSON file directory and get the JSON and the path to it
        /// </summary>
        /// <returns>A value tuple with the JSON node and the path</returns>
        public static (JsonNode jsonFile, string path) AskForJsonFile()
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                string? userInput = Console.ReadLine()?.Trim().Replace("\"", string.Empty);
                Console.ForegroundColor = ConsoleColor.Red;
                if (string.IsNullOrWhiteSpace(userInput))
                {
                    Console.WriteLine("Enter a directory");
                    continue;
                }
                if (Directory.Exists(userInput))
                {
                    Console.WriteLine("Enter a file instead of a folder, enter again");
                    continue;
                }
                if (!JsonMethods.CheckJsonValid(userInput!))
                {
                    Console.WriteLine("Could not read JSON file, enter again");
                    continue;
                }
                Console.ForegroundColor = ConsoleColor.White;
                return (JsonMethods.GetJsonFile(userInput)!, userInput);
            }
        }

        /// <summary>
        /// Ask the user for a JSON file directory and get the JSON and the path to it
        /// </summary>
        /// <returns>A value tuple with the JSON node and the path</returns>
        public static (JsonDocument jsonFile, string path) AskForJsonDocumentFile()
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                string? userInput = Console.ReadLine()?.Trim().Replace("\"", string.Empty);
                Console.ForegroundColor = ConsoleColor.Red;
                if (string.IsNullOrWhiteSpace(userInput))
                {
                    Console.WriteLine("Enter a directory");
                    continue;
                }
                if (Directory.Exists(userInput))
                {
                    Console.WriteLine("Enter a file instead of a folder, enter again");
                    continue;
                }
                if (!JsonMethods.CheckJsonValid(userInput!))
                {
                    Console.WriteLine("Could not read JSON file, enter again");
                    continue;
                }
                Console.ForegroundColor = ConsoleColor.White;
                return (JsonMethods.GetJsonDocmentFile(userInput)!, userInput);
            }
        }
        
        public static string OverwriteXFLPrompt(string xflPath)
        {
            UM.PrintColoredText([
                (ConsoleColor.DarkCyan, "Do you wish to overwrite the existing XFL or make a modified copy?\n"),
                (ConsoleColor.Green, "[1]"),
                (ConsoleColor.White, " - "),
                (ConsoleColor.DarkCyan, "Overwrite XFL "),
                (ConsoleColor.Red, "(Only use if you are sure there are no potential errors)\n"),
                (ConsoleColor.Green, "[2]"),
                (ConsoleColor.White, " - "),
                (ConsoleColor.DarkCyan, "Make a modified copy\n"),
            ]);
            
            int userInput = UserPrompts.AskForInt(1, 2);
            if (userInput == 1) return xflPath;

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter the name of the folder the XFL copy should use (will be put in the same parent directory as original XFL)");
            string? inputPath;
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                inputPath = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Red;
                if (string.IsNullOrEmpty(inputPath))
                {
                    Console.WriteLine("Enter a folder name");
                    continue;
                }
                break;
            }
            var parentFolder = Path.GetDirectoryName(xflPath);
            var copyXflFolder = Path.Join(parentFolder, inputPath);
            FileManagement.CopyFolder(xflPath, copyXflFolder);
            return copyXflFolder;
        }
    }
}