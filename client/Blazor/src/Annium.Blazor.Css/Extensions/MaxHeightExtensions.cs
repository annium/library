using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

public static class MaxHeightExtensions
{
    public static CssRule MaxHeight(this CssRule rule, string maxHeight) => rule.Set("max-height", maxHeight);
    public static CssRule MaxHeightPx(this CssRule rule, int maxHeight) => rule.MaxHeight(Invariant($"{maxHeight}px"));
    public static CssRule MaxHeightEm(this CssRule rule, int maxHeight) => rule.MaxHeight(Invariant($"{maxHeight}em"));
    public static CssRule MaxHeightRem(this CssRule rule, int maxHeight) => rule.MaxHeight(Invariant($"{maxHeight}rem"));
    public static CssRule MaxHeightPercent(this CssRule rule, int maxHeight) => rule.MaxHeight(Invariant($"{maxHeight}%"));
}