
using System.Text.Json;
using System.Text.Json.Nodes;

namespace UniversalMethods
{
    public static class JsonMethods
    {
        private static readonly JsonDocumentOptions jsonDocumentOptions = new()
        {
            AllowDuplicateProperties = true,
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        };

        /// <summary>
        /// Check if a JSON file exists and is a valid
        /// </summary>
        /// <param name="pathName">Path to file</param>
        /// <returns>True if the file is found and is a valid JSON, othewise false</returns>
        public static bool CheckJsonValid(string pathName)
        {
            try
            {
                // If the file contents can be retrieved, return true
                var fileContents = File.ReadAllBytes(pathName);
                JsonDocument.Parse(fileContents);
                return true;
            }
            catch (Exception)
            {
                // If there is any sort of error, return false
                return false;
            }
        }

        /// <summary>
        /// Get a JSON Node form of a JSON file
        /// </summary>
        /// <param name="filePath">Path to the JSON file</param>
        /// <returns>The JSON node equivalent of a file, returns null if no valid file is found</returns>
        public static JsonNode? GetJsonFile(string filePath)
        {
            JsonNode? jsonFile;
            if (!File.Exists(filePath))
            {
                jsonFile = null;
            }
            else
            {
                string rawFileText = File.ReadAllText(filePath);
                jsonFile = JsonNode.Parse(rawFileText, documentOptions:jsonDocumentOptions);
                try
                {
                    jsonFile?.AsObject().IndexOf(string.Empty);
                }
                catch (ArgumentException)
                {
                    jsonFile = null;
                }
            }
            return jsonFile;
        }

        /// <summary>
        /// Get a JSON Node form of a JSON file
        /// </summary>
        /// <param name="filePath">Path to the JSON file</param>
        /// <returns>The JSON node equivalent of a file, returns null if no valid file is found</returns>
        public static JsonDocument? GetJsonDocmentFile(string filePath)
        {
            JsonDocument? jsonFile;
            if (!File.Exists(filePath)) return null;
            
            string rawFileText = File.ReadAllText(filePath);
            jsonFile = JsonDocument.Parse(rawFileText);
            return jsonFile;
        }

        public static JsonNode? ReadFileJson(string jsonText)
        {
            try
            {
                var jsonFile = JsonNode.Parse(jsonText, documentOptions:jsonDocumentOptions);
                return jsonFile;
            }
            catch
            {
                return null;
            }
        }

        public static JsonDocument? ReadFileJsonDocument(string jsonText)
        {
            try
            {
                var jsonFile = JsonDocument.Parse(jsonText);
                return jsonFile;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get a list of keys that a JsonNode has
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<string> GetKeysFromJsonNode(JsonNode? input)
            => input?.AsObject().Select(kvp => kvp.Key).ToList() ?? [];
        
        /// <summary>
        /// Get a list of keys that a JsonElement has
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<string> GetKeysFromJsonElement(JsonElement input)
            => input.EnumerateObject().Select(property => property.Name).ToList();

        /// <summary>
        /// Convert then write a JSON file to a specified location
        /// </summary>
        /// <param name="filePath">Path to write to</param>
        /// <param name="jsonFile">The JSON that will be written</param>
        /// <param name="isIndented">Determine if the file should be indented or not, true by default</param>
        /// <param name="indentSize">Indentation size for writen JSON</param>
        public static void WriteJsonFile(string filePath, JsonNode jsonFile)
        {
            var fileText = jsonFile.ToJsonString(new JsonSerializerOptions { 
                WriteIndented = true,
                AllowDuplicateProperties = true,
                AllowTrailingCommas = true,
                IndentSize = 3
            });
            File.WriteAllText(filePath, fileText);
        }
    }
}