using System.Text.Json.Nodes;
using System.Xml.Linq;
using SixLabors.ImageSharp;
using XflComponents;
using UniversalMethods;

namespace HelperFunctions.Functions.Packages
{
    public class RemakeXflDataJson
    {
        public static void Function()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter an XFL");
            var xflPath = UM.AskForDirectory(["DOMDocument.xml", "data.json"]);
            var datajsonPath = Path.Join(xflPath, "data.json");
            var datajson = UM.GetJsonFile(datajsonPath)!;
            var resolution = datajson["resolution"]!.GetValue<double>();

            JsonNode? images = null;
            if (datajson.AsObject().ContainsKey("image"))
            {
                images = datajson["image"];
            }
            var idName = GetSpriteIDName(images);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Getting bitmap locations... ");
            var spriteNames = GetSpriteLocations(xflPath);
            spriteNames.Sort();
            ProgressChecker.WriteFinished();

            var spriteInfo = GetSpriteInfo(xflPath, spriteNames, resolution);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Writing new data.json... ");
            datajson["image"] = MakeNewDataInfo(spriteInfo, idName);
            UM.WriteJsonFile(datajsonPath, datajson);
            ProgressChecker.WriteFinished();

        }

        private static string GetSpriteIDName(JsonNode? firstSprite)
        {
            string message;
            if (firstSprite is null)
                message = "Enter the starting ID you want";
            else
                message = "Enter the starting ID you want, or enter nothing if you want to use the current ID";

            string? foundID = null;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(message);
                Console.ForegroundColor = ConsoleColor.Magenta;
                var idInput = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Red;
                if (string.IsNullOrWhiteSpace(idInput))
                {
                    if (firstSprite is null || foundID == "")
                    {
                        continue;
                    }

                    if (foundID is null)
                    {
                        foundID = GetFoundID(firstSprite);
                        if (foundID == "")
                        {
                            Console.WriteLine("Could not determine found ID, enter your own");
                            message = "Enter the starting ID you want";
                            continue;
                        }
                    }
                    Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 1);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("~~~~~");
                    idInput = foundID;
                }
                else
                {

                    idInput = idInput.ToUpper();
                    if (!idInput.StartsWith("IMAGE"))
                    {
                        idInput = "IMAGE_" + idInput;
                    }
                    if (!idInput.EndsWith("_"))
                    {
                        idInput += "_";
                    }
                }

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write($"Example would be [{idInput}SPRITE_50X50], is this okay to use?");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(" (Y/N)");
                bool shouldExit = false;
                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    var userInput = Console.ReadLine()?.ToUpper();
                    if (userInput == "Y")
                    {
                        break;
                    }
                    else if (userInput == "N")
                    {
                        shouldExit = true;
                        break;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Enter Y or N");
                    }
                }
                if (!shouldExit) return idInput!;
            }
        }

        private static string GetFoundID(JsonNode images)
        {
            var firstSpriteName = UM.GetKeysFromJsonNode(images)[0];
            var firstSpriteID = images[firstSpriteName]!["id"]!.GetValue<string>();
            firstSpriteName = firstSpriteName.ToUpper();
            if (!firstSpriteID.EndsWith(firstSpriteName))
            {
                return "";
            }

            var toReturnID = firstSpriteID.Replace(firstSpriteName, "");
            return toReturnID;
        }

        private static List<string> GetSpriteLocations(string xflPath)
        {
            XDocument document = XDocument.Load(Path.Join(xflPath, "DOMDocument.xml"));
            using var documentReader = document.CreateReader();
            DOMDocument DOMDocumentObject = (DOMDocument?)DOMDocument.serializer.Deserialize(documentReader)!;

            var spriteNames = DOMDocumentObject.GetAllBitmapNames();
            return spriteNames;
        }

        private static List<(string name, int width, int height)> GetSpriteInfo(string xflPath, List<string> spriteNames, double resolution)
        {
            List<(string name, int width, int height)> spriteInfo = [];
            
            Console.ForegroundColor = ConsoleColor.Green;
            var addedSprites = new ProgressChecker("Finding sprite files... ", spriteNames.Count);
            Console.ForegroundColor = ConsoleColor.Red;

            foreach (string spriteName in spriteNames)
            {
                var fullSpritePath = Path.Join(xflPath, "library", spriteName);
                int width;
                int height;
                try
                {
                    using Image image = Image.Load(fullSpritePath);
                    width = (int)((image.Width * (1200.0 / resolution)) + 0.25);
                    height = (int)((image.Height * (1200.0 / resolution)) + 0.25);
                }
                catch (FileNotFoundException)
                {
                    addedSprites.RemoveOne();
                    Console.WriteLine($"Could not find sprite {spriteName}, will be skipped");
                    continue;
                }
                catch (OutOfMemoryException)
                {
                    addedSprites.RemoveOne();
                    Console.WriteLine($"Sprite {spriteName} could not be read, will be skipped");
                    continue;
                }
                catch (UnknownImageFormatException)
                {
                    addedSprites.RemoveOne();
                    Console.WriteLine($"Sprite {spriteName} could not be read, will be skipped");
                    continue;
                }
                var baseSpriteName = Path.GetFileName(spriteName)!.Replace(".png", "");
                spriteInfo.Add((baseSpriteName, width, height));
                addedSprites.AddOne();
            }

            return spriteInfo;
        }

        private static JsonObject MakeNewDataInfo(List<(string name, int width, int height)> spriteInfo, string idName)
        {
            var dataInfo = JsonNode.Parse("{}")!.AsObject();
            foreach (var (name, width, height) in spriteInfo)
            {
                var spriteID = MakeSpriteID(idName, name);
                var toParse = $"{{\"id\": \"{spriteID}\",   \"dimension\": {{\"width\": {width}, \"height\": {height}}},   \"additional\": null}}";
                var toAddJsonNode = JsonNode.Parse(toParse)!;
                KeyValuePair<string, JsonNode?> toAddPair = new(name, toAddJsonNode);
                dataInfo.Add(toAddPair);
            }

            return dataInfo;
        }

        private static string MakeSpriteID(string idName, string spriteName)
        {
            spriteName = spriteName.ToUpper();
            return idName + spriteName;
        }
    }
}