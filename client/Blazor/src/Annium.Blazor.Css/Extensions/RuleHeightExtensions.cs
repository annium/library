namespace Annium.Blazor.Css
{
    public static class RuleHeightExtensions
    {
        public static CssRule Height(this CssRule rule, string height) => rule.Set("height", height);
        public static CssRule HeightPx(this CssRule rule, int height) => rule.Height($"{height}px");
        public static CssRule HeightEm(this CssRule rule, int height) => rule.Height($"{height}em");
        public static CssRule HeightRem(this CssRule rule, int height) => rule.Height($"{height}rem");
        public static CssRule HeightPercent(this CssRule rule, int height) => rule.Height($"{height}%");
    }
}