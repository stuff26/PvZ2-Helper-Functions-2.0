using System.Text.Json;
using System.Text.Json.Nodes;
using HelperFunctions;
using SixLabors.ImageSharp;
using UniversalMethods;

namespace XflComponents
{
    public sealed class XFL
    {
        public string XflPath {get; set;}
        public DOMDocument DOMDocument {get; init;}
        public List<SymbolItem> Symbols {get; set;} = [];
        public List<Bitmap> Bitmaps {get; set;} = [];
        public int? Resolution {get; init;}
        public JsonObject SharedSpriteNames {get; init;} = [];

        public static readonly byte[] MainXflBytes = [80, 82, 79, 88, 89, 45, 67, 83, 53];
        public const string DOMDocumentFileName = "DOMDocument.xml";
        public const string LibraryDirName = "library";
        public const string DatajsonFileName = "data.json";
        public const string MainXflFileName = "main.xfl";

        public const string ImageFolder = "image";
        public const string SpriteFolder = "sprite";
        public const string LabelFolder = "label";
        public const string MediaFolder = "media";
        public const string MainSprite = "main_sprite";

        public const int ReferenceResolution = 1200;
        public const string DatajsonDefaultIDPrefix = "IMAGE_";
        private const int DatajsonVersion = 6;
        private static JsonObject DatajsonDefaultPosition => new()
        {
            ["x"] = 0,
            ["y"] = 0
        };

        public const string DatajsonVersionName = "version";
        public const string DatajsonResolutionName = "resolution";
        public const string DatajsonPositionName = "position";
        public const string DatajsonImageName = "image";
        public const string DatajsonSpriteName = "sprite";
        public const string DatajsonID = "id";
        public const string DatajsonDimension = "dimension";
        public const string DatajsonWidth = "width";
        public const string DatajsonHeight = "height";
        public const string DatajsonAdditional = "additional";

        /// <summary>
        /// Create an XFL object from an XFL path given
        /// </summary>
        /// <param name="xflPath">Path to XFL that will be processed into an object</param>
        /// <param name="options">Options that will affect how the XFL is initiazed</param>
        public XFL(string xflPath, XFLInitOptions options)
        {
            XflPath = xflPath; // Set XFL path
            var checkProgress = options.CheckProgress;

            // Get the DOMDocument
            if (checkProgress)
            {
                UM.PrintColoredText(ConsoleColor.Green, "Getting DOMDocument... ");
            }
            var DOMDocumentPath = Path.Join(xflPath, DOMDocumentFileName);
            DOMDocument = UM.GetDOMDocument(DOMDocumentPath);
            if (checkProgress) ProgressChecker.WriteFinished();

            // Get the data.json and get its resolution, or add fixed resolution if provided
            if (options.FixResolution > 0)
            {
                Resolution = options.FixResolution;
            }
            else if (options.GetDataJsonData)
            {
                if (checkProgress)
                {
                    UM.PrintColoredText(ConsoleColor.Green, "Getting data.json information... ");
                }
                var datajsonPath = Path.Join(xflPath, DatajsonFileName);
                var datajson = JsonMethods.GetJsonFile(datajsonPath)?.AsObject()!;
                if (Resolution is not null) 
                {
                    datajson.TryGetPropertyValue("resolution", out var resNode);
                    Resolution = resNode.Deserialize<int>();
                };
                datajson.TryGetPropertyValue("sprites", out var spriteNamesNode);
                if (spriteNamesNode is not null) SharedSpriteNames = spriteNamesNode.AsObject();
                Resolution = GetResolution(datajsonPath);

                if (checkProgress) ProgressChecker.WriteFinished();
            }

            // Get all the symbols listed in the DOMDocument
            if (options.GetSymbols)
            {
                var symbolNameList = DOMDocument.GetAllSymbolNames();
                Symbols = [];

                ProgressChecker? checkSymbols = null;
                if (checkProgress)
                {
                    checkSymbols = new("Processing symbols...", symbolNameList.Count);
                }

                foreach (var symbolName in symbolNameList)
                {
                    var symbolPath = Path.Join(xflPath, LibraryDirName, $"{symbolName}.xml");
                    var symbol = UM.GetSymbol(symbolPath);
                    Symbols.Add(symbol);
                    checkSymbols?.AddOne();
                }
            }

            // Get all the bitmaps listed in the DOMDocument
            if (options.GetBitmaps)
            {
                var bitmapNameList = DOMDocument.GetAllBitmapNames(getFolderNames:true, getFileEnding:false);
                Bitmaps = [];

                ProgressChecker? checkBitmaps = null;
                if (checkProgress)
                {
                    checkBitmaps = new("Processing bitmaps...", bitmapNameList.Count);
                }
                foreach (var bitmapName in bitmapNameList)
                {
                    var bitmapPath = Path.Join(xflPath, LibraryDirName, $"{bitmapName}.png");
                    var bitmap = new Bitmap(bitmapPath, bitmapName);
                    Bitmaps.Add(bitmap);
                    checkBitmaps?.AddOne();
                }
            }
        }

