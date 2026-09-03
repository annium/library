using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Provides extension methods for CSS height properties.
/// </summary>
public static class HeightExtensions
{
    /// <summary>
    /// Sets the height property with a string value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="height">The height value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule Height(this CssRule rule, string height) => rule.Set("height", height);

    /// <summary>
    /// Sets the height property with a pixel value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="height">The height in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule HeightPx(this CssRule rule, int height) => rule.Height(Invariant($"{height}px"));

    /// <summary>
    /// Sets the height property with an em value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="height">The height in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule HeightEm(this CssRule rule, int height) => rule.Height(Invariant($"{height}em"));

    /// <summary>
    /// Sets the height property with a rem value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="height">The height in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule HeightRem(this CssRule rule, int height) => rule.Height(Invariant($"{height}rem"));

    /// <summary>
    /// Sets the height property with a percentage value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="height">The height as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule HeightPercent(this CssRule rule, int height) => rule.Height(Invariant($"{height}%"));
}
