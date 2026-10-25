using System.Xml.Serialization;
using System.Xml;
using UniversalMethods;

namespace XflComponents
{

    /// <summary>
    /// A layer found within a timeline, contains a series of frames
    /// </summary>
    [XmlRoot("DOMLayer", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class AnimateLayer
    {
        // Strings
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;
        [XmlAttribute("color")]
        public string? Color { get; set; } = DefaultColor;
        [XmlAttribute("layerType")]
        public string? LayerType { get; set; }
        [XmlAttribute("current")]
        public string? Current { get; set; }
        [XmlAttribute("isSelected")]
        public string? IsSelected { get; set; }
        [XmlAttribute("animationType")]
        public string? AnimationType { get; set; }

        // Nums
        [XmlAttribute("heightMultiplier")]
        public string? HeightMultiplier { get; set; }
        [XmlAttribute("parentLayerIndex")]
        public string? ParentLayerIndex { get; set; }
        [XmlAttribute("alphaPercent")]
        public string? AlphaPercent { get; set; }

        // Booleans
        [XmlAttribute("hidden")]
        public string? Hidden { get; set; }
        [XmlAttribute("locked")]
        public string? Locked { get; set; }
        [XmlAttribute("autoNamed")]
        public string? AutoNamed { get; set; } = "false";
        [XmlAttribute("transparent")]
        public string? Transparent { get; set; }
        [XmlAttribute("highlighted")]
        public string? Highlighted { get; set; }
        [XmlAttribute("useOutlineView")]
        public string? UseOutlineView { get; set; }

        // Lists
        [XmlArray("frames", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DOMFrame", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<AnimateFrame> Frames { get; set; } = [];

        [XmlIgnore]
        public const string DefaultColor = "#4F4FFF";

        public bool ShouldSerializeName() => !string.IsNullOrWhiteSpace(Name);
        public AnimateLayer() {}

        public AnimateLayer(string name)
        {
            Name = name;
        }

        public AnimateLayer(string name, List<AnimateFrame> frames)
        {
            Name = name;
            Frames = frames;
        }

        public AnimateLayer(string name, AnimateFrame frame)
        {
            Name = name;
            Frames = [frame];
        }

        public override string ToString()
        {
            return $"Layer {Name}, frame count {GetLayerLength()}, main library item {GetMainLibraryItem() ?? "N/A"}";
        }

        /// <summary>
        /// Makes and returns a shallow copy of the current layer
        /// </summary>
        /// <returns>A shallow copy of the layer</returns>
        public AnimateLayer MakeCopy() => (AnimateLayer)MemberwiseClone();

        /// <summary>
        /// Gets the number of frames in a layer
        /// </summary>
        /// <returns>The number of frames there are</returns>
        public int GetNumOfFrames() => Frames.Count;

        /// <summary>
        /// Gets the length of the layer by checking when the last frame ends
        /// </summary>
        /// <returns>The length of the layer</returns>
        public int GetLayerLength()
        {
            if (Frames.Count == 0)
            {
                return 0;
            }

            var lastFrame = Frames[^1]; 
            int lastIndex = lastFrame.Index;
            int lastIndexDuration = lastFrame.Duration;

            return lastIndex + lastIndexDuration;
        }

        /// <summary>
        /// Checks if a layer has no frames, returns true if it does
        /// </summary>
        /// <returns>True if there are no frames found, otherwise false</returns>
        public bool IsEmpty() => Frames.Count == 0;

        /// <summary>
        /// Gets a list of every frame element that the layer contians
        /// </summary>
        /// <returns>A list of frame elements that exist in the layer</returns>
        public List<FrameElements> GetAllFrameElements() => Frames.SelectMany(f => f.Elements).ToList();

        /// <summary>
        /// Gets if a layer has any library items in it or is empty
        /// </summary>
        /// <returns>True if any library items are found in it, otherwise false</returns>
        public bool HasLibraryItems() => Frames.Any(f => f.GetAllLibraryItems().Count > 0);

        /// <summary>
        /// Gets a list of every unique library item in the layer in no particular order
        /// </summary>
        /// <returns>A string list of every library item</returns>
        public List<string> GetAllLibraryItems()
        {
            HashSet<string> allLibraryItems = [];
            Frames.ForEach(f => f.GetAllLibraryItems().ForEach(l => allLibraryItems.Add(l)));
            return allLibraryItems.ToList();
        }

        /// <summary>
        /// Gets the main library item used by checking the first frame in the 
        /// </summary>
        /// <returns>The first library item found in the layer</returns>
        public string? GetMainLibraryItem()
        {
            return Frames?.FirstOrDefault(f => f.GetMainLibraryItem() is not null)?.GetMainLibraryItem() ?? null;
        }

        /// <summary>
        /// Get all of the action frames in the layer
        /// </summary>
        /// <returns>A string list consisting of every action frame in the layer</returns>
        public List<string> GetActions() => Frames.Where(f => f.Actionscript is not null)
                                                  .SelectMany(f => f.GetActionScripts())
                                                  .ToList();

        /// <summary>
        /// Checks if the layer's frames has any elements
        /// </summary>
        /// <returns>True if there elements are found, otherwise false</returns>
        public bool HasFrameElements() => Frames.Any(f => f.Elements.Count > 0);

        /// <summary>
        /// checks if the layer's frames have any actions
        /// </summary>
        /// <returns>True if there are actiosn found, otherwise false</returns>
        public bool HasActions() => Frames.Any(f => f.Actionscript is not null);

        /// <summary>
        /// Checks if there are any labels found in the frames of the layer
        /// </summary>
        /// <returns>True if any labels are found, otherwise false</returns>
        public bool HasLabels() => Frames.Any(f => !string.IsNullOrEmpty(f.LabelType) || !string.IsNullOrEmpty(f.Name));

        /// <summary>
        /// Checks if a layer has more than one copy of a layer name
        /// </summary>
        /// <returns>True if there exists duplicate label names, otherwise false</returns>
        public bool HasDuplicateLabels()
        {
            var labels = GetLabels();
            var checkedLayers = new HashSet<string>();
            return labels.Any(l => !checkedLayers.Add(l));
        }

        /// <summary>
        /// Gets all the labels in a layer
        /// </summary>
        /// <returns>A list of all the label strings</returns>
        public List<string> GetLabels() => Frames.Where(f => f.Name is not null)
                                                 .Select(f => f.Name!)
                                                 .ToList();

        /// <summary>
        /// Move every frame's index forward by a certain amount by mutating
        /// </summary>
        /// <param name="amount">Amount of frames to move everything by</param>
        public void MoveFrames(int amount)
        {
            Frames.ForEach(f => f.Index += amount);
            Frames.Insert(0, new(0, amount));
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
            foreach (var frame in newLayer.Frames)
            {
                int index = frame.Index;
                if (index > endIndex) continue; // Skip frames that start after the end bound
                int duration = frame.Duration;

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
                frame.Index -= beginIndex;
                if (frame.Index < 0)
                {
                    frame.Duration += frame.Index; // Will decrease since index < 0
                    frame.Index = 0;
                }
                if (frame.Index + frame.Duration > maxDuration)
                {
                    frame.Duration = maxDuration - frame.Index + 1;
                }
            }

            return newLayer;
        }

        /// <summary>
        /// Remove empty frames that trail at the end of a layer
        /// </summary>
        public void RemoveTrailingFrames()
        {
            // Loop backwards through the frames
            for (int frameIndex = Frames.Count - 1;
                frameIndex >= 0;
                frameIndex--)
            {   
                // Get the current frame and remove it if it has no elements
                var currentFrame = Frames[frameIndex];
                if (currentFrame.Elements.Count == 0)
                {
                    Frames.RemoveAt(frameIndex);
                }

                // If this frame has elements, then this is the last frame with elements and thus operation should stop
                else
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Checks through all frames in the layer and mutates and removes ones that have a duration of 0 or less
        /// </summary>
        public void RemoveZeroDurationFrames()
        {
            Frames = Frames.Where(f => f.Duration > 0).ToList();
        }

        /// <summary>
        /// If the first frame in the layer is not at index 0, move it back and move all other frames back the same amount
        /// </summary>
        public void FixFramePositions()
        {
            if (Frames.Count == 0) return;

            var firstFrame = Frames[0];
            var moveAmount = firstFrame.Index;

            foreach (var frame in Frames)
            {
                frame.Index -= moveAmount;
            }
        }
    }
}