        public static XFL? GetXFLSafely(string xflPath, XFLInitOptions options)
        {
            try
            {
                var xfl = new XFL(xflPath, options);
                return xfl;
            }

            catch (FileNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nCould not find file");
            }
            catch (JsonException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nCould read {DatajsonFileName}, enter again");
            }
            catch (InvalidOperationException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nCould not process symbol files, enter again");
            }
            catch (InvalidImageContentException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nCould not process bitmap files, enter again");
            }

            return null;
        }
        
        /// <summary>
        /// Get the number of symbols that exist in the XFL
        /// </summary>
        /// <returns>An int representing the number of symbols in the XFL</returns>
        public int GetNumSymbols() => Symbols.Count;

        /// <summary>
        /// Get the number of bitmaps that exist in the XFL
        /// </summary>
        /// <returns>An int representing the number of bitmaps in the XFL</returns>
        public int GetNumBitmaps() => Bitmaps.Count;

        /// <summary>
        /// Get an array of all the symbol names in the XFL
        /// </summary>
        /// <returns>An array of all the symbol names found in the XFL</returns>
        public string[] GetAllSymbolNames() => Symbols.Select(s => s.Name).ToArray();

        /// <summary>
        /// Get an array of all the bitmap names in the XFL
        /// </summary>
        /// <returns>An array of all the bitmap names found in the XFL</returns>
        public string[] GetAllBitmapNames() => Bitmaps.Select(b => b.Name).ToArray();

        /// <summary>
        /// Get the resolution of the XFL via its data.json
        /// </summary>
        /// <param name="datajsonPath">Path of the data.json to check</param>
        /// <param name="addFile">If true, `DatajsonFileName` will be added to the end of the path to check</param>
        /// <returns>An int representing the resolution of the XFL</returns>
        public static int GetResolution(string path, bool addFile = false)
        {
            // If addfile is true, add "data.json" to the end of the given path
            if (addFile) path = Path.Join(path, DatajsonFileName);

            // Get data.json file
            var datajson = JsonMethods.GetJsonFile(path);

            // Read file for the resolution
            int resolution = datajson!["resolution"].Deserialize<int>();
            return resolution;
        }

        /// <summary>
        /// Get the resolution of the XFL via its data.json
        /// </summary>
        /// <param name="datajsonPath">Path of the data.json to check</param>
        /// <param name="addFile">If true, `DatajsonFileName` will be added to the end of the path to check</param>
        /// <returns>An int representing the resolution of the XFL</returns>
        public int GetResolution(bool addFile = false)
        {
            return GetResolution(XflPath, addFile);
        }

