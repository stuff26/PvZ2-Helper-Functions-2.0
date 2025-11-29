using System.Xml.Serialization;
using System.Xml;

namespace XflComponents
{
    /// <summary>
    /// Main object for any symbol file
    /// </summary>
    [XmlRoot("DOMSymbolItem", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public class SymbolItem
    {
        // Serializer
        [XmlIgnore]
        public static readonly XmlSerializer serializer = new(typeof(SymbolItem));


        // Strings
        [XmlAttribute]
        public string? name { get; set; }
        [XmlAttribute]
        public string? itemID { get; set; }
        [XmlAttribute]
        public string? symbolType { get; set; }
        [XmlAttribute]
        public string? lastModified { get; set; }
        [XmlAttribute]
        public string? lastUniqueIdentifier { get; set; }

        // Lists
        [XmlArray("timeline", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DOMTimeline", typeof(SymbolTimeline), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<SymbolTimeline?>? TimelineList { get; set; }

        [XmlIgnore]
        public SymbolTimeline? Timeline
        {
            get
            {
                if (TimelineList is null || TimelineList.Count == 0)
                {
                    return null;
                }
                else
                {
                    return TimelineList[0];
                }
            }
            set
            {
                if (TimelineList is null)
                {
                    TimelineList = [value];
                }
                else if (TimelineList.Count == 0)
                {
                    TimelineList.Add(value);
                }
                else
                {
                    TimelineList[0] = value;
                }
            }
        }

        public SymbolItem() { }

        public SymbolItem(string inputName)
        {
            TimelineList = [new SymbolTimeline() {
                name = inputName
            }];
            name = inputName.Split("/")[^1];
            symbolType = "graphic";
        }

        public static SymbolItem MakeSingleFrameSymbolItem(string libraryItemName, string symbolName, string timelineName, string? elementType = null)
        {
            var newFrame = AnimateFrame.GetSingleKeyframe(0, 1, libraryItemName, elementType);
            var newSymbol = new SymbolItem
            {
                name = symbolName,
                symbolType = "graphic",
                Timeline = new SymbolTimeline()
                {
                    Layers = [new AnimateLayer(){
                        Frames = [newFrame],
                        color = "#4F4FFF",
                        name = "1"
                    }
                    ],
                    name = timelineName
                }
            };

            return newSymbol;
        }

        /// <summary>
        /// Check if the name of the symbol file matches the one in the symbol file
        /// </summary>
        /// <returns>True if the names match, otherwise false</returns>
        public bool NameMatchesTimeline()
        {
            return Timeline?.name == GetFileName();
        }

        /// <summary>
        /// Get the folder this symbol file is in according to the name
        /// </summary>
        /// <returns>The folder directory this symbol file it is in, returns "" if none are found</returns>
        public string GetFolder()
        {
            if (name is null)
            {
                return "";
            }
            string[] folderNames = name.Split("/");
            if (folderNames.Length == 1) return folderNames[0];
            
            string folderDir = "";
            for (int i = 0; i < folderNames.Length - 1; i++)
            {
                folderDir += folderNames[i] + "/";
            }

            return folderDir[..^1];
        }

        /// <summary>
        /// Get the base file name of the symbol file, getting rid of folders
        /// </summary>
        /// <returns>A string of the base file name</returns>
        public string GetFileName()
        {
            if (name is null)
            {
                return "";
            }
            string[] folderNames = name.Split("/");
            return folderNames[^1];
        }

        public override string ToString()
        {
            return $"Symbol named {name} with {Timeline!.GetLayerCount()} layers";
        }
    }
}