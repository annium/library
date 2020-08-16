namespace Annium.Blazor.Css
{
    public static class RuleGeometryExtensions
    {
        public static CssRule WidthPx(this CssRule rule, int width) => rule.Set("width", $"{width}px");
        public static CssRule HeightPx(this CssRule rule, int height) => rule.Set("height", $"{height}px");
    }
}