        /// <summary>
        /// Add a prefix to every symbol file name in the XFL
        /// </summary>
        /// <param name="prefixToAdd">Prefix to add to the symbol names</param>
        public void AddPrefixToSymbols(string prefixToAdd)
        {
            DOMDocument.SymbolItemList = [];
            foreach (var symbol in Symbols)
            {
                var symbolName = symbol.Name;
                var symbolNameBase = Path.GetFileName(symbolName);
                var symbolPath = Path.GetDirectoryName(symbolName);
                symbolNameBase = prefixToAdd + symbolNameBase;
                var newSymbolName = $"{symbolPath}/{symbolNameBase}";

                symbol.ChangeName(newSymbolName);
                DOMDocument.AddNewSymbolItem(newSymbolName);
            }
        }
        /// <summary>
        /// Add a prefix to every bitmap file name in the XFL
        /// </summary>
        /// <param name="prefixToAdd">Prefix to add to the bitmap names</param>
        public void AddPrefixToBitmaps(string prefixToAdd)
        {
            foreach (var bitmapItem in DOMDocument.BitmapItemList)
            {
                var bitmapName = bitmapItem.Name;
                var bitmapNameBase = Path.GetFileName(bitmapName);
                var bitmapPath = Path.GetDirectoryName(bitmapName);
                bitmapNameBase = prefixToAdd + bitmapNameBase;
                var newBitmapName = $"{bitmapPath}/{bitmapNameBase}";
                bitmapItem.ChangeName(newBitmapName);
            }
        }

        /// <summary>
        /// Sets the `SymbolItemList` in the DOMDocument to an empty list, removing symbol references
        /// </summary>
        public void ClearDOMDocumentSymbolItems()
        {
            DOMDocument.SymbolItemList = [];
        }

        /// <summary>
        /// Sets the `BitmapItemList` in the DOMDocument to an empty list
        /// </summary>
        public void ClearDOMDocumentBitmapItems()
        {
            DOMDocument.BitmapItemList = [];
        }

        /// <summary>
        /// Adds a new symbol to the XFL
        /// </summary>
        /// <param name="symbol">Symbol that will be added</param>
        public void AddSymbol(SymbolItem symbol)
        {
            Symbols.Add(symbol);
        }

        /// <summary>
        /// Adds a new bitmap to the XFL
        /// </summary>
        /// <param name="bitmap">Bitmap that will be added</param>
        public void AddBitmap(Bitmap bitmap)
        {
            Bitmaps.Add(bitmap);
        }

        /// <summary>
        /// Adds a new bitmap to the XFL from a directory
        /// </summary>
        /// <param name="bitmapPath">Path of the bitmap that will be added</param>
        /// <param name="bitmapName">Name of added bitmap</param>
        public void AddBitmap(string bitmapPath, string bitmapName)
        {
            var bitmap = new Bitmap(bitmapPath, bitmapName);
            Bitmaps.Add(bitmap);
        }
        
        /// <summary>
        /// Save the XFL to some directory, including the library, DOMDocument data.json, and main.xfl
        /// </summary>
        /// <param name="path">Path to save the XFL to, by default is `XflPath`</param>
        public void SaveXfl(string? path = null, string? datajsonIDPrefix = DatajsonDefaultIDPrefix, bool removeUnusedFles = false)
        {
            // Create the XFL directory if it doesn't exist yet
            path ??= XflPath;
            if (!Directory.Exists(path))
            {
                FileManagement.CreateNestedFolder(path);
            }
            else if (removeUnusedFles)
            {
                Directory.Delete(path, true);
                Directory.CreateDirectory(path);
            }

            // Save the DOMDocument
            WriteDOMDocument(path, addFile:true);

            // Save the symbols in the library
            WriteSymbolFiles(path, addLibrary:true);
            WriteBitmapFiles(path, addLibrary:true);

            // Save data.json
            if (Resolution is not null) WriteDataJson(path, idPrefix : datajsonIDPrefix, addFile:true);

            // Make a main.xfl
            var mainXflPath = Path.Join(path, MainXflFileName);
            if (!File.Exists(mainXflPath))
            {
                WriteMainXfl(mainXflPath, addFile:false);
            }
        }

