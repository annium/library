namespace Annium.Blazor.Css
{
    public static class RuleBorderRadiusExtensions
    {
        public static CssRule BorderRadius(this CssRule rule, string borderRadius) =>
            rule.Set("border-radius", borderRadius);

        public static CssRule BorderRadiusPx(this CssRule rule, double borderRadius) =>
            rule.BorderRadius($"{borderRadius}px");

        public static CssRule BorderRadiusEm(this CssRule rule, double borderRadius) =>
            rule.BorderRadius($"{borderRadius}em");

        public static CssRule BorderRadiusRem(this CssRule rule, double borderRadius) =>
            rule.BorderRadius($"{borderRadius}rem");

        public static CssRule BorderRadiusPercent(this CssRule rule, double borderRadius) =>
            rule.BorderRadius($"{borderRadius}%");

        public static CssRule BorderRadius(this CssRule rule, string borderRadiusTopLeftBottomRight, string borderRadiusTopRightBottomLeft)
            => rule.Set("border-radius", $"{borderRadiusTopLeftBottomRight} {borderRadiusTopRightBottomLeft}");

        public static CssRule BorderRadiusPx(this CssRule rule, double borderRadiusTopLeftBottomRight, double borderRadiusTopRightBottomLeft) =>
            rule.BorderRadius($"{borderRadiusTopLeftBottomRight}px", $"{borderRadiusTopRightBottomLeft}px");

        public static CssRule BorderRadiusEm(this CssRule rule, double borderRadiusTopLeftBottomRight, double borderRadiusTopRightBottomLeft) =>
            rule.BorderRadius($"{borderRadiusTopLeftBottomRight}em", $"{borderRadiusTopRightBottomLeft}em");

        public static CssRule BorderRadiusRem(this CssRule rule, double borderRadiusTopLeftBottomRight, double borderRadiusTopRightBottomLeft) =>
            rule.BorderRadius($"{borderRadiusTopLeftBottomRight}rem", $"{borderRadiusTopRightBottomLeft}rem");

        public static CssRule BorderRadiusPercent(this CssRule rule, double borderRadiusTopLeftBottomRight, double borderRadiusTopRightBottomLeft) =>
            rule.BorderRadius($"{borderRadiusTopLeftBottomRight}%", $"{borderRadiusTopRightBottomLeft}%");

        public static CssRule BorderRadius(this CssRule rule, string borderRadiusTopLeft, string borderRadiusTopRightBottomLeft, string borderRadiusBottomRight) =>
            rule.Set("border-radius", $"{borderRadiusTopLeft} {borderRadiusTopRightBottomLeft} {borderRadiusBottomRight}");

        public static CssRule BorderRadiusPx(this CssRule rule, double borderRadiusTopLeft, double borderRadiusTopRightBottomLeft, double borderRadiusBottomRight) =>
            rule.BorderRadius($"{borderRadiusTopLeft}px", $"{borderRadiusTopRightBottomLeft}px", $"{borderRadiusBottomRight}px");

        public static CssRule BorderRadiusEm(this CssRule rule, double borderRadiusTopLeft, double borderRadiusTopRightBottomLeft, double borderRadiusBottomRight) =>
            rule.BorderRadius($"{borderRadiusTopLeft}em", $"{borderRadiusTopRightBottomLeft}em", $"{borderRadiusBottomRight}em");

        public static CssRule BorderRadiusRem(this CssRule rule, double borderRadiusTopLeft, double borderRadiusTopRightBottomLeft, double borderRadiusBottomRight) =>
            rule.BorderRadius($"{borderRadiusTopLeft}rem", $"{borderRadiusTopRightBottomLeft}rem", $"{borderRadiusBottomRight}rem");

        public static CssRule BorderRadiusPercent(this CssRule rule, double borderRadiusTopLeft, double borderRadiusTopRightBottomLeft, double borderRadiusBottomRight) =>
            rule.BorderRadius($"{borderRadiusTopLeft}%", $"{borderRadiusTopRightBottomLeft}%", $"{borderRadiusBottomRight}%");

        public static CssRule BorderRadius(this CssRule rule, string borderRadiusTopLeft, string borderRadiusTopRight, string borderRadiusBottomRight, string borderRadiusBottomLeft)
            => rule.Set("border-radius", $"{borderRadiusTopLeft} {borderRadiusTopRight} {borderRadiusBottomRight} {borderRadiusBottomLeft}");

        public static CssRule BorderRadiusPx(this CssRule rule, double borderRadiusTopLeft, double borderRadiusTopRight, double borderRadiusBottomRight, double borderRadiusBottomLeft) =>
            rule.BorderRadius($"{borderRadiusTopLeft}px", $"{borderRadiusTopRight}px", $"{borderRadiusBottomRight}px", $"{borderRadiusBottomLeft}px");

        public static CssRule BorderRadiusEm(this CssRule rule, double borderRadiusTopLeft, double borderRadiusTopRight, double borderRadiusBottomRight, double borderRadiusBottomLeft) =>
            rule.BorderRadius($"{borderRadiusTopLeft}em", $"{borderRadiusTopRight}em", $"{borderRadiusBottomRight}em", $"{borderRadiusBottomLeft}em");

        public static CssRule BorderRadiusRem(this CssRule rule, double borderRadiusTopLeft, double borderRadiusTopRight, double borderRadiusBottomRight, double borderRadiusBottomLeft) =>
            rule.BorderRadius($"{borderRadiusTopLeft}rem", $"{borderRadiusTopRight}rem", $"{borderRadiusBottomRight}rem", $"{borderRadiusBottomLeft}rem");

        public static CssRule BorderRadiusPercent(this CssRule rule, double borderRadiusTopLeft, double borderRadiusTopRight, double borderRadiusBottomRight, double borderRadiusBottomLeft) =>
            rule.BorderRadius($"{borderRadiusTopLeft}%", $"{borderRadiusTopRight}%", $"{borderRadiusBottomRight}%", $"{borderRadiusBottomLeft}%");

        public static CssRule BorderRadiusTopLeft(this CssRule rule, string borderRadius) =>
            rule.Set("border-top-left-radius", borderRadius);

        public static CssRule BorderRadiusTopLeftPx(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusTopLeft($"{borderRadius}px");

        public static CssRule BorderRadiusTopLeftEm(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusTopLeft($"{borderRadius}em");

        public static CssRule BorderRadiusTopLeftRem(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusTopLeft($"{borderRadius}rem");

        public static CssRule BorderRadiusTopLeftPercent(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusTopLeft($"{borderRadius}%");

        public static CssRule BorderRadiusTopRight(this CssRule rule, string borderRadius) =>
            rule.Set("border-top-right-radius", borderRadius);

        public static CssRule BorderRadiusTopRightPx(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusTopRight($"{borderRadius}px");

        public static CssRule BorderRadiusTopRightEm(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusTopRight($"{borderRadius}em");

        public static CssRule BorderRadiusTopRightRem(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusTopRight($"{borderRadius}rem");

        public static CssRule BorderRadiusTopRightPercent(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusTopRight($"{borderRadius}%");

        public static CssRule BorderRadiusBottomRight(this CssRule rule, string borderRadius) =>
            rule.Set("border-bottom-right-radius", borderRadius);

        public static CssRule BorderRadiusBottomRightPx(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusBottomRight($"{borderRadius}px");

        public static CssRule BorderRadiusBottomRightEm(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusBottomRight($"{borderRadius}em");

        public static CssRule BorderRadiusBottomRightRem(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusBottomRight($"{borderRadius}rem");

        public static CssRule BorderRadiusBottomRightPercent(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusBottomRight($"{borderRadius}%");

        public static CssRule BorderRadiusBottomLeft(this CssRule rule, string borderRadius) =>
            rule.Set("border-bottom-left-radius", borderRadius);

        public static CssRule BorderRadiusBottomLeftPx(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusBottomLeft($"{borderRadius}px");

        public static CssRule BorderRadiusBottomLeftEm(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusBottomLeft($"{borderRadius}em");

        public static CssRule BorderRadiusBottomLeftRem(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusBottomLeft($"{borderRadius}rem");

        public static CssRule BorderRadiusBottomLeftPercent(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusBottomLeft($"{borderRadius}%");
    }
}