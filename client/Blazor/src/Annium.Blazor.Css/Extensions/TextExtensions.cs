// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Css;

/// <summary>
/// Provides extension methods for CSS text styling.
/// </summary>
public static class TextExtensions
{
    /// <summary>
    /// Sets the text-align property to the specified alignment value.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the text alignment to.</param>
    /// <param name="align">The text alignment value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule TextAlign(this CssRule rule, TextAlign align) => rule.Set("text-align", align);
}