        /// <summary>
        /// Write the DOMDocument to a certain path
        /// </summary>
        /// <param name="path">Path to write the DOMDocument to, by default is the built in path</param>
        /// <param name="addFile">If true, `DOMDocumentFileName` will be added to the end of the path</param>
        public void WriteDOMDocument(string? path = null, bool addFile = false)
        {
            path ??= XflPath;
            if (addFile) path = Path.Join(path, DOMDocumentFileName);
            XmlMethods.SaveXmlDocument(path, DOMDocument, DOMDocument.serializer);
        }

        /// <summary>
        /// Write every symbol to a listed directory
        /// </summary>
        /// <param name="path">Directory to write the symbol files to</param>
        /// <param name="addLibrary">If true, `LibraryDirName` will be added as the parent folder for each symbol</param>
        public void WriteSymbolFiles(string? path = null, bool addLibrary = false)
        {
            if (Symbols is null) return;
            path ??= XflPath;

            foreach (var symbol in Symbols) // Loop through every symbol object
            {
                // Get the symbol name and parent folder
                var symbolName = symbol.Name;
                string symbolPath;
                if (addLibrary) symbolPath = Path.Join(path, LibraryDirName, $"{symbolName}.xml");
                else symbolPath = Path.Join(path, $"{symbolName}.xml");

                // Create parent directory if it is not made yet
                var folderToMake = Path.GetDirectoryName(symbolPath)!;
                FileManagement.CreateNestedFolder(folderToMake);

                // Make the symbol path and save it
                XmlMethods.SaveXmlDocument(symbolPath, symbol, SymbolItem.serializer);
            }
        }

        /// <summary>
        /// Write every bitmap to a listed directory
        /// </summary>
        /// <param name="path">Directory to write the bitmap files to</param>
        public void WriteBitmapFiles(string? path = null, bool addLibrary = false)
        {
            if (Bitmaps is null) return;
            path ??= XflPath;

            foreach (var bitmap in Bitmaps) // Loop through every bitmap object
            {
                // Get the bitmap name and parent folder
                var bitmapName = bitmap.Name;
                string bitmapPath;
                if (addLibrary) bitmapPath = Path.Join(path, LibraryDirName, bitmapName);
                else bitmapPath = Path.Join(path, bitmapName);

                // Create parent directory if it is not made yet
                var folderToMake = Path.GetDirectoryName(bitmapPath);
                FileManagement.CreateNestedFolder(folderToMake!);

                // Make the symbol path and save it
                bitmap.SaveFile(bitmapPath);
            }
        }

        /// <summary>
        /// Get the data.json path of the XFL
        /// </summary>
        /// <returns>The absolute path to the XFL data.json</returns>
        public string GetDataJsonPath() => Path.Join(XflPath, DatajsonFileName);

        /// <summary>
        /// Makes and writes a data.json to a certain path
        /// </summary>
        /// <param name="datajsonPath">Path to write the data.json to</param>
        /// <param name="idPrefix">Prefix to add to IDs in data.json</param>
        /// <param name="addFile">If true, `DatajsonFileName` will be added to the end of `datajsonPath`</param>
        public void WriteDataJson(string? datajsonPath = null, string? idPrefix = null, bool addFile = false)
        {
            idPrefix ??= DatajsonDefaultIDPrefix;
            datajsonPath ??= XflPath;
            if (addFile) datajsonPath = Path.Join(datajsonPath, DatajsonFileName);
            var datajson = MakeDataJson(idPrefix);
            JsonMethods.WriteJsonFile(datajsonPath, datajson);
        }


