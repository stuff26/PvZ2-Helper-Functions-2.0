using System.Text;
using System.Xml.Linq;
using XflComponents;
using UniversalMethods;
using System.Text.Json;

namespace HelperFunctions.Functions.Packages
{
    public class CheckActionFrames
    {
        readonly static List<string> packagesFiles = ["ZOMBIETYPES.JSON", "PLANTTYPES.JSON", "GRIDITEMTYPES.JSON", 
        "COLLECTABLETYPES.JSON", "CREATURETYPES.JSON", "PROJECTILETYPES.JSON", "ARMORTYPES.JSON"];
        static Dictionary<string, List<string>>? collectedCodenames = [];
        readonly static Dictionary<string, string> packageToType = new(){
            {"spawn_zombie", "ZOMBIETYPES"},
            {"spawn_plant", "PLANTTYPES"},
            {"spawn_grid", "GRIDITEMTYPES"},
            {"spawn_collectable", "COLLECTABLETYPES"},
            {"set_sky_collectable", "COLLECTABLETYPES"},
            {"spawn_projectile", "PROJECTILETYPES"},
            {"spawn_creature", "CREATURETYPES"},
            {"apply_armor", "ARMORTYPES"}
        };

        readonly static List<string> currentActionFrames = [
            "spawn_zombie", "spawn_plant", "spawn_grid",
            "spawn_collectable", "spawn_projectile", "spawn_creature",
            "apply_armor", "die", "destroy", "transform", "use_action_with_index",
            "set_sky_collectable", "disable_sundropper", "enable_sundropper",
            "set_invisible", "set_visible", "set_plantfood", "play_anim",
            "set_render_layer", "sink_start", "sink_stop"
        ];
        readonly static List<string> soloArgs = [
            "IfAlive", "IfDead", "OnWater", "!OnWater", "OffsetByGrid", "OffsetByCoords",
            "DisplacePlant", "IgnoreGridLayers", "SetPosition"
        ];
        readonly static List<string> listArgs = ["Include", "Exclude", "HasConditions", "TransformArgs"];
        readonly static List<string> colonArgs = [
          "Type", "ActionIndex", "mX", "mY", "mZ", "HpBelow", "HpAbove",
            "IfTeam", "Chance", "Team", "TimeAlive", "PastXLocation", "BeforeXLocation", "OnXLocation",
            "PastYLocation", "BeforeYLocation", "OnYLocation", "SkyCollectable", "HasSun", "Value", "IfEntity"
        ];

