namespace Annium.Blazor.Css
{
    public static class RuleFontExtensions
    {
        public static CssRule FontSizePx(this CssRule rule, int width) => rule.Set("font-size", $"{width}px");
    }
}