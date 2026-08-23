using System.Text;
using XflComponents;
using UniversalMethods;
using System.Text.Json;
using System.Globalization;
using System.Xml;

namespace HelperFunctions.Functions.Packages
{
    public class CheckActionFrames
    {
        private static readonly HashSet<string> packagesFiles = ["ZOMBIETYPES.JSON", "PLANTTYPES.JSON", "GRIDITEMTYPES.JSON", 
        "COLLECTABLETYPES.JSON", "CREATURETYPES.JSON", "PROJECTILETYPES.JSON", "ARMORTYPES.JSON"];
        private static Dictionary<string, HashSet<string>>? collectedCodenames = [];
        private static readonly Dictionary<string, string> packageToType = new(){
            {"spawn_zombie", "ZOMBIETYPES"},
            {"spawn_plant", "PLANTTYPES"},
            {"spawn_grid", "GRIDITEMTYPES"},
            {"spawn_collectable", "COLLECTABLETYPES"},
            {"set_sky_collectable", "COLLECTABLETYPES"},
            {"spawn_projectile", "PROJECTILETYPES"},
            {"spawn_creature", "CREATURETYPES"},
            {"apply_armor", "ARMORTYPES"}
        };
        static HashSet<string> animLabels = [];

        private static readonly HashSet<string> currentActionFrames = [
            "spawn_zombie", "spawn_plant", "spawn_grid",
            "spawn_collectable", "spawn_projectile", "spawn_creature",
            "apply_armor", "die", "destroy", "transform", "use_action_with_index",
            "set_sky_collectable", "disable_sundropper", "enable_sundropper",
            "set_invisible", "set_visible", "set_plantfood", "play_anim",
            "set_render_layer", "sink_start", "sink_stop"
        ];
        private static readonly HashSet<string> soloArgs = [
            "IfAlive", "IfDead", "OnWater", "!OnWater", "OffsetByGrid", "OffsetByCoords",
            "DisplacePlant", "IgnoreGridLayers", "SetPosition"
        ];
        private static readonly HashSet<string> listArgs = ["Include", "Exclude", "HasConditions", "TransformArgs"];
        private static readonly HashSet<string> colonArgs = [
          "Type", "ActionIndex", "mX", "mY", "mZ", "HpBelow", "HpAbove",
            "IfTeam", "Chance", "Team", "TimeAlive", "PastXLocation", "BeforeXLocation", "OnXLocation",
            "PastYLocation", "BeforeYLocation", "OnYLocation", "SkyCollectable", "HasSun", "Value", "IfEntity"
        ];

        public static void Function()
        {
            // Get DOMDocument and package files
            UM.PrintColoredText(
            [
                (ConsoleColor.DarkCyan, "Enter an "),
                (ConsoleColor.Green, "XFL"),
                (ConsoleColor.DarkCyan, " or "),
                (ConsoleColor.Green, "DOMDocument\n")
            ]);
            var DOMDocumentObject = AskForDOMDocument();
            if (DOMDocumentObject.GetActionLayer() is null)
            {
                UM.PrintColoredText(ConsoleColor.Yellow, "No action layer found, ending function");
                return;
            }
            collectedCodenames = AskForPackages();
            UM.PrintColoredText(ConsoleColor.Green, "Retrieving action frames... ");

            // Setup to get all the possible args and labels inside XFL
            var fullArgs = soloArgs.ToList();
            fullArgs.AddRange(colonArgs);
            fullArgs.AddRange(listArgs);
            animLabels = DOMDocumentObject.GetAllLabels().ToHashSet();

            // Get all of the action frames found inside the DOMDocument, separated by type and args
            var actionFrameList = GetAllActionFrames(DOMDocumentObject);
            ProgressChecker.WriteFinished();
            if (actionFrameList.Count == 0) // If no action frames are found, terminate process
            {
                UM.PrintColoredText(ConsoleColor.Yellow, "No action frames found, ending process", separateLines:true);
                return;
            }
            bool errorMessage = false; // Tracks if any error messages are found

            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var (actionType, actionArgs, index) in actionFrameList)
            {
                // Check through every action frame
                var result = CheckActionFrame(actionType, actionArgs);
                if (result is null) continue; // If no errors were found, move on
                if (!errorMessage)
                {
                    errorMessage = true;
                    Console.WriteLine();
                }

                // Write out error message
                UM.PrintColoredText(
                [
                    (ConsoleColor.Green, $"Index {index + 1}: "),
                    (ConsoleColor.Magenta, $"\"{actionType}\", \"{RebuildArgs(actionArgs)}\""),
                    (ConsoleColor.Red, "\nFOUND ERRORS:\n"),
                    (ConsoleColor.DarkCyan, $"{result}\n")
                ]);
            }
            // If no errors have been found
            if (!errorMessage)
            {
                UM.PrintColoredText(ConsoleColor.Yellow, "No errors found");
            }

        }

