using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using UniversalMethods;

namespace HelperFunctions
{
    public static class Program
    {
        public static void Main()
        {
            Console.Clear();
            UM.PrintColoredText([
                (ConsoleColor.Yellow, "PvZ2 Helper Functions"),
                (ConsoleColor.DarkCyan, " by "),
                (ConsoleColor.Green, "Stuff26\n"),
                (ConsoleColor.DarkCyan, "Version "),
                (ConsoleColor.Green, "2.2\n"),
                (ConsoleColor.DarkCyan, "Intended for usage with files from "),
                (ConsoleColor.Green, "Sen 4.0"),
                (ConsoleColor.DarkCyan, " by "),
                (ConsoleColor.Green, "Haruma"),
                (ConsoleColor.DarkCyan, ", compatible with "),
                (ConsoleColor.Green, "Snowie Lib V2"),
                (ConsoleColor.DarkCyan, " by "),
                (ConsoleColor.Green, "Snowie\n"),
            ]);
            Console.WriteLine();
            PrintDashedLine();

            JsonNode? functionsJson = GetJsonNodeFileInLibrary("HelperFunctions.Functions.json");
            if (functionsJson is null) return;

            var functionsList = DisplayOptions(functionsJson!);
            Console.ForegroundColor = ConsoleColor.White;
            bool isRepeat = false;
            while (true)
            {
                var selectedFunction = AskWhichFunction(functionsList, isRepeat);
                if (selectedFunction is null) return;

                var method = selectedFunction.GetMethod("Function");
                PrintDashedLine();
                while (true)
                {
                    try
                    {
                        method!.Invoke(null, null);
                        break;
                    }
                    catch (Exception e)
                    {
                        UM.PrintColoredText([
                            (ConsoleColor.Red, $"ERROR: {e.GetBaseException()}\n"),
                            (ConsoleColor.DarkCyan, "Would you like to try this again?"),
                            (ConsoleColor.Yellow, " (Y/N)\n")
                            ]);
                    }
                    var tryAgain = UserPrompts.AskYesOrNo();
                    if (!tryAgain)
                    {
                        break;
                    }
                    PrintDashedLine();
                }
                UM.PrintColoredText(ConsoleColor.Yellow, "Finished", separateLines:true);
                PrintDashedLine();
                UM.PrintColoredText(ConsoleColor.DarkCyan, "Enter another function to use, or enter nothing to exit", separateLines:true);
                isRepeat = true;
            }
        }

        public static JsonNode? GetJsonNodeFileInLibrary(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            JsonNode? FunctionsJson;
            using (Stream stream = assembly.GetManifestResourceStream(fileName)!)
            {
                if (stream is null)
                {
                    Console.WriteLine($"Error reading {fileName}");
                    return null;
                }
                using StreamReader reader = new(stream);
                FunctionsJson = JsonMethods.ReadFileJson(reader!.ReadToEnd()!)!;
                if (FunctionsJson is null)
                {
                    Console.WriteLine($"Could not read {fileName}");
                    return null;
                }
            }
            return FunctionsJson;
        }
        
        public static JsonDocument? GetJsonDocumentFileInLibrary(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            JsonDocument? FunctionsJson;
            using (Stream stream = assembly.GetManifestResourceStream(fileName)!)
            {
                if (stream is null)
                {
                    Console.WriteLine($"Error reading {fileName}");
                    return null;
                }
                using StreamReader reader = new(stream);
                FunctionsJson = JsonMethods.ReadFileJsonDocument(reader!.ReadToEnd()!)!;
                if (FunctionsJson is null)
                {
                    Console.WriteLine($"Could not read {fileName}");
                    return null;
                }
            }
            return FunctionsJson;
        }

        private static List<HelperFunction> DisplayOptions(JsonNode FunctionsJson)
        {
            // Setup
            int currentNum = 1;
            List<HelperFunction> HelperFunctions = [];

            // Loop through each section
            var keys = JsonMethods.GetKeysFromJsonNode(FunctionsJson);
            int i = 0;
            foreach (var functionSectionName in keys)
            {
                var functionSection = FunctionsJson[functionSectionName];
                // Display the function section
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("- " + functionSectionName);
                PrintDashedLine();
                Console.WriteLine();

                // Loop through each available function
                foreach (var function in functionSection!.AsArray())
                {
                    // Deserialize the function details into an object
                    var helperFunction = function.Deserialize<HelperFunction>();
                    HelperFunctions.Add(helperFunction!);

                    // Display the details for the function
                    helperFunction!.PrintDescription(currentNum);
                    Console.WriteLine();
                    currentNum++;
                }
                i++;
                PrintDashedLine();
            }

            // Return
            return HelperFunctions;
        }

        private static void PrintDashedLine()
        {
            UM.PrintColoredText(ConsoleColor.White, DashedLine, separateLines:true);
        }

        private static Type? AskWhichFunction(List<HelperFunction> functionsList, bool isRepeat)
        {
            int numOfFunctions = functionsList.Count;
            Console.ForegroundColor = ConsoleColor.Magenta;
            int numInput = UserPrompts.AskForInt(1, numOfFunctions, isRepeat);

            if (numInput == 0) return null;
            var selectedFunction = functionsList[numInput - 1];
            return selectedFunction.GetFunctionClass();
        }


        private static readonly string DashedLine = "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~";
    }

    public class HelperFunction
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Input { get; set; }
        public required string Output { get; set; }
        public required string ClassName { get; set; }

        /// <summary>
        /// Get all of the details for the function in the form of a string
        /// </summary>
        /// <param name="currentNum">Function number to display that the user will input to select it</param>
        /// <returns>A string with all the necessary details for the function</returns>
        public void PrintDescription(int currentNum)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"[{currentNum}]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($" - ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"{Name}");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"* ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"{Description}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Get the function class so the function can be run
        /// </summary>
        /// <returns>A type object that is the intended function's class</returns>
        public Type GetFunctionClass()
        {
            return Type.GetType($"HelperFunctions.Functions.Packages.{ClassName}")!;
        }
    }
}