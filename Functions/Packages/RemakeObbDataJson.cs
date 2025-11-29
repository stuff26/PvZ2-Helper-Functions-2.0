using System.Text.Json.Nodes;
using UniversalMethods;

namespace HelperFunctions.Functions.Packages
{
    public class RemakeObbDataJson
    {
        public static void Function()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter an obb folder");
            var filePath = UM.AskForDirectory(["data.json"], ["packet"]);
            var datajsonPath = Path.Join(filePath, "data.json");
            var packagesPath = Path.Join(filePath, "packet");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Retrieving files... ");
            var fileList = Directory.GetFiles(packagesPath, "*.*", SearchOption.TopDirectoryOnly);
            var newFileList = new List<string>();
            foreach (var fileDir in fileList)
            {
                // If file directory is a folder instead of a file, don't add
                if (Directory.Exists(fileDir))
                {
                    continue;
                }
                // If the file ends in .scg, add it
                if (fileDir.EndsWith(".scg"))
                {
                    var toAddFileName = Path.GetFileName(fileDir)!.Replace(".scg", "");
                    newFileList.Add(toAddFileName);
                }
            }
            ProgressChecker.WriteFinished();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Writing data.json...");
            var datajsonFile = UM.GetJsonFile(datajsonPath)!;
            datajsonFile["packet"] = new JsonArray([.. newFileList.Select(n => JsonValue.Create(n))]);
            UM.WriteJsonFile(datajsonPath, datajsonFile);
            ProgressChecker.WriteFinished();
        }
    }
}