        private static DOMDocument AskForDOMDocument()
        {
            while (true)
            {
                // Ask for path to DOMDocument
                var (inputPath, isFile) = UserPrompts.AskForPath([XFL.DOMDocumentFileName]);
                Console.ForegroundColor = ConsoleColor.Red;
                if (!isFile) // If an XFL is given instead, change the path to the DOMDocument directly
                {
                    inputPath = Path.Join(inputPath, XFL.DOMDocumentFileName);
                }

                if (!File.Exists(inputPath))
                {
                    Console.WriteLine($"Could not find file {inputPath}, enter again");
                    continue;
                }
                
                try
                {
                    // Deserialize DOMDocument
                    return UM.GetDOMDocument(inputPath);
                }
                catch (XmlException)
                {
                    Console.WriteLine($"{XFL.DOMDocumentFileName} is an invalid XML, enter again");
                    continue;
                }
                catch (IOException)
                {
                    Console.WriteLine($"Could not access {XFL.DOMDocumentFileName}, enter again");
                    continue;
                }
                catch
                {
                    Console.WriteLine("Could not read DOMDocument.xml, enter again");
                    continue;
                }
            }
        }
    
        private static Dictionary<string, HashSet<string>>? AskForPackages()
        {
            // Prompt message
            UM.PrintColoredText(
            [
                (ConsoleColor.DarkCyan, "Enter the packages dir you want to compare"),
                (ConsoleColor.Yellow, " (or enter nothing if you want to skip checking invalid entities)\n")
            ]);

            var packagesDir = UserPrompts.AskForDirectory(packagesFiles.ToArray(), allowNoAnswer:true);
            if (string.IsNullOrWhiteSpace(packagesDir)) return null; // If no answer was given, allow codenames to be null
            Dictionary<string, HashSet<string>> collectedCodenames = [];

            // Loop through the want packages files
            foreach (var package in packagesFiles)
            {
                HashSet<string> codenames = [];

                // Get path to package file and turn into JsonnOde
                var packageDir = Path.Join(packagesDir, package);
                var packagesFile = JsonMethods.GetJsonFile(packageDir)!;

                // Check alias based files
                if (package == "ARMORTYPES.JSON" || package == "PROJECTILETYPES.JSON")
                {
                    foreach (var packageObject in packagesFile["objects"]!.AsArray())
                    {
                        if (packageObject!.AsObject().ContainsKey("aliases"))
                        {
                            // Turn aliases into string list and add to codenames
                            var aliases = JsonSerializer.Deserialize<List<string>>(packageObject["aliases"]!)!;
                            aliases.ForEach(a => codenames.Add(a));
                        }
                    }

                }
                // Check TypeName based files
                else
                {
                    foreach (var packageObject in packagesFile["objects"]!.AsArray())
                    {
                        // Same deal for this try-catch block
                        try
                        {
                            if (packageObject!.AsObject().ContainsKey("objdata"))
                            {
                                var objdata = packageObject["objdata"]!;
                                if (objdata.AsObject().ContainsKey("TypeName"))
                                {
                                    var typename = ((string)objdata["TypeName"]!.AsValue())!;
                                    codenames.Add(typename);
                                }
                            }
                        }
                        catch{} 
                    }
                }
                collectedCodenames.Add(Path.ChangeExtension(package, null), codenames); // Add found codenames

            }
            return collectedCodenames;
        }
    
