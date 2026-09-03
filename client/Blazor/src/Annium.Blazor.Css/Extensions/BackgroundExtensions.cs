// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Css;

/// <summary>
/// Provides extension methods for setting background-related CSS properties.
/// </summary>
public static class BackgroundExtensions
{
    /// <summary>
    /// Sets the background-color CSS property.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="color">The color value to set.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BackgroundColor(this CssRule rule, string color) => rule.Set("background-color", color);
}
