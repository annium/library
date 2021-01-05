namespace Annium.Blazor.Css
{
    public static class PositionExtensions
    {
        public static CssRule PositionAbsolute(this CssRule rule) =>
            rule.Position("absolute");

        public static CssRule PositionRelative(this CssRule rule) =>
            rule.Position("relative");

        public static CssRule PositionFixed(this CssRule rule) =>
            rule.Position("fixed");

        public static CssRule PositionSticky(this CssRule rule) =>
            rule.Position("sticky");

        private static CssRule Position(this CssRule rule, string position) =>
            rule.Set("position", position);
    }
}