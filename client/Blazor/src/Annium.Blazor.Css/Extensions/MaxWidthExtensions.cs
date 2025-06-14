using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Extension methods for applying max-width CSS properties to CSS rules.
/// </summary>
public static class MaxWidthExtensions
{
    /// <summary>
    /// Sets the max-width CSS property with a custom value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="maxWidth">The maximum width value to apply.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MaxWidth(this CssRule rule, string maxWidth) => rule.Set("max-width", maxWidth);

    /// <summary>
    /// Sets the max-width CSS property with a pixel value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="maxWidth">The maximum width value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MaxWidthPx(this CssRule rule, int maxWidth) => rule.MaxWidth(Invariant($"{maxWidth}px"));

    /// <summary>
    /// Sets the max-width CSS property with an em value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="maxWidth">The maximum width value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MaxWidthEm(this CssRule rule, int maxWidth) => rule.MaxWidth(Invariant($"{maxWidth}em"));

    /// <summary>
    /// Sets the max-width CSS property with a rem value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="maxWidth">The maximum width value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MaxWidthRem(this CssRule rule, int maxWidth) => rule.MaxWidth(Invariant($"{maxWidth}rem"));

    /// <summary>
    /// Sets the max-width CSS property with a percentage value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="maxWidth">The maximum width value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MaxWidthPercent(this CssRule rule, int maxWidth) => rule.MaxWidth(Invariant($"{maxWidth}%"));
}
