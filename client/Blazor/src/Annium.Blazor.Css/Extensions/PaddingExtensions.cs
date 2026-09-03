using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Provides extension methods for CSS padding styling.
/// </summary>
public static class PaddingExtensions
{
    /// <summary>
    /// Sets the padding property to the specified value.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The padding value as a string.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule Padding(this CssRule rule, string padding) => rule.Set("padding", padding);

    /// <summary>
    /// Sets the padding property to the specified value in pixels.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The padding value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingPx(this CssRule rule, double padding) => rule.Padding(Invariant($"{padding}px"));

    /// <summary>
    /// Sets the padding property to the specified value in em units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The padding value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingEm(this CssRule rule, double padding) => rule.Padding(Invariant($"{padding}em"));

    /// <summary>
    /// Sets the padding property to the specified value in rem units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The padding value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingRem(this CssRule rule, double padding) => rule.Padding(Invariant($"{padding}rem"));

    /// <summary>
    /// Sets the padding property to the specified value as a percentage.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The padding value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingPercent(this CssRule rule, double padding) => rule.Padding(Invariant($"{padding}%"));

    /// <summary>
    /// Sets the padding property with separate vertical and horizontal values.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="paddingVertical">The vertical padding value.</param>
    /// <param name="paddingHorizontal">The horizontal padding value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule Padding(this CssRule rule, string paddingVertical, string paddingHorizontal) =>
        rule.Set("padding", Invariant($"{paddingVertical} {paddingHorizontal}"));

    /// <summary>
    /// Sets the padding property with separate vertical and horizontal values in pixels.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="paddingVertical">The vertical padding value in pixels.</param>
    /// <param name="paddingHorizontal">The horizontal padding value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingPx(this CssRule rule, double paddingVertical, double paddingHorizontal) =>
        rule.Padding(Invariant($"{paddingVertical}px"), Invariant($"{paddingHorizontal}px"));

    /// <summary>
    /// Sets the padding property with separate vertical and horizontal values in em units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="paddingVertical">The vertical padding value in em units.</param>
    /// <param name="paddingHorizontal">The horizontal padding value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingEm(this CssRule rule, double paddingVertical, double paddingHorizontal) =>
        rule.Padding(Invariant($"{paddingVertical}em"), Invariant($"{paddingHorizontal}em"));

    /// <summary>
    /// Sets the padding property with separate vertical and horizontal values in rem units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="paddingVertical">The vertical padding value in rem units.</param>
    /// <param name="paddingHorizontal">The horizontal padding value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingRem(this CssRule rule, double paddingVertical, double paddingHorizontal) =>
        rule.Padding(Invariant($"{paddingVertical}rem"), Invariant($"{paddingHorizontal}rem"));

    /// <summary>
    /// Sets the padding property with separate vertical and horizontal values as percentages.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="paddingVertical">The vertical padding value as a percentage.</param>
    /// <param name="paddingHorizontal">The horizontal padding value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingPercent(this CssRule rule, double paddingVertical, double paddingHorizontal) =>
        rule.Padding(Invariant($"{paddingVertical}%"), Invariant($"{paddingHorizontal}%"));

