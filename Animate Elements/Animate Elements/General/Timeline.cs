using System.Xml.Serialization;
using System.Xml;
using UniversalMethods;


namespace XflComponents
{
    /// <summary>
    /// Object with all of the layers in a file
    /// </summary>
    [XmlRoot("DOMTimeline", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class SymbolTimeline
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;
        [XmlAttribute("currentFrame")]
        public string? CurrentFrame { get; set; }

        // Lists
        [XmlArray("layers", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DOMLayer", typeof(AnimateLayer), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<AnimateLayer> Layers { get; set; } = [];

        public bool ShouldSerializeName() => !string.IsNullOrWhiteSpace(Name);

        /// <summary>
        /// Replace the current layers with a new list of layers
        /// </summary>
        /// <param name="NewLayers">Layers that will replace the current ones</param>
        public void ReplaceLayers(List<AnimateLayer> newLayers)
        {
            Layers = newLayers;
        }

        /// <summary>
        /// Get the number of layers in the timeline
        /// </summary>
        /// <returns>An int saying the number of layers in the timeline</returns>
        public int GetLayerCount() => Layers.Count;

        /// <summary>
        /// Get a list of every frame used in the timeline
        /// </summary>
        /// <returns>A list of every frame found</returns>
        public List<AnimateFrame> GetAllFrames() => Layers.SelectMany(l => l.Frames).ToList();

        /// <summary>
        /// Get a list of every element used in the timeline
        /// </summary>
        /// <returns>A list of every element found in the timeline</returns>
        public List<FrameElements> GetAllElements() => Layers.SelectMany(l => l.GetAllFrameElements()).ToList();

        /// <summary>
        /// Get a list of every library item used in the timeline
        /// </summary>
        /// <param name="unique">If true, duplicates will be removed</param>
        /// <returns>A list of every library item found in the timeline</returns>
        public List<string> GetAllLibraryItems() => Layers.SelectMany(l => l.GetAllLibraryItems()).ToList();

        /// <summary>
        /// Gets every unique library item in the layer
        /// </summary>
        /// <returns>A string list with every unique library item</returns>
        public List<string> GetAllUniqueLibraryItems()
        {
            HashSet<string> libraryItems = [];
            Layers.ForEach(l => l.GetAllLibraryItems()
                                 .ForEach(li => libraryItems.Add(li)));
            return libraryItems.ToList();
        }

        /// <summary>
        /// Get a list of every unique library item used in the timeline sorted alphabetically
        /// </summary>
        /// <returns>An alphabetically sorted list of every library item found in the timeline</returns>
        public List<string> GetSortedUniqueLibraryItems()
        {
            var allLibraryItems = GetAllUniqueLibraryItems();
            allLibraryItems.Sort();
            return allLibraryItems;
        }

        /// <summary>
        /// Go through all layers in the timeline and remove ones that contain no elements by mutating
        /// </summary>
        public void RemoveEmptyLayers()
        {
            Layers = RemoveEmptyLayers(Layers);
        }

        /// <summary>
        /// Go through all layers in the timeline and remove ones that contain no elements
        /// </summary>
        /// <returns>List of layers without the empty layers
        public static List<AnimateLayer> RemoveEmptyLayers(List<AnimateLayer> layers)
        {
            // Get list of layers that have library items
            var newLayerList = layers.Where(l => l.GetAllLibraryItems().Count != 0).ToList();

            // If the layer list is empty, add an empty layer for safe measures
            if (newLayerList.Count == 0)
            {
                AnimateFrame emptyFrame = new(0);
                AnimateLayer emptyLayer = new()
                {
                    Name = string.Empty,
                    Color = AnimateLayer.DefaultColor,
                    Frames = [emptyFrame],
                };
                newLayerList.Add(emptyLayer);
            }

            return newLayerList;
        }

        /// <summary>
        /// Attempt to find the first layer with a specified name and return it
        /// </summary>
        /// <param name="nameToFind">Layer name to try to find</param>
        /// <returns>An AnimateLayer object with the wanted name if found, otherwise null</returns>
        public AnimateLayer? GetLayerByName(string nameToFind) => Layers.Find(l => l.Name == nameToFind);

        public static List<AnimateLayer> CutLayers(List<AnimateLayer> layers, int beginIndex, int endIndex = -1)
        {
            var newLayers = new List<AnimateLayer>();
            foreach (var layer in layers)
            {
                var toAddLayer = layer.CutLayer(beginIndex, endIndex);
                newLayers.Add(toAddLayer);
            }
            newLayers = RemoveEmptyLayers(newLayers);

            return newLayers;
        }

        /// <summary>
        /// Mutates and cuts out a portion of all of the frames in the timeline and replaces the layers with it
        /// </summary>
        /// <param name="beginIndex"></param>
        /// <param name="endIndex"></param>
        public void CutLayers(int beginIndex, int endIndex = -1)
        {
            Layers = CutLayers(Layers, beginIndex, endIndex);
        }

        /// <summary>
        /// Get a list of all layer names in the timeline
        /// </summary>
        /// <returns>A string list of all layer names in the timeline</returns>
        public List<string> GetLayerNames() => Layers.Select(l => l.Name).ToList();

        /// <summary>
        /// Get the total length of the timeline, determined by the longest layer in the timeline
        /// </summary>
        /// <returns>An int representing the length of the timeline</returns>
        public int GetTotalLength() => Layers.Count == 0
                                       ? 0
                                       : Layers.Max(l => l.GetLayerLength());

        /// <summary>
        /// Move every frame in the timeline by a certain amount
        /// </summary>
        /// <param name="amount">Amount to move every frame by</param>
        public void MoveFrames(int amount)
        {
            if (amount == 0) return;
            Layers.ForEach(l => l.MoveFrames(amount));
        }

        /// <summary>
        /// Remove any empty frames at the end of every layer found in the timeline, remove empty layers
        /// </summary>
        public void RemoveTrailingFrames()
        {
            for (int layerIndex = Layers.Count - 1; layerIndex >= 0; layerIndex--)
            {
                var currentLayer = Layers[layerIndex];
                currentLayer.RemoveTrailingFrames();
                if (currentLayer.IsEmpty())
                {
                    Layers.RemoveAt(layerIndex);
                }
            }
        }

        /// <summary>
        /// Get all of the action scripts found in the frames of the timeline
        /// </summary>
        /// <returns>A string list of all the found action frames</returns>
        public List<string> GetActionScripts(bool splitLines = false)
        {
            List<string> actionFrames = [];
            var frames = GetAllFrames();
            foreach (var frame in frames)
            {
                actionFrames.AddRange(frame.GetActionScripts(splitLines:splitLines));
            }
            return actionFrames;
        }

        /// <summary>
        /// Gets a string represention of the timeline, with timeline name and number of layers
        /// </summary>
        /// <returns>A string with basic details of timeline</returns>
        public override string ToString()
        {
            return $"Timeline named {Name} and with {Layers.Count} layers";
        }
    }
}