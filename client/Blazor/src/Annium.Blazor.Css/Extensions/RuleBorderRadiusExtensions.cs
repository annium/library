using static System.FormattableString;

namespace Annium.Blazor.Css
{
    public static class RuleBorderRadiusExtensions
    {
        public static CssRule BorderRadius(this CssRule rule, string borderRadius) =>
            rule.Set("border-radius", borderRadius);

        public static CssRule BorderRadiusPx(this CssRule rule, double borderRadius) =>
            rule.BorderRadius(Invariant($"{borderRadius}px"));

        public static CssRule BorderRadiusEm(this CssRule rule, double borderRadius) =>
            rule.BorderRadius(Invariant($"{borderRadius}em"));

        public static CssRule BorderRadiusRem(this CssRule rule, double borderRadius) =>
            rule.BorderRadius(Invariant($"{borderRadius}rem"));

        public static CssRule BorderRadiusPercent(this CssRule rule, double borderRadius) =>
            rule.BorderRadius(Invariant($"{borderRadius}%"));

        public static CssRule BorderRadius(this CssRule rule, string borderRadiusTopLeftBottomRight, string borderRadiusTopRightBottomLeft)
            => rule.Set("border-radius", Invariant($"{borderRadiusTopLeftBottomRight} {borderRadiusTopRightBottomLeft}"));

        public static CssRule BorderRadiusPx(this CssRule rule, double borderRadiusTopLeftBottomRight, double borderRadiusTopRightBottomLeft) =>
            rule.BorderRadius(Invariant($"{borderRadiusTopLeftBottomRight}px"), Invariant($"{borderRadiusTopRightBottomLeft}px"));

        public static CssRule BorderRadiusEm(this CssRule rule, double borderRadiusTopLeftBottomRight, double borderRadiusTopRightBottomLeft) =>
            rule.BorderRadius(Invariant($"{borderRadiusTopLeftBottomRight}em"), Invariant($"{borderRadiusTopRightBottomLeft}em"));

        public static CssRule BorderRadiusRem(this CssRule rule, double borderRadiusTopLeftBottomRight, double borderRadiusTopRightBottomLeft) =>
            rule.BorderRadius(Invariant($"{borderRadiusTopLeftBottomRight}rem"), Invariant($"{borderRadiusTopRightBottomLeft}rem"));

        public static CssRule BorderRadiusPercent(this CssRule rule, double borderRadiusTopLeftBottomRight, double borderRadiusTopRightBottomLeft) =>
            rule.BorderRadius(Invariant($"{borderRadiusTopLeftBottomRight}%"), Invariant($"{borderRadiusTopRightBottomLeft}%"));

        public static CssRule BorderRadius(this CssRule rule, string borderRadiusTopLeft, string borderRadiusTopRightBottomLeft, string borderRadiusBottomRight) =>
            rule.Set("border-radius", Invariant($"{borderRadiusTopLeft} {borderRadiusTopRightBottomLeft} {borderRadiusBottomRight}"));

        public static CssRule BorderRadiusPx(this CssRule rule, double borderRadiusTopLeft, double borderRadiusTopRightBottomLeft, double borderRadiusBottomRight) =>
            rule.BorderRadius(Invariant($"{borderRadiusTopLeft}px"), Invariant($"{borderRadiusTopRightBottomLeft}px"), Invariant($"{borderRadiusBottomRight}px"));

        public static CssRule BorderRadiusEm(this CssRule rule, double borderRadiusTopLeft, double borderRadiusTopRightBottomLeft, double borderRadiusBottomRight) =>
            rule.BorderRadius(Invariant($"{borderRadiusTopLeft}em"), Invariant($"{borderRadiusTopRightBottomLeft}em"), Invariant($"{borderRadiusBottomRight}em"));

        public static CssRule BorderRadiusRem(this CssRule rule, double borderRadiusTopLeft, double borderRadiusTopRightBottomLeft, double borderRadiusBottomRight) =>
            rule.BorderRadius(Invariant($"{borderRadiusTopLeft}rem"), Invariant($"{borderRadiusTopRightBottomLeft}rem"), Invariant($"{borderRadiusBottomRight}rem"));

        public static CssRule BorderRadiusPercent(this CssRule rule, double borderRadiusTopLeft, double borderRadiusTopRightBottomLeft, double borderRadiusBottomRight) =>
            rule.BorderRadius(Invariant($"{borderRadiusTopLeft}%"), Invariant($"{borderRadiusTopRightBottomLeft}%"), Invariant($"{borderRadiusBottomRight}%"));