        /// <summary>
        /// Make a data.json JsonNode object using the bitmaps part of the XFL
        /// </summary>
        /// <param name="idPrefix">Prefix to add to all of the bitmap IDs inside the data.json</param>
        /// <returns>A JsonObject of the made data.json</returns>
        /// <exception cref="MissingFieldException">If `Resolution` is null, this error is thrown</exception>
        public JsonObject MakeDataJson(string? idPrefix = null)
        {
            idPrefix ??= DatajsonDefaultIDPrefix;

            if (Resolution is null) throw new MissingFieldException("Resolution is not initialized");

            // Make data json object, add default values
            var datajson = new JsonObject
            {
                [DatajsonVersionName] = DatajsonVersion,
                [DatajsonResolutionName] = Resolution,
                [DatajsonPositionName] = DatajsonDefaultPosition,
                [DatajsonImageName] = null,
                [DatajsonSpriteName] = SharedSpriteNames
            };
            if (Bitmaps is null) return datajson;

            // Fix id prefix by adding a _ if it doesn't have one and making it all uppercase
            if (!idPrefix.EndsWith('_')) idPrefix += '_';
            idPrefix = idPrefix.ToUpper();

            // Make the entries for the image part of the data json object
            if (Bitmaps is not null)
            {
                var imageDataJson = new JsonObject();
                foreach (var bitmap in Bitmaps)
                {
                    // Get the width and height of each bitmap then fix the size according to the resolution
                    var (width, height) = bitmap.Size;
                    (width, height) = CalculateDataJsonSize(width, height);

                    // Find the bitmap name, create the id, add all of the created values together
                    var bitmapName = Path.GetFileName(bitmap.Name);
                    var id = idPrefix + bitmapName.ToUpper();
                    var toAddObject = new JsonObject
                    {
                        [DatajsonID] = id,
                        [DatajsonDimension] = new JsonObject
                        {
                            [DatajsonWidth] = width,
                            [DatajsonHeight] = height
                        },
                        [DatajsonAdditional] = null
                    };
                    
                    // Parse the object and add it to the image object
                    KeyValuePair<string, JsonNode?> toAddValue = new(bitmapName, toAddObject);
                    imageDataJson.Add(toAddValue);
                }
                
                // Add the image to the data json object
                datajson[DatajsonImageName] = imageDataJson;
            }

            // Return
            return datajson;
        }

        /// <summary>
        /// Write the "main.xfl" file to a path
        /// </summary>
        /// <param name="path">Path to write the main.xfl file to, by default the built in XFL path</param>
        /// <param name="addFile">If true, `MainXflFileName` will be added to the end of the path</param>
        public void WriteMainXfl(string? path = null, bool addFile = false)
        {
            path ??= XflPath;
            if (addFile) path = Path.Join(path, MainXflFileName);
            File.WriteAllBytes(path, MainXflBytes);
        }

        /// <summary>
        /// Calculate the new width or height based on the given resolution
        /// </summary>
        /// <param name="size">Height or width given to recalculate</param>
        /// <param name="resolution">Resolution of the xfl, used in calculation</param>
        /// <returns>The recalculated size</returns>
        public int CalculateDataJsonSize(double size)
        {
            if (Resolution is null) throw new ArgumentNullException("Resolution is not initialized");
            return (int) (size * ((double)ReferenceResolution / Resolution) + 0.25);
        }

        /// <summary>
        /// Calculate the new width and height based on the given resolution
        /// </summary>
        /// <param name="width">Width given to recalculate</param>
        /// <param name="heigth">Height given to recalculate</param>
        /// <param name="resolution">Resolution of the xfl, used in calculation</param>
        /// <returns>A tuple of the recalculated width and height</returns>
        public (int width, int height) CalculateDataJsonSize(int width, int heigth)
        {
            int newWidth = CalculateDataJsonSize(width);
            int newHeight = CalculateDataJsonSize(heigth);
            return (newWidth, newHeight);
        }

        /// <summary>
        /// Updates the symbol references in the DOMDocument to what `Symbols` has
        /// </summary>
        public void UpdateSymbolItemReferences()
        {
            ClearDOMDocumentSymbolItems();
            foreach (var symbol in Symbols)
            {
                DOMDocument.AddNewSymbolItem(symbol.Name, includesEnd:false);
            }
        }

        /// <summary>
        /// Updates bitmap references in the DOMDocument to what `Bitmaps` has
        /// </summary>
        public void UpdateBitmapItemReferences()
        {
            ClearDOMDocumentBitmapItems();
            foreach (var bitmap in Bitmaps)
            {
                DOMDocument.AddNewBitmapItem(bitmap.Name);
            }
        }

