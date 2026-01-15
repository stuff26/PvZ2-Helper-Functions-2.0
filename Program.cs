using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using UniversalMethods;

namespace HelperFunctions
{
    public class Program
    {
        public static void Main()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("PvZ2 Helper Functions by Stuff26");
            Console.WriteLine("Version 2.0");
            Console.WriteLine("Intended for usage with files from Sen 4.0 by Haruma");
            PrintDashedLine();
            Console.WriteLine();

            JsonNode? functionsJson = GetFileInLibrary("HelperFunctions.Functions.json");
            if (functionsJson is null) return;

            var functionsList = DisplayOptions(functionsJson!);
            Console.ForegroundColor = ConsoleColor.White;
            var selectedFunction = AskWhichFunction(functionsList);

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
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR: {e.GetBaseException()}");
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine("Would you like to try this again? (Y/N)");
                }
                bool tryAgain = false;
                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    string? userInput = Console.ReadLine()?.ToUpper();
                    var validResponses = new List<string>() { "Y", "N" };
                    Console.ForegroundColor = ConsoleColor.Red;
                    if (string.IsNullOrWhiteSpace(userInput) || !validResponses.Contains(userInput))
                    {
                        Console.WriteLine("Enter Y or N");
                        continue;
                    }

                    if (userInput.Equals("Y", StringComparison.CurrentCultureIgnoreCase))
                    {
                        tryAgain = true;
                    }
                    else if (userInput.Equals("N", StringComparison.CurrentCultureIgnoreCase))
                    {
                        tryAgain = false;
                    }
                    break;
                }
                if (!tryAgain)
                {
                    break;
                }
                PrintDashedLine();
            }

            PrintDashedLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Finished ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("(press enter to exit)");
            Console.ReadLine();
        }

        public static JsonNode? GetFileInLibrary(string fileName)
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
                FunctionsJson = UM.ReadFileJson(reader!.ReadToEnd()!)!;
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
            var keys = UM.GetKeysFromJsonNode(FunctionsJson);
            int numSections = keys.Count;
            int i = 0;
            foreach (var functionSectionName in keys)
            {
                var functionSection = FunctionsJson[functionSectionName];
                // Display the function section
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("- " + functionSectionName);
                PrintDashedLine();

                // Loop through each available function
                foreach (var function in functionSection!.AsArray())
                {
                    // Deserialize the function details into an object
                    var helperFunction = function.Deserialize<HelperFunction>();
                    HelperFunctions.Add(helperFunction!);

                    // Display the details for the function
                    Console.ForegroundColor = ConsoleColor.Green;
                    helperFunction!.PrintDescription(currentNum);
                    Console.WriteLine();
                    currentNum++;
                }
                i++;
                PrintDashedLine();
                if (i != numSections)
                {
                    Console.WriteLine();
                }
            }

            // Return
            return HelperFunctions;
        }

        private static void PrintDashedLine()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(DashedLine);
        }

        private static Type AskWhichFunction(List<HelperFunction> functionsList)
        {
            int numOfFunctions = functionsList.Count;
            Console.ForegroundColor = ConsoleColor.Magenta;
            int numInput = UM.AskForInt(1, numOfFunctions);

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
            Console.Write($"Function:  ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"{Description}");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"Input:     ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"{Input}");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"Output:    ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"{Output}");
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