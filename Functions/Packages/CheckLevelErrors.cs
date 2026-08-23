using System.Text.Json;
using UniversalMethods;

namespace HelperFunctions.Functions.Packages
{
    public static class CheckLevelErrors
    {
        private static readonly string[] wantedFiles =
            ["ZombieTypes.json", "PlantTypes.json", "GridItemTypes.json",
            "LevelModules.json", "GameFeatures.json", "CreatureTypes.json",
            "CollectableTypes.json"];
        private static readonly string levelCheckingGuideDir = "HelperFunctions.LevelCheckingGuide.json";
        private static Dictionary<string, (string fileName, List<string> codenames)>? codenamesDirectory;

        public static void Function()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter the level file you want to scan");
            var level = UserPrompts.AskForJsonDocumentFile().jsonFile!.RootElement;

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter the packages directory you want to scan with");
            var packagesDir = UserPrompts.AskForDirectory(wantedFiles);
            var packagesDictionary = GetPackagesFiles(packagesDir);
            
            var levelCheckingGuideFile = Program.GetJsonDocumentFileInLibrary(levelCheckingGuideDir)!.RootElement;
            var levelCheckingGuide = levelCheckingGuideFile.GetProperty("CheckingGuides")!;
            var childClasses = levelCheckingGuideFile.GetProperty("ChildClasses")!;
            var childClassesDictionary = MakeChildClasses(childClasses);
            var addedObjectDefinitions = JsonMethods.GetKeysFromJsonElement(levelCheckingGuide);

            codenamesDirectory = new()
            {
                {"planttypename", ("PlantTypes.json", GetNamesFromFiles(packagesDictionary["PlantTypes"], "typename")) },
                {"plantalias", ("PlantTypes.json", GetNamesFromFiles(packagesDictionary["PlantTypes"], "alias")) },
                {"zombietypename", ("ZombieTypes.json", GetNamesFromFiles(packagesDictionary["ZombieTypes"], "typename")) },
                {"zombiealias", ("ZombieTypes.json", GetNamesFromFiles(packagesDictionary["ZombieTypes"], "alias")) },
                {"levelmodule", ("LevelModules.json", GetNamesFromFiles(packagesDictionary["LevelModules"], "alias")) },
                {"currentlevelmodule", ("level file", GetNamesFromFiles(level, "alias")) },
                {"gamefeature", ("GameFeatures.json", GetNamesFromFiles(packagesDictionary["GameFeatures"], "feature"))},
                {"griditemtypename", ("GridItemTypes", GetNamesFromFiles(packagesDictionary["GridItemTypes"], "typename"))},
                {"griditemalias", ("GridItemTypes", GetNamesFromFiles(packagesDictionary["GridItemTypes"], "alias"))},
                {"dinotypename", ("CreatureTypes.json", GetNamesFromFiles(packagesDictionary["CreatureTypes"], "typename", misc:"dino"))},
                {"collectabletype", ("CollectableTypes.json", GetNamesFromFiles(packagesDictionary["CollectableTypes"], "typename"))}
            };

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            // Loop through every level object in level file
            var oldCursorPosition = Console.GetCursorPosition().Top;
            foreach (var levelObject in level.GetProperty("objects")!.EnumerateArray())
            {
                // Check that the object has a class and data, continue to next if not
                if (!levelObject.TryGetProperty("objclass", out var objclassElement)
                || !levelObject.TryGetProperty("objdata", out var objdata)) continue;

                var objclass = objclassElement.GetString()!;

                // Check if the class is a child class and continue if no guide is found
                if (childClassesDictionary.TryGetValue(objclass!, out string? value))
                {
                    objclass = value;
                }
                else if (!addedObjectDefinitions.Contains(objclass!)) continue;

                var allCheckingGuides = JsonMethods.GetKeysFromJsonElement(levelCheckingGuide.GetProperty(objclass!));
                var currentCheckingGuide = levelCheckingGuide.GetProperty(objclass)!;
                foreach (var checkingGuide in allCheckingGuides)
                {
                    if (!objdata.TryGetProperty(checkingGuide, out _)) continue; // If key is not found, skip
                    var keyCheckingGuide = currentCheckingGuide.GetProperty(checkingGuide)!;
                    try
                    {
                        var checkingSteps = keyCheckingGuide.Deserialize<List<string>>()!;
                        CheckLevelObject(objdata.GetProperty(checkingGuide)!, checkingSteps);
                    }
                    catch (JsonException)
                    {
                        foreach (var checkingStep in keyCheckingGuide.EnumerateArray())
                        {
                            var currentCheckingStep = checkingStep.Deserialize<List<string>>()!;
                            CheckLevelObject(objdata.GetProperty(checkingGuide)!, currentCheckingStep);
                        }
                    }
                }
            }
            if (oldCursorPosition == Console.GetCursorPosition().Top)
            {
                UM.PrintColoredText(ConsoleColor.Yellow, "No errors found", separateLines:true);
            }
        }