        private static List<(string actionType, List<string> actionArgs, int index)> GetAllActionFrames(DOMDocument DOMDocumentObject)
        {
            // Get every single frame found in DOMDocument
            var actionFrameList = new List<(string actionType, string actionArgs, int index)>();
            
            List<(List<string> actionScripts, int index)> actionScriptIndexes = DOMDocumentObject.GetActionLayer()!.Frames
                                                                .Where(f => f.GetActionScripts().Count > 0)
                                                                .Select(f => (f.GetActionScripts(splitLines:true), f.Index))
                                                                .ToList();
            if (actionScriptIndexes.Count == 0) return []; // If no action scripts found, return nothing
            
            var actionframePrefix = "fscommand(\"";
            foreach (var (actionScripts, index) in actionScriptIndexes)
            {
                foreach (var actionScript in actionScripts)
                {
                    if (!actionScript.StartsWith(actionframePrefix)) continue; // If the action frame doesn't start with fscommand, skip it
                    var currentActionFrame = actionScript.Replace(" ", string.Empty); // Remove spaces
                    if (currentActionFrame.EndsWith(';')) currentActionFrame += ";";
                    currentActionFrame = currentActionFrame.Substring(
                        actionframePrefix.Length, currentActionFrame.Length - 14); // Remove fscommand part and parenthesis
                    var splitAction = currentActionFrame.Split('"');
                    if (splitAction.Length != 3) continue;
                    var actionType = splitAction[0]; // First part will be the action type
                    if (!currentActionFrames.Contains(actionType)) continue; // If that type of action frame doesn't exist, skip it

                    var actionArgs = splitAction[2]; // Second part will be the args
                    actionFrameList.Add((actionType, actionArgs, index));
                }
            }
            actionFrameList = RemoveDuplicateActions(actionFrameList);
            var fullActionFrameList = SplitActionArgs(actionFrameList);
            return fullActionFrameList;
        }

        private static List<(string actionType, string actionArgs, int index)> RemoveDuplicateActions
        (List<(string actionType, string actionArgs, int index)> actionFrameList)
        {
            var addedActionFrames = new HashSet<(string actionType, string actionArgs, int index)>();
            var newActionFrameList = new List<(string actionType, string actionArgs, int index)>();
            foreach (var action in actionFrameList)
            {
                void addToActionFrameList() { newActionFrameList.Add(action); }
                if (newActionFrameList.Count == 0)
                {
                    addToActionFrameList();
                    continue;
                }
                if (addedActionFrames.Add(action))
                    addToActionFrameList();
            }

            return newActionFrameList;
        }

        private static List<(string actionType, List<string> actionArgs, int index)> SplitActionArgs(List<(string actionType, string actionArgs, int index)> actionFrameList)
        {
            List<(string actionType, List<string> actionArgs, int index)> fullActionFrameList = [];
            foreach (var actionFramePair in actionFrameList)
            {
                var (actionType, compositeArgs, index) = actionFramePair;
                List<string> actionArgs = [];
                string currentArg = string.Empty;
                bool isCheckingList = false; // Toggles to true if a list arg is being parsed
                foreach (char argChar in compositeArgs)
                {
                    // If a list arg is detected, switch to it
                    if (argChar == '[')
                    {
                        isCheckingList = true;
                        currentArg += '[';
                    }
                    // If a list arg is detected to end, switch off of it
                    else if (argChar == ']')
                    {
                        isCheckingList = false;
                        currentArg += ']';
                    }
                    // If an end to an arg is detected, add it to the list of args
                    else if (argChar == ',' && isCheckingList == false)
                    {
                        actionArgs.Add(currentArg);
                        currentArg = string.Empty;
                    }
                    // If none of the above is detected, and it to the current arg being built
                    else
                    {
                        currentArg += argChar;
                    }
                }
                if (currentArg != string.Empty)
                {
                    actionArgs.Add(currentArg);
                }

                fullActionFrameList.Add((actionType, actionArgs, index));
            }
            return fullActionFrameList;
        }
    
