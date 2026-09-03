using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Extension methods for applying max-height CSS properties to CSS rules.
/// </summary>
public static class MaxHeightExtensions
{
    /// <summary>
    /// Sets the max-height CSS property with a custom value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="maxHeight">The maximum height value to apply.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MaxHeight(this CssRule rule, string maxHeight) => rule.Set("max-height", maxHeight);

    /// <summary>
    /// Sets the max-height CSS property with a pixel value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="maxHeight">The maximum height value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MaxHeightPx(this CssRule rule, int maxHeight) => rule.MaxHeight(Invariant($"{maxHeight}px"));

    /// <summary>
    /// Sets the max-height CSS property with an em value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="maxHeight">The maximum height value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MaxHeightEm(this CssRule rule, int maxHeight) => rule.MaxHeight(Invariant($"{maxHeight}em"));

    /// <summary>
    /// Sets the max-height CSS property with a rem value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="maxHeight">The maximum height value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MaxHeightRem(this CssRule rule, int maxHeight) =>
        rule.MaxHeight(Invariant($"{maxHeight}rem"));

    /// <summary>
    /// Sets the max-height CSS property with a percentage value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="maxHeight">The maximum height value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MaxHeightPercent(this CssRule rule, int maxHeight) =>
        rule.MaxHeight(Invariant($"{maxHeight}%"));
}
