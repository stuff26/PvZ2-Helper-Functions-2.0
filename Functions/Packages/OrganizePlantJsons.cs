using System.Text.Json;
using System.Text.Json.Nodes;
using UniversalMethods;

namespace HelperFunctions.Functions.Packages
{
    public class OrganizePlantJsons
    {
        private static readonly string[] wantedFiles =
        ["PropertySheets.json", "PlantTypes.json", "PlantProperties.json", "PlantLevels.json", "Powers.json"];

        public static void Function()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter the packages folder you want to edit");
            string packagesDir = UM.AskForDirectory(wantedFiles);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Retrieving files... ");
            var fileList = GetNeededFiles(packagesDir)!;
            ProgressChecker.WriteFinished();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Getting plant order... ");
            var plantList = GetPlantOrder(fileList["PropertySheets"]!);
            ProgressChecker.WriteFinished();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Organizing PlantTypes... ");
            var propertyList = OrganizePlantTypes(fileList["PlantTypes"]!, plantList);
            ProgressChecker.WriteFinished();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Organizing PlantProperties... ");
            OrganizePlantProperties(fileList["PlantProperties"]!, propertyList);
            ProgressChecker.WriteFinished();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Organizing PlantLevels... ");
            var powerList = OrganizePlantLevels(fileList["PlantLevels"]!, plantList);
            ProgressChecker.WriteFinished();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Organizing Powers... ");
            OrganizePowers(fileList["Powers"]!, powerList);
            ProgressChecker.WriteFinished();

            Console.ForegroundColor = ConsoleColor.Green;
            var writeFiles = new ProgressChecker("Writing back files... ", fileList.Count-1);
            foreach (var filePair in fileList)
            {
                if (filePair.Key != "PropertySheets") 
                {
                    UM.WriteJsonFile(Path.Join(packagesDir, $"{filePair.Key}.json"), filePair.Value);
                    writeFiles.AddOne();
                }
            }
        }

        private static Dictionary<string, JsonNode> GetNeededFiles(string packagesDir)
        {
            Dictionary<string, JsonNode?> fileList = [];
            foreach (var wantedFile in wantedFiles)
            {
                JsonNode filetoAdd = UM.GetJsonFile($@"{packagesDir}\{wantedFile}")!;
                fileList[wantedFile.Replace(".json", "")] = filetoAdd;
            }

            return fileList!;
        }

        private static List<string> GetPlantOrder(JsonNode propertySheets)
        {
            var propertyObjects = propertySheets["objects"]!;
            JsonArray? plantNames = null;

            foreach (var prop in propertyObjects.AsArray())
            {
                if (!prop!.AsObject().ContainsKey("objclass")
                || prop["objclass"]!.GetValue<string>() != "GamePropertySheet") continue;

                plantNames = prop["objdata"]!["PlantTypeOrder"]!.AsArray();
                break;
            }

            return JsonSerializer.Deserialize<List<string>>(plantNames)!;
        }

        private static List<string> OrganizePlantTypes(JsonNode plantTypes, List<string> plantList)
        {
            var fullPlantList = new JsonNode[plantList.Count];
            List<JsonNode> unorganizedPlantList = [];

            foreach (var plantType in plantTypes["objects"]!.AsArray())
            {
                if (!plantType!.AsObject().ContainsKey("objdata"))
                {
                    continue;
                }
                else if (plantType["objdata"]!.AsObject().ContainsKey("TypeName"))
                {
                    unorganizedPlantList.Add(plantType);
                }
                else
                {
                    string typename = plantType["objdata"]!["TypeName"]!.GetValue<string>();
                    if (!plantList.Contains(typename))
                    {
                        unorganizedPlantList.Add(plantType);
                    }
                    else
                    {
                        int placement = plantList.IndexOf(typename);
                        fullPlantList[placement] = plantType;
                    }
                }
            }

            var newPlantList = fullPlantList.ToList();
            newPlantList.AddRange(unorganizedPlantList);

            var toAddArray = new JsonArray();
            var propertyList = new List<string>();
            foreach (var tempNode in newPlantList)
            {
                if (tempNode is not null)
                {
                    toAddArray.Add(JsonNode.Parse(tempNode.ToJsonString()));
                    if (tempNode["objdata"]!.AsObject().ContainsKey("Properties"))
                    {
                        var propertyName = tempNode["objdata"]!["Properties"]!.GetValue<string>();

                        propertyList.Add(UM.RemoveReference(propertyName));
                    }
                }
            }

            toAddArray.Add(JsonNode.Parse("{}"));
            plantTypes["objects"] = toAddArray;
            return propertyList;
        }