        /// <summary>
        /// Updates all symbol and bitmap references in the DOMDocument
        /// </summary>
        public void UpdateAllItemReferences()
        {
            UpdateSymbolItemReferences();
            UpdateBitmapItemReferences();
        }
        
        /// <summary>
        /// Makes a dictionary of each symbol's name with the SymbolItem object
        /// </summary>
        /// <returns>A dictionary with a symbol's name and its object</returns>
        public Dictionary<string, SymbolItem> GetSymbolObjectDict() => Symbols.ToDictionary(s => s.Name, s => s);

        /// <summary>
        /// Makes a dictionary of each bitmap's name with the SymbolItem object
        /// </summary>
        /// <returns>A dictionary with a bitmap's name and its object</returns>
        public Dictionary<string, Bitmap> GetBitmapObjectDict() => Bitmaps.ToDictionary(b => b.Name, b => b);

        public bool? IsSplitLabelsType() => IsSplitLabelsType(this);

        /// <summary>
        /// Checks if the XFL is a split labels type by checking the used file in the DOMDocument
        /// </summary>
        /// <returns>True if the XFL is a split labels type, false if it isn't, or null if it is inconclusive</returns>
        public static bool? IsSplitLabelsType(XFL xfl)
        {
            // Get instance layer and used library items in the layer
            var instanceLayer = xfl.DOMDocument.GetInstanceLayer();
            var usedLibraryItems = instanceLayer?.GetAllLibraryItems();

            // If the instance layer couldn't be found or no library items are used, return null
            if (usedLibraryItems is null || usedLibraryItems?.Count == 0) return null;

            // If there is more than 1 library item used, return true
            if (usedLibraryItems!.Count > 1)
            {
                return true;
            }
            // If the only library item used is main_sprite, return false
            if (usedLibraryItems[0] == MainSprite)
            {
                return false;
            }

            // If the above checks fail, return true
            return true;
        }

        public Type? IsSymbolOrBitmap(string name)
        {
            if (GetAllSymbolNames().Contains(name))
            {
                return typeof(SymbolItem);
            }
            else if (GetAllBitmapNames().Contains(name))
            {
                return typeof(Bitmap);
            }
            
            return null;
        }
        
        /// <summary>
        /// Finds and returns a wanted symbol from a given name
        /// </summary>
        /// <param name="name">Name for the symbol to be found, should not include ".xml" at end</param>
        /// <returns>The symbol wanted if found, or null if the symbol could not be found</returns>
        public SymbolItem? GetSymbolByName(string name) => Symbols.Find(symbol => symbol.Name == name);

        /// <summary>
        /// Finds if the XFL contains a symbol with some given name
        /// </summary>
        /// <param name="name">Symbol name to find, should not include ".xml" at end</param>
        /// <returns>True if a symbol was found, otherwise false</returns>
        public bool ContainsSymbolByName(string name) => Symbols.Any(symbol => symbol.Name == name);

        /// <summary>
        /// Finds and returns wanted bitmap from given name
        /// </summary>
        /// <param name="name">Name for the bitmap to be found, should not included ".png" at end</param>
        /// <returns>The bitmap wanted if found, or null if the bitmap could not be found</returns>
        public Bitmap? GetBitmapByName(string name) => Bitmaps.Find(bitmap => bitmap.Name == name);

        /// <summary>
        /// Get a list of symbols that belong in a certain directory of the XFL
        /// </summary>
        /// <param name="directory"></param>
        /// <returns>A list of symbols from the directory</returns>
        public List<SymbolItem> GetSymbolsInDirectory(string directory)
        => Symbols.Where(s => Path.GetDirectoryName(s.Name) == directory)
                  .ToList();

        /// <summary>
        /// Get a list of symbols that belong in the `ImageFolder` directory of the XFL
        /// </summary>
        /// <returns>A list of image symbols</returns>
        public List<SymbolItem> GetImageSymbols() => GetSymbolsInDirectory(ImageFolder);

        /// <summary>
        /// Get a list of symbols that belong in the `SpriteFolder` directory of the XFL
        /// </summary>
        /// <returns>A list of sprite symbols</returns>
        public List<SymbolItem> GetSpriteSymbols() => GetSymbolsInDirectory(SpriteFolder);

