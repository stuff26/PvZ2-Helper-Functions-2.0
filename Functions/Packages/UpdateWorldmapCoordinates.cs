using System.Text.Json.Nodes;
using UniversalMethods;

namespace HelperFunctions.Functions.Packages
{
    public static class UpdateWorldmapCoordinates
    {
        public static void Function()
        {
            // Get JSON file, path to file, and how much to change the x and y coordinate by
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter the worldmap file you want to edit");
            var (mapFile, mapPath) = UserPrompts.AskForJsonFile();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter how much you want to change the X coordinate by");
            var xChange = UserPrompts.AskForDouble();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter how much you want to change the Y coordinate by");
            var yChange = UserPrompts.AskForDouble();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Processing files...");
            var mapData = mapFile!["objects"]![0]!["objdata"]!;

            // Get the pieces and combine into a single list
            var mapPieces = mapData["m_mapPieces"]!.AsArray();
            var eventPieces = mapData["m_eventList"]!.AsArray();
            var combinedPieces = new List<JsonNode>();
            combinedPieces.AddRange(mapPieces!);
            combinedPieces.AddRange(eventPieces!);

            // Loop through each piece and change their X and Y positions
            foreach (var piece in combinedPieces)
            {
                var piecePositions = piece["m_position"]!;

                var xPosition = piecePositions["x"]!.GetValue<double>();
                var newXPosition = xPosition + xChange;
                piecePositions["x"] = Math.Round(newXPosition, 2);

                var yPosition = piecePositions["y"]!.GetValue<double>();
                var newYPosition = yPosition + yChange;
                piecePositions["y"] = Math.Round(newYPosition, 2);
            }

            // Write to file
            JsonMethods.WriteJsonFile(mapPath, mapFile);
            ProgressChecker.WriteFinished();
        }
    }
}