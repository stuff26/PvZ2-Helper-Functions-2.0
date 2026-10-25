using System.Xml.Serialization;
using System.Xml;
using UniversalMethods;

namespace XflComponents
{
    /// <summary>
    /// Parent class for different element types used in a frame
    /// </summary>
    [XmlInclude(typeof(SymbolInstance))]
    [XmlInclude(typeof(BitmapInstance))]
    public abstract class FrameElements
    {
        // NOTE:
        // Any newly added properties must be added to CopyProperties()

        // Strings
        [XmlAttribute("libraryItemName")]
        public string LibraryItemName { get; set; } = string.Empty;
        [XmlAttribute("firstFrame")]
        public string? FirstFrame { get; set; }
        [XmlAttribute("name")]
        public string? Name { get; set; }
        [XmlAttribute("selected")]
        public string? Selected { get; set; }
        [XmlAttribute("accName")]
        public string? AccName { get; set; }
        [XmlAttribute("description")]
        public string? Description { get; set; }
        [XmlAttribute("shortcut")]
        public string? Shortcut { get; set; }
        [XmlAttribute("tabIndex")]
        public string? TabIndex { get; set; }
        [XmlAttribute("silent")]
        public string? Silent { get; set; }
        [XmlAttribute("forceSimple")]
        public string? ForceSimple { get; set; }
        [XmlAttribute("hasAccessibleData")]
        public string? HasAccessibleData { get; set; }
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
        [XmlAttribute("symbolType")]
        public string? SymbolType { get; set; }
        [XmlAttribute("matrix3D")]
        public string? Matrix3D { get; set; }
        [XmlAttribute("centerPoint3DX")]
        public string? CenterPoint3DX { get; set; }
        [XmlAttribute("centerPoint3DY")]
        public string? CenterPoint3DY { get; set; }
        [XmlAttribute("loop")]
        public string? Loop { get; set; }

