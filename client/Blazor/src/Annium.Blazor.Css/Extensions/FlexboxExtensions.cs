namespace Annium.Blazor.Css
{
    public static class FlexboxExtensions
    {
        public static CssRule FlexRow(this CssRule rule, AlignItems alignItems, JustifyContent justifyContent, bool inline = false) =>
            rule.FlexBox(FlexDirection.Row, alignItems, justifyContent, inline);
        public static CssRule FlexColumn(this CssRule rule, AlignItems alignItems, JustifyContent justifyContent, bool inline = false) =>
            rule.FlexBox(FlexDirection.Column, alignItems, justifyContent, inline);
        public static CssRule FlexRowInverse(this CssRule rule, AlignItems alignItems, JustifyContent justifyContent, bool inline = false) =>
            rule.FlexBox(FlexDirection.RowInverse, alignItems, justifyContent, inline);
        public static CssRule FlexColumnInverse(this CssRule rule, AlignItems alignItems, JustifyContent justifyContent, bool inline = false) =>
            rule.FlexBox(FlexDirection.ColumnInverse, alignItems, justifyContent, inline);

        private static CssRule FlexBox(
            this CssRule rule,
            FlexDirection direction,
            AlignItems alignItems,
            JustifyContent justifyContent,
            bool inline
        )
        {
            rule.Set("display", inline ? "inline-flex" : "flex");
            rule.Set("flex-direction", direction);
            rule.Set("align-items", alignItems);
            rule.Set("justify-content", alignItems);

            return rule;
        }
    }
}