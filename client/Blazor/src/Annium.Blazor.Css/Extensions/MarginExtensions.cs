using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

public static class MarginExtensions
{
    public static CssRule Margin(this CssRule rule, string margin) => rule.Set("margin", margin);

    public static CssRule MarginPx(this CssRule rule, double margin) => rule.Margin(Invariant($"{margin}px"));

    public static CssRule MarginEm(this CssRule rule, double margin) => rule.Margin(Invariant($"{margin}em"));

    public static CssRule MarginRem(this CssRule rule, double margin) => rule.Margin(Invariant($"{margin}rem"));

    public static CssRule MarginPercent(this CssRule rule, double margin) => rule.Margin(Invariant($"{margin}%"));

    public static CssRule Margin(this CssRule rule, string marginVertical, string marginHorizontal) =>
        rule.Set("margin", Invariant($"{marginVertical} {marginHorizontal}"));

    public static CssRule MarginPx(this CssRule rule, double marginVertical, double marginHorizontal) =>
        rule.Margin(Invariant($"{marginVertical}px"), Invariant($"{marginHorizontal}px"));

    public static CssRule MarginEm(this CssRule rule, double marginVertical, double marginHorizontal) =>
        rule.Margin(Invariant($"{marginVertical}em"), Invariant($"{marginHorizontal}em"));

    public static CssRule MarginRem(this CssRule rule, double marginVertical, double marginHorizontal) =>
        rule.Margin(Invariant($"{marginVertical}rem"), Invariant($"{marginHorizontal}rem"));

    public static CssRule MarginPercent(this CssRule rule, double marginVertical, double marginHorizontal) =>
        rule.Margin(Invariant($"{marginVertical}%"), Invariant($"{marginHorizontal}%"));

    public static CssRule Margin(this CssRule rule, string marginTop, string marginHorizontal, string marginBottom) =>
        rule.Set("margin", Invariant($"{marginTop} {marginHorizontal} {marginBottom}"));

    public static CssRule MarginPx(this CssRule rule, double marginTop, double marginHorizontal, double marginBottom) =>
        rule.Margin(Invariant($"{marginTop}px"), Invariant($"{marginHorizontal}px"), Invariant($"{marginBottom}px"));

    public static CssRule MarginEm(this CssRule rule, double marginTop, double marginHorizontal, double marginBottom) =>
        rule.Margin(Invariant($"{marginTop}em"), Invariant($"{marginHorizontal}em"), Invariant($"{marginBottom}em"));

    public static CssRule MarginRem(
        this CssRule rule,
        double marginTop,
        double marginHorizontal,
        double marginBottom
    ) =>
        rule.Margin(Invariant($"{marginTop}rem"), Invariant($"{marginHorizontal}rem"), Invariant($"{marginBottom}rem"));

    public static CssRule MarginPercent(
        this CssRule rule,
        double marginTop,
        double marginHorizontal,
        double marginBottom
    ) => rule.Margin(Invariant($"{marginTop}%"), Invariant($"{marginHorizontal}%"), Invariant($"{marginBottom}%"));

    public static CssRule Margin(
        this CssRule rule,
        string marginTop,
        string marginRight,
        string marginBottom,
        string marginLeft
    ) => rule.Set("margin", Invariant($"{marginTop} {marginRight} {marginBottom} {marginLeft}"));

    public static CssRule MarginPx(
        this CssRule rule,
        double marginTop,
        double marginRight,
        double marginBottom,
        double marginLeft
    ) =>
        rule.Margin(
            Invariant($"{marginTop}px"),
            Invariant($"{marginRight}px"),
            Invariant($"{marginBottom}px"),
            Invariant($"{marginLeft}px")
        );

    public static CssRule MarginEm(
        this CssRule rule,
        double marginTop,
        double marginRight,
        double marginBottom,
        double marginLeft
    ) =>
        rule.Margin(
            Invariant($"{marginTop}em"),
            Invariant($"{marginRight}em"),
            Invariant($"{marginBottom}em"),
            Invariant($"{marginLeft}em")
        );

    public static CssRule MarginRem(
        this CssRule rule,
        double marginTop,
        double marginRight,
        double marginBottom,
        double marginLeft
    ) =>
        rule.Margin(
            Invariant($"{marginTop}rem"),
            Invariant($"{marginRight}rem"),
            Invariant($"{marginBottom}rem"),
            Invariant($"{marginLeft}rem")
        );

    public static CssRule MarginPercent(
        this CssRule rule,
        double marginTop,
        double marginRight,
        double marginBottom,
        double marginLeft
    ) =>
        rule.Margin(
            Invariant($"{marginTop}%"),
            Invariant($"{marginRight}%"),
            Invariant($"{marginBottom}%"),
            Invariant($"{marginLeft}%")
        );

    public static CssRule MarginLeft(this CssRule rule, string margin) => rule.Set("margin-left", margin);

    public static CssRule MarginLeftPx(this CssRule rule, double margin) => rule.MarginLeft(Invariant($"{margin}px"));

    public static CssRule MarginLeftEm(this CssRule rule, double margin) => rule.MarginLeft(Invariant($"{margin}em"));

    public static CssRule MarginLeftRem(this CssRule rule, double margin) => rule.MarginLeft(Invariant($"{margin}rem"));

    public static CssRule MarginLeftPercent(this CssRule rule, double margin) =>
        rule.MarginLeft(Invariant($"{margin}%"));

    public static CssRule MarginTop(this CssRule rule, string margin) => rule.Set("margin-top", margin);

    public static CssRule MarginTopPx(this CssRule rule, double margin) => rule.MarginTop(Invariant($"{margin}px"));

    public static CssRule MarginTopEm(this CssRule rule, double margin) => rule.MarginTop(Invariant($"{margin}em"));

    public static CssRule MarginTopRem(this CssRule rule, double margin) => rule.MarginTop(Invariant($"{margin}rem"));

    public static CssRule MarginTopPercent(this CssRule rule, double margin) => rule.MarginTop(Invariant($"{margin}%"));

    public static CssRule MarginRight(this CssRule rule, string margin) => rule.Set("margin-right", margin);

    public static CssRule MarginRightPx(this CssRule rule, double margin) => rule.MarginRight(Invariant($"{margin}px"));

    public static CssRule MarginRightEm(this CssRule rule, double margin) => rule.MarginRight(Invariant($"{margin}em"));

    public static CssRule MarginRightRem(this CssRule rule, double margin) =>
        rule.MarginRight(Invariant($"{margin}rem"));

    public static CssRule MarginRightPercent(this CssRule rule, double margin) =>
        rule.MarginRight(Invariant($"{margin}%"));

    public static CssRule MarginBottom(this CssRule rule, string margin) => rule.Set("margin-bottom", margin);

    public static CssRule MarginBottomPx(this CssRule rule, double margin) =>
        rule.MarginBottom(Invariant($"{margin}px"));

    public static CssRule MarginBottomEm(this CssRule rule, double margin) =>
        rule.MarginBottom(Invariant($"{margin}em"));

    public static CssRule MarginBottomRem(this CssRule rule, double margin) =>
        rule.MarginBottom(Invariant($"{margin}rem"));

    public static CssRule MarginBottomPercent(this CssRule rule, double margin) =>
        rule.MarginBottom(Invariant($"{margin}%"));
}
