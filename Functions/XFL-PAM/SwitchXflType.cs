using System.Text;
using System.Xml.Linq;
using XflComponents;
using UniversalMethods;

namespace HelperFunctions.Functions.Packages
{
    public class SwitchXflType
    {
        public static void Function()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter the XFL you want to convert");
            var xflPath = UM.AskForDirectory(["DOMDocument.xml"]);

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Enter the type of XFL this is");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[1] Split labels");
            Console.WriteLine("[2] Nonsplit labels");
            int xflType = UM.AskForInt(1, 2);

            var domdocumentPath = Path.Join(xflPath, "DOMDocument.xml");
            XDocument document = XDocument.Load(domdocumentPath);
            using var documentReader = document.CreateReader();
            DOMDocument DOMDocumentObject = (DOMDocument?)DOMDocument.serializer.Deserialize(documentReader)!;


            if (xflType == 1)
            {
                bool foundErrors = CheckXflType1Errors(DOMDocumentObject, xflPath);
                if (foundErrors) return;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Retrieving symbol details...");
                var labelSymbolDict = GetLabelSymbolDict(DOMDocumentObject, xflPath);
                if (labelSymbolDict is null) return;
                var labelDurations = DOMDocumentObject.GetLabelLengths()!;
                ProgressChecker.WriteFinished();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Creating main_sprite... ");
                var mainSymbolItem = new SymbolItem("main_sprite");
                // Add failsafe for duplicate labels later
                mainSymbolItem.Timeline!.Layers = CombineLayers(labelSymbolDict, labelDurations);
                ProgressChecker.WriteFinished();

                // Edit DOMDocument references
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Editing DOMDocument... ");
                DOMDocumentObject.AddNewSymbolItem("main_sprite");
                DOMDocumentObject.RemoveSymbolItem(labelSymbolDict.Keys.ToList(), "label");

                // Add symbol to DOMDocument instance layer
                var instanceLayer = DOMDocumentObject.Timeline!.GetLayerByName("instance")!;
                int mainSpriteLength = mainSymbolItem.Timeline.GetTotalLength();
                FrameElements toAddElement = new SymbolInstance()
                {
                    libraryItemName = "main_sprite",
                    symbolType = "graphic"
                };
                instanceLayer.Frames = [new AnimateFrame() {
                index = 0,
                duration = mainSpriteLength,
                Elements = [toAddElement]
                }];
                ProgressChecker.WriteFinished();

                // Delete label symbols
                Console.ForegroundColor = ConsoleColor.Green;
                var editFiles = new ProgressChecker("Writing and removing files... ",
                labelSymbolDict.Keys.Count + 2);
                var labelFolderDir = Path.Join(xflPath, "library", "label");
                foreach (var labelSymbolName in labelSymbolDict.Keys)
                {
                    var filePath = Path.Join(labelFolderDir, $"{labelSymbolName}.xml");
                    File.Delete(filePath);
                    editFiles.AddOne();
                }
                if (Directory.GetFiles(labelFolderDir).Length == 0)
                {
                    Directory.Delete(labelFolderDir);
                    DOMDocumentObject.RemoveFolderItem("label");
                }

                // Save DOMDocument + main sprite
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Saving files...");
                var mainSpritePath = Path.Join(xflPath, "library", "main_sprite.xml");
                UM.SaveXmlDocument(mainSpritePath, mainSymbolItem, UM.DummyXDocument, SymbolItem.serializer);
                editFiles.AddOne();
                UM.SaveXmlDocument(domdocumentPath, DOMDocumentObject, UM.DummyXDocument, DOMDocument.serializer);
                editFiles.AddOne();
            }

            if (xflType == 2)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Retrieving main_sprite... ");
                // Get main symbol, check for possible errors
                var mainSymbol = CheckXflType2Errors(DOMDocumentObject, xflPath);
                if (mainSymbol is null) return;
                var labelIndexes = DOMDocumentObject.GetLabelIndexes();
                ProgressChecker.WriteFinished();

