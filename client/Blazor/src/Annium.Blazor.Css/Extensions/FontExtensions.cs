using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Provides extension methods for CSS font properties.
/// </summary>
public static class FontExtensions
{
    /// <summary>
    /// Sets the font-family property.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="fontFamily">The font family value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule FontFamily(this CssRule rule, string fontFamily) => rule.Set("font-family", fontFamily);

    /// <summary>
    /// Sets the font-size property with a string value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="fontSize">The font size value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule FontSize(this CssRule rule, string fontSize) => rule.Set("font-size", fontSize);

    /// <summary>
    /// Sets the font-size property with a pixel value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="fontSize">The font size in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule FontSizePx(this CssRule rule, double fontSize) =>
        rule.Set("font-size", Invariant($"{fontSize}px"));

    /// <summary>
    /// Sets the font-size property with an em value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="fontSize">The font size in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule FontSizeEm(this CssRule rule, double fontSize) =>
        rule.Set("font-size", Invariant($"{fontSize}em"));

    /// <summary>
    /// Sets the font-size property with a rem value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="fontSize">The font size in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule FontSizeRem(this CssRule rule, double fontSize) =>
        rule.Set("font-size", Invariant($"{fontSize}rem"));

    /// <summary>
    /// Sets the font-weight property.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="fontWeight">The font weight value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule FontWeight(this CssRule rule, FontWeight fontWeight) => rule.Set("font-weight", fontWeight);

    /// <summary>
    /// Sets the font-weight property to normal (400).
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule FontWeightNormal(this CssRule rule) => rule.Set("font-weight", Css.FontWeight.W400);

    /// <summary>
    /// Sets the font-weight property to bold (700).
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule FontWeightBold(this CssRule rule) => rule.Set("font-weight", Css.FontWeight.W700);

    /// <summary>
    /// Sets the color property.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="color">The color value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule Color(this CssRule rule, string color) => rule.Set("color", color);
}
