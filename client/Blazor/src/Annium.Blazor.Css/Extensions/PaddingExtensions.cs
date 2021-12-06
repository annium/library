using static System.FormattableString;

namespace Annium.Blazor.Css;

public static class PaddingExtensions
{
    public static CssRule Padding(this CssRule rule, string padding) =>
        rule.Set("padding", padding);

    public static CssRule PaddingPx(this CssRule rule, double padding) =>
        rule.Padding(Invariant($"{padding}px"));

    public static CssRule PaddingEm(this CssRule rule, double padding) =>
        rule.Padding(Invariant($"{padding}em"));

    public static CssRule PaddingRem(this CssRule rule, double padding) =>
        rule.Padding(Invariant($"{padding}rem"));

    public static CssRule PaddingPercent(this CssRule rule, double padding) =>
        rule.Padding(Invariant($"{padding}%"));

    public static CssRule Padding(this CssRule rule, string paddingVertical, string paddingHorizontal)
        => rule.Set("padding", Invariant($"{paddingVertical} {paddingHorizontal}"));

    public static CssRule PaddingPx(this CssRule rule, double paddingVertical, double paddingHorizontal) =>
        rule.Padding(Invariant($"{paddingVertical}px"), Invariant($"{paddingHorizontal}px"));

    public static CssRule PaddingEm(this CssRule rule, double paddingVertical, double paddingHorizontal) =>
        rule.Padding(Invariant($"{paddingVertical}em"), Invariant($"{paddingHorizontal}em"));

    public static CssRule PaddingRem(this CssRule rule, double paddingVertical, double paddingHorizontal) =>
        rule.Padding(Invariant($"{paddingVertical}rem"), Invariant($"{paddingHorizontal}rem"));

    public static CssRule PaddingPercent(this CssRule rule, double paddingVertical, double paddingHorizontal) =>
        rule.Padding(Invariant($"{paddingVertical}%"), Invariant($"{paddingHorizontal}%"));

    public static CssRule Padding(this CssRule rule, string paddingTop, string paddingHorizontal, string paddingBottom) =>
        rule.Set("padding", Invariant($"{paddingTop} {paddingHorizontal} {paddingBottom}"));

    public static CssRule PaddingPx(this CssRule rule, double paddingTop, double paddingHorizontal, double paddingBottom) =>
        rule.Padding(Invariant($"{paddingTop}px"), Invariant($"{paddingHorizontal}px"), Invariant($"{paddingBottom}px"));

    public static CssRule PaddingEm(this CssRule rule, double paddingTop, double paddingHorizontal, double paddingBottom) =>
        rule.Padding(Invariant($"{paddingTop}em"), Invariant($"{paddingHorizontal}em"), Invariant($"{paddingBottom}em"));

    public static CssRule PaddingRem(this CssRule rule, double paddingTop, double paddingHorizontal, double paddingBottom) =>
        rule.Padding(Invariant($"{paddingTop}rem"), Invariant($"{paddingHorizontal}rem"), Invariant($"{paddingBottom}rem"));

    public static CssRule PaddingPercent(this CssRule rule, double paddingTop, double paddingHorizontal, double paddingBottom) =>
        rule.Padding(Invariant($"{paddingTop}%"), Invariant($"{paddingHorizontal}%"), Invariant($"{paddingBottom}%"));

    public static CssRule Padding(this CssRule rule, string paddingTop, string paddingRight, string paddingBottom, string paddingLeft)
        => rule.Set("padding", Invariant($"{paddingTop} {paddingRight} {paddingBottom} {paddingLeft}"));

    public static CssRule PaddingPx(this CssRule rule, double paddingTop, double paddingRight, double paddingBottom, double paddingLeft) =>
        rule.Padding(Invariant($"{paddingTop}px"), Invariant($"{paddingRight}px"), Invariant($"{paddingBottom}px"), Invariant($"{paddingLeft}px"));

    public static CssRule PaddingEm(this CssRule rule, double paddingTop, double paddingRight, double paddingBottom, double paddingLeft) =>
        rule.Padding(Invariant($"{paddingTop}em"), Invariant($"{paddingRight}em"), Invariant($"{paddingBottom}em"), Invariant($"{paddingLeft}em"));

    public static CssRule PaddingRem(this CssRule rule, double paddingTop, double paddingRight, double paddingBottom, double paddingLeft) =>
        rule.Padding(Invariant($"{paddingTop}rem"), Invariant($"{paddingRight}rem"), Invariant($"{paddingBottom}rem"), Invariant($"{paddingLeft}rem"));

    public static CssRule PaddingPercent(this CssRule rule, double paddingTop, double paddingRight, double paddingBottom, double paddingLeft) =>
        rule.Padding(Invariant($"{paddingTop}%"), Invariant($"{paddingRight}%"), Invariant($"{paddingBottom}%"), Invariant($"{paddingLeft}%"));

    public static CssRule PaddingLeft(this CssRule rule, string padding) =>
        rule.Set("padding-left", padding);

    public static CssRule PaddingLeftPx(this CssRule rule, double padding) =>
        rule.PaddingLeft(Invariant($"{padding}px"));

    public static CssRule PaddingLeftEm(this CssRule rule, double padding) =>
        rule.PaddingLeft(Invariant($"{padding}em"));

    public static CssRule PaddingLeftRem(this CssRule rule, double padding) =>
        rule.PaddingLeft(Invariant($"{padding}rem"));

    public static CssRule PaddingLeftPercent(this CssRule rule, double padding) =>
        rule.PaddingLeft(Invariant($"{padding}%"));

    public static CssRule PaddingTop(this CssRule rule, string padding) =>
        rule.Set("padding-top", padding);

    public static CssRule PaddingTopPx(this CssRule rule, double padding) =>
        rule.PaddingTop(Invariant($"{padding}px"));

    public static CssRule PaddingTopEm(this CssRule rule, double padding) =>
        rule.PaddingTop(Invariant($"{padding}em"));

    public static CssRule PaddingTopRem(this CssRule rule, double padding) =>
        rule.PaddingTop(Invariant($"{padding}rem"));

    public static CssRule PaddingTopPercent(this CssRule rule, double padding) =>
        rule.PaddingTop(Invariant($"{padding}%"));

    public static CssRule PaddingRight(this CssRule rule, string padding) =>
        rule.Set("padding-right", padding);

    public static CssRule PaddingRightPx(this CssRule rule, double padding) =>
        rule.PaddingRight(Invariant($"{padding}px"));

    public static CssRule PaddingRightEm(this CssRule rule, double padding) =>
        rule.PaddingRight(Invariant($"{padding}em"));

    public static CssRule PaddingRightRem(this CssRule rule, double padding) =>
        rule.PaddingRight(Invariant($"{padding}rem"));

    public static CssRule PaddingRightPercent(this CssRule rule, double padding) =>
        rule.PaddingRight(Invariant($"{padding}%"));

    public static CssRule PaddingBottom(this CssRule rule, string padding) =>
        rule.Set("padding-bottom", padding);

    public static CssRule PaddingBottomPx(this CssRule rule, double padding) =>
        rule.PaddingBottom(Invariant($"{padding}px"));

    public static CssRule PaddingBottomEm(this CssRule rule, double padding) =>
        rule.PaddingBottom(Invariant($"{padding}em"));

    public static CssRule PaddingBottomRem(this CssRule rule, double padding) =>
        rule.PaddingBottom(Invariant($"{padding}rem"));

    public static CssRule PaddingBottomPercent(this CssRule rule, double padding) =>
        rule.PaddingBottom(Invariant($"{padding}%"));
}