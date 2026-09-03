using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Extension methods for applying margin CSS properties to CSS rules.
/// </summary>
public static class MarginExtensions
{
    /// <summary>
    /// Sets the margin CSS property with a custom value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The margin value to apply.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule Margin(this CssRule rule, string margin) => rule.Set("margin", margin);

    /// <summary>
    /// Sets the margin CSS property with a pixel value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The margin value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginPx(this CssRule rule, double margin) => rule.Margin(Invariant($"{margin}px"));

    /// <summary>
    /// Sets the margin CSS property with an em value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The margin value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginEm(this CssRule rule, double margin) => rule.Margin(Invariant($"{margin}em"));

    /// <summary>
    /// Sets the margin CSS property with a rem value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The margin value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginRem(this CssRule rule, double margin) => rule.Margin(Invariant($"{margin}rem"));

    /// <summary>
    /// Sets the margin CSS property with a percentage value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The margin value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginPercent(this CssRule rule, double margin) => rule.Margin(Invariant($"{margin}%"));

    /// <summary>
    /// Sets the margin CSS property with separate vertical and horizontal values.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="marginVertical">The vertical margin value (top and bottom).</param>
    /// <param name="marginHorizontal">The horizontal margin value (left and right).</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule Margin(this CssRule rule, string marginVertical, string marginHorizontal) =>
        rule.Set("margin", Invariant($"{marginVertical} {marginHorizontal}"));

    /// <summary>
    /// Sets the margin CSS property with separate vertical and horizontal pixel values.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="marginVertical">The vertical margin value in pixels (top and bottom).</param>
    /// <param name="marginHorizontal">The horizontal margin value in pixels (left and right).</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginPx(this CssRule rule, double marginVertical, double marginHorizontal) =>
        rule.Margin(Invariant($"{marginVertical}px"), Invariant($"{marginHorizontal}px"));

    /// <summary>
    /// Sets the margin CSS property with separate vertical and horizontal em values.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="marginVertical">The vertical margin value in em units (top and bottom).</param>
    /// <param name="marginHorizontal">The horizontal margin value in em units (left and right).</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginEm(this CssRule rule, double marginVertical, double marginHorizontal) =>
        rule.Margin(Invariant($"{marginVertical}em"), Invariant($"{marginHorizontal}em"));

    /// <summary>
    /// Sets the margin CSS property with separate vertical and horizontal rem values.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="marginVertical">The vertical margin value in rem units (top and bottom).</param>
    /// <param name="marginHorizontal">The horizontal margin value in rem units (left and right).</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginRem(this CssRule rule, double marginVertical, double marginHorizontal) =>
        rule.Margin(Invariant($"{marginVertical}rem"), Invariant($"{marginHorizontal}rem"));

    /// <summary>
    /// Sets the margin CSS property with separate vertical and horizontal percentage values.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="marginVertical">The vertical margin value as a percentage (top and bottom).</param>
    /// <param name="marginHorizontal">The horizontal margin value as a percentage (left and right).</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginPercent(this CssRule rule, double marginVertical, double marginHorizontal) =>
        rule.Margin(Invariant($"{marginVertical}%"), Invariant($"{marginHorizontal}%"));

    /// <summary>
    /// Sets the margin CSS property with separate top, horizontal, and bottom values.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="marginTop">The top margin value.</param>
    /// <param name="marginHorizontal">The horizontal margin value (left and right).</param>
    /// <param name="marginBottom">The bottom margin value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule Margin(this CssRule rule, string marginTop, string marginHorizontal, string marginBottom) =>
        rule.Set("margin", Invariant($"{marginTop} {marginHorizontal} {marginBottom}"));