        // Lists
        [XmlArray("matrix", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("Matrix", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<ElementMatrix> MatrixList { get; set; } = [];

        [XmlIgnore]
        public ElementMatrix Matrix
        {
            get
            {
                if (MatrixList.Count == 0)
                {
                    MatrixList.Add(new());
                }
                return MatrixList[0];
            }
            set
            {
                if (MatrixList.Count == 0)
                    MatrixList.Add(value);
                else
                    MatrixList[0] = value;
            }
        }


        [XmlArray("transformationPoint", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("Point", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<XYPosition?> TransformationPointList { get; set; } = [];

        [XmlIgnore]
        public XYPosition? TransformationPoint
        {
            get
            {
                if (TransformationPointList.Count == 0)
                {
                    return null;
                }
                return TransformationPointList[0];
            }
            set
            {
                if (TransformationPointList.Count == 0)
                    TransformationPointList.Add(value);
                
                else
                    TransformationPointList[0] = value;
            }
        }

        [XmlElement("MatteColor", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public MatteColor? MatteColor { get; set; }

        [XmlArray("persistentData", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("PD", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<PersistantData>? PersistentData { get; set; }

        [XmlArray("filters", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("BlurFilter", typeof(BlurFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("GlowFilter", typeof(GlowFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DropShadowFilter", typeof(DropShadowFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("BevelFilter", typeof(BevelFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("GradientGlowFilter", typeof(GradientGlowFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("GradientBevelFilter", typeof(GradientBevelFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("AdjustColorFilter", typeof(AdjustColorFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<Filter> Filters { get; set; } = [];

        [XmlArray("color", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("Color", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<Color> Color { get; set; } = [];

        [XmlIgnore]
        public const string DefaultSymbolType = "graphic";
        [XmlIgnore]
        public const string DefaultLoop = "loop";

        public bool ShouldSerializeMatrixList() => MatrixList is not null && MatrixList.Count > 0;
        public bool ShouldSerializeColor() => Color is not null && Color.Count > 0;
        public bool ShouldSerializeTransformationPointList() => TransformationPointList.Count > 0;
        public bool ShouldSerializeFilters() => Filters is not null && Filters.Count > 0;
        public bool ShouldSerializeLibraryItemName() => !string.IsNullOrWhiteSpace(LibraryItemName);

        public override string ToString()
            => $"Element with library item {LibraryItemName} and symbol type {SymbolType} at {Matrix.XPosition}, {Matrix.YPosition}";

        /// <summary>
        /// Tests if the symbol type is what is wanted
        /// </summary>
        /// <param name="testingSymbolType"> Symbol type to test</param>
        /// <returns>True if the symbol types match, otherwise false</returns>
        public bool IsSymbolType(string testingSymbolType)
            => testingSymbolType == SymbolType;

        /// <summary>
        /// Edit both the X and Y positions of the matrix by a certain amount
        /// </summary>
        /// <param name="changeAmount"></param>
        public void EditPositions(double changeAmount)
        {
            Matrix.EditPositions(changeAmount);
        }

        /// <summary>
        /// Change the X and Y components of the matrix separately by a certain amount
        /// </summary>
        /// <param name="xChangeAmount"></param>
        /// <param name="yChangeAmount"></param>
        public void EditPositions(double xChangeAmount, double yChangeAmount)
        {
            Matrix.EditPositions(xChangeAmount, yChangeAmount);
        }

        public virtual SymbolInstance ToSymbolInstance() => (SymbolInstance)this;

        public virtual BitmapInstance ToBitmapInstance() => (BitmapInstance)this;

        public static void CopyProperties(FrameElements source, FrameElements result)
        {
            result.AccName = source.AccName;
            result.Bits32 = source.Bits32;
            result.BlendMode = source.BlendMode;
            result.CacheAsBitmap = source.CacheAsBitmap;
            result.CenterPoint3DX = source.CenterPoint3DX;
            result.CenterPoint3DY = source.CenterPoint3DY;
            result.Description = source.Description;
            result.ExportAsBitmap = source.ExportAsBitmap;
            result.FirstFrame = source.FirstFrame;
            result.ForceSimple = source.ForceSimple;
            result.HasAccessibleData = source.HasAccessibleData;
            result.IsVisible = source.IsVisible;
            result.LibraryItemName = source.LibraryItemName;
            result.Loop = source.Loop;
            result.Matrix3D = source.Matrix3D;
            result.Name = source.Name;
            result.Selected = source.Selected;
            result.Shortcut = source.Shortcut;
            result.Silent = source.Silent;
            result.SymbolType = source.SymbolType;
            result.TabIndex = source.TabIndex;

            result.Color = UM.MakeDeepCopy(source.Color);
            result.Filters = UM.MakeDeepCopy(source.Filters);
            result.MatrixList = UM.MakeDeepCopy(source.MatrixList);
            result.MatteColor = UM.MakeDeepCopy(source.MatteColor);
            result.PersistentData = UM.MakeDeepCopy(source.PersistentData);
            result.TransformationPointList = UM.MakeDeepCopy(source.TransformationPointList);
        }
    }

    /// <summary>
    /// Type of element that uses a symbol as its library item
    /// </summary>
    [XmlRoot("DOMSymbolInstance", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class SymbolInstance : FrameElements
    {
        public SymbolInstance()
        {
            SymbolType = DefaultSymbolType;
            Loop = DefaultLoop;
        }

        /// <summary>
        /// Convert the symbol instance into a bitmap instance with the same details
        /// </summary>
        /// <returns>A bitmap instance with the same details as this</returns>
        public override BitmapInstance ToBitmapInstance()
        {
            var result = new BitmapInstance();
            CopyProperties(this, result);
            return result;
        }

        /// <summary>
        /// Get back the symbol instance as itself with no changed properties
        /// </summary>
        /// <returns>This object</returns>
        public override SymbolInstance ToSymbolInstance() => this;
    }

    /// <summary>
    /// Type of element that uses a bitmap as its library item
    /// </summary>
    [XmlRoot("DOMBitmapInstance", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class BitmapInstance : FrameElements
    {
        public BitmapInstance()
        {
            SymbolType = DefaultSymbolType;
            Loop = DefaultLoop;
        }

        /// <summary>
        /// Convert the bitmap instance into a symbol instance with the same details
        /// </summary>
        /// <returns>A symbol instance with the same details as this</returns>
        public override SymbolInstance ToSymbolInstance()
        {
            var result = new SymbolInstance();
            CopyProperties(this, result);
            return result;
        }

        /// <summary>
        /// Get back the bitmap instance as itself with no changed properties
        /// </summary>
        /// <returns>This object</returns>
        public override BitmapInstance ToBitmapInstance() => this;
    }

    public sealed class ShapeInstance : FrameElements
    {
        [XmlArray("fills", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("FillStyle", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<ShapeFill>? Fills { get; set; }

        [XmlArray("edges", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("Edge", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<ShapeEdge>? Edges { get; set; }
    }

    public sealed class ShapeFill
    {
        [XmlAttribute("index")]
        public string? Index { get; set; }

        [XmlElement("LinearGradient", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public ShapeLinearGradient? LinearGradient { get; set; }
    }

    public sealed class ShapeLinearGradient
    {
        [XmlArray("matrix", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("Matrix", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<ElementMatrix>? Matrix { get; set; }

        [XmlElement("GradientEntry")]
        public List<GradientEntry>? GradiantEntry { get; set; }
    }

    public sealed class ShapeEdge
    {
        [XmlAttribute("fillStyle1")]
        public string? FillStyle1 {get; set; }
        [XmlAttribute("edges")]
        public string? Edges { get; set; }
    }
}