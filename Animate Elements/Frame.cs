using System.Xml.Serialization;
using System.Xml;

namespace XflComponents
{
    /// <summary>
    /// A keyframe found in a layer, specifices the index of a frame and what it contains
    /// </summary>
    [XmlRoot("DOMFrame", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public class AnimateFrame
    {
        // Nums
        [XmlIgnore]
        public int index { get; set; }
        [XmlAttribute("index")]
        public string? indexString
        {
            get => index.ToString();
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value == "null")
                    index = 0;
                else
                    index = int.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        [XmlIgnore]
        public int duration { 
            get
            {
                if (int.TryParse(durationString, out int result))
                {
                    return result;
                }
                else
                {
                    return 1;
                }
            }
            set
            {
                durationString = value.ToString();
            }
        }
        [XmlAttribute("duration")]
        public string? durationString { get; set; }

        // Strings
        [XmlAttribute]
        public string? name { get; set; }
        [XmlAttribute]
        public string? labelType { get; set; }
        [XmlAttribute]
        public string? tweenMode { get; set; }
        [XmlAttribute]
        public string? tweenType { get; set; }
        [XmlAttribute]
        public string? motionTweenSnap { get; set; }
        [XmlAttribute]
        public string? motionTweenRotate { get; set; }
        [XmlAttribute]
        public string? motionTweenScale { get; set; }
        [XmlAttribute]
        public string? isMotionObject { get; set; }
        [XmlAttribute]
        public string? visibleAnimationKeyframes { get; set; }
        [XmlAttribute]
        public string? keyMode { get; set; }
        [XmlAttribute]
        public string? cacheAsBitmap { get; set; }
        [XmlAttribute]
        public string? blendMode { get; set; }
        [XmlAttribute]
        public string? exportAsBitmap { get; set; }
        [XmlAttribute]
        public string? bits32 { get; set; }
        [XmlAttribute]
        public string? isVisible { get; set; }
        [XmlAttribute]
        public string? propagateRotMap { get; set; }
        [XmlAttribute]
        public string? propagateScaleXMap { get; set; }
        [XmlAttribute]
        public string? propagateScaleYMap { get; set; }
        [XmlAttribute]
        public string? propagateSkewXMap { get; set; }
        [XmlAttribute]
        public string? propagateSkewYMap { get; set; }
        [XmlAttribute]
        public string? easeMethodName { get; set; }
        [XmlAttribute]
        public string? acceleration { get; set; }
        [XmlAttribute]
        public string? hasCustomEase { get; set; }

        [XmlArray("motionObjectXML", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("AnimationCore", typeof(AnimationCore), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<AnimationCore>? AnimationCores { get; set; }

        [XmlArray("tweens", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("Ease", typeof(TweenEase), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("CustomEase", typeof(CustomEase), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<Easing>? Tweens { get; set; }

        [XmlElement(Namespace = "http://ns.adobe.com/xfl/2008/")]
        public Actionscript? Actionscript { get; set; }


        [XmlArray("elements", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DOMSymbolInstance", typeof(SymbolInstance), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DOMBitmapInstance", typeof(BitmapInstance), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<FrameElements> Elements { get; set; } = [];

        [XmlArray("frameColor", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("Color", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<Color?>? Color { get; set; }

        [XmlArray("frameFilters", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("BlurFilter", typeof(BlurFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("GlowFilter", typeof(GlowFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DropShadowFilter", typeof(DropShadowFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("BevelFilter", typeof(BevelFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("GradientGlowFilter", typeof(GradientGlowFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("GradientBevelFilter", typeof(GradientBevelFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("AdjustColorFilter", typeof(AdjustColorFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<Filter?>? Filters { get; set; }



        public bool ShouldSerializeAnimationCores() => AnimationCores != null && AnimationCores.Count > 0;
        public bool ShouldSerializeTweens() => Tweens != null && Tweens.Count > 0;
        public bool ShouldSerializeColor() => Color != null && Color.Count > 0;
        public bool ShouldSerializeFilters() => Filters != null && Filters.Count > 0;
        public override string ToString()
        {
            return $"Frame with index {index} and duration {duration}";
        }

        /// <summary>
        /// Initializes and returns a frame with no elements
        /// </summary>
        /// <param name="index">Intended index for the frame to be at</param>
        /// <param name="duration">Duration to set the frame to, defaults to 1</param>
        /// <returns>A frame with no elements</returns>
        public static AnimateFrame GetEmptyFrame(int index, int duration = 1)
        {
            var toReturn = new AnimateFrame
            {
                keyMode = "9728",
                index = index,
                Elements = [],
                duration = duration
            };

            return toReturn;
        }

        public static AnimateFrame GetSingleKeyframe(int index, int duration, string libraryItem, string? elementType = null)
        {

            if (elementType == "BitmapInstance")
            {
                var newElement = new BitmapInstance()
                {
                    libraryItemName = libraryItem,
                    symbolType = "graphic",
                    loop = "loop"
                };
                var newFrame = new AnimateFrame()
                {
                    index = index,
                    duration = duration,
                    Elements = [newElement]
                };

                return newFrame;
            }
            else
            {
                var newElement = new SymbolInstance()
                {
                    libraryItemName = libraryItem,
                    symbolType = "graphic",
                    loop = "loop"
                };
                var newFrame = new AnimateFrame()
                {
                    index = index,
                    duration = duration,
                    Elements = [newElement]
                };

                return newFrame;
            }
        }

        public static List<AnimateFrame> GetKeyframeSeries(Dictionary<string, (int start, int end)> details)
        {
            var newFrames = new List<AnimateFrame>();
            foreach (var keyframeDetail in details)
            {
                int index = keyframeDetail.Value.start;
                int duration = keyframeDetail.Value.end - index + 1;
                var label = $"label/{keyframeDetail.Key}";

                var toAddFrame = GetSingleKeyframe(index, duration, label);
                newFrames.Add(toAddFrame);
            }

            return newFrames;
        }

        /// <summary>
        /// Initializes a frame with no elements and adds a name as well
        /// </summary>
        /// <param name="index">Intended index for the frame to be at</param>
        /// <param name="name">Label name to add</param>
        /// <param name="wantedDuration">Duration to set the frame to, defaults to 1</param>
        /// <returns>An empty frame with a label</returns>
        public static AnimateFrame GetLabelFrame(int index, string name, int wantedDuration = 1)
        {
            var toReturn = GetEmptyFrame(index, wantedDuration);

            toReturn.name = name;
            toReturn.labelType = "name";

            return toReturn;
        }

        /// <summary>
        /// Checks to see if there are multiple elements in one frame
        /// </summary>
        /// <returns>True if the amount of elements exceeds 1, otherwise false</returns>
        public bool HasMultipleElements()
        {
            return Elements.Count > 1;
        }

        /// <summary>
        /// Checks if there are multiple types of elements in one frame (ex symbol instance and bitmap instance)
        /// </summary>
        /// <returns>True if multiple element types are found, otherwise false</returns>
        public bool HasMultipleElementTypes()
        {
            Type? firstElementType = null;
            bool firstLoop = true;
            foreach (FrameElements element in Elements)
            {
                if (firstLoop)
                {
                    firstLoop = false;
                    firstElementType = element.GetType();
                }

                if (!element.GetType().Equals(firstElementType))
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets a list of every library item used in the elements of a frame
        /// </summary>
        /// <returns>List of strings that contain every library item used, returns empty list if none are found</returns>
        public List<string> GetAllLibraryItems()
        {
            HashSet<string>? allLibraryItems = [];

            foreach (FrameElements loopElement in Elements)
            {
                string? loopLibraryItemName = loopElement.libraryItemName;
                if (loopLibraryItemName is not null)
                {
                    allLibraryItems.Add(loopLibraryItemName);
                }
            }

            return allLibraryItems.ToList();
        }

        /// <summary>
        /// Gets the first library item used in elements, ignoring others
        /// </summary>
        /// <returns>The name of the first library item used, or an empty string if none are found</returns>
        public string GetMainLibraryItem()
        {
            return Elements[0].libraryItemName ?? "";
        }

        /// <summary>
        /// Gets all of the action scripts the frame may have
        /// </summary>
        /// <returns>All of the actions scripts in the frame, return empty list if none are found</returns>
        public List<string> GetActionScripts(bool splitLines = false)
        {
            if (Actionscript is null) return [];
            var actionScripts = Actionscript.GetScripts();
            if (splitLines)
            {
                List<string> splitActionScripts = [];
                foreach (var actionScript in actionScripts)
                {
                    splitActionScripts.AddRange(actionScript.Split("\n"));
                }
                return splitActionScripts;
            }
            
            return actionScripts;
        }

        /// <summary>
        /// Checks if the frame has any tweens
        /// </summary>
        /// <returns>True if there are attributes that are used for tweens, otherwise false</returns>
        public bool HasTweens()
        {
            return tweenType is not null || motionTweenSnap is not null;
        }

        public bool HasTransformations()
        {
            foreach (var element in Elements)
            {
                var matrix = element.Matrix;
                if (matrix is null) continue;
                List<double?> values = [matrix.a, matrix.b, matrix.c, matrix.d];
                foreach (var value in values)
                {
                    if (value != null && value != 0.0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}