                Console.ForegroundColor = ConsoleColor.Green;
                var makeLabelSymbols = new ProgressChecker("Writing new label symbols... ", labelIndexes.Count);
                // Add "label" folder
                Directory.CreateDirectory(Path.Join(xflPath, "library", "label"));
                DOMDocumentObject.AddNewFolderItem("label");

                // Loop through every label in DOMDocument
                foreach (var label in labelIndexes)
                {
                    // Get values
                    var labelName = label.Key;
                    var (start, end) = label.Value;

                    // Make new symbol
                    var newSymbol = UM.MakeDeepCopy(mainSymbol);
                    newSymbol.name = $"label/{labelName}";

                    // Cut layers, adjust other timeline features
                    var timeline = newSymbol.Timeline!;
                    timeline.CutLayers(start, end);
                    timeline.name = labelName;
                    timeline.currentFrame = null;

                    // Save new symbol, adjust DOMDocument with new symbol
                    var symbolPath = Path.Join(xflPath, "library", "label", $"{labelName}.xml");
                    UM.SaveXmlDocument(symbolPath, newSymbol, UM.DummyXDocument, SymbolItem.serializer);
                    DOMDocumentObject.AddNewSymbolItem($"label/{labelName}");

                    makeLabelSymbols.AddOne();
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Editing DOMDocument... ");
                // Add instance layer to DOMDocument
                var instanceLayer = DOMDocumentObject.Timeline.GetLayerByName("instance");
                instanceLayer ??= new AnimateLayer()
                {
                    name = "layer",
                    color = AnimateLayer.defaultColor
                };
                instanceLayer.Frames = AnimateFrame.GetKeyframeSeries(labelIndexes);

                // Remove main_sprite
                DOMDocumentObject.RemoveSymbolItem("main_sprite");
                UM.SaveXmlDocument(domdocumentPath, DOMDocumentObject, UM.DummyXDocument, DOMDocument.serializer);
                ProgressChecker.WriteFinished();
            }
        }

        private static Dictionary<string, SymbolItem>? GetLabelSymbolDict(DOMDocument DOMDocumentObject, string xflPath)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            var labelList = DOMDocumentObject.GetAllLabels();
            if (labelList.Count != labelList.Distinct().Count())
            {
                Console.WriteLine("Duplicate labels are found, ensure that all labels are unique");
                return null;
            }
            var labelSymbolDict = new Dictionary<string, SymbolItem?>();
            foreach (var label in labelList)
            {
                labelSymbolDict.Add(label, null);
            }

            int numOfMissingSymbols = labelSymbolDict.Count;
            foreach (var symbol in DOMDocumentObject.GetAllSymbolNames())
            {
                // Check if the symbol is a label symbol type
                if (!symbol.StartsWith("label/")) continue;
                var symbolEndName = symbol.Split("/")[^1].Replace(".xml", "");

                // Check if the DOMDocument uses it, proceed if it does
                if (labelSymbolDict.ContainsKey(symbolEndName))
                {
                    // Deserialize symbol into an object and add it to the dictionary
                    var symbolPath = Path.Join(xflPath, "library", symbol);
                    var symbolDocument = XDocument.Load(symbolPath);
                    using var symbolReader = symbolDocument.CreateReader();
                    var toAddSymbol = (SymbolItem?)SymbolItem.serializer.Deserialize(symbolReader);
                    labelSymbolDict[symbolEndName] = toAddSymbol;

                    numOfMissingSymbols--;
                    if (numOfMissingSymbols == 0)
                        break;
                }
            }

            if (numOfMissingSymbols > 0)
            {
                var errorMessage = new StringBuilder("Could not find the following symbols, ensure symbols are found in the label folder");
                foreach (var symbolPair in labelSymbolDict)
                {
                    if (symbolPair.Value is null)
                    {
                        errorMessage.AppendLine(symbolPair.Key);
                    }
                }
                Console.WriteLine(errorMessage.ToString());
                return null;
            }

            return labelSymbolDict!;
        }
        private static List<AnimateLayer> CombineLayers(Dictionary<string, SymbolItem> labelSymbolDict, Dictionary<string, int> labelDuration)
        {
            List<AnimateLayer> mainLayers = [];
            int currentFramePosition = 0;
            foreach (var labelSymbolPair in labelSymbolDict)
            {
                if (currentFramePosition > 0)
                    labelSymbolPair.Value.Timeline!.MoveFrames(currentFramePosition);
                var symbolLayers = labelSymbolPair.Value.Timeline!.Layers!;
                mainLayers.AddRange(symbolLayers);
                currentFramePosition += labelDuration[labelSymbolPair.Key];
            }

            return mainLayers;
        }
        private static bool CheckXflType1Errors(DOMDocument DOMDocument, string xflPath)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            bool foundErrors = false;

            var labels = DOMDocument.GetAllLabels();
            if (labels.Distinct().ToList().Count != labels.Count)
            {
                foundErrors = true;
                Console.WriteLine("Duplicate labels are found in the DOMDocument");
            }

            var symbols = DOMDocument.GetAllSymbolNames();
            foreach (var label in labels)
            {
                if (!symbols.Contains($"label/{label}.xml"))
                {
                    foundErrors = true;
                    Console.WriteLine($"Missing label symbol {label}");
                }
            }

            return foundErrors;
        }
        private static SymbolItem? CheckXflType2Errors(DOMDocument DOMDocument, string xflPath)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            bool foundErrors = false;
            SymbolItem? mainSprite = null;
            if (!DOMDocument.ContainsSymbolItem("main_sprite.xml"))
            {
                Console.WriteLine("Could not find main_sprite in DOMDocument");
                foundErrors = true;
            }
            else
            {
                var libraryPath = Path.Join(xflPath, "library");
                var libraryItems = Directory.GetFiles(libraryPath, "*.*", SearchOption.TopDirectoryOnly).ToList();
                if (!libraryItems.Contains(Path.Join(libraryPath, "main_sprite.xml")))
                {
                    Console.WriteLine("Could not find main_sprite in library folder");
                    foundErrors = true;
                }
                else
                {
                    var mainSpritePath = Path.Join(xflPath, "library", "main_sprite.xml");
                    XDocument tempDocument = XDocument.Load(mainSpritePath);
                    using var symbolItemReader = tempDocument.CreateReader();
                    mainSprite = (SymbolItem?)SymbolItem.serializer.Deserialize(symbolItemReader)!;
                }
            }

            // Check if instance layer exists
            var labels = DOMDocument.Timeline.GetLayerNames();
            AnimateLayer? instanceLayer = null;
            if (!labels.Contains("instance"))
            {
                Console.WriteLine("Could not find instance layer in DOMDocument");
                foundErrors = true;
            }
            else
            {
                instanceLayer = DOMDocument.Timeline.GetLayerByName("instance");
            }

            if (instanceLayer is not null)
            {
                int instanceLayerLength = instanceLayer.GetLayerLength();
                if (mainSprite is not null && instanceLayerLength != mainSprite.Timeline!.GetTotalLength())
                {
                    Console.WriteLine("Instance layer and main_sprite do not have the same length, ensure they have the same length");
                    foundErrors = true;
                }

                var instanceLayerLibraryItems = instanceLayer.GetAllLibraryItems();
                int numOfInstanceLayerItems = instanceLayerLibraryItems.Count;
                if (numOfInstanceLayerItems == 0)
                {
                    Console.WriteLine("Instance layer has no library items, it should only use \"main_sprite\"");
                    foundErrors = true;
                }
                else if (numOfInstanceLayerItems > 1)
                {
                    Console.WriteLine("Instance layer uses more than one library item, it should only use \"main_sprite\"");
                    foundErrors = true;
                }
                if (!instanceLayerLibraryItems.Contains("main_sprite"))
                {
                    Console.WriteLine("Instance layer does not use \"main_sprite\", it should only be using that sprite");
                    foundErrors = true;
                }
            }

            if (foundErrors)
            {
                return null;
            }
            return mainSprite;
        }
    }
}