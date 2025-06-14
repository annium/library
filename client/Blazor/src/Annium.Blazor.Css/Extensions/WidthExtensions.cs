using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Provides extension methods for CSS width styling.
/// </summary>
public static class WidthExtensions
{
    /// <summary>
    /// Sets the width property to the specified value.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the width to.</param>
    /// <param name="width">The width value as a string.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule Width(this CssRule rule, string width) => rule.Set("width", width);

    /// <summary>
    /// Sets the width property to the specified value in pixels.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the width to.</param>
    /// <param name="width">The width value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule WidthPx(this CssRule rule, int width) => rule.Width(Invariant($"{width}px"));

    /// <summary>
    /// Sets the width property to the specified value in em units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the width to.</param>
    /// <param name="width">The width value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule WidthEm(this CssRule rule, int width) => rule.Width(Invariant($"{width}em"));

    /// <summary>
    /// Sets the width property to the specified value in rem units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the width to.</param>
    /// <param name="width">The width value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule WidthRem(this CssRule rule, int width) => rule.Width(Invariant($"{width}rem"));

    /// <summary>
    /// Sets the width property to the specified value as a percentage.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the width to.</param>
    /// <param name="width">The width value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule WidthPercent(this CssRule rule, int width) => rule.Width(Invariant($"{width}%"));
}
