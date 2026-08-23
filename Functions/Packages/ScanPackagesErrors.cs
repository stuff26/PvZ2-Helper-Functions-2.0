using System.Text.Json;
using UniversalMethods;

namespace HelperFunctions.Functions.Packages
{
    public static class ScanPackagesErrors
    {
        private static Dictionary<string, HashSet<string>>? propertyNames;
        
        private static readonly Dictionary<string, string> codenameTypeToFile = new()
        {
            {"GridItemAliases", "GridItemTypes"},
            {"PlantAliases", "PlantTypes"},
            {"ZombieAliases", "ZombieTypes"},
            {"GridItemProps", "GridItemProps"},
            {"ProjectileTypes", "ProjectileTypes"},
            {"ArmorTypes", "ArmorTypes"},
            {"ZombieProperties", "ZombieProperties"},
            {"PropertySheets", "PropertySheets"},
            {"PlantProperties", "PlantProperties"}
        };
        private static readonly Dictionary<string, string> referenceCodenames = codenameTypeToFile.ToDictionary(x => x.Value, x => x.Key);

        private static readonly Dictionary<string, string> actionTypeToPropertyName = new()
        {
            {"spawn_projectile", "ProjectileTypes"},
            {"spawn_plant", "PlantTypes"},
            {"spawn_grid", "GridItemTypes"},
            {"spawn_zombie", "ZombieTypes"},
            {"spawn_collectable", "CollectableTypes"},
            {"set_sky_collectable", "CollectableTypes"},
            {"spawn_creature", "CreatureTypes"},
            {"apply_armor", "ArmorTypes"},
            {"transform", "transform"}
        };
        private static readonly Dictionary<string, string> transformArgToPropertyName = new()
        {
            {"Zombie", "ZombieTypes"},
            {"Plant", "PlantTypes"},
            {"GridItem", "GridItemTypes"}
        };
        private static readonly List<string> aliasObjects = ["ArmorTypes", "GridItemProps", "LevelModules", "PlantProperties",
        "ProjectileTypes", "PropertySheets", "ZombieSwapLists", "ZombieProperties", "ZombieActions"];

        public static void Function()
        {
            // Setup
            string[] wantedFiles = ["Armortypes.json", "CollectableTypes.json", "CreatureTypes.json", "EffectObjectTypes.json",
            "GridItemTypes.json", "GridItemProps.json", "LevelModules.json", "PlantTypes.json", "PlantProperties.json",
            "Powers.json", "ProjectileTypes.json", "PropertySheets.json", "ZombieTypes.json", "ZombieProperties.json",
            "ZombieActions.json"];
            var packgeCheckingGuideDir = @"HelperFunctions.PackagesCheckingGuide.json";

            // Ask the user for the packages directory
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter the packages you want to scan");
            var packagesDir = UserPrompts.AskForDirectory(wantedFiles);
            var packageCheckingGuide = Program.GetJsonDocumentFileInLibrary(packgeCheckingGuideDir)!.RootElement;

            // Get all the necessary codenames
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("Getting necessary files and codenames... ");
            propertyNames = GetAllProperties(packagesDir);
            ProgressChecker.WriteFinished();

            // Check all the files
            string[] filesToCheck = ["ArmorTypes", "CollectableTypes", "CreatureTypes", "EffectObjectTypes",
            "GridItemTypes", "GridItemProps", "LevelModules", "PlantAlmanacData", "PlantFamilyTypes", "PlantTypes",
            "PlantProperties", "PlantLevels", "PowerupTypes", "ProjectileTypes", "PropertySheets", "ToolPackets", "ZombieSwapLists", 
            "ZombieTypes", "ZombieProperties", "ZombieActions"];
            foreach (var fileToCheck in filesToCheck)
            {
                // Get checking guide for respective file
                if (!packageCheckingGuide.TryGetProperty(fileToCheck, out _)) continue; // If couldn't find checking guide, skip
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"--- ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{fileToCheck}.json");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($" ---");
                
                var fileGuide = packageCheckingGuide.GetProperty(fileToCheck)!;
                if (!fileGuide.TryGetProperty("CheckingGuides", out var checkingGuides))
                {
                    checkingGuides = JsonElement.Parse("false");
                }

                Dictionary<string, string>? childClasses = null;
                if (fileGuide.TryGetProperty("ChildClasses", out var rawChildClasses))
                {
                    childClasses = MakeChildClasses(rawChildClasses);
                }
                fileGuide.TryGetProperty("UniversalGuides", out var universalGuides);
                fileGuide.TryGetProperty("ActionFrameGuide", out var actionFrameGuides);

                // Get file
                var fileDir = Path.Join(packagesDir, $"{fileToCheck}.json");
                var file = JsonMethods.GetJsonDocmentFile(fileDir);
                if (file is null) // If the file couldn't be found, continue
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Could not read {fileToCheck}.json, will be skipped\n");
                    continue;
                } 

