using static System.FormattableString;

namespace Annium.Blazor.Css
{
    public static class HeightExtensions
    {
        public static CssRule Height(this CssRule rule, string height) => rule.Set("height", height);
        public static CssRule HeightPx(this CssRule rule, int height) => rule.Height(Invariant($"{height}px"));
        public static CssRule HeightEm(this CssRule rule, int height) => rule.Height(Invariant($"{height}em"));
        public static CssRule HeightRem(this CssRule rule, int height) => rule.Height(Invariant($"{height}rem"));
        public static CssRule HeightPercent(this CssRule rule, int height) => rule.Height(Invariant($"{height}%"));
    }
}