using XflComponents;
using UniversalMethods;
using System.Text.Json;

namespace HelperFunctions.Functions.Packages
{
    public static class RemakeXflDataJson
    {
        public static void Function()
        {
            // Get XFL
            UM.PrintColoredText(ConsoleColor.DarkCyan, "Enter an XFL", separateLines:true);
            var xfl = XFL.AskForXFL(new()
            {
                GetSymbols = false,
                GetBitmaps = true,
                GetDataJsonData = true,
                CheckProgress = true
            });

            // If the XFL has no bitmaps, make it now and exit. Skip this otherwise
            if (xfl.GetNumBitmaps() == 0)
            {
                UM.PrintColoredText([
                    (ConsoleColor.DarkCyan, "No bitmaps found in XFL"),
                    (ConsoleColor.Green, "Writing new data.json... ")
                ], separateLines:true);
                xfl.WriteDataJson(addFile:true);
                ProgressChecker.WriteFinished();
                return;
            }
            
            // Ask for the ID prefix that will be added to the data.json IDs
            var spritePrefix = GetSpriteIDName(xfl);

            // Write new data.json
            UM.PrintColoredText(ConsoleColor.Green, "Writing new data.json... ");
            xfl.WriteDataJson(idPrefix:spritePrefix, addFile:true);
            ProgressChecker.WriteFinished();

        }

        private static string GetSpriteIDName(XFL xfl)
        {
            UM.PrintColoredText(ConsoleColor.DarkCyan, "Enter the starting ID you want, or enter nothing if you want to use the current ID",
             separateLines:true);

            string? foundID = null;
            bool checkedForID = false;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                var idInput = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Red;
                
                // If nothing is entered, use pre-existing ID
                var noInputGiven = string.IsNullOrWhiteSpace(idInput);
                if (checkedForID && noInputGiven)
                {
                    // If an ID was checked already and none was found
                    if (foundID is null)
                    {
                        Console.WriteLine("ID could not be found, enter your own ID");
                        continue;
                    }
                    // If an ID was found already
                    else
                    {
                        idInput = foundID;
                    }
                }
                // If an ID was not checked for yet and no input is given
                else if (noInputGiven)
                {
                    foundID = GetFoundID(xfl.GetDataJsonPath());
                    checkedForID = true;
                    
                    // If an ID could not be found
                    if (foundID is null) 
                    {
                        Console.WriteLine("Could not determine found ID, enter your own");
                        continue;
                    }

                    idInput = foundID;
                }

                // If the found ID prefix contains characters that aren't letters, numbers, or '_'
                if (idInput!.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
                {
                    UM.PrintColoredText(
                    [
                        (ConsoleColor.Red, "ID Prefix "),
                        (ConsoleColor.Green, idInput!),
                        (ConsoleColor.Red, " contains invalid characters, enter again\n")
                    ]);
                    continue;
                }
                idInput = FormatIDPrefix(idInput!);

                UM.PrintColoredText([
                    (ConsoleColor.DarkCyan, "Example would be "),
                    (ConsoleColor.Green, $"{idInput}"),
                    (ConsoleColor.Yellow, $"SPRITE_50X50"),
                    (ConsoleColor.DarkCyan, ", is that what you want to use?"),
                    (ConsoleColor.Yellow, " (Y/N)\n"),
                ]);

                var shouldExit = UserPrompts.AskYesOrNo();
                if (shouldExit) return idInput;
                else
                {
                    UM.PrintColoredText(ConsoleColor.DarkCyan, "Enter another ID", separateLines:true);
                }
            }
        }

        private static string FormatIDPrefix(string idPrefix)
        {
            idPrefix = idPrefix!.ToUpper();
            if (!idPrefix.StartsWith(XFL.DatajsonDefaultIDPrefix))
            {
                idPrefix = XFL.DatajsonDefaultIDPrefix + idPrefix;
            }
            if (!idPrefix.EndsWith('_'))
            {
                idPrefix += "_";
            }
            return idPrefix;
        }
        private static string? GetFoundID(string datajsonPath)
        {
            // Get data.json
            var datajson = JsonMethods.GetJsonFile(datajsonPath)?.AsObject();
            if (datajson is null || !datajson.TryGetPropertyValue(XFL.DatajsonImageName, out var images))
            {
                return null;
            }

            // Get a list of keys from images, return null if there are no keys
            var imagesKeys = JsonMethods.GetKeysFromJsonNode(images);
            if (images is null || imagesKeys.Count == 0) return null;

            foreach (var imageName in imagesKeys)
            {
                var imageObject = images[imageName];
                var imageID = imageObject?[XFL.DatajsonID]?.AsValue().Deserialize<string>();
                if (imageID is null || !imageID.Contains(imageName, StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                return imageID[..imageID.IndexOf(imageName, StringComparison.CurrentCultureIgnoreCase)];
            }

            return null;
        }
    }
}