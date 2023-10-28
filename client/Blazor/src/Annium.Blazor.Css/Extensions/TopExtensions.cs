using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

public static class TopExtensions
{
    public static CssRule Top(this CssRule rule, string top) => rule.Set("top", top);

    public static CssRule TopPx(this CssRule rule, int top) => rule.Top(Invariant($"{top}px"));

    public static CssRule TopEm(this CssRule rule, int top) => rule.Top(Invariant($"{top}em"));

    public static CssRule TopRem(this CssRule rule, int top) => rule.Top(Invariant($"{top}rem"));

    public static CssRule TopPercent(this CssRule rule, int top) => rule.Top(Invariant($"{top}%"));
}
