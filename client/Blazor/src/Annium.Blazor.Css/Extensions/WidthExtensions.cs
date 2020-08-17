using static System.FormattableString;
namespace Annium.Blazor.Css
{
    public static class WidthExtensions
    {
        public static CssRule Width(this CssRule rule, string width) => rule.Set("width", width);
        public static CssRule WidthPx(this CssRule rule, int width) => rule.Width(Invariant($"{width}px"));
        public static CssRule WidthEm(this CssRule rule, int width) => rule.Width(Invariant($"{width}em"));
        public static CssRule WidthRem(this CssRule rule, int width) => rule.Width(Invariant($"{width}rem"));
        public static CssRule WidthPercent(this CssRule rule, int width) => rule.Width(Invariant($"{width}%"));
    }
}