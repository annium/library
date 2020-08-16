namespace Annium.Blazor.Css
{
    public static class RuleGeometryExtensions
    {
        public static IRule WidthPx(this IRule rule, int width) => rule.Set("width", $"{width}px");
        public static IRule HeightPx(this IRule rule, int height) => rule.Set("height", $"{height}px");
    }
}