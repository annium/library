// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Css;

/// <summary>
/// Provides extension methods for CSS display properties.
/// </summary>
public static class DisplayExtensions
{
    /// <summary>
    /// Sets the display property to block.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule DisplayBlock(this CssRule rule) => rule.Display("block");

    /// <summary>
    /// Sets the display property to flex.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule DisplayFlex(this CssRule rule) => rule.Display("flex");

    /// <summary>
    /// Sets the display property to inline.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule DisplayInline(this CssRule rule) => rule.Display("inline");

    /// <summary>
    /// Sets the display property to inline-block.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule DisplayInlineBlock(this CssRule rule) => rule.Display("inline-block");

    /// <summary>
    /// Sets the display property to inline-flex.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule DisplayInlineFlex(this CssRule rule) => rule.Display("inline-flex");

    /// <summary>
    /// Sets the display property to the specified value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="display">The display value to set.</param>
    /// <returns>The modified CSS rule.</returns>
    private static CssRule Display(this CssRule rule, string display) => rule.Set("display", display);
}
