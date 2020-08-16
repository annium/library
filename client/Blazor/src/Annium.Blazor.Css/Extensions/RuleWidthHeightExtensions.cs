namespace Annium.Blazor.Css
{
    public static class RuleWidthHeightExtensions
    {
        public static CssRule Width(this CssRule rule, string width) => rule.Set("width", width);
        public static CssRule WidthPx(this CssRule rule, int width) => rule.Width($"{width}px");
        public static CssRule WidthEm(this CssRule rule, int width) => rule.Width($"{width}em");
        public static CssRule WidthRem(this CssRule rule, int width) => rule.Width($"{width}rem");
        public static CssRule WidthPercent(this CssRule rule, int width) => rule.Width($"{width}%");
    }
}