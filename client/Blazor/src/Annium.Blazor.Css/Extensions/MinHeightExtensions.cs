using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Extension methods for applying min-height CSS properties to CSS rules.
/// </summary>
public static class MinHeightExtensions
{
    /// <summary>
    /// Sets the min-height CSS property with a custom value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="minHeight">The minimum height value to apply.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MinHeight(this CssRule rule, string minHeight) => rule.Set("min-height", minHeight);

    /// <summary>
    /// Sets the min-height CSS property with a pixel value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="minHeight">The minimum height value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MinHeightPx(this CssRule rule, int minHeight) => rule.MinHeight(Invariant($"{minHeight}px"));

    /// <summary>
    /// Sets the min-height CSS property with an em value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="minHeight">The minimum height value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MinHeightEm(this CssRule rule, int minHeight) => rule.MinHeight(Invariant($"{minHeight}em"));

    /// <summary>
    /// Sets the min-height CSS property with a rem value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="minHeight">The minimum height value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MinHeightRem(this CssRule rule, int minHeight) =>
        rule.MinHeight(Invariant($"{minHeight}rem"));

    /// <summary>
    /// Sets the min-height CSS property with a percentage value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="minHeight">The minimum height value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MinHeightPercent(this CssRule rule, int minHeight) =>
        rule.MinHeight(Invariant($"{minHeight}%"));
}
