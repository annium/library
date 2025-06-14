using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Provides extension methods for CSS top positioning.
/// </summary>
public static class TopExtensions
{
    /// <summary>
    /// Sets the top property to the specified value.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the top position to.</param>
    /// <param name="top">The top position value as a string.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule Top(this CssRule rule, string top) => rule.Set("top", top);

    /// <summary>
    /// Sets the top property to the specified value in pixels.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the top position to.</param>
    /// <param name="top">The top position value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule TopPx(this CssRule rule, int top) => rule.Top(Invariant($"{top}px"));

    /// <summary>
    /// Sets the top property to the specified value in em units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the top position to.</param>
    /// <param name="top">The top position value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule TopEm(this CssRule rule, int top) => rule.Top(Invariant($"{top}em"));

    /// <summary>
    /// Sets the top property to the specified value in rem units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the top position to.</param>
    /// <param name="top">The top position value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule TopRem(this CssRule rule, int top) => rule.Top(Invariant($"{top}rem"));

    /// <summary>
    /// Sets the top property to the specified value as a percentage.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the top position to.</param>
    /// <param name="top">The top position value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule TopPercent(this CssRule rule, int top) => rule.Top(Invariant($"{top}%"));
}
