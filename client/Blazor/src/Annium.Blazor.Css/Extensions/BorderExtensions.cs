namespace Annium.Blazor.Css
{
    public static class BorderExtensions
    {
        public static CssRule Border(this CssRule rule, string border) =>
            rule.Set("border", border);
        public static CssRule BorderTop(this CssRule rule, string border) =>
            rule.Set("border-top", border);
        public static CssRule BorderBottom(this CssRule rule, string border) =>
            rule.Set("border-bottom", border);
        public static CssRule BorderLeft(this CssRule rule, string border) =>
            rule.Set("border-left", border);
        public static CssRule BorderRight(this CssRule rule, string border) =>
            rule.Set("border-right", border);
    }
}