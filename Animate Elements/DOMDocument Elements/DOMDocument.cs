using System.Xml.Serialization;
using System.Xml;

namespace XflComponents
{

    [XmlRoot("DOMDocument", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public class DOMDocument
    {
        // Statics
        [XmlIgnore]
        public static readonly XmlSerializer serializer = new(typeof(DOMDocument));

        [XmlAttribute]
        public string? width { get; set; }
        [XmlAttribute]
        public string? height { get; set; }
        [XmlAttribute("frameRate")]
        public string? frameRateString { get; set; }
        [XmlIgnore]
        public int? frameRate
        {
            get
            {
                if (string.IsNullOrEmpty(frameRateString)) return null;
                int results;
                if (!int.TryParse(frameRateString, out results)) return null;
                return results;
            }
        }
        [XmlAttribute]
        public string? currentTimeline { get; set; }
        [XmlAttribute]
        public string? xflVersion { get; set; }
        [XmlAttribute]
        public string? creatorInfo { get; set; }
        [XmlAttribute]
        public string? platform { get; set; }
        [XmlAttribute]
        public string? versionInfo { get; set; }
        [XmlAttribute]
        public string? majorVersion { get; set; }
        [XmlAttribute]
        public string? buildNumber { get; set; }
        [XmlAttribute]
        public string? gridSpacingX { get; set; }
        [XmlAttribute]
        public string? gridSpacingY { get; set; }
        [XmlAttribute]
        public string? gridSnapAccuracy { get; set; }
        [XmlAttribute]
        public string? gridSnapTo { get; set; }
        [XmlAttribute]
        public string? guidesLocked { get; set; }
        [XmlAttribute]
        public string? gridVisible { get; set; }
        [XmlAttribute]
        public string? rulerVisible { get; set; }
        [XmlAttribute]
        public string? viewAngle3D { get; set; }
        [XmlAttribute]
        public string? vanishingPoint3DX { get; set; }
        [XmlAttribute]
        public string? vanishingPoint3DY { get; set; }
        [XmlAttribute]
        public string? nextSceneIdentifier { get; set; }
        [XmlAttribute]
        public string? playOptionsPlayLoop { get; set; }
        [XmlAttribute]
        public string? playOptionsPlayPages { get; set; }
        [XmlAttribute]
        public string? playOptionsPlayFrameActions { get; set; }
        [XmlAttribute]
        public string? autoSaveHasPrompted { get; set; }
        [XmlAttribute]
        public string? filetypeGUID { get; set; }
        [XmlAttribute]
        public string? fileGUID { get; set; }

        [XmlArray("folders", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DOMFolderItem", typeof(DOMFolder), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<DOMFolder?>? FolderList { get; set; }

        [XmlArray("media", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DOMBitmapItem", typeof(DOMBitmapItem), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<DOMBitmapItem?>? BitmapItemList { get; set; }

        [XmlArray("symbols", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("Include", typeof(DOMSymbolItem), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<DOMSymbolItem?>? SymbolItemList { get; set; }

        [XmlArray("timelines", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DOMTimeline", typeof(SymbolTimeline), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public required List<SymbolTimeline?> TimelineList { get; set; } = [];

        [XmlElement("scripts", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public object? scripts { get; set; }

        [XmlElement("PrinterSettings", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public object? PrinterSettings { get; set; }

        [XmlArray("publishHistory", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("PublishItem", typeof(PublishItem), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<PublishItem>? publishHistory { get; set; }

        [XmlElement("SaveCustomEase", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<SaveCustomEase>? CustomEases { get; set; }

        [XmlIgnore]
        public SymbolTimeline Timeline
        {
            get
            {
                if (TimelineList.Count == 0 || TimelineList[0] is null)
                {
                    return new();
                }
                else
                {
                    return TimelineList[0]!;
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

        /// <summary>
        /// Get a list of all symbol names listed in the DOMDocument
        /// </summary>
        /// <param name="getEndNames">Symbol names will not contain folder names if true</param>
        /// <returns>A list of every symbol name</returns>
        public List<string> GetAllSymbolNames(bool getEndNames = false, bool getFileEnding = false)
        {
            if (SymbolItemList is null) return new List<string>();
            var symbolNames = new List<string>();
            foreach (DOMSymbolItem? symbol in SymbolItemList)
            {
                if (symbol is null || symbol.href is null || symbol.GetEndSymbolFile() is null)
                {
                    continue;
                }

                if (getEndNames)
                {
                    symbolNames.Add(symbol.GetEndSymbolFile()!);
                }
                else
                {
                    symbolNames.Add(symbol.href);
                }
            }
            return symbolNames;
        }

        /// <summary>
        /// Get a list of all bitmap names listed in the DOMDocument
        /// </summary>
        /// <param name="getEndNames">Bitmap names will not contain folder names if true</param>
        /// <returns>A list of every bitmap name</returns>
        public List<string> GetAllBitmapNames(bool getEndNames = false, bool getFileEnding = false)
        {
            var bitmapNames = new List<string>();
            if (BitmapItemList is null) return bitmapNames;
            foreach (DOMBitmapItem? bitmap in BitmapItemList)
            {
                if (bitmap is null || bitmap.name is null || bitmap.href is null 
                || bitmap.GetEndBitmapFile() is null)
                {
                    continue;
                }

                if (getEndNames)
                {
                    bitmapNames.Add(bitmap.GetEndBitmapFile()!);
                }
                if (getFileEnding)
                {
                    bitmapNames.Add(bitmap.href!);
                }
                else
                {
                    bitmapNames.Add(bitmap.name!);
                }
            }
            return bitmapNames;
        }

        /// <summary>
        /// Get a list of all bitmap and symbol names listed in the DOMDocument
        /// </summary>
        /// <param name="getEndNames">Bitmap and symbol names will not contain folder names if true</param>
        /// <returns>A list of every bitmap and symbol name</returns>
        public List<string> GetAllSymbolBitmapNames(bool getEndNames = false, bool getFileEnding = false)
        {
            var names = new List<string>();
            names.AddRange(GetAllSymbolNames(getEndNames));
            names.AddRange(GetAllBitmapNames(getEndNames, getFileEnding));
            return names;
        }

        /// <summary>
        /// Gets all of the labels found in the label layer of the DOMDocument
        /// </summary>
        /// <returns>A string list of all the labels in order of what they are found in, or an empty list if none are found</returns>
        public List<string> GetAllLabels()
        {
            if (Timeline is null || Timeline.Layers is null)
                return [];
            AnimateLayer? labelLayer = null;

            foreach (var layer in Timeline.Layers)
            {
                if (layer.name == "label")
                {
                    labelLayer = layer;
                    break;
                }
            }
            if (labelLayer is null || labelLayer.Frames is null)
                return [];

            List<string> labels = [];
            foreach (var frame in labelLayer.Frames)
            {
                if (frame.name is not null)
                    labels.Add(frame.name);
            }

            return labels;
        }

        /// <summary>
        /// Get a dictionary containing every label and the length of every layer
        /// </summary>
        /// <returns>A dictionary with the key being the label name and the value being an int representing the frame duration</returns>
        public Dictionary<string, int> GetLabelLengths()
        {
            if (Timeline is null || Timeline.Layers is null)
                return [];
            AnimateLayer? labelLayer = null;

            foreach (var layer in Timeline.Layers)
            {
                if (layer.name == "label")
                {
                    labelLayer = layer;
                    break;
                }
            }
            if (labelLayer is null || labelLayer.Frames is null)
                return [];

            var toReturn = new Dictionary<string, int>();
            foreach (var frame in labelLayer.Frames)
            {
                if (frame.name is not null)
                {
                    toReturn.Add(frame.name, frame.duration);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get a dictionary containing every label name and the start and ending indexes of each label
        /// </summary>
        /// <returns>A dictionary with the key being the label name and the value being a tuple of the start and end indexes</returns>
        public Dictionary<string, (int start, int end)> GetLabelIndexes()
        {

            if (Timeline is null || Timeline.Layers is null)
                return [];
            AnimateLayer? labelLayer = null;

            foreach (var layer in Timeline.Layers)
            {
                if (layer.name == "label")
                {
                    labelLayer = layer;
                    break;
                }
            }
            if (labelLayer is null || labelLayer.Frames is null)
                return [];

            var toReturn = new Dictionary<string, (int start, int end)>();
            foreach (var frame in labelLayer.Frames)
            {
                if (frame.name is not null)
                {
                    toReturn.Add(frame.name, (frame.index, frame.index + frame.duration - 1));
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Adds a new symbol to the DOMDocument
        /// </summary>
        /// <param name="name">Symbol item name to add, include folders but not ".xml"</param>
        public void AddNewSymbolItem(string name, bool includesEnd = false)
        {
            if (!includesEnd) name += ".xml";
            var toAddSymbolItem = new DOMSymbolItem()
            {
                href = name,
                loadImmediate = "false",
                itemIcon = "1"
            };
            SymbolItemList ??= [];
            SymbolItemList.Add(toAddSymbolItem);
        }

        /// <summary>
        /// Adds a new bitmap to the DOMDocument
        /// </summary>
        /// <param name="name">Bitmap item name to add, include folders but not ".png"</param>
        public void AddNewBitmapItem(string bitmapName)
        {
            var toAddBitmapItem = new DOMBitmapItem()
            {
                name = bitmapName,
                href = $"{bitmapName}.png",
                originalCompressionType = "lossless"
            };
            BitmapItemList ??= [];
            BitmapItemList.Add(toAddBitmapItem);
        }

        /// <summary>
        /// Removes a symbol item from the DOMDocument
        /// </summary>
        /// <param name="name"> Symbol name to be removed, include folders but not ".xml" (unless "includesEnd" is "true")</param>
        /// <param name="includesEnd">Put "true" if the end of each symbol item name includes ".xml"</param>
        public void RemoveSymbolItem(string name, bool includesEnd = true)
        {
            if (SymbolItemList is null) return;

            var NewSymbolItemList = new List<DOMSymbolItem>();
            foreach (var SymbolItem in SymbolItemList)
            {
                var nameToFind = SymbolItem!.href!;
                if (includesEnd) nameToFind = nameToFind.Replace(".xml", "");
                if (!(nameToFind == name)) // If name does not equate to provided name, skip it
                {
                    NewSymbolItemList.Add(SymbolItem);
                }
            }

            SymbolItemList = NewSymbolItemList!;
        }

        /// <summary>
        /// Removes list of symbol items from the DOMDocument
        /// </summary>
        /// <param name="nameList">List of symbol item names to remove, include folders but not ".xml" (unless "includesEnd" is "true")</param>
        /// <param name="beginning">Folder beginning to add to each symbol item name, do not include trailing "/"</param>
        /// <param name="includesEnd">Put "true" if the end of each symbol item name includes ".xml"</param>
        public void RemoveSymbolItem(List<string> nameList, string beginning = "", bool includesEnd = true)
        {
            if (SymbolItemList is null) return;

            var NewSymbolItemList = new List<DOMSymbolItem>();
            foreach (var SymbolItem in SymbolItemList)
            {
                var nameToFind = SymbolItem!.href!;
                if (includesEnd) nameToFind = nameToFind.Replace(".xml", "");
                if (beginning != "") nameToFind = nameToFind.Replace($"{beginning}/", "");

                if (!nameList.Contains(nameToFind))
                {
                    NewSymbolItemList.Add(SymbolItem);
                }
            }
            SymbolItemList = NewSymbolItemList!;
        }
        /// <summary>
        /// Removes a bitmap item from the DOMDocument
        /// </summary>
        /// <param name="name"> Bitmap name to be removed, include folders but not ".png" (unless "includesEnd" is "true")</param>
        /// <param name="includesEnd">Put "true" if the end of each symbol item name includes ".png"</param>
        public void RemoveBitmapItem(string name, bool includesEnd = true)
        {
            if (BitmapItemList is null) return;

            var NewBitmapItemList = new List<DOMBitmapItem>();
            foreach (var BitmapItem in BitmapItemList)
            {
                var nameToFind = BitmapItem!.href!;
                if (includesEnd) nameToFind = nameToFind.Replace(".png", "");
                if (!(nameToFind == name)) // If name does not equate to provided name, skip it
                {
                    NewBitmapItemList.Add(BitmapItem);
                }
            }

            BitmapItemList = NewBitmapItemList!;
        }

        /// <summary>
        /// Add a new folder item to the DOMDocument
        /// </summary>
        /// <param name="toAddName">Folder name to add</param>
        public void AddNewFolderItem(string toAddName)
        {
            if (FolderList is null) return;

            var newFolderItem = new DOMFolder()
            {
                name = toAddName
            };
            FolderList.Add(newFolderItem);
        }

        /// <summary>
        /// Remove a folder item to the DOMDocument
        /// </summary>
        /// <param name="name">Folder name to try to remove</param>
        public void RemoveFolderItem(string name)
        {
            if (FolderList is null) return;

            var NewFolderList = new List<DOMFolder>();
            foreach (var FolderItem in FolderList)
            {
                if (!(FolderItem!.name == name))
                {
                    NewFolderList.Add(FolderItem);
                }
            }

            FolderList = NewFolderList!;
        }

        /// <summary>
        /// Check if a symbol item exists in the DOMDocument
        /// </summary>
        /// <param name="name">Symbol item name to find</param>
        /// <returns>True if a symbol item is found, otherwise false</returns>
        public bool ContainsSymbolItem(string name)
        {
            if (SymbolItemList is null) return false;
            foreach (var symbolItem in SymbolItemList)
            {
                if (symbolItem is not null && symbolItem.href == name)
                {
                    return true;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return $"DOMDocument with {SymbolItemList?.Count} symbols and {BitmapItemList?.Count} bitmaps";
        }
    }

    public class SaveCustomEase
    {
        [XmlAttribute]
        public string? name { get; set; }

        [XmlElement("Point", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<XYPosition>? Point { get; set; }
    }

    public class PublishItem
    {
        [XmlAttribute]
        public string? publishSize { get; set; }
        [XmlAttribute]
        public string? publishTime { get; set; }
    }
}