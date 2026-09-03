using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Provides extension methods for CSS left positioning properties.
/// </summary>
public static class LeftExtensions
{
    /// <summary>
    /// Sets the left property with a string value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="left">The left position value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule Left(this CssRule rule, string left) => rule.Set("left", left);

    /// <summary>
    /// Sets the left property with a pixel value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="left">The left position in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule LeftPx(this CssRule rule, int left) => rule.Left(Invariant($"{left}px"));

    /// <summary>
    /// Sets the left property with an em value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="left">The left position in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule LeftEm(this CssRule rule, int left) => rule.Left(Invariant($"{left}em"));

    /// <summary>
    /// Sets the left property with a rem value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="left">The left position in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule LeftRem(this CssRule rule, int left) => rule.Left(Invariant($"{left}rem"));

    /// <summary>
    /// Sets the left property with a percentage value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="left">The left position as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule LeftPercent(this CssRule rule, int left) => rule.Left(Invariant($"{left}%"));
}
