// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Css;

/// <summary>
/// Provides extension methods for setting border-related CSS properties.
/// </summary>
public static class BorderExtensions
{
    /// <summary>
    /// Sets the border CSS property for all sides.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="border">The border value to set.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule Border(this CssRule rule, string border) => rule.Set("border", border);

    /// <summary>
    /// Sets the border-top CSS property.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="border">The border value to set for the top side.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderTop(this CssRule rule, string border) => rule.Set("border-top", border);

    /// <summary>
    /// Sets the border-bottom CSS property.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="border">The border value to set for the bottom side.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderBottom(this CssRule rule, string border) => rule.Set("border-bottom", border);

    /// <summary>
    /// Sets the border-left CSS property.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="border">The border value to set for the left side.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderLeft(this CssRule rule, string border) => rule.Set("border-left", border);

    /// <summary>
    /// Sets the border-right CSS property.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="border">The border value to set for the right side.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRight(this CssRule rule, string border) => rule.Set("border-right", border);
}