    /// <summary>
    /// Sets the padding property with separate top, horizontal, and bottom values.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="paddingTop">The top padding value.</param>
    /// <param name="paddingHorizontal">The horizontal padding value.</param>
    /// <param name="paddingBottom">The bottom padding value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule Padding(
        this CssRule rule,
        string paddingTop,
        string paddingHorizontal,
        string paddingBottom
    ) => rule.Set("padding", Invariant($"{paddingTop} {paddingHorizontal} {paddingBottom}"));

    /// <summary>
    /// Sets the padding property with separate top, horizontal, and bottom values in pixels.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="paddingTop">The top padding value in pixels.</param>
    /// <param name="paddingHorizontal">The horizontal padding value in pixels.</param>
    /// <param name="paddingBottom">The bottom padding value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingPx(
        this CssRule rule,
        double paddingTop,
        double paddingHorizontal,
        double paddingBottom
    ) =>
        rule.Padding(
            Invariant($"{paddingTop}px"),
            Invariant($"{paddingHorizontal}px"),
            Invariant($"{paddingBottom}px")
        );

    /// <summary>
    /// Sets the padding property with separate top, horizontal, and bottom values in em units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="paddingTop">The top padding value in em units.</param>
    /// <param name="paddingHorizontal">The horizontal padding value in em units.</param>
    /// <param name="paddingBottom">The bottom padding value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingEm(
        this CssRule rule,
        double paddingTop,
        double paddingHorizontal,
        double paddingBottom
    ) =>
        rule.Padding(
            Invariant($"{paddingTop}em"),
            Invariant($"{paddingHorizontal}em"),
            Invariant($"{paddingBottom}em")
        );

    /// <summary>
    /// Sets the padding property with separate top, horizontal, and bottom values in rem units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="paddingTop">The top padding value in rem units.</param>
    /// <param name="paddingHorizontal">The horizontal padding value in rem units.</param>
    /// <param name="paddingBottom">The bottom padding value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingRem(
        this CssRule rule,
        double paddingTop,
        double paddingHorizontal,
        double paddingBottom
    ) =>
        rule.Padding(
            Invariant($"{paddingTop}rem"),
            Invariant($"{paddingHorizontal}rem"),
            Invariant($"{paddingBottom}rem")
        );

    /// <summary>
    /// Sets the padding property with separate top, horizontal, and bottom values as percentages.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="paddingTop">The top padding value as a percentage.</param>
    /// <param name="paddingHorizontal">The horizontal padding value as a percentage.</param>
    /// <param name="paddingBottom">The bottom padding value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingPercent(
        this CssRule rule,
        double paddingTop,
        double paddingHorizontal,
        double paddingBottom
    ) => rule.Padding(Invariant($"{paddingTop}%"), Invariant($"{paddingHorizontal}%"), Invariant($"{paddingBottom}%"));

    /// <summary>
    /// Sets the padding property with separate values for all four sides.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="paddingTop">The top padding value.</param>
    /// <param name="paddingRight">The right padding value.</param>
    /// <param name="paddingBottom">The bottom padding value.</param>
    /// <param name="paddingLeft">The left padding value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule Padding(
        this CssRule rule,
        string paddingTop,
        string paddingRight,
        string paddingBottom,
        string paddingLeft
    ) => rule.Set("padding", Invariant($"{paddingTop} {paddingRight} {paddingBottom} {paddingLeft}"));

    /// <summary>
    /// Sets the padding property with separate values for all four sides in pixels.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="paddingTop">The top padding value in pixels.</param>
    /// <param name="paddingRight">The right padding value in pixels.</param>
    /// <param name="paddingBottom">The bottom padding value in pixels.</param>
    /// <param name="paddingLeft">The left padding value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingPx(
        this CssRule rule,
        double paddingTop,
        double paddingRight,
        double paddingBottom,
        double paddingLeft
    ) =>
        rule.Padding(
            Invariant($"{paddingTop}px"),
            Invariant($"{paddingRight}px"),
            Invariant($"{paddingBottom}px"),
            Invariant($"{paddingLeft}px")
        );

    /// <summary>
    /// Sets the padding property with separate values for all four sides in em units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="paddingTop">The top padding value in em units.</param>
    /// <param name="paddingRight">The right padding value in em units.</param>
    /// <param name="paddingBottom">The bottom padding value in em units.</param>
    /// <param name="paddingLeft">The left padding value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingEm(
        this CssRule rule,
        double paddingTop,
        double paddingRight,
        double paddingBottom,
        double paddingLeft
    ) =>
        rule.Padding(
            Invariant($"{paddingTop}em"),
            Invariant($"{paddingRight}em"),
            Invariant($"{paddingBottom}em"),
            Invariant($"{paddingLeft}em")
        );

    /// <summary>
    /// Sets the padding property with separate values for all four sides in rem units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="paddingTop">The top padding value in rem units.</param>
    /// <param name="paddingRight">The right padding value in rem units.</param>
    /// <param name="paddingBottom">The bottom padding value in rem units.</param>
    /// <param name="paddingLeft">The left padding value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingRem(
        this CssRule rule,
        double paddingTop,
        double paddingRight,
        double paddingBottom,
        double paddingLeft
    ) =>
        rule.Padding(
            Invariant($"{paddingTop}rem"),
            Invariant($"{paddingRight}rem"),
            Invariant($"{paddingBottom}rem"),
            Invariant($"{paddingLeft}rem")
        );

    /// <summary>
    /// Sets the padding property with separate values for all four sides as percentages.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="paddingTop">The top padding value as a percentage.</param>
    /// <param name="paddingRight">The right padding value as a percentage.</param>
    /// <param name="paddingBottom">The bottom padding value as a percentage.</param>
    /// <param name="paddingLeft">The left padding value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingPercent(
        this CssRule rule,
        double paddingTop,
        double paddingRight,
        double paddingBottom,
        double paddingLeft
    ) =>
        rule.Padding(
            Invariant($"{paddingTop}%"),
            Invariant($"{paddingRight}%"),
            Invariant($"{paddingBottom}%"),
            Invariant($"{paddingLeft}%")
        );

    /// <summary>
    /// Sets the padding-left property to the specified value.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The left padding value as a string.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingLeft(this CssRule rule, string padding) => rule.Set("padding-left", padding);

    /// <summary>
    /// Sets the padding-left property to the specified value in pixels.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The left padding value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingLeftPx(this CssRule rule, double padding) =>
        rule.PaddingLeft(Invariant($"{padding}px"));

    /// <summary>
    /// Sets the padding-left property to the specified value in em units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The left padding value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingLeftEm(this CssRule rule, double padding) =>
        rule.PaddingLeft(Invariant($"{padding}em"));

    /// <summary>
    /// Sets the padding-left property to the specified value in rem units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The left padding value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingLeftRem(this CssRule rule, double padding) =>
        rule.PaddingLeft(Invariant($"{padding}rem"));

    /// <summary>
    /// Sets the padding-left property to the specified value as a percentage.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The left padding value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingLeftPercent(this CssRule rule, double padding) =>
        rule.PaddingLeft(Invariant($"{padding}%"));

    /// <summary>
    /// Sets the padding-top property to the specified value.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The top padding value as a string.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingTop(this CssRule rule, string padding) => rule.Set("padding-top", padding);

    /// <summary>
    /// Sets the padding-top property to the specified value in pixels.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The top padding value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingTopPx(this CssRule rule, double padding) => rule.PaddingTop(Invariant($"{padding}px"));

    /// <summary>
    /// Sets the padding-top property to the specified value in em units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The top padding value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingTopEm(this CssRule rule, double padding) => rule.PaddingTop(Invariant($"{padding}em"));

    /// <summary>
    /// Sets the padding-top property to the specified value in rem units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The top padding value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingTopRem(this CssRule rule, double padding) =>
        rule.PaddingTop(Invariant($"{padding}rem"));

    /// <summary>
    /// Sets the padding-top property to the specified value as a percentage.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The top padding value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingTopPercent(this CssRule rule, double padding) =>
        rule.PaddingTop(Invariant($"{padding}%"));

    /// <summary>
    /// Sets the padding-right property to the specified value.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The right padding value as a string.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingRight(this CssRule rule, string padding) => rule.Set("padding-right", padding);

    /// <summary>
    /// Sets the padding-right property to the specified value in pixels.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The right padding value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingRightPx(this CssRule rule, double padding) =>
        rule.PaddingRight(Invariant($"{padding}px"));

    /// <summary>
    /// Sets the padding-right property to the specified value in em units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The right padding value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingRightEm(this CssRule rule, double padding) =>
        rule.PaddingRight(Invariant($"{padding}em"));

    /// <summary>
    /// Sets the padding-right property to the specified value in rem units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The right padding value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingRightRem(this CssRule rule, double padding) =>
        rule.PaddingRight(Invariant($"{padding}rem"));

    /// <summary>
    /// Sets the padding-right property to the specified value as a percentage.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The right padding value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingRightPercent(this CssRule rule, double padding) =>
        rule.PaddingRight(Invariant($"{padding}%"));

    /// <summary>
    /// Sets the padding-bottom property to the specified value.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The bottom padding value as a string.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingBottom(this CssRule rule, string padding) => rule.Set("padding-bottom", padding);

    /// <summary>
    /// Sets the padding-bottom property to the specified value in pixels.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The bottom padding value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingBottomPx(this CssRule rule, double padding) =>
        rule.PaddingBottom(Invariant($"{padding}px"));

    /// <summary>
    /// Sets the padding-bottom property to the specified value in em units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The bottom padding value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingBottomEm(this CssRule rule, double padding) =>
        rule.PaddingBottom(Invariant($"{padding}em"));

    /// <summary>
    /// Sets the padding-bottom property to the specified value in rem units.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The bottom padding value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingBottomRem(this CssRule rule, double padding) =>
        rule.PaddingBottom(Invariant($"{padding}rem"));

    /// <summary>
    /// Sets the padding-bottom property to the specified value as a percentage.
    /// </summary>
    /// <param name="rule">The CSS rule to apply the padding to.</param>
    /// <param name="padding">The bottom padding value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule PaddingBottomPercent(this CssRule rule, double padding) =>
        rule.PaddingBottom(Invariant($"{padding}%"));
}
