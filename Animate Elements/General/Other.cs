using System.Xml.Serialization;
using System.Xml;

namespace XflComponents
{
    /// <summary>
    /// Contains sprite details such as what library item it used, color filters, and transformations
    /// </summary>
    [XmlRoot("Matrix", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class ElementMatrix
    {
        // Nums
        [XmlAttribute("a")]
        public string? AString { get; set; }
        [XmlIgnore]
        public double A
        {
            get
            {
                if (string.IsNullOrEmpty(AString) || !double.TryParse(AString, out double result))
                {
                    return 1.0;
                }
                return result;
            }
            set
            {
                if (value != 0.0)
                    AString = value.ToString();
                else
                    AString = null;
            }
        }

        [XmlAttribute("b")]
        public string? BString { get; set; }
        [XmlIgnore]
        public double B
        {
            get
            {
                if (string.IsNullOrEmpty(BString) || !double.TryParse(BString, out double result))
                {
                    return 0.0;
                }
                return result;
            }
            set
            {
                if (value != 0.0)
                    BString = value.ToString();
                else
                    BString = null;
            }
        }

        [XmlAttribute("c")]
        public string? CString { get; set; }
        [XmlIgnore]
        public double C
        {
            get
            {
                if (string.IsNullOrEmpty(CString) || !double.TryParse(CString, out double result))
                {
                    return 0.0;
                }
                return result;
            }
            set
            {
                if (value != 0.0)
                    CString = value.ToString();
                else
                    CString = null;
            }
        }

        [XmlAttribute("d")]
        public string? DString { get; set; }
        [XmlIgnore]
        public double D
        {
            get
            {
                if (string.IsNullOrEmpty(DString) || !double.TryParse(DString, out double result))
                {
                    return 1.0;
                }
                return result;
            }
            set
            {
                if (value != 0.0)
                    DString = value.ToString();
                else
                    DString = null;
            }
        }

        [XmlIgnore]
        public double XPosition {
            get
            {
                if (string.IsNullOrEmpty(XPositionString) || !double.TryParse(XPositionString, out double result))
                {
                    return 0.0;
                }
                return result;
            }
            set
            {
                if (value != 0.0)
                    XPositionString = value.ToString();
                else
                    XPositionString = null;
            }
        }
        [XmlAttribute("tx")]
        public string? XPositionString { get; set; }

        [XmlIgnore]
        public double YPosition {
            get
            {
                if (string.IsNullOrEmpty(YPositionString) || !double.TryParse(YPositionString, out double result))
                {
                    return 0.0;
                }
                return result;
            }
            set
            {
                if (value != 0.0)
                    YPositionString = value.ToString();
                else
                    YPositionString = null;
            }
        }
        [XmlAttribute("ty")]
        public string? YPositionString { get; set; }

        public override string ToString()
        {
            return $"Matrix with position {XPosition}, {YPosition}";
        }

        /// <summary>
        /// Add/Subtract both the X and Y coordinates by the same amount
        /// </summary>
        /// <param name="changeAmount">Amount to change the coordinates by</param>
        public void EditPositions(double changeAmount)
        {
            EditPositions(changeAmount, changeAmount);
        }

        /// <summary>
        /// Add/Subtract the X and Y coordinates by differing amounts
        /// </summary>
        /// <param name="XChangeAmount">Amount to change the X coordinate by</param>
        /// <param name="YChangeAmount">Amount to change the Y coordinate by</param>
        public void EditPositions(double XChangeAmount, double YChangeAmount)
        {
            XPosition += XChangeAmount;
            YPosition += YChangeAmount;
        }

        /// <summary>
        /// Gets both the X and Y positions at once
        /// </summary>
        /// <returns>A tuple with the X and Y coordinates</returns>
        public (double XPosition, double YPosition) GetPositions() => (XPosition, YPosition);
    }

    /// <summary>
    /// Color filters on an element, such as transparency and brightness
    /// </summary>
    [XmlRoot("Color", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class Color
    {
        // Nums
        [XmlAttribute("brightness")]
        public string? Brightness { get; set; }
        [XmlAttribute("tintMultiplier")]
        public string? TintMultiplier { get; set; }
        [XmlAttribute("redMultiplier")]
        public string? RedMultiplier { get; set; }
        [XmlAttribute("greenMultiplier")]
        public string? GreenMultiplier { get; set; }
        [XmlAttribute("blueMultiplier")]
        public string? BlueMultiplier { get; set; }
        [XmlAttribute("alphaMultiplier")]
        public string? AlphaMultiplier { get; set; }
        [XmlAttribute("alphaOffset")]
        public string? AlphaOffset { get; set; }
        [XmlAttribute("redOffset")]
        public string? RedOffset { get; set; }
        [XmlAttribute("greenOffset")]
        public string? GreenOffset { get; set; }
        [XmlAttribute("blueOffset")]
        public string? BlueOffset { get; set; }

        // Strings
        [XmlAttribute("tintColor")]
        public string? TintColor { get; set; }
        [XmlIgnore]
        public string? ColorType => GetColorType();

        public override string ToString()
        {
            return $"Color with type {GetColorType()}";
        }

        /// <summary>
        /// Gets an alpha color filter
        /// </summary>
        /// <param name="alphaFilter">Amount to set the alpha filter to</param>
        /// <returns></returns>
        public static Color DefaultAlpha(double alphaFilter = 0.0)
        {
            Color toReturn = new()
            {
                AlphaMultiplier = alphaFilter.ToString()
            };
            return toReturn;
        }

        /// <summary>
        /// Gets an alpha color filter
        /// </summary>
        /// <param name="alphaFilter">Amount to set the alpha filter to</param>
        /// <returns>An alpha color object</returns>
        public static Color DefaultAlpha(string alphaFilter = "0.0")
        {
            Color toReturn = new()
            {
                AlphaMultiplier = alphaFilter
            };
            return toReturn;
        }

        /// <summary>
        /// Gets every color setting used for the advanced color filter
        /// </summary>
        /// <returns>An array with every color setting used for advanced color filter</returns>
        public string?[] GetAdvancedValues()
        {
            return [AlphaMultiplier, RedMultiplier, GreenMultiplier, BlueMultiplier,
                    AlphaOffset, RedOffset, GreenOffset, BlueOffset];
        }

        /// <summary>
        /// Checks all values the color object to check what type it is
        /// </summary>
        /// <returns>A string that says what type of color object this is, return "none" if nothing is found</returns>
        public string GetColorType()
        {
            if (Brightness is not null)
            {
                return "brightness";
            }

            if (TintMultiplier is not null || TintColor is not null)
            {
                return "tint";
            }

            if (AlphaMultiplier is not null)
            {
                string?[] advancedValues = GetAdvancedValues();

                for (int i = 1; i < advancedValues.Length; i++)
                {
                    if (advancedValues[i] is not null)
                    {
                        return "advanced";
                    }
                }

                return "alpha";
            }

            return "none";
        }
    }

    /// <summary>
    /// Specifices X and Y position of something
    /// </summary>
    [XmlRoot("Point", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class XYPosition
    {
        // Nums
        [XmlIgnore]
        public double? XPosition { get; set; }
        [XmlAttribute("x")]
        public string? XPositionString
        {
            get
            {
                if (XPosition is null || XPosition == 0)
                {
                    return null;
                }
                return XPosition.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value == "null")
                    XPosition = 0.0;
                else
                    XPosition = double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        [XmlIgnore]
        public double? YPosition { get; set; }
        [XmlAttribute("y")]
        public string? YPositionString
        {
            get
            {
                if (YPosition is null || YPosition == 0)
                {
                    return null;
                }
                return YPosition.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value == "null")
                    YPosition = 0.0;
                else
                    YPosition = double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public override string ToString()
        {
            return $"XYPosition with coordinates {XPosition}, {YPosition}";
        }
    }

    /// <summary>
    /// Dummy parent class for details of easing
    /// </summary>
    [XmlInclude(typeof(TweenEase))]
    [XmlInclude(typeof(CustomEase))]
    public class Easing { }

    /// <summary>
    /// Classic tween ease details
    /// </summary>
    [XmlRoot("Ease", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class TweenEase : Easing
    {
        [XmlAttribute("target")]
        public string? Target { get; set; }
        [XmlAttribute("method")]
        public string? Method { get; set; }
        [XmlAttribute("intensity")]
        public string? Intensity { get; set; }
    }

    /// <summary>
    /// Custom ease details
    /// </summary>
    [XmlRoot("CustomEase", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class CustomEase : Easing
    {
        [XmlAttribute("target")]
        public string? Target { get; set; }
        [XmlAttribute("name")]
        public string? Name { get; set; }

        [XmlElement("Point", Namespace = "http://ns.adobe.com/xfl/2008/")]
        public List<XYPosition>? Points { get; set; }
    }

    [XmlInclude(typeof(BlurFilter))]
    [XmlInclude(typeof(GlowFilter))]
    [XmlInclude(typeof(DropShadowFilter))]
    [XmlInclude(typeof(BevelFilter))]
    [XmlInclude(typeof(GradientGlowFilter))]
    [XmlInclude(typeof(GradientBevelFilter))]
    [XmlInclude(typeof(AdjustColorFilter))]
    public class Filter { }

    [XmlRoot("BlurFilter", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class BlurFilter : Filter
    {
        [XmlAttribute("blurX")]
        public string? BlurX { get; set; }
        [XmlAttribute("blurY")]
        public string? BlurY { get; set; }
        [XmlAttribute("quality")]
        public string? Quality { get; set; }
    }

    [XmlRoot("GlowFilter", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class GlowFilter : Filter
    {
        [XmlAttribute("blurX")]
        public string? BlurX { get; set; }
        [XmlAttribute("blurY")]
        public string? BlurY { get; set; }
        [XmlAttribute("color")]
        public string? Color { get; set; }
        [XmlAttribute("inner")]
        public string? Inner { get; set; }
        [XmlAttribute("knockout")]
        public string? Knockout { get; set; }
        [XmlAttribute("quality")]
        public string? Quality { get; set; }
        [XmlAttribute("strength")]
        public string? Strength { get; set; }
    }

    [XmlRoot("DropShadowFilter", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class DropShadowFilter : Filter
    {
        [XmlAttribute("angle")]
        public string? Angle { get; set; }
        [XmlAttribute("blurX")]
        public string? BlurX { get; set; }
        [XmlAttribute("blurY")]
        public string? BlurY { get; set; }
        [XmlAttribute("color")]
        public string? Color { get; set; }
        [XmlAttribute("distance")]
        public string? Distance { get; set; }
        [XmlAttribute("hideObject")]
        public string? HideObject { get; set; }
        [XmlAttribute("inner")]
        public string? Inner { get; set; }
        [XmlAttribute("knockout")]
        public string? Knockout { get; set; }
        [XmlAttribute("quality")]
        public string? Quality { get; set; }
        [XmlAttribute("strength")]
        public string? Strength { get; set; }
    }

    [XmlRoot("BevelFilter", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class BevelFilter : Filter
    {
        [XmlAttribute("blurX")]
        public string? BlurX { get; set; }
        [XmlAttribute("blurY")]
        public string? BlurY { get; set; }
        [XmlAttribute("quality")]
        public string? Quality { get; set; }
        [XmlAttribute("angle")]
        public string? Angle { get; set; }
        [XmlAttribute("distance")]
        public string? Distance { get; set; }
        [XmlAttribute("highlightColor")]
        public string? HighlightColor { get; set; }
        [XmlAttribute("knockout")]
        public string? Knockout { get; set; }
        [XmlAttribute("shadowColor")]
        public string? ShadowColor { get; set; }
        [XmlAttribute("strength")]
        public string? Strength { get; set; }
        [XmlAttribute("type")]
        public string? Type { get; set; }
    }

    [XmlRoot("GradientGlowFilter", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public sealed class GradientGlowFilter : Filter
    {
        [XmlAttribute("angle")]
        public string? Angle { get; set; }
        [XmlAttribute("blurX")]
        public string? BlurX { get; set; }
        [XmlAttribute("blurY")]
        public string? BlurY { get; set; }
        [XmlAttribute("quality")]
        public string? Quality { get; set; }
        [XmlAttribute("distance")]
        public string? Distance { get; set; }
        [XmlAttribute("knockout")]
        public string? Knockout { get; set; }
        [XmlAttribute("strength")]
        public string? Strength { get; set; }
        [XmlAttribute("type")]
        public string? Type { get; set; }
        [XmlElement]
        public List<GradientEntry> GradientEntry { get; set; } = [];

        public bool ShouldSerializeGradiantEntries() => GradientEntry.Count > 0;
    }

    public sealed class GradientEntry
    {
        [XmlAttribute("color")]
        public string? Color { get; set; }
        [XmlAttribute("alpha")]
        public string? Alpha { get; set; }
        [XmlAttribute("ratio")]
        public string? Ratio { get; set; }
    }

    public sealed class GradientBevelFilter : Filter
    {
        [XmlAttribute("angle")]
        public string? Angle { get; set; }
        [XmlAttribute("blurX")]
        public string? BlurX { get; set; }
        [XmlAttribute("blurY")]
        public string? BlurY { get; set; }
        [XmlAttribute("quality")]
        public string? Quality { get; set; }
        [XmlAttribute("distance")]
        public string? Distance { get; set; }
        [XmlAttribute("knockout")]
        public string? Knockout { get; set; }
        [XmlAttribute("strength")]
        public string? Strength { get; set; }
        [XmlAttribute("type")]
        public string? Type { get; set; }
        [XmlElement]
        public List<GradientEntry> GradientEntry { get; set; } = [];

        public bool ShouldSerializeGradiantEntry() => GradientEntry is not null && GradientEntry.Count > 0;
    }

    public sealed class AdjustColorFilter : Filter
    {
        [XmlAttribute("brightness")]
        public string? Brightness { get; set; }
        [XmlAttribute("contrast")]
        public string? Contrast { get; set; }
        [XmlAttribute("saturation")]
        public string? Saturation { get; set; }
        [XmlAttribute("hue")]
        public string? Hue { get; set; }
    }

    public sealed class MatteColor
    {
        [XmlAttribute("color")]
        public string? Color { get; set; }
    }

    public sealed class PersistantData
    {
        [XmlAttribute("n")]
        public string? N { get; set; }
        [XmlAttribute("v")]
        public string? V { get; set; }
    }
}