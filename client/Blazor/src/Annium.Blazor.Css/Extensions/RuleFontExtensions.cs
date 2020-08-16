namespace Annium.Blazor.Css
{
    public static class RuleFontExtensions
    {
        public static IRule FontSizePx(this IRule rule, int width) => rule.Set("font-size", $"{width}px");
    }
}