                // Loop through all the objects
                if (!file.RootElement.TryGetProperty("objects", out _)) // If there isn't an objects key in the file, skip it
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Could not find objects in {fileToCheck}.json, will be skipped\n");
                    continue;
                }
                bool isAliasObject = aliasObjects.Contains(fileToCheck);
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                var hasError = ScanFileObjects(file.RootElement, childClasses, checkingGuides, universalGuides, isAliasObject, actionFrameGuides);

                if (!hasError)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"No errors found in {fileToCheck}.json");
                }
                Console.WriteLine();
            }
        }

        public static Dictionary<string, HashSet<string>> GetAllProperties(string packagesDir)
        {
            Dictionary<string, HashSet<string>> propertyNames = [];

            // Alias files
            string[] aliasesFiles = ["ArmorTypes", "EffectObjectTypes", "GridItemProps", "LevelModules", "PlantProperties",
            "ProjectileTypes", "PropertySheets", "ZombieProperties", "ZombieActions"];
            foreach (var aliasFile in aliasesFiles)
            {
                var filePath = Path.Join(packagesDir, $"{aliasFile}.json");
                var file = JsonMethods.GetJsonDocmentFile(filePath)!;
                var aliases = GetAliases(file);
                propertyNames.Add(aliasFile, aliases);
            }

            // Typename files
            string[] typenameFiles = ["CollectableTypes", "CreatureTypes", "GridItemTypes", "PlantTypes", "Powers", "ZombieTypes"];
            foreach (var typenameFile in typenameFiles)
            {
                var filePath = Path.Join(packagesDir, $"{typenameFile}.json");
                var file = JsonMethods.GetJsonDocmentFile(filePath)!;
                var aliases = GetTypenames(file);
                propertyNames.Add(typenameFile, aliases);
            }

            // Aliases w/ typenames
            string[] aliasWithTypnameFiles = ["GridItemTypes", "PlantTypes", "ZombieTypes"];
            foreach (var fileName in aliasWithTypnameFiles)
            {
                var filePath = Path.Join(packagesDir, $"{fileName}.json");
                var file = JsonMethods.GetJsonDocmentFile(filePath)!;
                var aliases = GetAliases(file);
                propertyNames.Add($"{fileName.Replace("Types", "Aliases")}", aliases);
            }

            return propertyNames;
        }

        public static HashSet<string> GetAliases(JsonDocument file)
        {
            HashSet<string> totalAliases = [];
            foreach (var armorProp in file.RootElement.GetProperty("objects")!.EnumerateArray())
            {
                if (armorProp!.TryGetProperty("aliases", out var aliases))
                {
                    foreach (var alias in aliases.EnumerateArray())
                    {
                        totalAliases.Add(alias.Deserialize<string>()!);
                    }
                }
            }
            return totalAliases;
        }

        public static HashSet<string> GetTypenames(JsonDocument file)
        {
            HashSet<string> totalTypenames = [];
            foreach (var armorProp in file!.RootElement.GetProperty("objects")!.EnumerateArray())
            {
                if (!armorProp!.TryGetProperty("objdata", out var objdata)) continue;

                if (objdata.TryGetProperty("TypeName", out var typename))
                {
                    totalTypenames.Add(typename.Deserialize<string>()!);
                }
                else if (objdata.TryGetProperty("Typename", out typename))
                {
                    totalTypenames.Add(typename.Deserialize<string>()!);
                }
            }
            return totalTypenames;
        }

        private static Dictionary<string, string>? MakeChildClasses(JsonElement childClasses)
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

        public static bool ScanFileObjects(JsonElement file, Dictionary<string, string>? childClasses, JsonElement checkingGuides, JsonElement universalGuides, bool isAliasObject, JsonElement actionFrameGuides)
        {
            var fileObjects = file.GetProperty("objects")!.EnumerateArray();
            List<string> missingObjects;
            bool hasError = false;

            foreach (var fileObject in fileObjects)
            {
                missingObjects = [];

                // If the object is null or it doesn't contain an objclass or objdata, skip this object
                if (!fileObject!.TryGetProperty("objclass", out var objclassElement) ||
                !fileObject!.TryGetProperty("objdata", out var objdata)) continue;
                var objclass = objclassElement.GetString()!;
                

                string? objectName = null;
                if (isAliasObject && fileObject.TryGetProperty("aliases", out _))
                {
                    var aliases = fileObject.GetProperty("aliases")!.Deserialize<string[]>();
                    if (aliases is not null && aliases.Length > 0) objectName = aliases[0];
                }
                else if (!isAliasObject)
                {
                    if (objdata.TryGetProperty("TypeName", out var objectTypename) ||
                    objdata.TryGetProperty("Typename", out objectTypename))
                    {
                        objectName = objectTypename.Deserialize<string>();
                    }
                }
                // Go through universal guides
                if (universalGuides.ValueKind != JsonValueKind.Undefined)
                {
                    var keysList = JsonMethods.GetKeysFromJsonElement(universalGuides);
                    foreach (var key in keysList)
                    {
                        if (!objdata.TryGetProperty(key, out _)) continue;
                        var UGuide = universalGuides.GetProperty(key).Deserialize<JsonElement[]>();
                        if (UGuide is null) continue;

                        if (UGuide[0].ValueKind == JsonValueKind.String)
                        {
                            var currentUCheckingGuide = new List<string>();
                            foreach (var value in UGuide)
                            {
                                currentUCheckingGuide.Add(value.GetString()!);
                            }
                            CheckObject(objdata!.GetProperty(key)!, currentUCheckingGuide, missingObjects, objectName);
                        }
                        else if (UGuide[0].ValueKind == JsonValueKind.Array)
                        {
                            var UCheckingGuideList = new List<JsonElement>()!;
                            foreach (var value in UGuide)
                            {
                                UCheckingGuideList.Add(value);
                            }
                            foreach (var currentUCheckingGuide in UCheckingGuideList)
                            {
                                CheckObject(objdata!.GetProperty(key)!, currentUCheckingGuide.Deserialize<List<string>>()!, missingObjects, objectName);
                            }
                        }
                        
                    }
                    if (!hasError) hasError = missingObjects.Count > 0;
                }

                // Go through action frame stuff
                if (actionFrameGuides.ValueKind != JsonValueKind.Undefined
                 && actionFrameGuides.GetProperty("Objclass").GetString() == objclass)
                {
                    var actionFrameCheckingGuide = actionFrameGuides.GetProperty("Location").Deserialize<List<string>>()!;
                    CheckObject(objdata, actionFrameCheckingGuide, missingObjects, objectName, isActionFrame:true);
                    if (!hasError) hasError = missingObjects.Count > 0;
                }

                if (childClasses is not null && childClasses.ContainsKey(objclass))
                {
                    objclass = childClasses[objclass];
                }
                else if (checkingGuides.ValueKind == JsonValueKind.False ||
                !checkingGuides.TryGetProperty(objclass, out _)) continue; // If the objclass isn't found, skip this object
                
                var currentCheckingGuide = checkingGuides!.GetProperty(objclass);
                var allCheckingGuides = JsonMethods.GetKeysFromJsonElement(currentCheckingGuide);
                foreach (var key in allCheckingGuides)
                {
                    var recentCheckingGuide = currentCheckingGuide!.GetProperty(key)!;
                    if (!objdata!.TryGetProperty(key, out _) || recentCheckingGuide.GetArrayLength() == 0) continue;
                    if (recentCheckingGuide[0].ValueKind == JsonValueKind.String)
                    {
                        var checkingGuide = recentCheckingGuide.Deserialize<List<string>>()!;
                        CheckObject(objdata.GetProperty(key)!, checkingGuide, missingObjects, objectName);
                    }
                    else if (recentCheckingGuide[0].ValueKind == JsonValueKind.Array)
                    {
                        var checkingStepsArray = new List<JsonElement>();
                        foreach (var checkingStep in recentCheckingGuide.EnumerateArray())
                        {
                            checkingStepsArray.Add(checkingStep);
                        }
                        foreach (var checkingStep in checkingStepsArray)
                        {
                            CheckObject(objdata.GetProperty(key)!, checkingStep.Deserialize<List<string>>()!, missingObjects, objectName);
                        }
                        
                    }
                }
                if (!hasError) hasError = missingObjects.Count > 0;
            }

            return hasError;
        }

        private static void CheckObject(JsonElement jsonObject, List<string> currentCheckingGuide, List<string> missingObjects, string? objectName, bool isActionFrame = false)
        {
            var currentStep = string.Empty;
            if (currentCheckingGuide.Count > 0)
            {
                currentStep = currentCheckingGuide[0];
                currentCheckingGuide.RemoveAt(0);
            }

            if (currentStep.StartsWith("check"))
            {
                var checkingSettings = currentStep.Split("_").ToList();
                var foundValue = jsonObject.GetString()!;
                var toCompareTo = checkingSettings[1];
                if (currentStep.Contains("_ref"))
                {
                    if (foundValue.EndsWith("@CurrentLevel)") || foundValue.EndsWith("@.)")
                    || foundValue == "RTID(0)") return;
                    var tempResult = foundValue.Replace("RTID(", string.Empty).Replace(")", string.Empty).Split("@");
                    foundValue = tempResult[0];
                    toCompareTo = tempResult[1];
                    if (!referenceCodenames.TryGetValue(toCompareTo, out toCompareTo))
                    {
                        return;
                    }
                }
                if (currentStep.Contains("addbegin"))
                {
                    var toAddBegin = currentStep[(currentStep.IndexOf("addbegin(") + 9)..];
                    toAddBegin = toAddBegin[..toAddBegin.IndexOf(')')];
                    foundValue = toAddBegin + foundValue;
                }
                if (missingObjects.Contains(foundValue) || foundValue == string.Empty) return;
                var codenameListToCheck = propertyNames![toCompareTo];
                if (!codenameListToCheck.Contains(foundValue))
                {
                    missingObjects.Add(foundValue);

                    if (!codenameTypeToFile.TryGetValue(toCompareTo, out string? fileScanned))
                    {
                        fileScanned = toCompareTo;
                    }
                    PrintMissingEntityMessage(foundValue, fileScanned, objectName);
                }
            }
            else if (currentStep == "loop")
            {
                foreach (var value in jsonObject.EnumerateArray())
                {
                    CheckObject(value!, currentCheckingGuide.ToList(), missingObjects, objectName, isActionFrame);
                }
            }
            else if (currentStep.StartsWith('$'))
            {
                var keyToCheck = currentStep[1..]; // Remove "$"
                JsonElement result;
                if (!jsonObject.TryGetProperty(keyToCheck, out result)) return; // If the key is not found, exit out to prevent errors
                CheckObject(result, currentCheckingGuide, missingObjects, objectName, isActionFrame);
            }
            else if (currentCheckingGuide.Count == 0 && isActionFrame)
            {
                var actionType = jsonObject.GetProperty("ActionType").Deserialize<string>()!;
                if (!actionTypeToPropertyName.TryGetValue(actionType, out string? toCompareTo)) return;
                

                string foundValue;
                HashSet<string> codenameListToCheck;
                if (toCompareTo == "transform")
                {
                    var args = jsonObject.GetProperty("ActionArgs").Deserialize<string>()!;
                    var argsBack = args[(args.IndexOf("TransformArgs[") + 14)..];
                    var splitArgs = argsBack.Split(',');

                    var transformType = splitArgs[0].Trim();
                    if (!transformArgToPropertyName.TryGetValue(transformType, out toCompareTo)) return;
                    codenameListToCheck = propertyNames![toCompareTo];
                    foundValue = splitArgs[1].Trim().Replace("]", string.Empty);
                }
                else
                {
                    codenameListToCheck = propertyNames![toCompareTo];
                    var args = jsonObject.GetProperty("ActionArgs").Deserialize<string>()!;
                    var argsBack = args[(args.IndexOf("Type:") + 5)..];
                    if (argsBack.Contains(','))
                        foundValue = argsBack[..argsBack.IndexOf(',')];
                    else
                        foundValue = argsBack;

                }

                if (!codenameListToCheck.Contains(foundValue))
                {
                    missingObjects.Add(foundValue);
                    if (!codenameTypeToFile.TryGetValue(toCompareTo, out string? fileScanned))
                    {
                        fileScanned = toCompareTo;
                    }

                    PrintMissingEntityMessage(foundValue, fileScanned, objectName);
                }
            }
        }

        private static void PrintMissingEntityMessage(string foundValue, string fileScanned, string? objectName)
        {
            if (objectName is not null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{objectName} ");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Unknown object ");
            }
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write($"refers to ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{foundValue}");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write($" that isn't found in file ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{fileScanned}.json");
        }
    }
}