        private static string? CheckActionFrame(string actionType, List<string> actionArgs)
        {
            StringBuilder result = new(); // Error message to be returned

            // Keep track of duplicate args when they come up
            List<string> addedArgs = [];

            // Check if each arg exists
            foreach (var actionArg in actionArgs)
            {
                // If the arg is assumed to be a colon arg
                if (actionArg.Contains(':'))
                {
                    var splitArgs = actionArg.Split(':');
                    if (splitArgs.Length != 2) // If there are too many colons found in the arg
                    {
                        result.AppendLine($"Argument has too many colons, should only contain 1: [{actionArg}]");
                        continue;
                    }

                    var frontArg = splitArgs[0];
                    if (!colonArgs.Contains(frontArg)) // If the added colon arg is not a valid colon arg
                    {
                        result.AppendLine($"Invalid argument: [{actionArg}]");
                        continue;
                    }
                    else if (addedArgs.Contains(frontArg)) // If the colon arg is already added, there should only be one
                    {
                        result.AppendLine($"Argument {frontArg} is duplicated, should only appear once");
                    }
                    else
                    {
                        addedArgs.Add(frontArg);
                    }

                    var backArg = splitArgs[1];
                    var toAdd = CheckColonArg(frontArg, backArg, actionType);
                    if (toAdd != string.Empty) result.Append(toAdd);
                }

                // If the arg is assumed to be a list arg
                else if (actionArg.Contains('['))
                {
                    var splitArgs = actionArg.Split('[');
                    if (splitArgs.Length != 2) // If there are too many '['s found
                    {
                        result.AppendLine($"Argument has too many '[', should only contain 1: [{actionArg}]");
                        continue;
                    }
                    var frontArg = splitArgs[0];
                    if (!listArgs.Contains(frontArg)) // If the list arg is not a valid list arg
                    {
                        result.AppendLine($"Invalid argument: [{actionArg}]");
                        continue;
                    }
                    else if (addedArgs.Contains(frontArg)) // If the list arg is already added
                    {
                        result.AppendLine($"Argument {frontArg} is duplicated, should only appear once");
                    }

                    var backArg = splitArgs[1].Replace("]", string.Empty);
                    var toAdd = CheckListArg(frontArg, backArg.Split(","));
                    if (toAdd != string.Empty) result.Append(toAdd);
                }

                // If the arg is assumed to be a solo arg (doesn't contain a colon or square bracket)
                else
                {
                    if (!soloArgs.Contains(actionArg))
                    {
                        result.AppendLine($"Invalid argument: [{actionArg}]");
                    }
                    else if (addedArgs.Contains(actionArg))
                    {
                        result.AppendLine($"Argument {actionArg} is duplicated, should only appear once");
                    }
                }
            }

            // Check arg specific things
            if (actionType == "set_render_layer") // Check for Value: in set_render_layer
            {
                if (!ContainsArgType(actionArgs, "Value:"))
                {
                    result.AppendLine($"A set_render_layer action frame doesn't have an assigned Value");
                }
            }

            else if (actionType == "use_action_with_index") // Check for ActionIndex: in use_action_with_index
            {
                if(!ContainsArgType(actionArgs, "ActionIndex:"))
                {
                    result.AppendLine($"A use_action_with_index action frame doesn't have an assigned ActionIndex");
                }
            }

            else if (actionType == "transform") // Check for TransformArgs in transform
            {
                if (!ContainsArgType(actionArgs, "TransformArgs["))
                {
                    result.AppendLine($"A transform action frame does not have assigned TransformArgs");
                }
            }

            else if (packageArgs.Contains(actionType)) // Check for Type: in action types that use it
            {
                if (!ContainsArgType(actionArgs, "Type:"))
                {
                    result.AppendLine($"A {actionType} action frame does not have an assigned type");
                }
            }

            // Check for args that conflict with each other
            static string RemoveExtraChars(string userInput) => userInput.Replace(":", string.Empty).Replace("[", string.Empty);
            foreach (var (arg1, arg2) in noPairArgs)
            {
                if (CheckConflictingArgs(arg1, arg2, actionArgs))
                {

                    var front = $"{RemoveExtraChars(arg1)} and {RemoveExtraChars(arg2)}";
                    result.AppendLine($"{front} shouldn't be together to prevent unpredictable behavior");
                }
            }

            // Check for args that shouldn't higher/lower than another
            foreach (var (minArg, maxArg) in noPairMinMaxargs)
            {
                if (CheckConflictingMinMaxArgs(minArg, maxArg, actionArgs))
                {
                    result.AppendLine($"{minArg}'s value should be less than {maxArg}'s, will not trigger otherwise");
                }
            }

            var toReturn = result.ToString();
            if (toReturn == string.Empty)
            {
                return null;
            }
            return toReturn;

        }