        /// <summary>
        /// Get a list of symbols that belong in the `LabelFolder` directory of the XFL
        /// </summary>
        /// <returns>A list of label symbols</returns>
        public List<SymbolItem> GetLabelSymbols() => GetSymbolsInDirectory(LabelFolder);

        /// <summary>
        /// Get the main sprite symbol in the XFL
        /// </summary>
        /// <returns>Main sprite symbol</returns>
        public SymbolItem? GetMainSpriteSymbol() => GetSymbolByName(MainSprite);

        /// <summary>
        /// Checks if the XFL has a `main_sprite` symbol
        /// </summary>
        /// <returns>True if `main_sprite` was found, otherwise false</returns>
        public bool HasMainSpriteSymbol() => ContainsSymbolByName(MainSprite);

        /// <summary>
        /// Get a list of symbols that aren't in sprite, image, label, or is main_sprite
        /// </summary>
        /// <returns>List of symbol items</returns>
        public List<SymbolItem> GetUnorganizedSymbols()
        {
            List<SymbolItem> organizedSymbols = [
                ..GetSpriteSymbols(), ..GetImageSymbols(), ..GetLabelSymbols(),
                ];

            var mainSprite = GetMainSpriteSymbol();
            if (mainSprite is not null) organizedSymbols.Add(mainSprite);

            var tracker = organizedSymbols.ToHashSet();
            return Symbols.Where(s => !tracker.Add(s)).ToList();
        }

        /// <summary>
        /// Get if there are symbols that aren't in sprite, image, label, or is main_sprite
        /// </summary>
        /// <returns>True if there are any unorganized symbols, otherwise false</returns>
        public bool HasUnorganizedSymbols() => GetUnorganizedSymbols().Count > 0;

        /// <summary>
        /// Get each bitmap's name with its width and height
        /// </summary>
        /// <returns>A dictionary with bitmap name mapped to its width and height tuple</returns>
        public Dictionary<string, (int width, int height)> GetBitmapSizeDictionary()
        {
            Dictionary<string, (int width, int height)> bitmapSizes = [];
            foreach (var bitmap in Bitmaps)
            {
                var (width, height) = bitmap.Size;
                var name = bitmap.Name;
                bitmapSizes.Add(name, (width, height));
            }
            return bitmapSizes;
        }

        public void ConvertToSplitLabels(bool checkProgress = false)
        {
            var isSplitLabels = IsSplitLabelsType() ?? throw new InvalidOperationException("Could not determine type of XFL this is");
            if (isSplitLabels is true) throw new InvalidOperationException("XFL is already split labels type");
            if (!DOMDocument.HasEssentialLayers()) throw new InvalidOperationException("DOMDocument is missing essential layers");

            var labelDetails = DOMDocument.GetLabelIndexes();
            var mainSpriteSymbol = GetMainSpriteSymbol()!;
            var instanceLayer = DOMDocument.GetInstanceLayer();
            instanceLayer!.Frames = [];

            ProgressChecker? convertToSplitLabels = null;
            if (checkProgress) convertToSplitLabels = new("Converting XFL to split labels... ", labelDetails.Count);
            foreach (var labelDetail in labelDetails)
            {
                var (labelName, (startingIndex, endingIndex)) = labelDetail;
                var labelSymbolLayers = SymbolTimeline.CutLayers(mainSpriteSymbol.Timeline.Layers, startingIndex, endingIndex);
                var symbolName = $"label/{labelName}";
                var labelSymbol = new SymbolItem(symbolName, labelSymbolLayers);

                AddSymbol(labelSymbol);
                DOMDocument.AddNewSymbolItem(symbolName);
                var instanceLayerFrame = AnimateFrame.GetSingleKeyframe(startingIndex, endingIndex - startingIndex + 1, symbolName, elementType:"SymbolInstance");
                instanceLayer.Frames.Add(instanceLayerFrame);

                convertToSplitLabels?.AddOne();
            }
            DOMDocument.RemoveSymbolItem(MainSprite, includesEnd:false);
            Symbols.Remove(mainSpriteSymbol);
        }