        private static void OrganizePlantProperties(JsonNode plantProperties, List<string> propertyList)
        {
            // Setup
            var allProperties = plantProperties["objects"]!;
            var organizedProperties = new JsonNode[propertyList.Count];
            Dictionary<string, JsonNode> animrigs = [];
            List<JsonNode> unorganizedProperties = [];

            // Setup each property
            foreach (var plantProp in allProperties.AsArray())
            {
                // Skip titles or other blocks that aren't proper properties
                if (plantProp!.AsObject().ContainsKey("objclass"))
                {
                    // Get the objclass and alias of each property
                    string objclass;
                    string alias;
                    try
                    {
                        objclass = plantProp["objclass"]!.GetValue<string>();
                        alias = plantProp!["aliases"]![0]!.GetValue<string>();
                    }
                    catch (ArgumentNullException)
                    {
                        continue;
                    }

                    // If an animrig is found, store it in a dictionary for animrigs
                    if (objclass == "PlantAnimRigPropertySheet")
                    {
                        animrigs.TryAdd(alias, plantProp);
                    }

                    else
                    {
                        // If the alias of the property is found, add it to its intended spot in the list of organized props
                        if (propertyList.Contains(alias))
                        {
                            int insertPlacement = propertyList.IndexOf(alias);
                            organizedProperties[insertPlacement] = plantProp;
                        }
                        // If the alias is not found, add to unorganized properties
                        else
                        {
                            unorganizedProperties.Add(plantProp);
                        }
                    }
                }
            }

            // Add together organized and unorganized plant lists
            var tempList = organizedProperties.ToList();
            tempList.AddRange(unorganizedProperties);

            // Add animrigs to list
            var newList = new JsonArray();
            foreach (var currentProp in tempList)
            {
                if (currentProp is null) continue;
                JsonNode? toAddAnimrig = null;

                if (currentProp["objdata"]!.AsObject().ContainsKey("AnimRigProps"))
                {
                    var animrig = UM.RemoveReference(currentProp["objdata"]!["AnimRigProps"]!.GetValue<string>());
                    if (animrigs.ContainsKey(animrig))
                    {
                        toAddAnimrig = animrigs[animrig];
                        animrigs.Remove(animrig); // Ensure that the same animrig doesn't get added twice
                    }
                }
                newList.Add(JsonNode.Parse(currentProp.ToJsonString())!);
                if (toAddAnimrig is not null)
                    newList.Add(JsonNode.Parse(toAddAnimrig.ToJsonString())!);
            }

            // Finish editing properties and return
            foreach (var remainingAnimrig in animrigs)
            {
                // Add any remaining animrigs that haven't been added yet
                newList.Add(JsonNode.Parse(remainingAnimrig.Value.ToJsonString())!);
            }
            newList.Add(JsonNode.Parse("{}")); // Dummy prop just for convenience when editing
            plantProperties["objects"] = newList;
        }

        private static List<string> OrganizePlantLevels(JsonNode plantLevels, List<string> plantList)
        {
            var allLevels = plantLevels["objects"]!;
            JsonNode[] organizedLevels = new JsonNode[plantList.Count];
            List<JsonNode> unorganizedLevels = [];
            foreach (var plantLevel in allLevels.AsArray())
            {
                if (plantLevel!.AsObject().ContainsKey("objdata"))
                {
                    var plantName = plantLevel["objdata"]!["TypeName"]!.GetValue<string>();
                    if (plantList.Contains(plantName))
                    {
                        int plantPlacement = plantList.IndexOf(plantName);
                        organizedLevels[plantPlacement] = plantLevel;
                    }
                    else
                    {
                        unorganizedLevels.Add(plantLevel);
                    }
                }
            }

            var fullList = new JsonArray();
            var tempList = organizedLevels.ToList();
            tempList.AddRange(unorganizedLevels);
            List<string> powerList = [];
            foreach (var level in tempList)
            {
                if (level is not null)
                {
                    fullList.Add(JsonNode.Parse(level.ToJsonString())!);

                    if (level["objdata"]!.AsObject().ContainsKey("StringStats"))
                    {
                        var stringStats = level["objdata"]!["StringStats"]!.AsArray();
                        foreach (var stringStat in stringStats)
                        {
                            if (stringStat is not null &&
                            stringStat.AsObject().ContainsKey("Type") &&
                            stringStat["Type"]!.GetValue<string>() == "Power" &&
                            stringStat.AsObject().ContainsKey("Values"))
                            {
                                var toAddPowers = stringStat["Values"]!.AsArray();
                                foreach (var power in toAddPowers)
                                {
                                    var powerName = power!.GetValue<string>();
                                    if (!powerList.Contains(powerName))
                                    {
                                        powerList.Add(powerName);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            plantLevels["objects"] = fullList;
            return powerList;
        }

        private static void OrganizePowers(JsonNode powers, List<string> powerList)
        {
            var organizedPowers = new JsonNode[powerList.Count];
            var unorganizedPowers = new List<JsonNode>();
            foreach (var power in powers["objects"]!.AsArray())
            {
                if (power is not null &&
                    power.AsObject().ContainsKey("objdata") &&
                    power["objdata"]!.AsObject().ContainsKey("TypeName"))
                {
                    var powerName = power["objdata"]!["TypeName"]!.GetValue<string>();
                    if (powerList.Contains(powerName))
                    {
                        int placement = powerList.IndexOf(powerName);
                        organizedPowers[placement] = power;
                    }
                    else if (powerName is not null)
                    {
                        unorganizedPowers.Add(power);
                    }
                }
            }

            var allPowers = new JsonArray();
            var tempList = organizedPowers.ToList();
            tempList.AddRange(unorganizedPowers);
            foreach (var power in tempList)
            {
                if (power is not null)
                {
                    allPowers.Add(JsonNode.Parse(power.ToJsonString())!);
                }
            }

            powers["objects"] = allPowers;
        }
    }
}