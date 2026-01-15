using System.Xml.Serialization;
using System.Xml;


namespace XflComponents
{
    /// <summary>
    /// Object with all of the layers in a file
    /// </summary>
    [XmlRoot("DOMTimeline", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public class SymbolTimeline
    {
        [XmlAttribute]
        public string? name { get; set; }
        [XmlAttribute]
        public string? currentFrame { get; set; }

        // Lists
        [XmlArray("layers", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DOMLayer", typeof(AnimateLayer), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<AnimateLayer> Layers { get; set; } = [];

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
        public int GetLayerCount()
        {
            return Layers.Count;
        }

        /// <summary>
        /// Get a list of every frame used in the timeline
        /// </summary>
        /// <returns>A list of every frame found</returns>
        public List<AnimateFrame> GetAllFrames()
        {
            List<AnimateFrame>? allFrames = [];
            foreach (AnimateLayer? layer in Layers)
            {
                List<AnimateFrame>? frames = layer?.Frames;
                if (layer is null || frames is null) continue;
                allFrames.AddRange(frames);
            }

            return allFrames;
        }

        /// <summary>
        /// Get a list of every element used in the timeline
        /// </summary>
        /// <returns>A list of every element found in the timeline</returns>
        public List<FrameElements> GetAllElements()
        {
            List<FrameElements> allElements = [];

            foreach (AnimateLayer? layer in Layers)
            {
                if (layer is null) continue;
                List<FrameElements> LayerElements = layer.GetAllFrameElements();
                allElements.AddRange(LayerElements);
            }

            return allElements;
        }

        /// <summary>
        /// Get a list of every library item used in the timeline
        /// </summary>
        /// <param name="unique">If true, duplicates will be removed</param>
        /// <returns>A list of every library item found in the timeline</returns>
        public List<string?> GetAllLibraryItems(bool unique = true)
        {
            List<string?> allLibraryItems = [];
            HashSet<string?> uniqueLibraryItems = [];

            foreach (AnimateLayer? layer in Layers)
            {
                List<string>? libraryItems = layer?.GetAllLibraryItems(unique);
                if (libraryItems is null) continue;

                foreach (string? libraryItemName in libraryItems)
                {
                    if (unique)
                        uniqueLibraryItems.Add(libraryItemName);
                    else
                        allLibraryItems.Add(libraryItemName);
                }
            }
            if (unique) return uniqueLibraryItems.ToList();
            else return allLibraryItems;
        }

        /// <summary>
        /// Get a list of every library item used in the timeline sorted alphabetically
        /// </summary>
        /// <param name="unique">If true, duplicates will be removed</param>
        /// <returns>An alphabetically sorted list of every library item found in the timeline</returns>
        public List<string?> GetAllLibraryItemsSorted(bool unique = true)
        {
            List<string?> allLibraryItems = GetAllLibraryItems(unique);
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
        public static List<AnimateLayer> RemoveEmptyLayers(List<AnimateLayer> Layers)
        {
            // Initialize new list of layers
            var newLayerList = new List<AnimateLayer>();

            // Loop through layer list, add ones that contain elements
            foreach (AnimateLayer layer in Layers)
            {
                if (layer.Frames is null || layer.Frames.Count == 0) continue;
                var libraryItems = layer.GetAllLibraryItems();
                if (libraryItems.Count != 0)
                {
                    newLayerList.Add(layer);
                }
            }

            // If the layer list is empty, add an empty layer for safe measures
            if (newLayerList.Count == 0)
            {
                AnimateFrame emptyFrame = AnimateFrame.GetEmptyFrame(0);
                AnimateLayer emptyLayer = new()
                {
                    name = "",
                    color = "#4F4FFF",
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
        public AnimateLayer? GetLayerByName(string nameToFind)
        {
            foreach (var layer in Layers)
            {
                if (layer.name == nameToFind)
                {
                    return layer;
                }
            }

            return null;
        }
        /// <summary>
        /// Mutates and cuts out a portion of all of the frames in the timeline and replaces the layers with it
        /// </summary>
        /// <param name="beginIndex"></param>
        /// <param name="endIndex"></param>
        public void CutLayers(int beginIndex, int endIndex = -1)
        {
            var newLayers = new List<AnimateLayer>();
            foreach (var layer in Layers!)
            {
                var toAddLayer = layer.CutLayer(beginIndex, endIndex);
                newLayers.Add(toAddLayer);
            }
            newLayers = RemoveEmptyLayers(newLayers);

            Layers = newLayers;
        }

        public List<string> GetLayerNames()
        {
            HashSet<string> layerNames = [];
            foreach (var layer in Layers)
            {
                var name = layer.name;
                if (!string.IsNullOrEmpty(name))
                    layerNames.Add(name);
            }

            return layerNames.ToList();
        }

        /// <summary>
        /// Get the total length of the timeline, determined by the longest layer in the timeline
        /// </summary>
        /// <returns>An int representing the length of the timeline</returns>
        public int GetTotalLength()
        {
            var layers = Layers;

            int maxTime = 0;
            foreach (var layer in layers)
            {
                int layerLength = layer.GetLayerLength();
                if (maxTime < layerLength)
                {
                    maxTime = layerLength;
                }
            }

            return maxTime;
        }

        /// <summary>
        /// Move every frame in the timeline by a certain amount
        /// </summary>
        /// <param name="amount">Amount to move every frame by</param>
        public void MoveFrames(int amount)
        {
            foreach (var layer in Layers!)
            {
                layer.MoveFrames(amount);
            }
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
                if (!currentLayer.HasFrames())
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

        public override string ToString()
        {
            return $"Timeline named {name} and with {Layers?.Count} layers";
        }
    }
}