        public static CssRule BorderRadius(this CssRule rule, string borderRadiusTopLeft, string borderRadiusTopRight, string borderRadiusBottomRight, string borderRadiusBottomLeft)
            => rule.Set("border-radius", Invariant($"{borderRadiusTopLeft} {borderRadiusTopRight} {borderRadiusBottomRight} {borderRadiusBottomLeft}"));

        public static CssRule BorderRadiusPx(this CssRule rule, double borderRadiusTopLeft, double borderRadiusTopRight, double borderRadiusBottomRight, double borderRadiusBottomLeft) =>
            rule.BorderRadius(Invariant($"{borderRadiusTopLeft}px"), Invariant($"{borderRadiusTopRight}px"), Invariant($"{borderRadiusBottomRight}px"), Invariant($"{borderRadiusBottomLeft}px"));

        public static CssRule BorderRadiusEm(this CssRule rule, double borderRadiusTopLeft, double borderRadiusTopRight, double borderRadiusBottomRight, double borderRadiusBottomLeft) =>
            rule.BorderRadius(Invariant($"{borderRadiusTopLeft}em"), Invariant($"{borderRadiusTopRight}em"), Invariant($"{borderRadiusBottomRight}em"), Invariant($"{borderRadiusBottomLeft}em"));

        public static CssRule BorderRadiusRem(this CssRule rule, double borderRadiusTopLeft, double borderRadiusTopRight, double borderRadiusBottomRight, double borderRadiusBottomLeft) =>
            rule.BorderRadius(Invariant($"{borderRadiusTopLeft}rem"), Invariant($"{borderRadiusTopRight}rem"), Invariant($"{borderRadiusBottomRight}rem"), Invariant($"{borderRadiusBottomLeft}rem"));

        public static CssRule BorderRadiusPercent(this CssRule rule, double borderRadiusTopLeft, double borderRadiusTopRight, double borderRadiusBottomRight, double borderRadiusBottomLeft) =>
            rule.BorderRadius(Invariant($"{borderRadiusTopLeft}%"), Invariant($"{borderRadiusTopRight}%"), Invariant($"{borderRadiusBottomRight}%"), Invariant($"{borderRadiusBottomLeft}%"));

        public static CssRule BorderRadiusTopLeft(this CssRule rule, string borderRadius) =>
            rule.Set("border-top-left-radius", borderRadius);

        public static CssRule BorderRadiusTopLeftPx(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusTopLeft(Invariant($"{borderRadius}px"));

        public static CssRule BorderRadiusTopLeftEm(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusTopLeft(Invariant($"{borderRadius}em"));

        public static CssRule BorderRadiusTopLeftRem(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusTopLeft(Invariant($"{borderRadius}rem"));

        public static CssRule BorderRadiusTopLeftPercent(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusTopLeft(Invariant($"{borderRadius}%"));

        public static CssRule BorderRadiusTopRight(this CssRule rule, string borderRadius) =>
            rule.Set("border-top-right-radius", borderRadius);

        public static CssRule BorderRadiusTopRightPx(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusTopRight(Invariant($"{borderRadius}px"));

        public static CssRule BorderRadiusTopRightEm(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusTopRight(Invariant($"{borderRadius}em"));

        public static CssRule BorderRadiusTopRightRem(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusTopRight(Invariant($"{borderRadius}rem"));

        public static CssRule BorderRadiusTopRightPercent(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusTopRight(Invariant($"{borderRadius}%"));

        public static CssRule BorderRadiusBottomRight(this CssRule rule, string borderRadius) =>
            rule.Set("border-bottom-right-radius", borderRadius);

        public static CssRule BorderRadiusBottomRightPx(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusBottomRight(Invariant($"{borderRadius}px"));

        public static CssRule BorderRadiusBottomRightEm(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusBottomRight(Invariant($"{borderRadius}em"));

        public static CssRule BorderRadiusBottomRightRem(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusBottomRight(Invariant($"{borderRadius}rem"));

        public static CssRule BorderRadiusBottomRightPercent(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusBottomRight(Invariant($"{borderRadius}%"));

        public static CssRule BorderRadiusBottomLeft(this CssRule rule, string borderRadius) =>
            rule.Set("border-bottom-left-radius", borderRadius);

        public static CssRule BorderRadiusBottomLeftPx(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusBottomLeft(Invariant($"{borderRadius}px"));

        public static CssRule BorderRadiusBottomLeftEm(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusBottomLeft(Invariant($"{borderRadius}em"));

        public static CssRule BorderRadiusBottomLeftRem(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusBottomLeft(Invariant($"{borderRadius}rem"));

        public static CssRule BorderRadiusBottomLeftPercent(this CssRule rule, double borderRadius) =>
            rule.BorderRadiusBottomLeft(Invariant($"{borderRadius}%"));
    }
}