        private static string CheckColonArg(string argFront, string argBack, string actionType)
        {
            StringBuilder result = new();
            if (intArgs.Contains(argFront)) // Check for args that are meant to use an integer
            {
                if (!int.TryParse(argBack,
                                  NumberStyles.Integer,
                                  CultureInfo.InvariantCulture,
                                  out _))
                {
                    result.AppendLine($"{argFront} arg does not use an int, ensure it does: [{argFront}:{argBack}]");
                }
            }

            else if (floatArgs.Contains(argFront)) // Check for args that are meant to use a float
            {
                if (!float.TryParse(argBack,
                                    NumberStyles.Float,
                                    CultureInfo.InvariantCulture,
                                    out _))
                {
                    result.AppendLine($"{argFront} arg does not use a float, ensure it does: [{argFront}:{argBack}]");
                }
            }

            else if (argFront == "IfTeam" || argFront == "Team") // Check team based args
            {
                if (!validTeams.Contains(argBack))
                {
                    result.AppendLine($"{argFront} uses a team that doesn't exist: [{argFront}:{argBack}]");
                }
            }
            
            else if (argFront == "Type") // Check that Type args refer to something valid
            {
                if (actionType == "play_anim") // Check if the anim refered to exists
                {
                    if (!animLabels.Contains(argBack))
                    {
                        result.AppendLine($"{actionType} refers to a nonexistent label in DOMDocument: [{argFront}:{argBack}]");
                    }
                }
                else if (collectedCodenames is not null && packageToType.TryGetValue(actionType, out var collectionToCheck)) // Check ones that refer to packages
                {
                    if (!collectedCodenames[collectionToCheck].Contains(argBack))
                    {
                        result.AppendLine($"{actionType} refers to a nonexistent entity in {collectionToCheck}: [{argFront}:{argBack}]");
                    }
                }
                
                
            }

            else if (argFront == "SkyCollectable" &&
            collectedCodenames is not null) // Check SkyCollectable refers to a valid collectable type
            {
                if (!collectedCodenames["COLLECTABLETYPES"].Contains(argBack))
                {
                    result.AppendLine($"{argFront} refers to a nonexistent entity in COLLECTABLETYPES: [{argFront}:{argBack}]");
                }
            }

            else if (argFront == "IfEntity") // Check IfEntity refers to a valid entity type
            {
                List<string> validEntities = ["Plant", "Zombie", "GridItem"];
                if (!validEntities.Contains(argBack))
                {
                    result.AppendLine($"{argFront} refers to a nonexistent entity type: [{argFront}:{argBack}]");
                }
            }
            return result.ToString();
        }

