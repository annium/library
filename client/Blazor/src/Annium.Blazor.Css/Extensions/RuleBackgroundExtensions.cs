namespace Annium.Blazor.Css
{
    public static class RuleBackgroundExtensions
    {
        public static CssRule BackgroundColor(this CssRule rule, string color) => rule.Set("background-color", color);
    }
}