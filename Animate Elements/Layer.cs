using System.Xml.Serialization;
using System.Xml;
using UniversalMethods;

namespace XflComponents
{

    /// <summary>
    /// A layer found within a timeline, contains a series of frames
    /// </summary>
    [XmlRoot("DOMLayer", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public class AnimateLayer
    {
        [XmlIgnore]
        public static readonly string defaultColor = "#4F4FFF";
        // Strings
        [XmlAttribute]
        public string name { get; set; } = "";
        [XmlAttribute]
        public string? color { get; set; }
        [XmlAttribute]
        public string? layerType { get; set; }
        [XmlAttribute]
        public string? current { get; set; }
        [XmlAttribute]
        public string? isSelected { get; set; }
        [XmlAttribute]
        public string? animationType { get; set; }

        // Nums
        [XmlAttribute]
        public string? heightMultiplier { get; set; }
        [XmlAttribute]
        public string? parentLayerIndex { get; set; }
        [XmlAttribute]
        public string? alphaPercent { get; set; }

        // Booleans
        [XmlAttribute]
        public string? hidden { get; set; }
        [XmlAttribute]
        public string? locked { get; set; }
        [XmlAttribute]
        public string? autoNamed { get; set; }
        [XmlAttribute]
        public string? transparent { get; set; }
        [XmlAttribute]
        public string? highlighted { get; set; }
        [XmlAttribute]
        public string? useOutlineView { get; set; }

        // Lists
        [XmlArray("frames", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DOMFrame", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<AnimateFrame>? Frames { get; set; }

        public override string ToString()
        {
            return $"Layer {name}, frame count {GetLayerLength()}";
        }

        /// <summary>
        /// Makes and returns a shallow copy of the current layer
        /// </summary>
        /// <returns>A shallow copy of the layer</returns>
        public AnimateLayer MakeCopy()
        {
            return (AnimateLayer)MemberwiseClone();
        }

        /// <summary>
        /// Checks if there is a nonzero number of frames in the layer
        /// </summary>
        /// <returns>True if there is at least one frame, otherwise false</returns>
        public bool HasFrames()
        {
            return Frames?.Count > 0;
        }

        /// <summary>
        /// Gets the number of frames in a layer
        /// </summary>
        /// <returns>The number of frames there are, returns 0 if none are found</returns>
        public int GetNumOfFrames()
        {
            if (Frames is null) return 0;
            return Frames.Count;
        }

        /// <summary>
        /// Gets the length of the layer by checking when the last frame ends
        /// </summary>
        /// <returns>The length of the layer</returns>
        public int GetLayerLength()
        {
            if (Frames is null || Frames.Count == 0)
            {
                return 0;
            }

            var lastFrame = Frames[^1];
            int lastIndex = lastFrame.index;
            int lastIndexDuration = lastFrame.duration;

            return lastIndex + lastIndexDuration;
        }

        /// <summary>
        /// Gets a list of every frame element that the layer contians
        /// </summary>
        /// <returns>A list of frame elements that exist in the layer</returns>
        public List<FrameElements> GetAllFrameElements()
        {
            if (Frames is null) return [];
            var AllElements = new List<FrameElements>();
            foreach (AnimateFrame? frame in Frames)
            {
                List<FrameElements>? FrameElements = frame?.Elements;
                if (frame is null || FrameElements is null) continue;
                AllElements.AddRange(FrameElements);
            }

            return AllElements;
        }

        /// <summary>
        /// Gets a list of every library item in the layer
        /// </summary>
        /// <param name="uniqueNames">If true, the returned list will not contain any duplicates</param>
        /// <returns>A string list of every library item</returns>
        public List<string> GetAllLibraryItems(bool uniqueNames = true)
        {
            List<string> AllLibraryItems = [];
            if (Frames is null) return AllLibraryItems;

            foreach (AnimateFrame? loopFrame in Frames)
            {
                List<string> LoopLibraryItems = loopFrame.GetAllLibraryItems();

                foreach (string libraryItemName in LoopLibraryItems)
                {
                    AllLibraryItems.Add(libraryItemName);
                }
            }
            if (uniqueNames)
            {
                AllLibraryItems = AllLibraryItems.Distinct().ToList();
            }
            return AllLibraryItems;
        }

        /// <summary>
        /// Gets the main library item used by checking the first frame in the 
        /// </summary>
        /// <returns>The first library item found in the layer</returns>
        public string GetMainLibraryItem()
        {
            var LibraryItems = GetAllLibraryItems();
            if (LibraryItems.Count > 0) return LibraryItems[0];
            else return "";
        }

        /// <summary>
        /// Get all of the action frames in the layer
        /// </summary>
        /// <returns>A string list consisting of every action frame in the layer</returns>
        public List<string> GetActions()
        {
            var FoundActions = new List<string>();
            if (Frames is null) return FoundActions;

            foreach (var frame in Frames)
            {
                if (frame.Actionscript is not null)
                {
                    var actionScripts = frame.GetActionScripts();
                    FoundActions.AddRange(actionScripts);
                }
            }

            return FoundActions;
        }

        /// <summary>
        /// Checks if the layer's frames has any elements
        /// </summary>
        /// <returns>True if there elements are found, otherwise false</returns>
        public bool HasFrameElements()
        {
            if (Frames is null) return false;
            foreach (var frame in Frames)
            {
                if (frame?.Elements?.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// checks if the layer's frames have any actions
        /// </summary>
        /// <returns>True if there are actiosn found, otherwise false</returns>
        public bool HasActions()
        {
            if (Frames is null) return false;
            foreach (var frame in Frames)
            {
                if (frame?.Actionscript is not null)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if there are any labels found in the frames of the layer
        /// </summary>
        /// <returns>True if any labels are found, otherwise false</returns>
        public bool HasLabels()
        {
            if (Frames is null) return false;
            foreach (var frame in Frames)
            {
                if (!string.IsNullOrEmpty(frame.labelType) || !string.IsNullOrEmpty(frame.name))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Move every frame's index by a certain amount
        /// </summary>
        /// <param name="amount">Amount of frames to move everything by</param>
        public void MoveFrames(int amount)
        {
            if (Frames is null) return;

            foreach (var frame in Frames)
            {
                frame.index += amount;
            }
            var blankFrame = new AnimateFrame()
            {
                index = 0,
                duration = amount
            };
            Frames.Insert(0, blankFrame);
        }

        /// <summary>
        /// Cut out a portion of a layer's frames and make a deep copy of the layer with it, note indexes start at 0 instead of 1
        /// </summary>
        /// <param name="beginIndex">First frame index to include</param>
        /// <param name="endIndex">Last frame index to include</param>
        /// <returns>A deep copy of the layer with cut out frames</returns>
        public AnimateLayer CutLayer(int beginIndex, int endIndex = -1)
        {
            if (endIndex < 0) endIndex = GetLayerLength();
            int maxDuration = endIndex - beginIndex;

            // Setup
            var newLayer = UM.MakeDeepCopy(this);
            var newFrames = new List<AnimateFrame>();

            // Find all frames that have
            foreach (var frame in newLayer.Frames!)
            {
                int index = frame.index;
                if (index > endIndex) continue; // Skip frames that start after the end bound
                int duration = frame.duration;

                if ((index >= beginIndex && index < endIndex) // Beginning index is within bounds
                || index + duration > beginIndex) // A keyframe lasts into the bounds
                {
                    newFrames.Add(frame);
                }
            }
            newLayer.Frames = newFrames;
            if (newFrames.Count == 0)
            {
                return newLayer;
            }

            // Fix all of the frames to be in the right spot
            foreach (var frame in newFrames)
            {
                frame.index -= beginIndex;
                if (frame.index < 0)
                {
                    frame.duration += frame.index; // Will decrease since index < 0
                    frame.index = 0;
                }
                if (frame.index + frame.duration > maxDuration)
                {
                    frame.duration = maxDuration - frame.index + 1;
                }
            }

            return newLayer;
        }

        /// <summary>
        /// Remove empty frames that trail at the end of a layer
        /// </summary>
        public void RemoveTrailingFrames()
        {
            if (Frames is null) return;
            for (int frameIndex = Frames!.Count - 1; frameIndex >= 0; frameIndex--)
            {
                var currentFrame = Frames[frameIndex];
                if (currentFrame.Elements.Count == 0)
                {
                    Frames.RemoveAt(frameIndex);
                }
                else
                {
                    return;
                }
            }
        }
    }
}