        private static Dictionary<string, JsonElement> GetPackagesFiles(string packagesDir)
        {

            Dictionary<string, JsonElement> fileList = [];
            foreach (var wantedFile in wantedFiles)
            {
                JsonDocument filetoAdd = JsonMethods.GetJsonDocmentFile($@"{packagesDir}\{wantedFile}")!;
                fileList.Add(wantedFile.Replace(".json", string.Empty), filetoAdd.RootElement);
            }

            return fileList;
        }

        private static Dictionary<string, string> MakeChildClasses(JsonElement childClasses)
        {
            var childClassesDictionary = new Dictionary<string, string>();
            var parentClasses = JsonMethods.GetKeysFromJsonElement(childClasses);
            foreach (var parentClass in parentClasses)
            {
                var tempChildClasses = childClasses.GetProperty(parentClass)!;
                foreach (var childClass in tempChildClasses.EnumerateArray())
                {
                    var childClassName = childClass!.GetString()!;
                    childClassesDictionary.Add(childClassName, parentClass);
                }
            }

            return childClassesDictionary;
        }

        private static List<string> GetNamesFromFiles(JsonElement fileNode, string typeToGet, string misc = "")
        {
            List<string> toReturn = [];
            foreach (var nodeObject in fileNode.GetProperty("objects")!.EnumerateArray())
            {
                if (typeToGet == "alias")
                {
                    if (nodeObject!.TryGetProperty("aliases", out var aliasesElement))
                    {
                        var aliases = aliasesElement.EnumerateArray();
                        foreach (var alias in aliases)
                        {
                            toReturn.Add(alias!.GetString()!);
                        }
                    }
                }
                if (typeToGet == "typename")
                {
                    if (nodeObject!.TryGetProperty("objdata", out _)
                    && nodeObject.GetProperty("objdata")!.TryGetProperty("TypeName", out var typenameElement))
                    {
                        var typename = typenameElement.GetString()!;
                        if (misc == "dino") typename = typename[4..];
                        toReturn.Add(typename);
                    }
                }
                ;
                if (typeToGet == "feature")
                {
                    if (nodeObject!.TryGetProperty("objdata", out _)
                    && nodeObject.GetProperty("objdata")!.TryGetProperty("Feature", out var featureElement))
                    {
                        var feature = featureElement.GetString()!;
                        toReturn.Add(feature);
                    }
                }
            }

            return toReturn;
        }
    
        private static void CheckLevelObject(JsonElement jsonObject, List<string> currentCheckingGuide)
        {
            var currentStep = currentCheckingGuide[0];
            currentCheckingGuide.RemoveAt(0);
            if (currentStep.StartsWith("check"))
            {
                var checkingSettings = currentStep.Split("_").ToList();
                var foundValue = jsonObject.GetString()!;
                var toCompareTo = checkingSettings[1];
                if (currentStep.Contains("_ref"))
                {
                    if (foundValue.EndsWith("@CurrentLevel)") || foundValue.EndsWith("@.)"))
                    {
                        toCompareTo = "current" + toCompareTo;
                    }
                    foundValue = UM.RemoveReference(foundValue);
                }

                var (filename, codenameListToCheck) = codenamesDirectory![toCompareTo];
                if (currentStep.Contains("_begininclude"))
                {
                    foreach (var validCodename in codenameListToCheck)
                    {
                        if (validCodename.StartsWith(foundValue))
                        {
                            return;
                        }
                    }
                    Console.WriteLine($"Could not find {foundValue} in {filename}");
                }
                else if (!codenameListToCheck.Contains(foundValue))
                {
                    Console.WriteLine($"Could not find {foundValue} in {filename}");
                }
            }
            if (currentStep == "loop")
            {
                foreach (var value in jsonObject.EnumerateArray())
                {
                    CheckLevelObject(value!, currentCheckingGuide.ToList());
                }
            }
            if (currentStep.StartsWith('$'))
            {
                var keyToCheck = currentStep[1..]; // Remove "$"
                if (!jsonObject.TryGetProperty(keyToCheck, out var data)) return; // If the key is not found, exit out to prevent errors
                CheckLevelObject(data!, currentCheckingGuide);
            }
        }
    }
}