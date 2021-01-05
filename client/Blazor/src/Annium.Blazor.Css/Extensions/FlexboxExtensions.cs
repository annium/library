namespace Annium.Blazor.Css
{
    public static class FlexboxExtensions
    {
        public static CssRule FlexRow(this CssRule rule, AlignItems alignItems, JustifyContent justifyContent, bool inline = false) =>
            rule.FlexBox(Css.FlexDirection.Row, alignItems, justifyContent, inline);

        public static CssRule FlexColumn(this CssRule rule, AlignItems alignItems, JustifyContent justifyContent, bool inline = false) =>
            rule.FlexBox(Css.FlexDirection.Column, alignItems, justifyContent, inline);

        public static CssRule FlexRowReverse(this CssRule rule, AlignItems alignItems, JustifyContent justifyContent, bool inline = false) =>
            rule.FlexBox(Css.FlexDirection.RowReverse, alignItems, justifyContent, inline);

        public static CssRule FlexColumnReverse(this CssRule rule, AlignItems alignItems, JustifyContent justifyContent, bool inline = false) =>
            rule.FlexBox(Css.FlexDirection.ColumnReverse, alignItems, justifyContent, inline);

        public static CssRule Flex(this CssRule rule, int growAndShrink, string basis = "auto") => rule
            .FlexGrow(growAndShrink)
            .FlexShrink(growAndShrink)
            .FlexBasis(basis);

        public static CssRule Flex(this CssRule rule, int grow, int shrink, string basis = "auto") => rule
            .FlexGrow(grow)
            .FlexShrink(shrink)
            .FlexBasis(basis);

        public static CssRule FlexDirection(this CssRule rule, FlexDirection direction) => rule
            .Set("flex-direction", direction);

        public static CssRule AlignItems(this CssRule rule, AlignItems alignItems) => rule
            .Set("align-items", alignItems);

        public static CssRule AlignSelf(this CssRule rule, AlignItems alignItems) => rule
            .Set("align-self", alignItems);

        public static CssRule JustifyContent(this CssRule rule, JustifyContent justifyContent) => rule
            .Set("justify-content", justifyContent);

        public static CssRule FlexGrow(this CssRule rule, int grow) => rule
            .Set("flex-grow", $"{grow}");

        public static CssRule FlexShrink(this CssRule rule, int shrink) => rule
            .Set("flex-shrink", $"{shrink}");

        public static CssRule FlexBasis(this CssRule rule, string basis) => rule
            .Set("flex-basis", basis);

        public static CssRule FlexBasis(this CssRule rule, int basis) => rule
            .Set("flex-basis", $"{basis}");

        public static CssRule FlexWrap(this CssRule rule, int basis) => rule
            .Set("flex-basis", $"{basis}");

        private static CssRule FlexBox(
            this CssRule rule,
            FlexDirection direction,
            AlignItems alignItems,
            JustifyContent justifyContent,
            bool inline
        )
        {
            if (inline)
                rule.DisplayInlineFlex();
            else
                rule.DisplayFlex();

            return rule
                .FlexDirection(direction)
                .AlignItems(alignItems)
                .JustifyContent(justifyContent);
        }
    }
}