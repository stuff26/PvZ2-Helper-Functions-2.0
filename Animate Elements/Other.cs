using System.Xml.Serialization;
using System.Xml;

namespace XflComponents
{
    /// <summary>
    /// Contains sprite details such as what library item it used, color filters, and transformations
    /// </summary>
    [XmlRoot("Matrix", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public class ElementMatrix
    {
        // Nums
        [XmlAttribute("a")]
        public string? aString { get; set; }
        [XmlIgnore]
        public double a
        {
            get
            {
                if (string.IsNullOrEmpty(aString) || !double.TryParse(aString, out double result))
                {
                    return 1.0;
                }
                return result;
            }
            set
            {
                if (value != 0.0)
                    aString = value.ToString();
                else
                    aString = null;
            }
        }

        [XmlAttribute("b")]
        public string? bString { get; set; }
        [XmlIgnore]
        public double b
        {
            get
            {
                if (string.IsNullOrEmpty(bString) || !double.TryParse(bString, out double result))
                {
                    return 0.0;
                }
                return result;
            }
            set
            {
                if (value != 0.0)
                    bString = value.ToString();
                else
                    bString = null;
            }
        }

        [XmlAttribute("c")]
        public string? cString { get; set; }
        [XmlIgnore]
        public double c
        {
            get
            {
                if (string.IsNullOrEmpty(cString) || !double.TryParse(cString, out double result))
                {
                    return 0.0;
                }
                return result;
            }
            set
            {
                if (value != 0.0)
                    cString = value.ToString();
                else
                    cString = null;
            }
        }

        [XmlAttribute("d")]
        public string? dString { get; set; }
        [XmlIgnore]
        public double d
        {
            get
            {
                if (string.IsNullOrEmpty(dString) || !double.TryParse(dString, out double result))
                {
                    return 1.0;
                }
                return result;
            }
            set
            {
                if (value != 0.0)
                    dString = value.ToString();
                else
                    dString = null;
            }
        }

        [XmlIgnore]
        public double xPosition {
            get
            {
                if (string.IsNullOrEmpty(xPositionString) || !double.TryParse(xPositionString, out double result))
                {
                    return 0.0;
                }
                return result;
            }
            set
            {
                if (value != 0.0)
                    xPositionString = value.ToString();
                else
                    xPositionString = null;
            }
        }
        [XmlAttribute("tx")]
        public string? xPositionString { get; set; }

        [XmlIgnore]
        public double yPosition {
            get
            {
                if (string.IsNullOrEmpty(yPositionString) || !double.TryParse(yPositionString, out double result))
                {
                    return 0.0;
                }
                return result;
            }
            set
            {
                if (value != 0.0)
                    yPositionString = value.ToString();
                else
                    yPositionString = null;
            }
        }
        [XmlAttribute("ty")]
        public string? yPositionString { get; set; }

        public override string ToString()
        {
            return $"Matrix with position {xPosition}, {yPosition}";
        }

        /// <summary>
        /// Add/Subtract both the X and Y coordinates by the same amount
        /// </summary>
        /// <param name="changeAmount">Amount to change the coordinates by</param>
        public void EditPositions(double changeAmount)
        {
            xPosition += changeAmount;
            yPosition += changeAmount;
        }

        /// <summary>
        /// Add/Subtract the X and Y coordinates by differing amounts
        /// </summary>
        /// <param name="XChangeAmount">Amount to change the X coordinate by</param>
        /// <param name="YChangeAmount">Amount to change the Y coordinate by</param>
        public void EditPositions(double XChangeAmount, double YChangeAmount)
        {
            xPosition += XChangeAmount;
            yPosition += YChangeAmount;
        }

        /// <summary>
        /// Gets both the X and Y positions at once
        /// </summary>
        /// <returns>An array with the X and Y coordinates</returns>
        public double[] GetPositions()
        {
            double[] positions = [xPosition, yPosition];
            return positions;
        }
    }

    /// <summary>
    /// Color filters on an element, such as transparency and brightness
    /// </summary>
    [XmlRoot("Color", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public class Color
    {
        // Nums
        [XmlAttribute]
        public string? brightness { get; set; }
        [XmlAttribute]
        public string? tintMultiplier { get; set; }
        [XmlAttribute]
        public string? redMultiplier { get; set; }
        [XmlAttribute]
        public string? greenMultiplier { get; set; }
        [XmlAttribute]
        public string? blueMultiplier { get; set; }
        [XmlAttribute]
        public string? alphaMultiplier { get; set; }
        [XmlAttribute]
        public string? alphaOffset { get; set; }
        [XmlAttribute]
        public string? redOffset { get; set; }
        [XmlAttribute]
        public string? greenOffset { get; set; }
        [XmlAttribute]
        public string? blueOffset { get; set; }

        // Strings
        [XmlAttribute]
        public string? tintColor { get; set; }

        [XmlIgnore]
        public string? colorType { get => ColorType(); }

        public override string ToString()
        {
            return $"Color with type {ColorType()}";
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
                alphaMultiplier = alphaFilter.ToString()
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
                alphaMultiplier = alphaFilter
            };
            return toReturn;
        }

        /// <summary>
        /// Gets every color setting used for the advanced color filter
        /// </summary>
        /// <returns>An array with every color setting used for advanced color filter</returns>
        public string?[] GetAdvancedValues()
        {
            string?[] advancedValues = [alphaMultiplier, redMultiplier, greenMultiplier, blueMultiplier,
                                    alphaOffset, redOffset, greenOffset, blueOffset];

            return advancedValues;
        }

        /// <summary>
        /// Checks all values the color object to check what type it is
        /// </summary>
        /// <returns>A string that says what type of color object this is, return "none" if nothing is found</returns>
        public string ColorType()
        {
            if (brightness is not null)
            {
                return "brightness";
            }

            if (tintMultiplier is not null || tintColor is not null)
            {
                return "tint";
            }

            if (alphaMultiplier is not null)
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
    public class XYPosition
    {
        // Nums
        [XmlIgnore]
        public double? xPosition { get; set; }
        [XmlAttribute("x")]
        public string? xPositionString
        {
            get
            {
                if (xPosition is null || xPosition == 0)
                {
                    return null;
                }
                return xPosition.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value == "null")
                    xPosition = 0.0;
                else
                    xPosition = double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        [XmlIgnore]
        public double? yPosition { get; set; }
        [XmlAttribute("y")]
        public string? yPositionString
        {
            get
            {
                if (yPosition is null || yPosition == 0)
                {
                    return null;
                }
                return yPosition.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value == "null")
                    yPosition = 0.0;
                else
                    yPosition = double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public override string ToString()
        {
            return $"XYPosition with coordinates {xPosition}, {yPosition}";
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
    public class TweenEase : Easing
    {
        [XmlAttribute]
        public string? target { get; set; }
        [XmlAttribute]
        public string? method { get; set; }
        [XmlAttribute]
        public string? intensity { get; set; }
    }

    /// <summary>
    /// Custom ease details
    /// </summary>
    [XmlRoot("CustomEase", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public class CustomEase : Easing
    {
        [XmlAttribute]
        public string? target { get; set; }
        [XmlAttribute]
        public string? name { get; set; }

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
    public class BlurFilter : Filter
    {
        [XmlAttribute]
        public string? blurX { get; set; }
        [XmlAttribute]
        public string? blurY { get; set; }
        [XmlAttribute]
        public string? quality { get; set; }
    }

    [XmlRoot("GlowFilter", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public class GlowFilter : Filter
    {
        [XmlAttribute]
        public string? blurX { get; set; }
        [XmlAttribute]
        public string? blurY { get; set; }
        [XmlAttribute]
        public string? color { get; set; }
        [XmlAttribute]
        public string? inner { get; set; }
        [XmlAttribute]
        public string? knockout { get; set; }
        [XmlAttribute]
        public string? quality { get; set; }
        [XmlAttribute]
        public string? strength { get; set; }
    }

    [XmlRoot("DropShadowFilter", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public class DropShadowFilter : Filter
    {
        [XmlAttribute]
        public string? angle { get; set; }
        [XmlAttribute]
        public string? blurX { get; set; }
        [XmlAttribute]
        public string? blurY { get; set; }
        [XmlAttribute]
        public string? color { get; set; }
        [XmlAttribute]
        public string? distance { get; set; }
        [XmlAttribute]
        public string? hideObject { get; set; }
        [XmlAttribute]
        public string? inner { get; set; }
        [XmlAttribute]
        public string? knockout { get; set; }
        [XmlAttribute]
        public string? quality { get; set; }
        [XmlAttribute]
        public string? strength { get; set; }
    }

    [XmlRoot("BevelFilter", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public class BevelFilter : Filter
    {
        [XmlAttribute]
        public string? blurX { get; set; }
        [XmlAttribute]
        public string? blurY { get; set; }
        [XmlAttribute]
        public string? quality { get; set; }
        [XmlAttribute]
        public string? angle { get; set; }
        [XmlAttribute]
        public string? distance { get; set; }
        [XmlAttribute]
        public string? highlightColor { get; set; }
        [XmlAttribute]
        public string? knockout { get; set; }
        [XmlAttribute]
        public string? shadowColor { get; set; }
        [XmlAttribute]
        public string? strength { get; set; }
        [XmlAttribute]
        public string? type { get; set; }
    }

    [XmlRoot("GradientGlowFilter", Namespace = "http://ns.adobe.com/xfl/2008/")]
    public class GradientGlowFilter : Filter
    {
        [XmlAttribute]
        public string? angle { get; set; }
        [XmlAttribute]
        public string? blurX { get; set; }
        [XmlAttribute]
        public string? blurY { get; set; }
        [XmlAttribute]
        public string? quality { get; set; }
        [XmlAttribute]
        public string? distance { get; set; }
        [XmlAttribute]
        public string? knockout { get; set; }
        [XmlAttribute]
        public string? strength { get; set; }
        [XmlAttribute]
        public string? type { get; set; }
        [XmlElement]
        public List<GradientEntry?>? GradientEntry { get; set; }

        public bool ShouldSerializeGradiantEntries() => GradientEntry != null && GradientEntry.Count > 0;
    }

    public class GradientEntry
    {
        [XmlAttribute]
        public string? color { get; set; }
        [XmlAttribute]
        public string? alpha { get; set; }
        [XmlAttribute]
        public string? ratio { get; set; }
    }

    public class GradientBevelFilter : Filter
    {
        [XmlAttribute]
        public string? angle { get; set; }
        [XmlAttribute]
        public string? blurX { get; set; }
        [XmlAttribute]
        public string? blurY { get; set; }
        [XmlAttribute]
        public string? quality { get; set; }
        [XmlAttribute]
        public string? distance { get; set; }
        [XmlAttribute]
        public string? knockout { get; set; }
        [XmlAttribute]
        public string? strength { get; set; }
        [XmlAttribute]
        public string? type { get; set; }
        [XmlElement]
        public List<GradientEntry?>? GradientEntry { get; set; }

        public bool ShouldSerializeGradiantEntry() => GradientEntry != null && GradientEntry.Count > 0;
    }

    public class AdjustColorFilter : Filter
    {
        [XmlAttribute]
        public string? brightness { get; set; }
        [XmlAttribute]
        public string? contrast { get; set; }
        [XmlAttribute]
        public string? saturation { get; set; }
        [XmlAttribute]
        public string? hue { get; set; }
    }

    public class MatteColor
    {
        [XmlAttribute]
        public string? color { get; set; }
    }
}