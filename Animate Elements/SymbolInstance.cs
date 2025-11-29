using System.Xml.Serialization;
using System.Xml;
using UniversalMethods;
using System.Text.Json;

namespace XflComponents
{
    /// <summary>
    /// Parent class for different element types used in a frame
    /// </summary>
    [XmlInclude(typeof(SymbolInstance))]
    [XmlInclude(typeof(BitmapInstance))]
    public class FrameElements
    {
        // Strings
        [XmlAttribute]
        public string libraryItemName { get; set; } = "";
        [XmlAttribute]
        public string? firstFrame { get; set; }
        [XmlAttribute]
        public string? name { get; set; }
        [XmlAttribute]
        public string? selected { get; set; }
        [XmlAttribute]
        public string? accName { get; set; }
        [XmlAttribute]
        public string? description { get; set; }
        [XmlAttribute]
        public string? shortcut { get; set; }
        [XmlAttribute]
        public string? tabIndex { get; set; }
        [XmlAttribute]
        public string? silent { get; set; }
        [XmlAttribute]
        public string? forceSimple { get; set; }
        [XmlAttribute]
        public string? hasAccessibleData { get; set; }
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
        public string? symbolType { get; set; }
        [XmlAttribute]
        public string? matrix3D { get; set; }
        [XmlAttribute]
        public string? centerPoint3DX { get; set; }
        [XmlAttribute]
        public string? centerPoint3DY { get; set; }
        [XmlAttribute]
        public string? loop { get; set; }

        // Lists
        [XmlArray("matrix", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("Matrix", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<ElementMatrix?>? MatrixList { get; set; }

        [XmlIgnore]
        public ElementMatrix? Matrix
        {
            get
            {
                if (MatrixList is not null && MatrixList.Count > 0 && MatrixList[0] is not null)
                {
                    return MatrixList[0];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (MatrixList is null)
                    MatrixList = [];

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
                if (TransformationPointList is not null && TransformationPointList.Count > 0
                && TransformationPointList[0] is not null)
                {
                    return TransformationPointList[0];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (TransformationPointList is null)
                {
                    TransformationPointList = [value];
                }
                else if (TransformationPointList.Count == 0)
                {
                    TransformationPointList.Add(value);
                }
                else
                {
                    TransformationPointList[0] = value;
                }
            }
        }

        [XmlElement("MatteColor", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public MatteColor? MatteColor { get; set; }

        [XmlArray("filters", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("BlurFilter", typeof(BlurFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("GlowFilter", typeof(GlowFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("DropShadowFilter", typeof(DropShadowFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("BevelFilter", typeof(BevelFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("GradientGlowFilter", typeof(GradientGlowFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("GradientBevelFilter", typeof(GradientBevelFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("AdjustColorFilter", typeof(AdjustColorFilter), Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<Filter?>? Filters { get; set; }

        [XmlArray("color", Namespace = "http://ns.adobe.com/xfl/2008/")]
        [XmlArrayItem("Color", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<Color?>? Color { get; set; }

        public bool ShouldSerializeMatrixList() => MatrixList != null && MatrixList.Count > 0;
        public bool ShouldSerializeColor() => Color != null && Color.Count > 0;
        public bool ShouldSerializeTransformationPoint() => TransformationPoint != null;
        public bool ShouldSerializeFilters() => Filters != null && Filters.Count > 0;
        public bool ShouldSerializeMatteColor() => MatteColor != null;

        public override string ToString()
        {
            return $"Element with library item {libraryItemName} and symbol type {symbolType}";
        }

        /// <summary>
        /// Tests if the symbol type is what is wanted
        /// </summary>
        /// <param name="testingSymbolType"> Symbol type to test</param>
        /// <returns>True if the symbol types match, otherwise false</returns>
        public bool IsSymbolType(string testingSymbolType)
        {
            return testingSymbolType == symbolType;
        }

        /// <summary>
        /// Edit both the X and Y positions of the matrix by a certain amount
        /// </summary>
        /// <param name="changeAmount"></param>
        public void EditPositions(double changeAmount)
        {
            Matrix?.EditPositions(changeAmount);
        }

        /// <summary>
        /// Change the X and Y components of the matrix separately by a certain amount
        /// </summary>
        /// <param name="xChangeAmount"></param>
        /// <param name="yChangeAmount"></param>
        public void EditPositions(double xChangeAmount, double yChangeAmount)
        {
            Matrix?.EditPositions(xChangeAmount, yChangeAmount);
        }

        public virtual Type GetSymbolType()
        {
            return typeof(FrameElements);
        }

        public virtual SymbolInstance ToSymbolInstance()
        {
            return (SymbolInstance)this;
        }
    }

    /// <summary>
    /// Type of element that uses a symbol as its library item
    /// </summary>
    [XmlRoot("DOMSymbolInstance", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public class SymbolInstance : FrameElements
    {
        /// <summary>
        /// Convert the symbol instance into a bitmap instance with the same details
        /// </summary>
        /// <returns>A bitmap instance with the same details as this</returns>
        public BitmapInstance ToBitmapInstance()
        {
            BitmapInstance toReturn = new BitmapInstance();
            toReturn.symbolType = symbolType;
            toReturn.loop = loop;
            toReturn.libraryItemName = libraryItemName;

            return toReturn;
        }

        /// <summary>
        /// Get the type of instance this symbol instance is
        /// </summary>
        /// <returns>A reference type object with what this is</returns>
        public override Type GetSymbolType()
        {
            return typeof(SymbolInstance);
        }

        public override SymbolInstance ToSymbolInstance()
        {
            return base.ToSymbolInstance();
        }
    }

    /// <summary>
    /// Type of element that uses a bitmap as its library item
    /// </summary>
    [XmlRoot("DOMBitmapInstance", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public class BitmapInstance : FrameElements
    {
        /// <summary>
        /// Convert the bitmap instance into a symbol instance with the same details
        /// </summary>
        /// <returns>A symbol instance with the same details as this</returns>
        public SymbolInstance ConvertToSymbolInstance()
        {
            var toReturn = JsonSerializer.Deserialize<SymbolInstance>(JsonSerializer.Serialize(this))!;
            if (toReturn.Matrix is null) toReturn.MatrixList = null;
            if (toReturn.TransformationPoint is null) toReturn.TransformationPointList = [];
            return toReturn;
        }

        /// <summary>
        /// Get the type of instance this symbol instance is
        /// </summary>
        /// <returns>A reference type object with what this is</returns>
        public override Type GetSymbolType()
        {
            return typeof(BitmapInstance);
        }

        public override SymbolInstance ToSymbolInstance()
        {
            return ConvertToSymbolInstance();
        }
    }
}