// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Css;

/// <summary>
/// Provides extension methods for setting cursor CSS property.
/// </summary>
public static class CursorExtensions
{
    /// <summary>
    /// Sets the cursor CSS property to crosshair.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule CursorCrosshair(this CssRule rule) => rule.Cursor("crosshair");

    /// <summary>
    /// Sets the cursor CSS property to help.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule CursorHelp(this CssRule rule) => rule.Cursor("help");

    /// <summary>
    /// Sets the cursor CSS property to move.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule CursorMove(this CssRule rule) => rule.Cursor("move");

    /// <summary>
    /// Sets the cursor CSS property to pointer.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule CursorPointer(this CssRule rule) => rule.Cursor("pointer");

    /// <summary>
    /// Sets the cursor CSS property to text.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule CursorText(this CssRule rule) => rule.Cursor("text");

    /// <summary>
    /// Sets the cursor CSS property to wait.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule CursorWait(this CssRule rule) => rule.Cursor("wait");

    /// <summary>
    /// Sets the cursor CSS property with the specified cursor value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="cursor">The cursor value to set.</param>
    /// <returns>The modified CSS rule.</returns>
    private static CssRule Cursor(this CssRule rule, string cursor) => rule.Set("cursor", cursor);
}