        /// <summary>
        /// Ask for an XFL directory to process into an object
        /// </summary>
        /// <param name="options">Options that will affect how the XFL is initiazed</param>
        /// <param name="manadatoryPass">Functions that are required to pass "true" in order to return XFL, with corresponding error message to give if they fail</param>
        /// <returns>An XFL object created from the directory inputted by the user</returns>
        public static XFL AskForXFL(XFLInitOptions options, Dictionary<Func<XFL, bool>, string>? manadatoryPass = null)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                var userInput = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(userInput) || !Directory.Exists(userInput))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Enter a valid path");
                    continue;
                }

                try
                {
                    XFL xfl = new(userInput, options);
                    if (manadatoryPass is null) return xfl; // If no manadatory passes are provided, return xfl

                    bool success = true;
                    foreach (var (function, message) in manadatoryPass)
                    {
                        if (!function(xfl))
                        {
                            UM.PrintColoredText(ConsoleColor.Red, message, separateLines:true);
                            success = false;
                            break;
                        }
                    }
                    if (!success) continue;
                    return xfl;
                }
                catch (FileNotFoundException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nCould not find file");
                    continue;
                }
                catch (JsonException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nCould read {DatajsonFileName}, enter again");
                    continue;
                }
                catch (InvalidOperationException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nCould not process symbol files, enter again");
                    continue;
                }
                catch (InvalidImageContentException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nCould not process bitmap files, enter again");
                    continue;
                }
            }
        }

        /// <summary>
        /// Gets a list of labels that don't have a corresponding label symbol by name
        /// </summary>
        /// <returns>A list of labels without their own label symbol</returns>
        public List<string> GetLabelsWithNoSymbol() => DOMDocument.GetAllLabels()
                                                                  .ToDictionary(l => l, l => GetSymbolByName($"{LabelFolder}/{l}"))
                                                                  .Where(kvp => kvp.Value is null)
                                                                  .Select(kvp => kvp.Key)
                                                                  .ToList();
        
        /// <summary>
        /// Checks if all the labels in the DOMDocument match the size of the corresponding label symbol
        /// </summary>
        /// <returns>True if every label in DOMDocument matches label symbol, otherwise false</returns>
        public bool LabelSymbolsMatchLabelSize()
        {
            static string addLabelFolder(string name) => $"{LabelFolder}/{name}";
            var domdocumentLabelLengths = DOMDocument.GetLabelLengths();
            var labelNames = domdocumentLabelLengths.Keys.ToList();
            var labelSymbolNames = domdocumentLabelLengths.Keys.Select(addLabelFolder);
            var labelSymbolLengths = labelSymbolNames.ToDictionary(l => l, l => GetSymbolByName(l)?.Timeline.GetTotalLength());
            if (labelSymbolLengths.Any(kvp => kvp.Value is null)) return false;

            return !domdocumentLabelLengths.Keys.Any(l => labelSymbolLengths[addLabelFolder(l)] != domdocumentLabelLengths[l]);
        }

        /// <summary>
        /// Gets all the symbols in an XFL given its path
        /// </summary>
        /// <param name="path">Path to the XFL</param>
        /// <returns>A string list of the absolute path to every symbol in the XFL</returns>
        public static List<string> GetAllSymbolDirectories(string path)
        {
            var xfl = new XFL(path, new(){GetSymbols = true, GetBitmaps = false});
            var symbolList = xfl.DOMDocument.GetAllSymbolNames();
            return symbolList.Select(s => Path.Join(path, LibraryDirName, $"{s}.xml")).ToList();
        }
    }

    public class XFLInitOptions
    {
        public bool CheckProgress { get; init; } = false;

        public required bool GetSymbols { get; init; }
        public required bool GetBitmaps { get; init; }

        public bool GetDataJsonData { get; set; } = false;
        public int FixResolution { get; set; } = 0;
    }
}