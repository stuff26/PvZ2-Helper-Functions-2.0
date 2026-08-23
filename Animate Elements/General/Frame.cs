using System.Xml.Serialization;
using System.Xml;
using System.Numerics;

namespace XflComponents
{
    /// <summary>
    /// A keyframe found in a layer, specifices the index of a frame and what it contains
    /// </summary>
    [XmlRoot("DOMFrame", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class AnimateFrame
    {
        // Nums
        [XmlIgnore]
        public int Index { get; set; }
        [XmlAttribute("index")]
        public string? IndexString
        {
            get => Index.ToString();
            set
            {
                if (string.IsNullOrWhiteSpace(value)
                || value == "null" ||
                !int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture,
                out int result))
                    Index = 0;
                else
                    Index = result;
            }
        }
        [XmlIgnore]
        public int Duration {
            get
            {
                if (string.IsNullOrWhiteSpace(DurationString) || 
                !int.TryParse(DurationString, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out int result))
                {
                    return 1;
                }
                return result;
            }
            set
            {
                DurationString = value.ToString();
            }
        }

        [XmlAttribute("duration")]
        public string? DurationString {get; set;}

        // Strings
        [XmlAttribute("name")]
        public string? Name { get; set; }
        [XmlAttribute("labelType")]
        public string? LabelType { get; set; }
        [XmlAttribute("tweenMode")]
        public string? TweenMode { get; set; }
        [XmlAttribute("tweenType")]
        public string? TweenType { get; set; }
        [XmlAttribute("motionTweenSnap")]
        public string? MotionTweenSnap { get; set; }
        [XmlAttribute("motionTweenRotate")]
        public string? MotionTweenRotate { get; set; }
        [XmlAttribute("motionTweenScale")]
        public string? MotionTweenScale { get; set; }
        [XmlAttribute("isMotionObject")]
        public string? IsMotionObject { get; set; }
        [XmlAttribute("visibleAnimationKeyframes")]
        public string? VisibleAnimationKeyframes { get; set; }
        [XmlAttribute("keyMode")]
        public string? KeyMode { get; set; } = DefaultKeymode;
        [XmlAttribute("cacheAsBitmap")]
        public string? CacheAsBitmap { get; set; }
        [XmlAttribute("blendMode")]
        public string? BlendMode { get; set; }
        [XmlAttribute("exportAsBitmap")]
        public string? ExportAsBitmap { get; set; }
        [XmlAttribute("bits32")]
        public string? Bits32 { get; set; }
        [XmlAttribute("isVisible")]
        public string? IsVisible { get; set; }
        [XmlAttribute("propagateRotMap")]
        public string? PropagateRotMap { get; set; }
        [XmlAttribute("propagateScaleXMap")]
        public string? PropagateScaleXMap { get; set; }
        [XmlAttribute("propagateScaleYMap")]
        public string? PropagateScaleYMap { get; set; }
        [XmlAttribute("propagateSkewXMap")]
        public string? PropagateSkewXMap { get; set; }
        [XmlAttribute("propagateSkewYMap")]
        public string? PropagateSkewYMap { get; set; }
        [XmlAttribute("easeMethodName")]
        public string? EaseMethodName { get; set; }
        [XmlAttribute("rigPropagationMatrix")]
        public string? RigPropagationMatrix { get; set; }
        [XmlAttribute("acceleration")]
        public string? Acceleration { get; set; }
        [XmlAttribute("hasCustomEase")]
        public string? HasCustomEase { get; set; }

        [XmlArray("motionObjectXML", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("AnimationCore", typeof(AnimationCore), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<AnimationCore> AnimationCores { get; set; } = [];

        [XmlArray("tweens", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("Ease", typeof(TweenEase), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("CustomEase", typeof(CustomEase), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<Easing> Tweens { get; set; } = [];

        [XmlElement(Namespace = "http://ns.adobe.com/xfl/2008/")]
        public Actionscript? Actionscript { get; set; }


        [XmlArray("elements", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DOMSymbolInstance", typeof(SymbolInstance), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DOMBitmapInstance", typeof(BitmapInstance), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DOMShape", typeof(ShapeInstance), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<FrameElements> Elements { get; set; } = [];

        [XmlArray("frameColor", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("Color", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<Color> Color { get; set; } = [];

        [XmlArray("frameFilters", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("BlurFilter", typeof(BlurFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("GlowFilter", typeof(GlowFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DropShadowFilter", typeof(DropShadowFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("BevelFilter", typeof(BevelFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("GradientGlowFilter", typeof(GradientGlowFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("GradientBevelFilter", typeof(GradientBevelFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("AdjustColorFilter", typeof(AdjustColorFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<Filter> Filters { get; set; } = [];

        [XmlIgnore]
        public const string DefaultKeymode = "9728";
        [XmlIgnore]
        public const string StopAction = "stop();";

        public bool ShouldSerializeAnimationCores() => AnimationCores is not null && AnimationCores.Count > 0;
        public bool ShouldSerializeTweens() => Tweens.Count > 0;
        public bool ShouldSerializeColor() => Color.Count > 0;
        public bool ShouldSerializeFilters() => Filters.Count > 0;

        public override string ToString()
        {
            return $"Frame with index {Index} and duration {Duration}";
        }

        public AnimateFrame() {}

        public AnimateFrame(int index)
        {
            Index = index;
        }

        public AnimateFrame(int index, int duration)
        {
            Index = index;
            Duration = duration;
        }

        public static AnimateFrame GetSingleKeyframe(int index, int duration, string libraryItem, string? elementType = null)
        {
            FrameElements element = elementType == "BitmapInstance"
            ? new BitmapInstance()
                {
                    LibraryItemName = libraryItem,
                }
            : new SymbolInstance()
                {
                    LibraryItemName = libraryItem,
                };

            return new AnimateFrame()
                {
                    Index = index,
                    Duration = duration,
                    Elements = [element]
                };
        }

        public static List<AnimateFrame> GetKeyframeSeries(Dictionary<string, (int start, int end)> details)
        {
            var newFrames = new List<AnimateFrame>();
            foreach (var keyframeDetail in details)
            {
                int index = keyframeDetail.Value.start;
                int duration = keyframeDetail.Value.end - index + 1;
                var label = $"{XFL.LabelFolder}/{keyframeDetail.Key}";

                var toAddFrame = GetSingleKeyframe(index, duration, label);
                newFrames.Add(toAddFrame);
            }

            return newFrames;
        }

        public static AnimateFrame GetSingleStopActionKeyframe(int index, int duration = 1)
        {
            var frame = new AnimateFrame(index, duration);
            
            var cdataScripts = new CDataScript()
            {
                Text = StopAction
            };
            var actionScripts = new Actionscript()
            {
                Scripts = [cdataScripts]
            };

            frame.Actionscript = actionScripts;
            return frame;
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
            var toReturn = new AnimateFrame(index, wantedDuration)
            {
                Name = name,
                LabelType = "name"
            };

            return toReturn;
        }

        /// <summary>
        /// Checks to see if there are multiple elements in one frame
        /// </summary>
        /// <returns>True if the amount of elements exceeds 1, otherwise false</returns>
        public bool HasMultipleElements() => Elements.Count > 1;

        /// <summary>
        /// Checks if there are multiple types of elements in one frame (ex symbol instance and bitmap instance)
        /// </summary>
        /// <returns>True if multiple element types are found, otherwise false</returns>
        public bool HasMultipleElementTypes()
        {
            if (Elements.Count < 2) return false;
            var firstType = Elements[0].GetType();
            return Elements.Skip(1).Any(e => e.GetType() != firstType);
        }

        /// <summary>
        /// Gets a list of every library item used in the elements of a frame
        /// </summary>
        /// <returns>List of strings that contain every library item used, returns empty list if none are found</returns>
        public List<string> GetAllLibraryItems()
        {
            HashSet<string>? allLibraryItems = [];
            Elements.ForEach(e => allLibraryItems.Add(e.LibraryItemName));
            return allLibraryItems.ToList();
        }

        /// <summary>
        /// Gets the first library item used in elements, ignoring others
        /// </summary>
        /// <returns>The name of the first library item used, or an empty string if none are found</returns>
        public string? GetMainLibraryItem()
        {
            if (Elements.Count > 0)
                return Elements[0].LibraryItemName;
            else
                return null;
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
                actionScripts.ForEach(acs => splitActionScripts.AddRange(acs.Split('\n')
                                                                            .Select(f => f.Replace("\n", ""))));
                return splitActionScripts;
            }
            
            return actionScripts;
        }

        /// <summary>
        /// Checks if the frame has any tweens
        /// </summary>
        /// <returns>True if there are attributes that are used for tweens, otherwise false</returns>
        public bool HasTweens() => TweenType is not null || MotionTweenSnap is not null;

        public bool HasTransformations() => Elements.Select(e => e.Matrix)
                                                    .Any(m => m.A != 1.0 || m.B != 0.0 || m.C != 0.0 || m.D != 1.0);
    }
}