        private static string CheckListArg(string argFront, string[] argBack)
        {
            StringBuilder result = new();

            // Check if HasConditions refers to only valid conditions
            if (argFront == "HasConditions")
            {
                string invalidConditions = "[";
                foreach (var condition in argBack)
                {
                    if (!validConditions.Contains(condition)) // If a nonexistent condition is found
                    {
                        if (invalidConditions.Length == 1)
                            invalidConditions += $"{condition}";
                        else
                            invalidConditions += $", {condition}";
                    }
                }
                invalidConditions += "]";
                if (invalidConditions.Length != 2)
                {
                    result.AppendLine($"The following nonexistent conditions were added in a HasConditions arg: [{invalidConditions}]");
                }
            }
            
            // Check if Include/Exclude refers to only existing entities
            else if (argFront == "Include" || argFront == "Exclude"
                    && collectedCodenames is not null)
            {
                HashSet<string> packagesToCheck = [..collectedCodenames!.GetValueOrDefault("ZOMBIETYPES", []),
                                       ..collectedCodenames!.GetValueOrDefault("PLANTTYPES", [])];

                var invalidCodenames = "[";
                foreach (var typename in argBack)
                {
                    if (!packagesToCheck.Contains(typename)) // If a noexistent entity is found 
                    {
                        if (invalidCodenames.Length == 1)
                        {
                            invalidCodenames += $"{typename}";
                        }
                        else
                        {
                            invalidCodenames += $", {typename}";
                        }
                    }
                }
                invalidCodenames += "]";
                if (invalidCodenames.Length != 2)
                {
                    result.AppendLine($"The following nonexistent codenames were found in a {argFront} list: {invalidCodenames}");
                }
            }

            // Transform check
            else if (argFront == "TransformArgs")
            {
                if (argBack.Length < 2) // If not enough arguments for TransformArgs were provided
                {
                    result.AppendLine("The TransformArgs do not have enough arguments");
                }
                else {
                    var transformType = argBack[0];
                    if (!transformTypeToCollection.ContainsKey(transformType)) // If the transform type doesn't exist
                    {
                        result.AppendLine($"The following transform type doesn't exist: [{transformType}]");
                    }
                    else if (collectedCodenames is not null) // Check if the spawned entity exists
                    {
                        var typename = argBack[1];
                        var collectionToCheck = transformTypeToCollection[transformType];
                        if (!collectedCodenames[collectionToCheck].Contains(typename)) // If the entity found doesn't exist
                        {
                            result.AppendLine($"The following transform typename is not found in {collectionToCheck}: {typename}");
                        }
                    }
                    
                    // Check miscellaneous args if they exist
                    for (int i = 2; i < argBack.Length; i++)
                    {
                        var transformArg = argBack[i];
                        if (!validTransformArgs.Contains(transformArg))
                        {
                            result.AppendLine($"The following transform arg does not exist: {transformArg}");
                        }
                    }
                }
            }
            return result.ToString();
        }

        private static bool ContainsArgType(List<string> actionArgs, string wantedArg)
        => actionArgs.Any(a => a.StartsWith(wantedArg));

        private static bool CheckConflictingArgs(string arg1, string arg2, List<string> actionArgs) 
        => actionArgs.Any(a => a.StartsWith(arg1)) && actionArgs.Any(a => a.StartsWith(arg2));

        private static bool CheckConflictingMinMaxArgs(string minArg, string maxArg, List<string> actionArgs)
        {
            float? minArgValue = null, maxArgValue = null;
            static float? CheckList(string input, string argType)
            {
                if (!input.StartsWith(argType) ||
                input.Split(":").Length != 2
                || !float.TryParse(input.Split(":")[1], out float toAddArgType)) return null;
                return toAddArgType;
            }
            foreach (var actionArg in actionArgs)
            {
                minArgValue ??= CheckList(actionArg, minArg);
                maxArgValue ??= CheckList(actionArg, maxArg);
                if (minArgValue is not null && maxArgValue is not null) break;
            }
            
            if (minArgValue is not null && maxArgValue is not null &&
            minArgValue >= maxArgValue)
            {
                return true;
            }
            return false;
        }

