// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Css;

/// <summary>
/// Provides extension methods for CSS position styling.
/// </summary>
public static class PositionExtensions
{
    /// <summary>
    /// Sets the position property to 'absolute'.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the position to.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PositionAbsolute(this CssRule rule) => rule.Position("absolute");

    /// <summary>
    /// Sets the position property to 'relative'.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the position to.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PositionRelative(this CssRule rule) => rule.Position("relative");

    /// <summary>
    /// Sets the position property to 'fixed'.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the position to.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PositionFixed(this CssRule rule) => rule.Position("fixed");

    /// <summary>
    /// Sets the position property to 'sticky'.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the position to.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PositionSticky(this CssRule rule) => rule.Position("sticky");

    /// <summary>
    /// Sets the position property to the specified value.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the position to.</param>
    /// <param name="position">The position value.</param>
    /// <returns>The modified CSS rule.</returns>
    private static CssRule Position(this CssRule rule, string position) => rule.Set("position", position);
}
