using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using System.Xml.Serialization;
using XflComponents;

namespace UniversalMethods
{
    public static class UM
    {
        public readonly static XDocument DummyXDocument = new();

        /// <summary>
        /// Check if a JSON file is a valid JSON
        /// </summary>
        /// <param name="pathName">Path to file</param>
        /// <returns>True if the file is found and is a valid JSON, othewise false</returns>
        public static bool CheckJsonValid(string pathName)
        {
            try
            {
                string fileText = File.ReadAllText(pathName);
                JsonNode.Parse(fileText);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Get a JSON Node form of a JSON file
        /// </summary>
        /// <param name="filePath">Path to the JSON file</param>
        /// <returns>The JSON node equivalent of a file, returns null if no valid file is found</returns>
        public static JsonNode? GetJsonFile(string filePath)
        {
            JsonNode? jsonFile;
            if (!File.Exists(filePath))
            {
                jsonFile = null;
            }
            else
            {
                string rawFileText = File.ReadAllText(filePath);
                jsonFile = JsonNode.Parse(rawFileText);
                try
                {
                    jsonFile?.AsObject().IndexOf("");
                }
                catch (ArgumentException)
                {
                    jsonFile = null;
                }
            }
            return jsonFile;
        }

        public static JsonNode? ReadFileJson(string jsonText)
        {
            try
            {
                var jsonFile = JsonNode.Parse(jsonText);
                return jsonFile;
            }
            catch
            {
                return null;
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
                string? userInput = Console.ReadLine();
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
                if (!CheckJsonValid(userInput!))
                {
                    Console.WriteLine("Could not read JSON file, enter again");
                    continue;
                }
                Console.ForegroundColor = ConsoleColor.White;
                return (GetJsonFile(userInput)!, userInput);
            }
        }

        /// <summary>
        /// Convert then write a JSON file to a specified location
        /// </summary>
        /// <param name="filePath">Path to write to</param>
        /// <param name="jsonFile">The JSON that will be written</param>
        /// <param name="isIndented">Determine if the file should be indented or not, true by default</param>
        public static void WriteJsonFile(string filePath, JsonNode jsonFile, bool isIndented = true)
        {
            string fileText = jsonFile.ToJsonString(new JsonSerializerOptions { WriteIndented = isIndented });
            File.WriteAllText(filePath, fileText);
        }

        /// <summary>
        /// Determine if a set of files exist in a directory
        /// </summary>
        /// <param name="checkingFiles">Set of files to try to find in the directory</param>
        /// <param name="basePath">Folder to check in</param>
        /// <returns>A value tuple that consists of a bool that is true if no missing files are found and a string list of missing files</returns>
        public static (bool exists, List<string> missingFiles) FilesExist(string[] checkingFiles, string basePath)
        {
            List<string> missingFiles = [];
            bool exists = true;
            foreach (string file in checkingFiles)
            {
                string filePath = Path.Join(basePath, file);
                if (!File.Exists(filePath) || !CheckJsonValid(filePath))
                {
                    missingFiles.Add(file);
                    exists = false;
                }
            }

            return (exists, missingFiles);
        }

        /// <summary>
        /// Make a JSON array with filler JSON nodes
        /// </summary>
        /// <param name="length">Length of returned JSON array</param>
        /// <returns>A JSON array of specified length</returns>
        public static JsonNode?[] MakeDummyJsonArr(int length)
        {
            List<JsonNode?> dummyList = [];
            JsonNode dummyJson = JsonNode.Parse("{\"test\": 1}")!;
            for (int i = 0; i < length; i++)
            {
                dummyList.Add(dummyJson);
            }
            return dummyList.ToArray();
        }

        /// <summary>
        /// Write an XML document in a specified location
        /// </summary>
        /// <param name="documentPath">Path to save the XML to</param>
        /// <param name="newDocument">Object that should be serialized into an XML</param>
        /// <param name="originalDocument">XDocument to use when serializing</param>
        /// <param name="serializer">Serializer to determine what type of object should be serialized</param>
        public static void SaveXmlDocument(string documentPath, object newDocument, XDocument originalDocument, XmlSerializer serializer)
        {

            string newXml;
            using (var sw = new StringWriter())
            {
                serializer.Serialize(sw, newDocument);
                newXml = sw.ToString();
            }
            XDocument updatedDocument = XDocument.Parse(newXml);
            originalDocument.Root?.ReplaceWith(updatedDocument.Root);

            File.WriteAllText(documentPath, updatedDocument.ToString());
            //originalDocument.Save(documentPath);
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
                    Console.WriteLine("Please enter a number");
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
                    Console.WriteLine("Please enter a number");
                    continue;
                }

                if (double.TryParse(userInput, out double toReturnDouble))
                {
                    if (toReturnDouble < min || toReturnDouble > max)
                    {
                        if (max != double.MaxValue)
                            Console.WriteLine($"Please enter a number within {min} and {max}");
                        else
                            Console.WriteLine($"Please enter a number of at least {min}");
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
                    Console.WriteLine("Please enter a number");
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
        public static int AskForInt(int min, int max = int.MaxValue)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                string? userInput = Console.ReadLine()?.Trim();
                Console.ForegroundColor = ConsoleColor.Red;
                if (string.IsNullOrEmpty(userInput))
                {
                    Console.WriteLine("Please enter a number");
                    continue;
                }

                if (int.TryParse(userInput, out int toReturnInt))
                {
                    if (toReturnInt < min || toReturnInt > max)
                    {
                        if (max != int.MaxValue)
                            Console.WriteLine($"Please enter a number within {min} and {max}");
                        else
                            Console.WriteLine($"Please enter a number of at least {min}");
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
        /// Ask the user for a path to a symbol item
        /// </summary>
        /// <returns>A value tuple of the XDocument, path to symbol item, and object symbol item</returns>
        public static (XDocument document, string documentPath, SymbolItem symbol) AskForSymbolItem()
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
                var toReturn = (document, pathInput, symbol);
                return toReturn;
            }
        }

        /// <summary>
        /// Convert a list to a string for testing
        /// </summary>
        /// <param name="input">List to convert</param>
        /// <returns>A string representation of an array</returns>
        public static string ArrayToString(List<object> input)
        {
            string currentString = "[";
            foreach (object currentObject in input)
            {
                currentString += currentObject.ToString() + ", ";
            }
            currentString = currentString[..^1] + "]";
            return currentString;
        }

        /// <summary>
        /// Convert an array to a string for testing
        /// </summary>
        /// <param name="input">Array to convert</param>
        /// <returns>A string representation of an array</returns>
        public static string ArrayToString(object[] input)
        {
            string currentString = "[";
            foreach (object currentObject in input)
            {
                currentString += currentObject.ToString() + ", ";
            }
            currentString = currentString[..^1] + "]";
            return currentString;
        }

        /// <summary>
        /// Convert a list to a string for testing
        /// </summary>
        /// <param name="input">List to convert</param>
        /// <returns>A string representation of an array</returns>
        public static string ArrayToString(List<string> input)
        {
            string currentString = "[ ";
            foreach (object currentObject in input)
            {
                currentString += currentObject.ToString() + ", ";
            }
            currentString = currentString[..^1] + "]";
            return currentString;
        }

        /// <summary>
        /// Convert an array to a string for testing
        /// </summary>
        /// <param name="input">Array to convert</param>
        /// <returns>A string representation of an array</returns>
        public static string ArrayToString(string[] input)
        {
            string currentString = "[ ";
            foreach (object currentObject in input)
            {
                currentString += currentObject.ToString() + ", ";
            }
            currentString = currentString[..^1] + "]";
            return currentString;
        }

        /// <summary>
        /// Convert a list to a string for testing
        /// </summary>
        /// <param name="input">List to convert</param>
        /// <returns>A string representation of an array</returns>
        public static string ArrayToString(List<int> input)
        {
            string currentString = "[";
            foreach (int currentObject in input)
            {
                currentString += currentObject + ", ";
            }
            currentString = currentString[..^2] + "]";
            return currentString;
        }

        /// <summary>
        /// Turn a list of integers into a set of values
        /// </summary>
        /// <param name="values">List of integers to convert</param>
        /// <returns>List that contains each value pair</returns>
        public static List<List<int>> TurnIntoValueRange(List<int> values)
        {
            List<List<int>> ValueRange = [];
            if (values.Count == 0) return ValueRange;
            values.Sort();

            int lastValue = -1;
            List<int> currentValueRange = [];
            foreach (int num in values)
            {
                if (lastValue == -1)
                {
                    currentValueRange.Add(num);
                }
                else if (num != lastValue + 1)
                {
                    currentValueRange.Add(lastValue);
                    ValueRange.Add(currentValueRange);
                    currentValueRange = [num];
                }
                lastValue = num;
            }

            currentValueRange.Add(lastValue);
            ValueRange.Add(currentValueRange);
            return ValueRange;
        }

        /// <summary>
        /// Remove the reference from a property line (ex Plant@PlantProperties)
        /// </summary>
        /// <param name="reference">Property line to convert</param>
        /// <returns>Property line without reference</returns>
        public static string RemoveReference(string reference)
        {
            return reference.Replace("RTID(", "").Replace(")", "").Split("@")[0];
        }

        /// <summary>
        /// Get a list of keys that a JsonNode has
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<string> GetKeysFromJsonNode(JsonNode? input)
        {
            if (input is null) return [];
            return input.AsObject().Select(kvp => kvp.Key).ToList();
        }

        /// <summary>
        /// Ask for a directory from the user and ensure the directory exists
        /// </summary>
        /// <param name="wantedFiles">Set of files that should be checked to find</param>
        /// <returns>The directory entered by the user</returns>
        public static string AskForDirectory(string[]? wantedFiles = null, string[]? wantedDirs = null)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                var userInput = Console.ReadLine()?.Replace("\"", "");
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
                var userInput = Console.ReadLine()?.Trim().Replace("\"", "");
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
        /// Make a deepcopy of an object
        /// </summary>
        /// <typeparam name="T">Object type of the copied object</typeparam>
        /// <param name="toCopy">Object to copy</param>
        /// <returns>A deepcopy of an object</returns>
        /// <exception cref="ArgumentNullException">If toCopy is a null object</exception>
        public static T MakeDeepCopy<T>(T toCopy)
        {
            if (toCopy == null)
                throw new ArgumentNullException(nameof(toCopy));

            var serializer = new XmlSerializer(typeof(T));
            using (var ms = new MemoryStream())
            {
                serializer.Serialize(ms, toCopy);
                ms.Position = 0;
                return (T)serializer.Deserialize(ms)!;
            }
        }

        /// <summary>
        /// Get all of the symbols found in the DOMDocument
        /// </summary>
        /// <param name="xflPath">Path to XFL that contains DOMDocument</param>
        /// <returns>A string list of every symbol file found in DOMDocument</returns>
        public static List<string> GetAllSymbolDirectories(string xflPath)
        {
            string domdocumentPath = Path.Join(xflPath, "DOMDocument.xml");
            XDocument document = XDocument.Load(domdocumentPath);
            using var documentReader = document.CreateReader();
            DOMDocument? DOMDocumentObject = (DOMDocument?)DOMDocument.serializer.Deserialize(documentReader);
            if (DOMDocumentObject is null) return [];

            List<string> SymbolDirectories = DOMDocumentObject.GetAllSymbolNames();
            for (int i = 0; i < SymbolDirectories.Count; i++)
            {
                string dir = SymbolDirectories[i];
                dir = Path.Join(xflPath, "library", dir);
                SymbolDirectories[i] = dir;
            }

            return SymbolDirectories;
        }

        public static bool DictionaryContainsOnlyEmptyLists(Dictionary<string, Dictionary<string, string>> input)
        {
            foreach (var value in input)
            {
                if (value.Value.Count > 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Remove the contents of a folder
        /// </summary>
        /// <param name="folderToRemove">Folder contents to remove</param>
        /// <param name="removedir">If true, the folder will be deleted, otherwise the folder will stay and be empty</param>
        public static void EmptyFolder(string folderToRemove, bool removedir)
        {
            var files = Directory.GetFiles(folderToRemove);
            foreach (var file in files)
            {
                File.Delete(file);
            }
            if (removedir) Directory.Delete(folderToRemove);
        }

        /// <summary>
        /// Copy the contents of one folder to another and recursively copy over its nested files and folders
        /// </summary>
        /// <param name="oldFolder">Folder that will be copied from</param>
        /// <param name="newFolder">Folder that will be deleted and then copied to</param>
        public static void CopyFolder(string oldFolder, string newFolder)
        {
            if (Directory.Exists(newFolder))
                EmptyFolder(newFolder, false);
            else
                Directory.CreateDirectory(newFolder);

            var filesToCopy = Directory.GetFiles(oldFolder);
            foreach (var file in filesToCopy)
            {
                var baseFileName = Path.GetFileName(file);
                var newFilePath = Path.Join(newFolder, baseFileName);
                File.Copy(file, newFilePath);
            }

            var directoriesToCopy = Directory.GetDirectories(oldFolder);
            foreach (var dir in directoriesToCopy)
            {
                var baseDir = dir.Split("\\")[^1];
                var newNestedFolder = Path.Join(newFolder, baseDir);
                CopyFolder(dir, newNestedFolder);
            }
        }

        public static void RenameFile(string file, string newName)
        {
            var parentDir = Path.GetDirectoryName(file);
            var newPath = Path.Join(parentDir, newName);
            File.Move(file, newPath);
        }
    }
}