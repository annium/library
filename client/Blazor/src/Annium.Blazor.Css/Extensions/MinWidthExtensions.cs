using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

public static class MinWidthExtensions
{
    public static CssRule MinWidth(this CssRule rule, string minWidth) => rule.Set("min-width", minWidth);

    public static CssRule MinWidthPx(this CssRule rule, int minWidth) => rule.MinWidth(Invariant($"{minWidth}px"));

    public static CssRule MinWidthEm(this CssRule rule, int minWidth) => rule.MinWidth(Invariant($"{minWidth}em"));

    public static CssRule MinWidthRem(this CssRule rule, int minWidth) => rule.MinWidth(Invariant($"{minWidth}rem"));

    public static CssRule MinWidthPercent(this CssRule rule, int minWidth) => rule.MinWidth(Invariant($"{minWidth}%"));
}
