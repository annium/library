namespace Annium.Blazor.Css
{
    public static class RulePaddingExtensions
    {
        public static CssRule Padding(this CssRule rule, string padding) =>
            rule.Set("padding", padding);

        public static CssRule PaddingPx(this CssRule rule, double padding) =>
            rule.Padding($"{padding}px");

        public static CssRule PaddingEm(this CssRule rule, double padding) =>
            rule.Padding($"{padding}em");

        public static CssRule PaddingRem(this CssRule rule, double padding) =>
            rule.Padding($"{padding}rem");

        public static CssRule PaddingPercent(this CssRule rule, double padding) =>
            rule.Padding($"{padding}%");

        public static CssRule Padding(this CssRule rule, string paddingVertical, string paddingHorizontal)
            => rule.Set("padding", $"{paddingVertical} {paddingHorizontal}");

        public static CssRule PaddingPx(this CssRule rule, double paddingVertical, double paddingHorizontal) =>
            rule.Padding($"{paddingVertical}px", $"{paddingHorizontal}px");

        public static CssRule PaddingEm(this CssRule rule, double paddingVertical, double paddingHorizontal) =>
            rule.Padding($"{paddingVertical}em", $"{paddingHorizontal}em");

        public static CssRule PaddingRem(this CssRule rule, double paddingVertical, double paddingHorizontal) =>
            rule.Padding($"{paddingVertical}rem", $"{paddingHorizontal}rem");

        public static CssRule PaddingPercent(this CssRule rule, double paddingVertical, double paddingHorizontal) =>
            rule.Padding($"{paddingVertical}%", $"{paddingHorizontal}%");

        public static CssRule Padding(this CssRule rule, string paddingTop, string paddingHorizontal, string paddingBottom) =>
            rule.Set("padding", $"{paddingTop} {paddingHorizontal} {paddingBottom}");

        public static CssRule PaddingPx(this CssRule rule, double paddingTop, double paddingHorizontal, double paddingBottom) =>
            rule.Padding($"{paddingTop}px", $"{paddingHorizontal}px", $"{paddingBottom}px");

        public static CssRule PaddingEm(this CssRule rule, double paddingTop, double paddingHorizontal, double paddingBottom) =>
            rule.Padding($"{paddingTop}em", $"{paddingHorizontal}em", $"{paddingBottom}em");

        public static CssRule PaddingRem(this CssRule rule, double paddingTop, double paddingHorizontal, double paddingBottom) =>
            rule.Padding($"{paddingTop}rem", $"{paddingHorizontal}rem", $"{paddingBottom}rem");

        public static CssRule PaddingPercent(this CssRule rule, double paddingTop, double paddingHorizontal, double paddingBottom) =>
            rule.Padding($"{paddingTop}%", $"{paddingHorizontal}%", $"{paddingBottom}%");

        public static CssRule Padding(this CssRule rule, string paddingTop, string paddingRight, string paddingBottom, string paddingLeft)
            => rule.Set("padding", $"{paddingTop} {paddingRight} {paddingBottom} {paddingLeft}");

        public static CssRule PaddingPx(this CssRule rule, double paddingTop, double paddingRight, double paddingBottom, double paddingLeft) =>
            rule.Padding($"{paddingTop}px", $"{paddingRight}px", $"{paddingBottom}px", $"{paddingLeft}px");

        public static CssRule PaddingEm(this CssRule rule, double paddingTop, double paddingRight, double paddingBottom, double paddingLeft) =>
            rule.Padding($"{paddingTop}em", $"{paddingRight}em", $"{paddingBottom}em", $"{paddingLeft}em");

        public static CssRule PaddingRem(this CssRule rule, double paddingTop, double paddingRight, double paddingBottom, double paddingLeft) =>
            rule.Padding($"{paddingTop}rem", $"{paddingRight}rem", $"{paddingBottom}rem", $"{paddingLeft}rem");

        public static CssRule PaddingPercent(this CssRule rule, double paddingTop, double paddingRight, double paddingBottom, double paddingLeft) =>
            rule.Padding($"{paddingTop}%", $"{paddingRight}%", $"{paddingBottom}%", $"{paddingLeft}%");

        public static CssRule PaddingLeft(this CssRule rule, string padding) =>
            rule.Set("padding-left", padding);

        public static CssRule PaddingLeftPx(this CssRule rule, double padding) =>
            rule.PaddingLeft($"{padding}px");

        public static CssRule PaddingLeftEm(this CssRule rule, double padding) =>
            rule.PaddingLeft($"{padding}em");

        public static CssRule PaddingLeftRem(this CssRule rule, double padding) =>
            rule.PaddingLeft($"{padding}rem");

        public static CssRule PaddingLeftPercent(this CssRule rule, double padding) =>
            rule.PaddingLeft($"{padding}%");

        public static CssRule PaddingTop(this CssRule rule, string padding) =>
            rule.Set("padding-top", padding);

        public static CssRule PaddingTopPx(this CssRule rule, double padding) =>
            rule.PaddingTop($"{padding}px");

        public static CssRule PaddingTopEm(this CssRule rule, double padding) =>
            rule.PaddingTop($"{padding}em");

        public static CssRule PaddingTopRem(this CssRule rule, double padding) =>
            rule.PaddingTop($"{padding}rem");

        public static CssRule PaddingTopPercent(this CssRule rule, double padding) =>
            rule.PaddingTop($"{padding}%");

        public static CssRule PaddingRight(this CssRule rule, string padding) =>
            rule.Set("padding-right", padding);

        public static CssRule PaddingRightPx(this CssRule rule, double padding) =>
            rule.PaddingRight($"{padding}px");

        public static CssRule PaddingRightEm(this CssRule rule, double padding) =>
            rule.PaddingRight($"{padding}em");

        public static CssRule PaddingRightRem(this CssRule rule, double padding) =>
            rule.PaddingRight($"{padding}rem");

        public static CssRule PaddingRightPercent(this CssRule rule, double padding) =>
            rule.PaddingRight($"{padding}%");

        public static CssRule PaddingBottom(this CssRule rule, string padding) =>
            rule.Set("padding-bottom", padding);

        public static CssRule PaddingBottomPx(this CssRule rule, double padding) =>
            rule.PaddingBottom($"{padding}px");

        public static CssRule PaddingBottomEm(this CssRule rule, double padding) =>
            rule.PaddingBottom($"{padding}em");

        public static CssRule PaddingBottomRem(this CssRule rule, double padding) =>
            rule.PaddingBottom($"{padding}rem");

        public static CssRule PaddingBottomPercent(this CssRule rule, double padding) =>
            rule.PaddingBottom($"{padding}%");
    }
}