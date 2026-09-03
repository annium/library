using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Provides extension methods for setting bottom positioning CSS properties
/// </summary>
public static class BottomExtensions
{
    /// <summary>
    /// Sets the bottom positioning property with a custom value
    /// </summary>
    /// <param name="rule">The CSS rule to modify</param>
    /// <param name="bottom">The bottom positioning value</param>
    /// <returns>The modified CSS rule</returns>
    public static CssRule Bottom(this CssRule rule, string bottom) => rule.Set("bottom", bottom);

    /// <summary>
    /// Sets the bottom positioning property with a pixel value
    /// </summary>
    /// <param name="rule">The CSS rule to modify</param>
    /// <param name="bottom">The bottom positioning value in pixels</param>
    /// <returns>The modified CSS rule</returns>
    public static CssRule BottomPx(this CssRule rule, int bottom) => rule.Bottom(Invariant($"{bottom}px"));

    /// <summary>
    /// Sets the bottom positioning property with an em value
    /// </summary>
    /// <param name="rule">The CSS rule to modify</param>
    /// <param name="bottom">The bottom positioning value in em units</param>
    /// <returns>The modified CSS rule</returns>
    public static CssRule BottomEm(this CssRule rule, int bottom) => rule.Bottom(Invariant($"{bottom}em"));

    /// <summary>
    /// Sets the bottom positioning property with a rem value
    /// </summary>
    /// <param name="rule">The CSS rule to modify</param>
    /// <param name="bottom">The bottom positioning value in rem units</param>
    /// <returns>The modified CSS rule</returns>
    public static CssRule BottomRem(this CssRule rule, int bottom) => rule.Bottom(Invariant($"{bottom}rem"));

    /// <summary>
    /// Sets the bottom positioning property with a percentage value
    /// </summary>
    /// <param name="rule">The CSS rule to modify</param>
    /// <param name="bottom">The bottom positioning value as a percentage</param>
    /// <returns>The modified CSS rule</returns>
    public static CssRule BottomPercent(this CssRule rule, int bottom) => rule.Bottom(Invariant($"{bottom}%"));
}
