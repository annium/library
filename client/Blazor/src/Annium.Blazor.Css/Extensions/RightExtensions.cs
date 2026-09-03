using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Provides extension methods for CSS right positioning.
/// </summary>
public static class RightExtensions
{
    /// <summary>
    /// Sets the right property to the specified value.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the right position to.</param>
    /// <param name="right">The right position value as a string.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule Right(this CssRule rule, string right) => rule.Set("right", right);

    /// <summary>
    /// Sets the right property to the specified value in pixels.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the right position to.</param>
    /// <param name="right">The right position value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule RightPx(this CssRule rule, int right) => rule.Right(Invariant($"{right}px"));

    /// <summary>
    /// Sets the right property to the specified value in em units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the right position to.</param>
    /// <param name="right">The right position value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule RightEm(this CssRule rule, int right) => rule.Right(Invariant($"{right}em"));

    /// <summary>
    /// Sets the right property to the specified value in rem units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the right position to.</param>
    /// <param name="right">The right position value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule RightRem(this CssRule rule, int right) => rule.Right(Invariant($"{right}rem"));

    /// <summary>
    /// Sets the right property to the specified value as a percentage.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the right position to.</param>
    /// <param name="right">The right position value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule RightPercent(this CssRule rule, int right) => rule.Right(Invariant($"{right}%"));
}