        public static void Function()
        {
            // Get DOMDocument and package files
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter an XFL or DOMDocument");
            var DOMDocumentObject = AskForDOMDocument();
            collectedCodenames = AskForPackages();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Retrieving action frames... ");

            // Setup to get all the possible args and labels inside XFL
            var fullArgs = soloArgs.ToList();
            fullArgs.AddRange(colonArgs);
            fullArgs.AddRange(listArgs);
            animLabels = DOMDocumentObject.GetAllLabels();

            // Get all of the action frames found inside the DOMDocument, separated by type and args
            var actionFrameList = GetAllActionFrames(DOMDocumentObject);
            ProgressChecker.WriteFinished();
            if (actionFrameList.Count == 0) // If no action frames are found, terminate process
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No action frames found");
                return;
            }
            bool errorMessage = false; // Tracks if any error messages are found

            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var (actionType, actionArgs) in actionFrameList)
            {
                // Check through every action frame
                var result = CheckActionFrame(actionType, actionArgs, fullArgs);
                if (result is null) continue; // If no errors were found, move on
                errorMessage = true;

                // Write out error message
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.Write("FOUND ERROR: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"\"{actionType}\", ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"\"{RebuildArgs(actionArgs)}\"");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write(result);
            }
            // If no errors have been found
            if (!errorMessage)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No errors found");
            }

        }

        public static DOMDocument AskForDOMDocument()
        {
            while (true)
            {
                // Ask for path to DOMDocument
                var (inputPath, isFile) = UM.AskForPath(["DOMDocument.xml"]);
                if (!isFile) // If an XFL is given instead, change the path to the DOMDocument directly
                {
                    inputPath = Path.Join(inputPath, "DOMDocument.xml");
                }
                
                try
                {
                    // Deserialize DOMDocument
                    XDocument document = XDocument.Load(inputPath!);
                    using var documentReader = document.CreateReader();
                    DOMDocument DOMDocumentObject = (DOMDocument?)DOMDocument.serializer.Deserialize(documentReader)!;
                    return DOMDocumentObject;
                }
                catch
                {
                    // Something went wrong reading DOMDocument, such as syntax error
                    Console.WriteLine("Could not read DOMDocument.xml, enter again");
                    continue;
                }
            }
        }
    
        public static Dictionary<string, List<string>>? AskForPackages()
        {
            // Prompt message
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter the packages dir you want to compare");
            Console.WriteLine("(or enter nothing if you want to skip checking invalid entities)");

            var packagesDir = UM.AskForDirectory([.. packagesFiles], allowNoAnswer:true);
            if (packagesDir == "") return null; // If no answer was given, allow codenames to be null
            Dictionary<string, List<string>> collectedCodenames = [];

            // Loop through the want packages files
            foreach (var package in packagesFiles)
            {
                List<string> codenames = [];

                // Get path to package file and turn into JsonnOde
                var packageDir = Path.Join(packagesDir, package);
                var packagesFile = UM.GetJsonFile(packageDir)!;

                // Check alias based files
                if (package == "ARMORTYPES.JSON" || package == "PROJECTILETYPES.JSON")
                {
                    try
                    {
                        // Try-catch block is mostly just for cases of duplicate keys, in which case object will be skipped
                        // (will be extremely rare so there won't be any workaround for it)
                        foreach (var packageObject in packagesFile["objects"]!.AsArray())
                        {
                            if (packageObject!.AsObject().ContainsKey("aliases"))
                            {
                                // Turn aliases into string list and add to codenames
                                var aliases = JsonSerializer.Deserialize<List<string>>(packageObject["aliases"]!)!;
                                codenames.AddRange(aliases);
                            }
                        }
                    }
                    catch {}

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
                collectedCodenames.Add(package.Replace(".JSON", ""), codenames); // Add found codenames

            }
            return collectedCodenames;
        }
    
        public static List<(string actionType, List<string> actionArgs)> GetAllActionFrames(DOMDocument DOMDocumentObject)
        {
            // Get every single frame found in DOMDocument
            var actionScripts = DOMDocumentObject.Timeline.GetActionScripts();
            if (actionScripts.Count == 0) return []; // If no action scripts found, return nothing
            var actionFrameList = new List<(string actionType, string actionArgs)>();

            // Loop through every separated action frame
            foreach (var actionFrame in actionScripts)
            {
                if (!actionFrame.StartsWith("fscommand")) continue; // If the action frame doesn't start with fscommand, skip it
                var currentActionFrame = actionFrame.Replace(" ", ""); // Remove spaces
                currentActionFrame = currentActionFrame.Substring(11, currentActionFrame.Length - 14); // Remove fscommand part and parenthesis
                var splitAction = currentActionFrame.Split('"');
                var actionType = splitAction[0]; // First part will be the action type
                if (!currentActionFrames.Contains(actionType)) continue; // If that type of action frame doesn't exist, skip it

                var actionArgs = splitAction[2]; // Second part will be the args
                actionFrameList.Add((actionType, actionArgs));
            }

            actionFrameList = RemoveDuplicateActionFrames(actionFrameList);
            var fullActionFrameList = SplitActionArgs(actionFrameList);
            return fullActionFrameList;
        }

        public static List<(string actionType, List<string> actionArgs)> SplitActionArgs(List<(string actionType, string actionArgs)> actionFrameList)
        {
            List<(string actionType, List<string> actionArgs)> fullActionFrameList = [];
            foreach (var actionFramePair in actionFrameList)
            {
                var (actionType, compositeArgs) = actionFramePair;
                List<string> actionArgs = [];
                string currentArg = "";
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
                        currentArg = "";
                    }
                    // If none of the above is detected, and it to the current arg being built
                    else
                    {
                        currentArg += argChar;
                    }
                }
                if (currentArg != "")
                {
                    actionArgs.Add(currentArg);
                }

                fullActionFrameList.Add((actionType, actionArgs));
            }
            return fullActionFrameList;
        }

        public static List<(string actionType, string actionArgs)> RemoveDuplicateActionFrames(
            List<(string actionType, string actionArgs)> actionFrameList)
        {
            Dictionary<string, List<string>> duplicateTracker = [];
            List<(string actionType, string actionArgs)> keptActionFrames = [];
            foreach (var actionFramePair in actionFrameList)
            {
                var (actionType, actionArgs) = actionFramePair;
                if (!duplicateTracker.TryGetValue(actionType, out List<string>? duplicateArgs)) // If no duplicate arg type is found
                {
                    keptActionFrames.Add((actionType, actionArgs));
                    duplicateArgs = [actionArgs];
                    duplicateTracker.Add(actionType, duplicateArgs);
                    continue;
                }

                if (!duplicateArgs.Contains(actionArgs))
                {
                    keptActionFrames.Add((actionType, actionArgs));
                    duplicateTracker[actionType].Add(actionArgs);
                }
            }
            return keptActionFrames;
        }
    
        public static string? CheckActionFrame(string actionType, List<string> actionArgs, List<string> fullArgs)
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
                    if (toAdd != "") result.AppendLine(toAdd);
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

                    var backArg = splitArgs[1].Replace("]", "");
                    var toAdd = CheckListArg(frontArg, backArg.Split(","));
                    if (toAdd != "") result.AppendLine(toAdd);
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
            Func<string, string> removeExtraChars = userInput => userInput.Replace(":", "").Replace("[", "");
            foreach (var (arg1, arg2) in noPairArgs)
            {
                if (CheckConflictingArgs(arg1, arg2, actionArgs))
                {

                    var front = $"{removeExtraChars(arg1)} and {removeExtraChars(arg2)}";
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
            if (toReturn == "")
            {
                return null;
            }
            return toReturn;

        }

        public static string CheckColonArg(string argFront, string argBack, string actionType)
        {
            StringBuilder result = new();
            if (intArgs.Contains(argFront)) // Check for args that are meant to use an integer
            {
                if (!int.TryParse(argBack, out _))
                {
                    result.AppendLine($"{argFront} arg does not use an int, ensure it does: [{argFront}:{argBack}]");
                }
            }

            else if (floatArgs.Contains(argFront)) // Check for args that are meant to use a float
            {
                if (!float.TryParse(argBack, out _))
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
                        result.AppendLine($"{actionType} refers to a nonexistent label in DOMDocument: {argFront}:{argBack}");
                    }
                }
                else if (collectedCodenames is not null && packageToType.ContainsKey(actionType)) // Check ones that refer to packages
                {
                    var collectionToCheck = packageToType[actionType];
                    if (!collectedCodenames[collectionToCheck].Contains(argBack))
                    {
                        result.AppendLine($"{actionType} refers to a nonexistent entity in {collectionToCheck}: {argFront}:{argBack}");
                    }
                }
                
                
            }

            else if (argFront == "SkyCollectable" &&
            collectedCodenames is not null) // Check SkyCollectable refers to a valid collectable type
            {
                if (!collectedCodenames["COLLECTABLETYPES"].Contains(argBack))
                {
                    result.AppendLine($"{argFront} refers to a nonexistent enttiy in COLLECTABLETYPES: {argFront}:{argBack}");
                }
            }

            else if (argFront == "IfEntity") // Check IfEntity refers to a valid entity type
            {
                List<string> validEntities = ["Plant", "Zombie", "GridItem"];
                if (!validEntities.Contains(argBack))
                {
                    result.AppendLine($"{argFront} refers to a nonexistent entity type: {argFront}:{argBack}");
                }
            }
            return result.ToString();
        }

        public static string CheckListArg(string argFront, string[] argBack)
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
                    result.AppendLine($"The following nonexistent conditions were added in a HasConditions arg: {invalidConditions}");
                }
            }
            
            // Check if Include/Exclude refers to only existing entities
            else if (argFront == "Include" || argFront == "Exclude")
            {
                var packagesToCheck = collectedCodenames!["ZOMBIETYPES"].ToList();
                List<string> packagesToAdd = ["PLANTTYPES", "GRIDITEMTYPES", "PROJECTILETYPES", "CREATURETYPES"];
                foreach (var package in packagesToAdd) packagesToCheck.AddRange(collectedCodenames![package]);

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
                if (invalidCodenames.Length == 2)
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
                        result.AppendLine($"The following transform type doesn't exist: {transformType}");
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

        public static bool ContainsArgType(List<string> actionArgs, string wantedArg)
        {
            foreach (var actionArg in actionArgs)
            {
                if (actionArg.StartsWith(wantedArg))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CheckConflictingArgs(string arg1, string arg2, List<string> actionArgs)
        {
            bool hasArg1 = false, hasArg2 = false;
            foreach (var actionArg in actionArgs)
            {
                if (actionArg.StartsWith(arg1) || actionArg == arg1)
                    hasArg1 = true;
                if (actionArg.StartsWith(arg2) || actionArg == arg2)
                    hasArg2 = true;
                if (hasArg1 && hasArg2) return true;
            }
            return false;
        }

        public static bool CheckConflictingMinMaxArgs(string minArg, string maxArg, List<string> actionArgs)
        {
            float? minArgValue = null, maxArgValue = null;
            Func<string, string, float?> checklist = (string input, string argType) =>
            {
                if (!input.StartsWith(argType) ||
                input.Split(":").Length != 2
                || !float.TryParse(input.Split(":")[1], out float toAddArgType)) return null;
                return toAddArgType;
            };
            foreach (var actionArg in actionArgs)
            {
                minArgValue ??= checklist(actionArg, minArg);
                maxArgValue ??= checklist(actionArg, maxArg);
                if (minArgValue is not null && maxArgValue is not null) break;
            }
            
            if (minArgValue is not null && maxArgValue is not null &&
            minArgValue >= maxArgValue)
            {
                return true;
            }
            return false;
        }

        public static string RebuildArgs(List<string> actionArgs)
        {
            var args = "";
            for (int i = 0; i < actionArgs.Count; i++)
            {
                if (i == actionArgs.Count-1)
                {
                    args += actionArgs[i];
                }
                else
                {
                    args += $"{actionArgs[i]}, ";
                }
            }
            return args;
        }

        readonly static List<string> validConditions = ["lightning", "tossed", "warpingIn", "potionspeed1", "potionspeed2", "potionspeed3",
        "potiontoughness1", "potiontoughness2", "potiontoughness3", "potionsuper1", "potionsuper2", "potionsuper3", "hypnotized",
        "sunbeaned", "morphedtogargantuar", "hasplantfood", "damageflash", "zombossstun", "haunted", "sapped", "unsuspendable",
        "speeddown1", "speeddown2", "speeddown3", "speeddown4", "warpingOut", "terrified", "shrinking", "contagiouspoison", "decaypoison",
        "bleeding", "bloomingheartdebuff", "hotdateattraction", "solarflared", "suiciding", "stackableslow", "suncarrier250", 
        "suncarrier50", "suncarrier100", "dazeystunned", "iceblocked", "gummed", "stickybombed", "petrified", "invisibleslow",
        "concealmintdamagescale", "poweredconcealmintdamagescalepowered", "corpseexplosion", "rapidfire", "plantfoodflash",
        "highlighted", "froststage1", "froststage2", "notfiring", "stunnedbyzombielove", "supershadowboosted", "lifted_off",
        "pvineboosted1", "pvineboosted2", "pvineboosted3", "chill", "flashing", "butter", "freeze", "stalled", "blockolistunned",
        "hungered", "speedup1", "speedup2", "speedup3", "speedup4", "present_boxed", "icecubed", "invincible", "squidified",
        "sheeped", "rush", "stun"];        
        readonly static List<string> intArgs = ["PastXLocation", "BeforeXLocation", "OnXLocation",
        "PastYLocation", "BeforeYLocation", "OnYLocation", "ActionIndex"];
        readonly static List<string> floatArgs = ["mX", "mY", "mZ", "HpBelow", "HpAbove", "Chance", "TimeAlive", "HasSun"];
        readonly static List<string> validTeams = ["Plant", "Zombie", "Neutral", "None"];
        readonly static List<string> validTransformArgs = ["KeepArmor", "KeepConditions", "KeepHP"];
        readonly static Dictionary<string, string> transformTypeToCollection = new()
        {
            {"Zombie", "ZOMBIETYPES"},
            {"Plant", "PLANTTYPES"},
            {"Grid", "GRIDITEMTYPES"},
            {"Collectable", "COLLECTABLETYPES"},
            {"Projectile", "PROJECTILETYPES"},
            {"Creature", "CREATURETYPES"}
        };
        readonly static List<string> packageArgs = ["spawn_zombie", "spawn_plant", "spawn_grid", "spawn_collectable",
        "spawn_projectile", "spawn_creature", "apply_armor", "set_sky_collectable"];
        static List<string> animLabels = [];
        readonly static List<(string arg1, string arg2)> noPairArgs = [
            ("OnWater", "!OnWater"),
            ("OffsetByGrid", "OffsetByCoords"),
            ("Include[", "Exclude["),
            ("IfAlive", "IfDead"),
            ("OnXLocation:", "PastXLocation:"),
            ("OnXLocation:", "BeforeXLocation:"),
            ("OnYLocation:", "PastYLocation:"),
            ("OnYLocation:", "BeforeYLocation:")
        ];
        readonly static List<(string minArg, string maxArg)> noPairMinMaxargs = [
            ("HpAbove", "HpBelow"),
            ("PastXLocation", "BeforeXLocation"),
            ("PastYLocation", "BeforeYLocation")
        ];
    }
}