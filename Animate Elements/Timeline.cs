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
        public List<AnimateLayer>? Layers { get; set; } = [];

        /// <summary>
        /// Replace the current layers with a new list of layers
        /// </summary>
        /// <param name="NewLayers">Layers that will replace the current ones</param>
        public void ReplaceLayers(List<AnimateLayer>? NewLayers)
        {
            Layers = NewLayers;
        }

        /// <summary>
        /// Get the number of layers in the timeline
        /// </summary>
        /// <returns>An int saying the number of layers in the timeline</returns>
        public int GetLayerCount()
        {
            if (Layers is null) return 0;
            return Layers.Count;
        }

        /// <summary>
        /// Get a list of every frame used in the timeline
        /// </summary>
        /// <returns>A list of every frame found</returns>
        public List<AnimateFrame> GetAllFrames()
        {
            List<AnimateFrame>? AllFrames = [];
            if (Layers is null) return AllFrames;
            foreach (AnimateLayer? layer in Layers)
            {
                List<AnimateFrame>? Frames = layer?.Frames;
                if (layer is null || Frames is null) continue;
                AllFrames.AddRange(Frames);
            }

            return AllFrames;
        }

        /// <summary>
        /// Get a list of every element used in the timeline
        /// </summary>
        /// <returns>A list of every element found in the timeline</returns>
        public List<FrameElements> GetAllElements()
        {
            if (Layers is null) return [];
            List<FrameElements> AllElements = [];

            foreach (AnimateLayer? layer in Layers)
            {
                if (layer is null) continue;
                List<FrameElements> LayerElements = layer.GetAllFrameElements();
                AllElements.AddRange(LayerElements);
            }

            return AllElements;
        }

        /// <summary>
        /// Get a list of every library item used in the timeline
        /// </summary>
        /// <param name="unique">If true, duplicates will be removed</param>
        /// <returns>A list of every library item found in the timeline</returns>
        public List<string?> GetAllLibraryItems(bool unique = true)
        {
            List<string?>? AllLibraryItems = [];
            if (Layers is null) return AllLibraryItems;

            foreach (AnimateLayer? layer in Layers)
            {
                List<string>? LibraryItems = layer?.GetAllLibraryItems(unique);
                if (LibraryItems is null) continue;

                foreach (string? libraryItemName in LibraryItems)
                {
                    AllLibraryItems.Add(libraryItemName);
                }
            }
            if (unique)
            {
                AllLibraryItems = AllLibraryItems.Distinct().ToList();
            }
            return AllLibraryItems;
        }

        /// <summary>
        /// Get a list of every library item used in the timeline sorted alphabetically
        /// </summary>
        /// <param name="unique">If true, duplicates will be removed</param>
        /// <returns>An alphabetically sorted list of every library item found in the timeline</returns>
        public List<string?> GetAllLibraryItemsSorted(bool unique = true)
        {
            List<string?> AllLibraryItems = GetAllLibraryItems(unique);
            AllLibraryItems.Sort();
            return AllLibraryItems;
        }

        /// <summary>
        /// Go through all layers in the timeline and remove ones that contain no elements
        /// </summary>
        public void RemoveEmptyLayers()
        {
            if (Layers is null) return;

            // Initialize new list of layers
            var NewLayerList = new List<AnimateLayer>();

            // Loop through layer list, add ones that contain elements
            foreach (AnimateLayer layer in Layers)
            {
                if (layer.Frames is null || layer.Frames.Count == 0) continue;
                var libraryItems = layer.GetAllLibraryItems();
                if (libraryItems.Count != 0)
                {
                    NewLayerList.Add(layer);
                }
            }

            // If the layer list is empty, add an empty layer for safe measures
            if (NewLayerList.Count == 0)
            {
                AnimateFrame emptyFrame = AnimateFrame.GetEmptyFrame(0);
                AnimateLayer emptyLayer = new()
                {
                    name = "",
                    color = "#4F4FFF",
                    Frames = [emptyFrame],
                };
                NewLayerList.Add(emptyLayer);
            }

            Layers = NewLayerList;
        }

        /// <summary>
        /// Go through all layers in the timeline and remove ones that contain no elements
        /// </summary>
        public static List<AnimateLayer> RemoveEmptyLayers(List<AnimateLayer> Layers)
        {
            // Initialize new list of layers
            var NewLayerList = new List<AnimateLayer>();

            // Loop through layer list, add ones that contain elements
            foreach (AnimateLayer layer in Layers)
            {
                if (layer.Frames is null || layer.Frames.Count == 0) continue;
                var libraryItems = layer.GetAllLibraryItems();
                if (libraryItems.Count != 0)
                {
                    NewLayerList.Add(layer);
                }
            }

            // If the layer list is empty, add an empty layer for safe measures
            if (NewLayerList.Count == 0)
            {
                AnimateFrame emptyFrame = AnimateFrame.GetEmptyFrame(0);
                AnimateLayer emptyLayer = new()
                {
                    name = "",
                    color = "#4F4FFF",
                    Frames = [emptyFrame],
                };
                NewLayerList.Add(emptyLayer);
            }

            return NewLayerList;
        }

        /// <summary>
        /// Attempt to find the first layer with a specified name and return it
        /// </summary>
        /// <param name="nameToFind">Layer name to try to find</param>
        /// <returns>An AnimateLayer object with the wanted name if found, otherwise null</returns>
        public AnimateLayer? GetLayerByName(string nameToFind)
        {
            if (Layers is null) return new AnimateLayer();
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
        /// Cuts out a portion of all of the frames in the timeline and replaces the layers with it
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
            List<string> layerNames = [];
            if (Layers is null) return layerNames;
            foreach (var layer in Layers)
            {
                var name = layer.name;
                if (!string.IsNullOrEmpty(name) && !layerNames.Contains(name))
                    layerNames.Add(name);
            }

            return layerNames;
        }

        /// <summary>
        /// Get the total length of the timeline, determined by the longest layer in the timeline
        /// </summary>
        /// <returns>An int representing the length of the timeline</returns>
        public int GetTotalLength()
        {
            var layers = Layers;
            if (layers is null) return 0;

            int maxTime = -1;
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

        public void RemoveTrailingFrames()
        {
            if (Layers is null) return;
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

        public override string ToString()
        {
            return $"Timeline named {name} and with {Layers?.Count} layers";
        }
    }
}