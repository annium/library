using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

public static class LeftExtensions
{
    public static CssRule Left(this CssRule rule, string left) => rule.Set("left", left);
    public static CssRule LeftPx(this CssRule rule, int left) => rule.Left(Invariant($"{left}px"));
    public static CssRule LeftEm(this CssRule rule, int left) => rule.Left(Invariant($"{left}em"));
    public static CssRule LeftRem(this CssRule rule, int left) => rule.Left(Invariant($"{left}rem"));
    public static CssRule LeftPercent(this CssRule rule, int left) => rule.Left(Invariant($"{left}%"));
}