using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Extension methods for applying min-width CSS properties to CSS rules.
/// </summary>
public static class MinWidthExtensions
{
    /// <summary>
    /// Sets the min-width CSS property with a custom value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="minWidth">The minimum width value to apply.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MinWidth(this CssRule rule, string minWidth) => rule.Set("min-width", minWidth);

    /// <summary>
    /// Sets the min-width CSS property with a pixel value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="minWidth">The minimum width value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MinWidthPx(this CssRule rule, int minWidth) => rule.MinWidth(Invariant($"{minWidth}px"));

    /// <summary>
    /// Sets the min-width CSS property with an em value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="minWidth">The minimum width value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MinWidthEm(this CssRule rule, int minWidth) => rule.MinWidth(Invariant($"{minWidth}em"));

    /// <summary>
    /// Sets the min-width CSS property with a rem value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="minWidth">The minimum width value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MinWidthRem(this CssRule rule, int minWidth) => rule.MinWidth(Invariant($"{minWidth}rem"));

    /// <summary>
    /// Sets the min-width CSS property with a percentage value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="minWidth">The minimum width value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MinWidthPercent(this CssRule rule, int minWidth) => rule.MinWidth(Invariant($"{minWidth}%"));
}
