using System.Xml.Serialization;
using System.Xml;

namespace XflComponents
{
    /// <summary>
    /// Main object for any symbol file
    /// </summary>
    [XmlRoot("DOMSymbolItem", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class SymbolItem
    {
        // Serializer
        [XmlIgnore]
        public static readonly XmlSerializer serializer = new(typeof(SymbolItem));

        // Strings
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;
        [XmlAttribute("itemID")]
        public string? ItemID { get; set; }
        [XmlAttribute("symbolType")]
        public string? SymbolType { get; set; } = DefaultSymbolType;
        [XmlAttribute("lastModified")]
        public string? LastModified { get; set; }
        [XmlAttribute("lastUniqueIdentifier")]
        public string? LastUniqueIdentifier { get; set; }

        // Lists
        [XmlArray("timeline", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DOMTimeline", typeof(SymbolTimeline), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<SymbolTimeline> TimelineList { get; set; } = [];

        [XmlIgnore]
        public SymbolTimeline Timeline
        {
            get
            {
                if (TimelineList.Count == 0)
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
                if (TimelineList.Count == 0) TimelineList = [value];
                else TimelineList[0] = value;
            }
        }

        public bool ShouldSerializeName() => !string.IsNullOrWhiteSpace(Name);

        public const string DefaultSymbolType = "graphic";

        public SymbolItem()
        {
        }

        public SymbolItem(string inputName)
        {
            Timeline = new SymbolTimeline() {
                Name = Path.GetFileName(inputName)
            };
            Name = inputName;
        }

        public SymbolItem(string inputName, List<AnimateLayer> layers)
        {
            Timeline = new SymbolTimeline() {
                Name = Path.GetFileName(inputName),
                Layers = layers
            };
            Name = inputName;
        }

        /// <summary>
        /// Makes a symbol with a single layer and keyframe, with a selected library item and returns it
        /// </summary>
        /// <param name="libraryItemName">Library item to be used by created symbol</param>
        /// <param name="symbolName">Name of the created symbol</param>
        /// <param name="elementType">Element type of library item, either bitmap or symbol instance</param>
        /// <returns>A created SymbolItem with a single layer and keyframe</returns>
        public static SymbolItem MakeSingleFrameSymbolItem(string libraryItemName, string symbolName, string? elementType = null)
        {
            var newFrame = AnimateFrame.GetSingleKeyframe(0, 1, libraryItemName, elementType);
            var newSymbol = new SymbolItem
            {
                Name = symbolName,
                Timeline = new SymbolTimeline()
                {
                    Layers = [new AnimateLayer("1", newFrame)],
                    Name = Path.GetFileName(symbolName)
                }
            };

            return newSymbol;
        }

        /// <summary>
        /// Check if the name of the symbol file matches the one in the symbol file
        /// </summary>
        /// <returns>True if the names match, otherwise false</returns>
        public bool NameMatchesTimeline() => Timeline?.Name == GetFileName();

        /// <summary>
        /// Get the folder this symbol file is in according to the name
        /// </summary>
        /// <returns>The folder directory this symbol file it is in, returns "" if none are found</returns>
        public string GetFolder()
        {
            var dirName = Path.GetDirectoryName(Name);
            return dirName ?? string.Empty;
        }

        /// <summary>
        /// Get the base file name of the symbol file, getting rid of folders
        /// </summary>
        /// <returns>A string of the base file name</returns>
        public string GetFileName()
        {
            var fileName = Path.GetFileName(Name);
            return fileName ?? string.Empty;
        }

        /// <summary>
        /// Changes the name of the symbol item, along with changing its timeline name
        /// </summary>
        /// <param name="newName">Name to change the symbol item to</param>
        public void ChangeName(string newName)
        {
            Name = newName;
            Timeline?.Name = Path.GetFileName(Name);
        }

        /// <summary>
        /// Makes a string representation of the symbol item with its name and number of layers
        /// </summary>
        /// <returns>A string with basic details of the symbol item</returns>
        public override string ToString()
        {
            return $"Symbol named {Name} with {Timeline.GetLayerCount()} layers";
        }
    }
}