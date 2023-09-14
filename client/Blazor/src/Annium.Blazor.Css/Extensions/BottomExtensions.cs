using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

public static class BottomExtensions
{
    public static CssRule Bottom(this CssRule rule, string bottom) => rule.Set("bottom", bottom);
    public static CssRule BottomPx(this CssRule rule, int bottom) => rule.Bottom(Invariant($"{bottom}px"));
    public static CssRule BottomEm(this CssRule rule, int bottom) => rule.Bottom(Invariant($"{bottom}em"));
    public static CssRule BottomRem(this CssRule rule, int bottom) => rule.Bottom(Invariant($"{bottom}rem"));
    public static CssRule BottomPercent(this CssRule rule, int bottom) => rule.Bottom(Invariant($"{bottom}%"));
}