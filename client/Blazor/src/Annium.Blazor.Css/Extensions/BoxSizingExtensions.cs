// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Css;

/// <summary>
/// Provides extension methods for setting box-sizing CSS property.
/// </summary>
public static class BoxSizingExtensions
{
    /// <summary>
    /// Sets the box-sizing CSS property to border-box.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BoxSizingBorderBox(this CssRule rule) => rule.Set("box-sizing", "border-box");

    /// <summary>
    /// Sets the box-sizing CSS property to content-box.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BoxSizingContentBox(this CssRule rule) => rule.Set("box-sizing", "content-box");
}
