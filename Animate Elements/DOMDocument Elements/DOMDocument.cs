using System.Xml.Serialization;
using System.Xml;

namespace XflComponents
{

    [XmlRoot("DOMDocument", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class DOMDocument
    {
        // Statics
        [XmlIgnore]
        public static readonly XmlSerializer serializer = new(typeof(DOMDocument));

        [XmlAttribute("backgroundColor")]
        public string? BackgroundColor {get; set;}
        [XmlAttribute("frameRate")]
        public string FrameRateString { get; set; } = "0";
        [XmlIgnore]
        public int FrameRate
        {
            get
            {
                if (string.IsNullOrEmpty(FrameRateString)
                    || !int.TryParse(FrameRateString, out var results)) return 0;
                return results;
            }
            set
            {
                FrameRateString = value.ToString();
            }
        }
        [XmlAttribute("width")]
        public string WidthString { get; set; } = DefaultSize.ToString();
        [XmlIgnore]
        public int Width
        {
            get
            {
                if (string.IsNullOrEmpty(WidthString)
                    || !int.TryParse(WidthString, out var results)) return DefaultSize;
                return results;
            }
            set
            {
                WidthString = value.ToString();
            }
        }
        [XmlAttribute("height")]
        public string HeightString { get; set; } = DefaultSize.ToString();
        [XmlIgnore]
        public int Height
        {
            get
            {
                if (string.IsNullOrEmpty(HeightString)
                    || !int.TryParse(HeightString, out var results)) return DefaultSize;
                return results;
            }
            set
            {
                HeightString = value.ToString();
            }
        }
        [XmlAttribute("currentTimeline")]
        public string? CurrentTimeline { get; set; }
        [XmlAttribute("xflVersion")]
        public string? XflVersion { get; set; }
        [XmlAttribute("creatorInfo")]
        public string? CreatorInfo { get; set; }
        [XmlAttribute("platform")]
        public string? Platform { get; set; }
        [XmlAttribute("versionInfo")]
        public string? VersionInfo { get; set; }
        [XmlAttribute("majorVersion")]
        public string? MajorVersion { get; set; }
        [XmlAttribute("buildNumber")]
        public string? BuildNumber { get; set; }
        [XmlAttribute("gridSpacingX")]
        public string? GridSpacingX { get; set; }
        [XmlAttribute("gridSpacingY")]
        public string? GridSpacingY { get; set; }
        [XmlAttribute("gridSnapAccuracy")]
        public string? GridSnapAccuracy { get; set; }
        [XmlAttribute("gridSnapTo")]
        public string? GridSnapTo { get; set; }
        [XmlAttribute("guidesLocked")]
        public string? GuidesLocked { get; set; }
        [XmlAttribute("gridVisible")]
        public string? GridVisible { get; set; }
        [XmlAttribute("rulerVisible")]
        public string? RulerVisible { get; set; }
        [XmlAttribute("viewAngle3D")]
        public string? ViewAngle3D { get; set; }
        [XmlAttribute("vanishingPoint3DX")]
        public string? VanishingPoint3DX { get; set; }
        [XmlAttribute("vanishingPoint3DY")]
        public string? VanishingPoint3DY { get; set; }
        [XmlAttribute("nextSceneIdentifier")]
        public string? NextSceneIdentifier { get; set; }
        [XmlAttribute("playOptionsPlayLoop")]
        public string? PlayOptionsPlayLoop { get; set; }
        [XmlAttribute("playOptionsPlayPages")]
        public string? PlayOptionsPlayPages { get; set; }
        [XmlAttribute("playOptionsPlayFrameActions")]
        public string? PlayOptionsPlayFrameActions { get; set; }
        [XmlAttribute("autoSaveHasPrompted")]
        public string? AutoSaveHasPrompted { get; set; }
        [XmlAttribute("filetypeGUID")]
        public string? FiletypeGUID { get; set; }
        [XmlAttribute("fileGUID")]
        public string? FileGUID { get; set; }

        [XmlArray("folders", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DOMFolderItem", typeof(DOMFolder), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<DOMFolder> FolderList { get; set; } = [];

        [XmlArray("media", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DOMBitmapItem", typeof(DOMBitmapItem), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<DOMBitmapItem> BitmapItemList { get; set; } = [];

        [XmlArray("symbols", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("Include", typeof(DOMSymbolItem), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<DOMSymbolItem> SymbolItemList { get; set; } = [];

        [XmlArray("timelines", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DOMTimeline", typeof(SymbolTimeline), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public required List<SymbolTimeline> TimelineList { get; set; } = [];

        [XmlElement("scripts", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public object? Scripts { get; set; }

        [XmlElement("PrinterSettings", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public object? PrinterSettings { get; set; }

        [XmlArray("publishHistory", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("PublishItem", typeof(PublishItem), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<PublishItem> PublishHistory { get; set; } = [];

        [XmlElement("SaveCustomEase", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<SaveCustomEase> CustomEases { get; set; } = [];

        public bool ShouldSerializeFolderList() => FolderList.Count > 0;
        public bool ShouldSerializeBitmapItemList() => BitmapItemList.Count > 0;
        public bool ShoudlSerializeSymbolItemList() => SymbolItemList.Count > 0;
        public bool ShouldSerializePublishHistory() => PublishHistory.Count > 0;
        public bool ShouldSerializeCustomEases() => CustomEases.Count > 0;
        public bool ShouldSerializeFrameRateString() => FrameRate > 0;
        /*public bool ShouldSerializeWidthString() => Width > 0;
        public bool ShouldSerializeHeightString() => Height > 0;*/

        [XmlIgnore]
        public required SymbolTimeline Timeline
        {
            get
            {
                if (TimelineList.Count == 0 || TimelineList[0] is null)
                {
                    return new();
                }
                else
                {
                    return TimelineList[0];
                }
            }
            set
            {
                if (TimelineList.Count == 0)
                {
                    TimelineList.Add(value);
                }
                else
                {
                    TimelineList[0] = value;
                }
            }
        }

        [XmlIgnore]
        public const string LabelLayer = "label";
        [XmlIgnore]
        public const string ActionLayer = "action";
        [XmlIgnore]
        public const string InstanceLayer = "instance";
        [XmlIgnore]
        public const string TimelineName = "animation";
        [XmlIgnore]
        public const int DefaultSize = 390;

        /// <summary>
        /// Get a list of all symbol names listed in the DOMDocument
        /// </summary>
        /// <param name="getEndNames">Symbol names will not contain folder names if true</param>
        /// <param name="getFileEnding">Symbol names will contain ".xml" at the end if true</param>
        /// <returns>A list of every symbol name</returns>
        public List<string> GetAllSymbolNames(bool getFolderNames = true, bool getFileEnding = false)
        {
            var symbolNames = SymbolItemList.OfType<DOMSymbolItem>()
                                            .Where(s => s.Href is not null)
                                            .Select(s => s.Href);
            if (!getFileEnding) symbolNames = symbolNames.Select(s => Path.ChangeExtension(s, null));
            if (!getFolderNames) symbolNames = symbolNames.Select(s => Path.GetFileName(s));
            return symbolNames.ToList();
        }

        /// <summary>
        /// Get a list of all bitmap names listed in the DOMDocument
        /// </summary>
        /// <param name="getFolderNames">Bitmap names will not contain folder names if true</param>
        /// <param name="getFileEnding">Bitmap names will contain ".png" at the end if true</param>
        /// <returns>A list of every bitmap name</returns>
        public List<string> GetAllBitmapNames(bool getFolderNames = true, bool getFileEnding = false)
        {
            var bitmapNames = BitmapItemList.OfType<DOMBitmapItem>()
                                            .Where(b => b.Href is not null)
                                            .Select(b => b.Href);
            if (!getFileEnding) bitmapNames = bitmapNames.Select(b => Path.ChangeExtension(b, null));
            if (!getFolderNames) bitmapNames = bitmapNames.Select(b => Path.GetFileName(b));
            return bitmapNames.ToList();
        }

        /// <summary>
        /// Get a list of all bitmap and symbol names listed in the DOMDocument
        /// </summary>
        /// <param name="getEndNames">Bitmap and symbol names will not contain folder names if true</param>
        /// <param name="getFileEnding">Bitmap and symbol names will contain ".xml" at the end if true</param>
        /// <returns>A list of every bitmap and symbol name</returns>
        public List<string> GetAllSymbolBitmapNames(bool getEndNames = false, bool getFileEnding = false)
        {
            return [..GetAllSymbolNames(getEndNames, getFileEnding),
                    ..GetAllBitmapNames(getEndNames, getFileEnding)];
        }

        /// <summary>
        /// Gets all of the labels found in the label layer of the DOMDocument
        /// </summary>
        /// <returns>A string list of all the labels in order of what they are found in, or an empty list if none are found</returns>
        public List<string> GetAllLabels()
        {
            var labelLayer = Timeline.GetLayerByName(LabelLayer);
            if (labelLayer is null
                || labelLayer.Frames is null) return [];

            return labelLayer.Frames
                             .Where(f => f.Name is not null)
                             .Select(f => f.Name!)
                             .ToList();
        }

        /// <summary>
        /// Finds if a certain label exists in the DOMDocument's label layer
        /// </summary>
        /// <param name="labelToFind">Label to try to find</param>
        /// <returns>True if the label exists, otherwise false</returns>
        public bool HasLabel(string labelToFind) => GetAllLabels().Contains(labelToFind);

        /// <summary>
        /// Get a dictionary containing every label and the length of every layer
        /// </summary>
        /// <returns>A dictionary with the key being the label name and the value being an int representing the frame duration</returns>
        public Dictionary<string, int> GetLabelLengths()
        {
            var labelLayer = Timeline.GetLayerByName(LabelLayer);
            if (labelLayer is null) return [];

            return labelLayer.Frames
                             .Where(f => f.Name is not null)
                             .ToDictionary(f => f.Name!, f => f.Duration);
        }

        /// <summary>
        /// Get a dictionary containing every label name and the start and ending indexes of each label
        /// </summary>
        /// <returns>A dictionary with the key being the label name and the value being a tuple of the start and end indexes</returns>
        public Dictionary<string, (int start, int end)> GetLabelIndexes()
            => Timeline.GetLayerByName(LabelLayer)?
                       .Frames
                       .Where(f => f.Name is not null)
                       .ToDictionary(f => f.Name!, f => (f.Index, f.Index + f.Duration - 1))

                       ?? [];

        /// <summary>
        /// Adds a new DOMSymbol to the DOMDocument
        /// </summary>
        /// <param name="name">Symbol item name to add, include folders but not ".xml"</param>
        /// <param name="includesEnd">If false, ".xml" will be added at the end of the added DOMSymbolItem</param>
        public void AddNewSymbolItem(string name, bool includesEnd = false)
        {
            if (!includesEnd) name += ".xml";
            var toAddSymbolItem = new DOMSymbolItem()
            {
                Href = name,
                LoadImmediate = "false",
                ItemIcon = "1"
            };
            SymbolItemList ??= [];
            SymbolItemList.Add(toAddSymbolItem);
        }

        /// <summary>
        /// Add a range of DOMSymbol names to the DOMDocument
        /// </summary>
        /// <param name="symbolNames">List of symbol names to add</param>
        /// <param name="includesEnd">If false, ".xml" will be added at the end of the added DOMSymbolItem</param>
        public void AddNewSymbolItemRange(string[] symbolNames, bool includesEnd = false)
        {
            foreach (var symbolName in symbolNames)
            {
                AddNewSymbolItem(symbolName, includesEnd);
            }
        }

        /// <summary>
        /// Adds a new BitmapItems to the DOMDocument
        /// </summary>
        /// <param name="bitmapName">Bitmap item name to add, include folders but not ".png"</param>
        public void AddNewBitmapItem(string bitmapName)
        {
            var toAddBitmapItem = new DOMBitmapItem()
            {
                Name = bitmapName,
                Href = $"{bitmapName}.png",
                AllowSmoothing = "true",
                CompressionType = "lossless",
                OriginalCompressionType = "lossless"
            };
            BitmapItemList ??= [];
            BitmapItemList.Add(toAddBitmapItem);
        }

        /// <summary>
        /// Adds a range of BitmapItems to the DOMDocument
        /// </summary>
        /// <param name="bitmapNameList">List of bitmap names to add</param>
        public void AddNewBitmapItemRange(string[] bitmapNameList)
        {
            foreach (var bitmapName in bitmapNameList)
            {
                AddNewBitmapItem(bitmapName);
            }
        }

        /// <summary>
        /// Removes a symbol item from the DOMDocument
        /// </summary>
        /// <param name="name"> Symbol name to be removed, include folders but not ".xml" (unless "includesEnd" is "true")</param>
        /// <param name="includesEnd">If false, ".xml" will be added to "name"</param>
        /// <returns>True if an item was removed successfully, otherwise false</returns>
        public bool RemoveSymbolItem(string name, bool includesEnd = true)
        {
            if (!includesEnd) name = Path.ChangeExtension(name, "xml");
            var oldSymbolCount = SymbolItemList.Count;
            SymbolItemList = SymbolItemList.Where(s => s is not null && s.Href != name).ToList();
            return oldSymbolCount != SymbolItemList.Count;
        }

        /// <summary>
        /// Removes list of symbol items from the DOMDocument
        /// </summary>
        /// <param name="nameList">List of symbol item names to remove, include folders but not ".xml" (unless "includesEnd" is "true")</param>
        /// <param name="beginning">Folder beginning to add to each symbol item name, do not include trailing "/"</param>
        /// <param name="includesEnd">If false, ".xml" will be added to the end of each name in `nameList`</param>
        /// <returns>True if at least one item was removed successfully, otherwise false</returns>
        public bool RemoveSymbolItem(List<string> nameList, string? beginning = null, bool includesEnd = true)
        {
            if (!includesEnd) nameList = nameList.Select(name => $"{name}.xml").ToList();
            if (beginning is not null) nameList = nameList.Select(name => Path.Join(beginning, name)).ToList();

            var oldSymbolCount = SymbolItemList.Count;
            SymbolItemList = SymbolItemList.Where(s => s.Href is null || !nameList.Contains(s.Href)).ToList();
            return oldSymbolCount != SymbolItemList.Count;
        }
        
        /// <summary>
        /// Removes a bitmap item from the DOMDocument
        /// </summary>
        /// <param name="name"> Bitmap name to be removed, include folders but not ".png" (unless "includesEnd" is "true")</param>
        /// <param name="includesEnd">Put "true" if the end of each symbol item name includes ".png"</param>
        /// <returns>True if an item was removed successfully, otherwise false</returns>
        public bool RemoveBitmapItem(string name, bool includesEnd = true)
        {
            if (!includesEnd) name = Path.ChangeExtension(name, "png");
            var oldBitmapCount = BitmapItemList.Count;
            BitmapItemList = BitmapItemList.Where(b => b is not null && b.Href != name).ToList();
            return oldBitmapCount != BitmapItemList.Count;
        }

        /// <summary>
        /// Add a new folder item to the DOMDocument
        /// </summary>
        /// <param name="toAddName">Folder name to add</param>
        public void AddNewFolderItem(string toAddName)
        {
            FolderList ??= [];
            FolderList.Add(new(){Name = toAddName});
        }

        /// <summary>
        /// Remove a folder item in the DOMDocument
        /// </summary>
        /// <param name="name">Folder name to try to remove</param>
        /// <returns>True if an item was removed successfully, otherwise false</returns>
        public bool RemoveFolderItem(string name)
        {
            var oldFolderCount = FolderList.Count;
            FolderList = FolderList.Where(f => f.Name is not null && 
                                          f.Name != name).ToList();
            return oldFolderCount != FolderList.Count;
        }

        /// <summary>
        /// Check if a symbol item exists in the DOMDocument
        /// </summary>
        /// <param name="name">Symbol item name to find</param>
        /// <returns>True if a symbol item is found, otherwise false</returns>
        public bool ContainsSymbolItem(string name) => SymbolItemList.Any(s => s.Href == name);
        
        /// <summary>
        /// Get a list of all used library items by the DOMDocument
        /// </summary>
        /// <returns>A list of used library items</returns>
        public List<string> GetUsedSymbols()
        {
            var instanceLayer = Timeline.GetLayerByName(InstanceLayer);
            return instanceLayer?.GetAllLibraryItems() ?? [];
        }

        /// <summary>
        /// Finds if the DOMDocument's timeline contains the instance layer
        /// </summary>
        /// <returns>True if the instance layer is found, otherwise false</returns>
        public bool ContainsInstanceLayer() => Timeline.Layers.Any(l => l.Name == InstanceLayer);

        /// <summary>
        /// Finds if the DOMDocument's timeline contains the action layer
        /// </summary>
        /// <returns>True if the action layer is found, otherwise false</returns>
        public bool ContainsActionLayer() => Timeline.Layers.Any(l => l.Name == ActionLayer);

        /// <summary>
        /// Finds if the DOMDocument's timeline contains the label layer
        /// </summary>
        /// <returns>True if the label layer is found, otherwise false</returns>
        public bool ContainsLabelLayer() => Timeline.Layers.Any(l => l.Name == LabelLayer);

        /// <summary>
        /// Finds if the DOMDocument's timeline contains the instance, action, and label layer
        /// </summary>
        /// <returns>True if the 3 necessary layers are found, otherwise false</returns>
        public bool ContainsEssentialLayers() => ContainsInstanceLayer() &&
                                                 ContainsActionLayer() &&
                                                 ContainsLabelLayer();

        /// <summary>
        /// Gets the DOMDocument's instance layer
        /// </summary>
        /// <returns>An AnimateLayer object that is the instance layer if found, otherwise null</returns>
        public AnimateLayer? GetInstanceLayer() => Timeline.Layers.Find(l => l.Name == InstanceLayer);

        /// <summary>
        /// Gets the DOMDocument's action layer
        /// </summary>
        /// <returns>An AnimateLayer object that is the action layer if found, otherwise null</returns>
        public AnimateLayer? GetActionLayer() => Timeline.Layers.Find(l => l.Name == ActionLayer);

        /// <summary>
        /// Gets the DOMDocument's label layer
        /// </summary>
        /// <returns>An AnimateLayer object that is the label layer if found, otherwise null</returns>
        public AnimateLayer? GetLabelLayer() => Timeline.Layers.Find(l => l.Name == LabelLayer);

        /// <summary>
        /// Gets a tuple of the instance, action, and label layer together
        /// </summary>
        /// <returns>A tuple AnimateLayer objects of the instance, action, and label layers, which each may be null if not found</returns>
        public (AnimateLayer? instanceLayer, AnimateLayer? actionLabel, AnimateLayer? labelLayer) GetEssentialLayers()
        => (GetInstanceLayer(), GetActionLayer(), GetLabelLayer());

        public bool HasEssentialLayers() => GetInstanceLayer() is not null && GetActionLayer() is not null && GetLabelLayer() is not null;

        /// <summary>
        /// Makes a default timeline for a DOMDocument with essentail layers
        /// </summary>
        /// <returns>A timeline with the essential DOMDocument layers</returns>
        public static SymbolTimeline MakeNewDOMDocumentTimeline()
        {
            var timeline = new SymbolTimeline()
            {
                Name = TimelineName,
                Layers = [new(LabelLayer), new(ActionLayer), new(InstanceLayer)]
            };

            return timeline;
        }

        /// <summary>
        /// Checks if the label layer of the DOMDocument contains any duplicate labels
        /// </summary>
        /// <returns>True if there are duplicates found, otherwise false</returns>
        public bool ContainsDuplicateLabels()
        {
            var labelNames = GetLabelLayer()?.Frames
                                            .Select(f => f.Name)
                                            .Where(n => n is not null);
            if (labelNames is null) return false;

            var duplicateCheck = new HashSet<string>();
            return labelNames.Any(n => !duplicateCheck.Add(n!));
        }

        /// <summary>
        /// Get a string representation of the DOMDocument, with the number of symbol items and bitmap items
        /// </summary>
        /// <returns>A string with basic details of the DOMDocument</returns>
        public override string ToString()
        {
            return $"DOMDocument with {SymbolItemList?.Count ?? 0} symbols and {BitmapItemList?.Count ?? 0} bitmaps";
        }
    }

    public class SaveCustomEase
    {
        [XmlAttribute("name")]
        public string? Name { get; set; }

        [XmlElement("Point", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<XYPosition>? Point { get; set; }
    }

    public class PublishItem
    {
        [XmlAttribute("publishSize")]
        public string? PublishSize { get; set; }
        [XmlAttribute("publishTime")]
        public string? PublishTime { get; set; }
    }
}