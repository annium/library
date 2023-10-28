// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Css;

public static class CursorExtensions
{
    public static CssRule CursorCrosshair(this CssRule rule) => rule.Cursor("crosshair");

    public static CssRule CursorHelp(this CssRule rule) => rule.Cursor("help");

    public static CssRule CursorMove(this CssRule rule) => rule.Cursor("move");

    public static CssRule CursorPointer(this CssRule rule) => rule.Cursor("pointer");

    public static CssRule CursorText(this CssRule rule) => rule.Cursor("text");

    public static CssRule CursorWait(this CssRule rule) => rule.Cursor("wait");

    private static CssRule Cursor(this CssRule rule, string cursor) => rule.Set("cursor", cursor);
}
