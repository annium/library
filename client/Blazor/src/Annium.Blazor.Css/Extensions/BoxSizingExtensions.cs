namespace Annium.Blazor.Css
{
    public static class BoxSizingExtensions
    {
        public static CssRule BoxSizingBorderBox(this CssRule rule) =>
            rule.Set("box-sizing", "border-box");

        public static CssRule BoxSizingContentBox(this CssRule rule) =>
            rule.Set("box-sizing", "content-box");
    }
}