        private static string RebuildArgs(List<string> actionArgs) => string.Join(", ", actionArgs);

        private static readonly HashSet<string> validConditions = ["lightning", "tossed", "warpingIn", "potionspeed1", "potionspeed2", "potionspeed3",
        "potiontoughness1", "potiontoughness2", "potiontoughness3", "potionsuper1", "potionsuper2", "potionsuper3", "hypnotized",
        "sunbeaned", "morphedtogargantuar", "hasplantfood", "damageflash", "zombossstun", "haunted", "sapped", "unsuspendable",
        "speeddown1", "speeddown2", "speeddown3", "speeddown4", "warpingOut", "terrified", "shrinking", "contagiouspoison", "decaypoison",
        "bleeding", "bloomingheartdebuff", "hotdateattraction", "solarflared", "suiciding", "stackableslow", "suncarrier250", 
        "suncarrier50", "suncarrier100", "dazeystunned", "iceblocked", "gummed", "stickybombed", "petrified", "invisibleslow",
        "concealmintdamagescale", "poweredconcealmintdamagescalepowered", "corpseexplosion", "rapidfire", "plantfoodflash",
        "highlighted", "froststage1", "froststage2", "notfiring", "stunnedbyzombielove", "supershadowboosted", "lifted_off",
        "pvineboosted1", "pvineboosted2", "pvineboosted3", "chill", "flashing", "butter", "freeze", "stalled", "blockolistunned",
        "hungered", "speedup1", "speedup2", "speedup3", "speedup4", "present_boxed", "icecubed", "invincible", "squidified",
        "sheeped", "rush", "stun", "weaken1", "weaken2", "weaken3", "weaken4"];        
        private static readonly HashSet<string> intArgs = ["PastXLocation", "BeforeXLocation", "OnXLocation",
        "PastYLocation", "BeforeYLocation", "OnYLocation", "ActionIndex"];
        private static readonly HashSet<string> floatArgs = ["mX", "mY", "mZ", "HpBelow", "HpAbove", "Chance", "TimeAlive", "HasSun"];
        private static readonly HashSet<string> validTeams = ["Plant", "Zombie", "Neutral", "None"];
        private static readonly HashSet<string> validTransformArgs = ["KeepArmor", "KeepConditions", "KeepHP"];
        private static readonly Dictionary<string, string> transformTypeToCollection = new()
        {
            {"Zombie", "ZOMBIETYPES"},
            {"Plant", "PLANTTYPES"},
            {"Grid", "GRIDITEMTYPES"},
            {"Collectable", "COLLECTABLETYPES"},
            {"Projectile", "PROJECTILETYPES"},
            {"Creature", "CREATURETYPES"}
        };
        private static readonly HashSet<string> packageArgs = ["spawn_zombie", "spawn_plant", "spawn_grid", "spawn_collectable",
        "spawn_projectile", "spawn_creature", "apply_armor", "set_sky_collectable"];
        private static readonly HashSet<(string arg1, string arg2)> noPairArgs = [
            ("OnWater", "!OnWater"),
            ("OffsetByGrid", "OffsetByCoords"),
            ("Include[", "Exclude["),
            ("IfAlive", "IfDead"),
            ("OnXLocation:", "PastXLocation:"),
            ("OnXLocation:", "BeforeXLocation:"),
            ("OnYLocation:", "PastYLocation:"),
            ("OnYLocation:", "BeforeYLocation:")
        ];
        private static readonly HashSet<(string minArg, string maxArg)> noPairMinMaxargs = [
            ("HpAbove", "HpBelow"),
            ("PastXLocation", "BeforeXLocation"),
            ("PastYLocation", "BeforeYLocation")
        ];
    }
}