    /// <summary>
    /// Sets the margin CSS property with separate top, horizontal, and bottom pixel values.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="marginTop">The top margin value in pixels.</param>
    /// <param name="marginHorizontal">The horizontal margin value in pixels (left and right).</param>
    /// <param name="marginBottom">The bottom margin value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginPx(this CssRule rule, double marginTop, double marginHorizontal, double marginBottom) =>
        rule.Margin(Invariant($"{marginTop}px"), Invariant($"{marginHorizontal}px"), Invariant($"{marginBottom}px"));

    /// <summary>
    /// Sets the margin CSS property with separate top, horizontal, and bottom em values.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="marginTop">The top margin value in em units.</param>
    /// <param name="marginHorizontal">The horizontal margin value in em units (left and right).</param>
    /// <param name="marginBottom">The bottom margin value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginEm(this CssRule rule, double marginTop, double marginHorizontal, double marginBottom) =>
        rule.Margin(Invariant($"{marginTop}em"), Invariant($"{marginHorizontal}em"), Invariant($"{marginBottom}em"));

    /// <summary>
    /// Sets the margin CSS property with separate top, horizontal, and bottom rem values.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="marginTop">The top margin value in rem units.</param>
    /// <param name="marginHorizontal">The horizontal margin value in rem units (left and right).</param>
    /// <param name="marginBottom">The bottom margin value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginRem(
        this CssRule rule,
        double marginTop,
        double marginHorizontal,
        double marginBottom
    ) =>
        rule.Margin(Invariant($"{marginTop}rem"), Invariant($"{marginHorizontal}rem"), Invariant($"{marginBottom}rem"));

    /// <summary>
    /// Sets the margin CSS property with separate top, horizontal, and bottom percentage values.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="marginTop">The top margin value as a percentage.</param>
    /// <param name="marginHorizontal">The horizontal margin value as a percentage (left and right).</param>
    /// <param name="marginBottom">The bottom margin value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginPercent(
        this CssRule rule,
        double marginTop,
        double marginHorizontal,
        double marginBottom
    ) => rule.Margin(Invariant($"{marginTop}%"), Invariant($"{marginHorizontal}%"), Invariant($"{marginBottom}%"));

    /// <summary>
    /// Sets the margin CSS property with separate values for all four sides.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="marginTop">The top margin value.</param>
    /// <param name="marginRight">The right margin value.</param>
    /// <param name="marginBottom">The bottom margin value.</param>
    /// <param name="marginLeft">The left margin value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule Margin(
        this CssRule rule,
        string marginTop,
        string marginRight,
        string marginBottom,
        string marginLeft
    ) => rule.Set("margin", Invariant($"{marginTop} {marginRight} {marginBottom} {marginLeft}"));

    /// <summary>
    /// Sets the margin CSS property with separate pixel values for all four sides.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="marginTop">The top margin value in pixels.</param>
    /// <param name="marginRight">The right margin value in pixels.</param>
    /// <param name="marginBottom">The bottom margin value in pixels.</param>
    /// <param name="marginLeft">The left margin value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
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

    /// <summary>
    /// Sets the margin CSS property with separate em values for all four sides.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="marginTop">The top margin value in em units.</param>
    /// <param name="marginRight">The right margin value in em units.</param>
    /// <param name="marginBottom">The bottom margin value in em units.</param>
    /// <param name="marginLeft">The left margin value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
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

    /// <summary>
    /// Sets the margin CSS property with separate rem values for all four sides.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="marginTop">The top margin value in rem units.</param>
    /// <param name="marginRight">The right margin value in rem units.</param>
    /// <param name="marginBottom">The bottom margin value in rem units.</param>
    /// <param name="marginLeft">The left margin value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
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

    /// <summary>
    /// Sets the margin CSS property with separate percentage values for all four sides.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="marginTop">The top margin value as a percentage.</param>
    /// <param name="marginRight">The right margin value as a percentage.</param>
    /// <param name="marginBottom">The bottom margin value as a percentage.</param>
    /// <param name="marginLeft">The left margin value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
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

    /// <summary>
    /// Sets the margin-left CSS property with a custom value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The left margin value to apply.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginLeft(this CssRule rule, string margin) => rule.Set("margin-left", margin);

    /// <summary>
    /// Sets the margin-left CSS property with a pixel value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The left margin value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginLeftPx(this CssRule rule, double margin) => rule.MarginLeft(Invariant($"{margin}px"));

    /// <summary>
    /// Sets the margin-left CSS property with an em value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The left margin value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginLeftEm(this CssRule rule, double margin) => rule.MarginLeft(Invariant($"{margin}em"));

    /// <summary>
    /// Sets the margin-left CSS property with a rem value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The left margin value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginLeftRem(this CssRule rule, double margin) => rule.MarginLeft(Invariant($"{margin}rem"));

    /// <summary>
    /// Sets the margin-left CSS property with a percentage value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The left margin value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginLeftPercent(this CssRule rule, double margin) =>
        rule.MarginLeft(Invariant($"{margin}%"));

    /// <summary>
    /// Sets the margin-top CSS property with a custom value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The top margin value to apply.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginTop(this CssRule rule, string margin) => rule.Set("margin-top", margin);

    /// <summary>
    /// Sets the margin-top CSS property with a pixel value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The top margin value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginTopPx(this CssRule rule, double margin) => rule.MarginTop(Invariant($"{margin}px"));

    /// <summary>
    /// Sets the margin-top CSS property with an em value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The top margin value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginTopEm(this CssRule rule, double margin) => rule.MarginTop(Invariant($"{margin}em"));

    /// <summary>
    /// Sets the margin-top CSS property with a rem value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The top margin value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginTopRem(this CssRule rule, double margin) => rule.MarginTop(Invariant($"{margin}rem"));

    /// <summary>
    /// Sets the margin-top CSS property with a percentage value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The top margin value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginTopPercent(this CssRule rule, double margin) => rule.MarginTop(Invariant($"{margin}%"));

    /// <summary>
    /// Sets the margin-right CSS property with a custom value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The right margin value to apply.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginRight(this CssRule rule, string margin) => rule.Set("margin-right", margin);

    /// <summary>
    /// Sets the margin-right CSS property with a pixel value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The right margin value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginRightPx(this CssRule rule, double margin) => rule.MarginRight(Invariant($"{margin}px"));

    /// <summary>
    /// Sets the margin-right CSS property with an em value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The right margin value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginRightEm(this CssRule rule, double margin) => rule.MarginRight(Invariant($"{margin}em"));

    /// <summary>
    /// Sets the margin-right CSS property with a rem value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The right margin value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginRightRem(this CssRule rule, double margin) =>
        rule.MarginRight(Invariant($"{margin}rem"));

    /// <summary>
    /// Sets the margin-right CSS property with a percentage value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The right margin value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginRightPercent(this CssRule rule, double margin) =>
        rule.MarginRight(Invariant($"{margin}%"));

    /// <summary>
    /// Sets the margin-bottom CSS property with a custom value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The bottom margin value to apply.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginBottom(this CssRule rule, string margin) => rule.Set("margin-bottom", margin);

    /// <summary>
    /// Sets the margin-bottom CSS property with a pixel value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The bottom margin value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginBottomPx(this CssRule rule, double margin) =>
        rule.MarginBottom(Invariant($"{margin}px"));

    /// <summary>
    /// Sets the margin-bottom CSS property with an em value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The bottom margin value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginBottomEm(this CssRule rule, double margin) =>
        rule.MarginBottom(Invariant($"{margin}em"));

    /// <summary>
    /// Sets the margin-bottom CSS property with a rem value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The bottom margin value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginBottomRem(this CssRule rule, double margin) =>
        rule.MarginBottom(Invariant($"{margin}rem"));

    /// <summary>
    /// Sets the margin-bottom CSS property with a percentage value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="margin">The bottom margin value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule MarginBottomPercent(this CssRule rule, double margin) =>
        rule.MarginBottom(Invariant($"{margin}%"));
}
