using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using UniversalMethods;

namespace HelperFunctions.Functions.Packages
{
    public class CheckLevelErrors
    {
        private static readonly string[] wantedFiles =
            ["ZombieTypes.json", "PlantTypes.json", "GridItemTypes.json",
            "LevelModules.json", "GameFeatures.json", "CreatureTypes.json",
            "CollectableTypes.json"];
        private static readonly string levelCheckingGuideDir = @"C:\Users\zacha\Documents\Coding Stuff\Helper Functions Remake\HelperFunctions\Functions\Packages\LevelCheckingGuide.json";
        private static readonly string packagesDir = @"C:\Users\zacha\Documents\Coding Stuff\packages";
        private static readonly string leveldir = @"C:\Users\zacha\Documents\Coding Stuff\Helper Functions Remake\HelperFunctions\BEACH1.json";
        private static Dictionary<string, (string fileName, List<string> codenames)>? codenamesDirectory;

        public static void Function()
        {
            var packagesDictionary = GetPackagesFiles();
            var level = UM.GetJsonFile(leveldir)!;
            var levelCheckingGuideFile = UM.GetJsonFile(levelCheckingGuideDir)!;
            var levelCheckingGuide = levelCheckingGuideFile["CheckingGuides"]!;
            var childClasses = levelCheckingGuideFile["ChildClasses"]!;
            var childClassesDictionary = MakeChildClasses(childClasses);
            var addedObjectDefinitions = UM.GetKeysFromJsonNode(levelCheckingGuide);

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

            // Loop through every level object in level file
            foreach (var levelObject in level["objects"]!.AsArray())
            {
                // Check that the object has a class and data, continue to next if not
                if (levelObject is null || !levelObject.AsObject().ContainsKey("objclass")
                || !levelObject.AsObject().ContainsKey("objdata")) continue;
                var objclass = (string)levelObject["objclass"]!.AsValue()!;
                var objdata = levelObject["objdata"]!;

                // Check if the class is a child class and continue if no guide is found
                if (childClassesDictionary.TryGetValue(objclass, out string? value))
                {
                    objclass = value;
                }
                else if (!addedObjectDefinitions.Contains(objclass)) continue;

                var allCheckingGuides = UM.GetKeysFromJsonNode(levelCheckingGuide[objclass]);
                var currentCheckingGuide = levelCheckingGuide[objclass]!;
                foreach (var checkingGuide in allCheckingGuides)
                {
                    if (!objdata.AsObject().ContainsKey(checkingGuide)) continue; // If key is not found, skip
                    var keyCheckingGuide = currentCheckingGuide[checkingGuide]!;
                    try
                    {
                        var checkingSteps = keyCheckingGuide.Deserialize<List<string>>()!;
                        CheckLevelObject(objdata[checkingGuide]!, checkingSteps);
                    }
                    catch (JsonException)
                    {
                        var checkingStepsArray = keyCheckingGuide.AsArray();
                        foreach (var checkingStep in checkingStepsArray)
                        {
                            var currentCheckingStep = checkingStep.Deserialize<List<string>>()!;
                            CheckLevelObject(objdata[checkingGuide]!, currentCheckingStep);
                        }
                    }
                }
            }
        }

        private static Dictionary<string, JsonNode> GetPackagesFiles()
        {

            Dictionary<string, JsonNode> fileList = [];
            foreach (var wantedFile in wantedFiles)
            {
                JsonNode filetoAdd = UM.GetJsonFile($@"{packagesDir}\{wantedFile}")!;
                fileList[wantedFile.Replace(".json", "")] = filetoAdd;
            }

            return fileList;
        }

        private static Dictionary<string, string> MakeChildClasses(JsonNode childClasses)
        {
            var childClassesDictionary = new Dictionary<string, string>();
            var parentClasses = UM.GetKeysFromJsonNode(childClasses);
            foreach (var parentClass in parentClasses)
            {
                var tempChildClasses = childClasses[parentClass]!.AsArray();
                foreach (var childClass in tempChildClasses)
                {
                    var childClassName = (string)childClass!.AsValue()!;
                    childClassesDictionary.Add(childClassName, parentClass);
                }
            }

            return childClassesDictionary;
        }

        private static List<string> GetNamesFromFiles(JsonNode fileNode, string typeToGet, string misc = "")
        {
            List<string> toReturn = [];
            foreach (var nodeObject in fileNode["objects"]!.AsArray())
            {
                if (typeToGet == "alias")
                {
                    if (nodeObject!.AsObject().ContainsKey("aliases"))
                    {
                        var aliases = nodeObject["aliases"]!.AsArray();
                        foreach (var alias in aliases)
                        {
                            toReturn.Add((string)alias!.AsValue()!);
                        }
                    }
                }
                if (typeToGet == "typename")
                {
                    if (nodeObject!.AsObject().ContainsKey("objdata")
                    && nodeObject["objdata"]!.AsObject().ContainsKey("TypeName"))
                    {
                        var typename = (string)nodeObject["objdata"]!["TypeName"]!.AsValue()!;
                        if (misc == "dino") typename = typename[4..];
                        toReturn.Add(typename);
                    }
                }
                ;
                if (typeToGet == "feature")
                {
                    if (nodeObject!.AsObject().ContainsKey("objdata")
                    && nodeObject["objdata"]!.AsObject().ContainsKey("Feature"))
                    {
                        var feature = (string)nodeObject["objdata"]!["Feature"]!.AsValue()!;
                        toReturn.Add(feature);
                    }
                }
            }

            return toReturn;
        }
    
        private static void CheckLevelObject(JsonNode jsonObject, List<string> currentCheckingGuide)
        {
            var currentStep = currentCheckingGuide[0];
            currentCheckingGuide.RemoveAt(0);
            if (currentStep.StartsWith("check"))
            {
                var checkingSettings = currentStep.Split("_").ToList();
                var foundValue = jsonObject.GetValue<string>();
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
                var currentArray = jsonObject.AsArray();
                foreach (var value in currentArray)
                {
                    CheckLevelObject(value!, currentCheckingGuide.ToList());
                }
            }
            if (currentStep.StartsWith('$'))
            {
                var keyToCheck = currentStep[1..]; // Remove "$"
                if (!jsonObject.AsObject().ContainsKey(keyToCheck)) return; // If the key is not found, exit out to prevent errors
                CheckLevelObject(jsonObject[keyToCheck]!, currentCheckingGuide);
            }
        }
    }
}