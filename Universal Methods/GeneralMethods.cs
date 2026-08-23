using System.Xml.Linq;
using System.Xml.Serialization;
using XflComponents;

namespace UniversalMethods
{
    public static class UM
    {
        public static void PrintColoredText(ConsoleColor color, string text, bool separateLines = false)
        {
            Console.ForegroundColor = color;
            if (separateLines) Console.WriteLine(text);
            else Console.Write(text);
        }

        public static void PrintColoredText(List<(ConsoleColor color, string text)> colorTextDict, bool separateLines = false)
        {
            foreach (var entry in colorTextDict)
            {
                var (color, text) = entry;
                Console.ForegroundColor = color;
                if (separateLines) Console.WriteLine(text);
                else Console.Write(text);
            }
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
                if (!File.Exists(filePath))
                {
                    missingFiles.Add(file);
                    exists = false;
                }
            }

            return (exists, missingFiles);
        }

        /// <summary>
        /// Converts an enumerable object into a readable string with contents
        /// </summary>
        /// <typeparam name="T">Object type inside the enumerable</typeparam>
        /// <param name="input">Enumerable to be looped through</param>
        /// <returns>A string with the format of [item1, item2, ...]</returns>
        public static string EnumToString<T>(IEnumerable<T> input)
        {
            var currentString = "[";
            foreach (var item in input)
            {
                currentString += item?.ToString() + ", ";
            }
            return currentString[..^1] + "]";
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
            return reference.Replace("RTID(", string.Empty).Replace(")", string.Empty).Split("@")[0];
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
            var serializer = new XmlSerializer(typeof(T));
            using var ms = new MemoryStream();
            serializer.Serialize(ms, toCopy);
            ms.Position = 0;
            return (T)serializer.Deserialize(ms)!;
        }
        
        /// <summary>
        /// Checks if a list contains duplicate entries
        /// </summary>
        /// <typeparam name="T">Content type of the list</typeparam>
        /// <param name="list">List to scan for duplicates</param>
        /// <returns>True if duplicates are found, otherwise false</returns>
        public static bool HasDuplicates<T>(List<T> list)
        {
            var foundItems = new HashSet<T>();
            return list.Any(item => !foundItems.Add(item));
        }

        public static DOMDocument GetDOMDocument(string DOMDocumentPath)
        {
            XDocument document = XDocument.Load(DOMDocumentPath!);
            using var documentReader = document.CreateReader();
            DOMDocument DOMDocumentObject = (DOMDocument?)DOMDocument.serializer.Deserialize(documentReader)!;
            return DOMDocumentObject;
        }

        public static SymbolItem GetSymbol(string symbolPath)
        {
            XDocument document = XDocument.Load(symbolPath!);
            using var documentReader = document.CreateReader();
            SymbolItem SymbolObject = (SymbolItem?)SymbolItem.serializer.Deserialize(documentReader)!;
            return SymbolObject;
        }
    }
}