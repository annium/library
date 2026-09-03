using static System.FormattableString;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Provides extension methods for setting border-radius CSS properties.
/// </summary>
public static class BorderRadiusExtensions
{
    /// <summary>
    /// Sets the border-radius CSS property with a string value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value to set.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadius(this CssRule rule, string borderRadius) =>
        rule.Set("border-radius", borderRadius);

    /// <summary>
    /// Sets the border-radius CSS property with a pixel value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value in pixels.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusPx(this CssRule rule, double borderRadius) =>
        rule.BorderRadius(Invariant($"{borderRadius}px"));

    /// <summary>
    /// Sets the border-radius CSS property with an em value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value in em units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusEm(this CssRule rule, double borderRadius) =>
        rule.BorderRadius(Invariant($"{borderRadius}em"));

    /// <summary>
    /// Sets the border-radius CSS property with a rem value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value in rem units.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusRem(this CssRule rule, double borderRadius) =>
        rule.BorderRadius(Invariant($"{borderRadius}rem"));

    /// <summary>
    /// Sets the border-radius CSS property with a percentage value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value as a percentage.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusPercent(this CssRule rule, double borderRadius) =>
        rule.BorderRadius(Invariant($"{borderRadius}%"));

    /// <summary>
    /// Sets the border-radius CSS property with separate values for opposite corners.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadiusTopLeftBottomRight">The border radius value for top-left and bottom-right corners.</param>
    /// <param name="borderRadiusTopRightBottomLeft">The border radius value for top-right and bottom-left corners.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadius(
        this CssRule rule,
        string borderRadiusTopLeftBottomRight,
        string borderRadiusTopRightBottomLeft
    ) => rule.Set("border-radius", Invariant($"{borderRadiusTopLeftBottomRight} {borderRadiusTopRightBottomLeft}"));

    /// <summary>
    /// Sets the border-radius CSS property with pixel values for opposite corners.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadiusTopLeftBottomRight">The border radius value in pixels for top-left and bottom-right corners.</param>
    /// <param name="borderRadiusTopRightBottomLeft">The border radius value in pixels for top-right and bottom-left corners.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusPx(
        this CssRule rule,
        double borderRadiusTopLeftBottomRight,
        double borderRadiusTopRightBottomLeft
    ) =>
        rule.BorderRadius(
            Invariant($"{borderRadiusTopLeftBottomRight}px"),
            Invariant($"{borderRadiusTopRightBottomLeft}px")
        );

    /// <summary>
    /// Sets the border-radius CSS property with em values for opposite corners.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadiusTopLeftBottomRight">The border radius value in em units for top-left and bottom-right corners.</param>
    /// <param name="borderRadiusTopRightBottomLeft">The border radius value in em units for top-right and bottom-left corners.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusEm(
        this CssRule rule,
        double borderRadiusTopLeftBottomRight,
        double borderRadiusTopRightBottomLeft
    ) =>
        rule.BorderRadius(
            Invariant($"{borderRadiusTopLeftBottomRight}em"),
            Invariant($"{borderRadiusTopRightBottomLeft}em")
        );

    /// <summary>
    /// Sets the border-radius CSS property with rem values for opposite corners.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadiusTopLeftBottomRight">The border radius value in rem units for top-left and bottom-right corners.</param>
    /// <param name="borderRadiusTopRightBottomLeft">The border radius value in rem units for top-right and bottom-left corners.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusRem(
        this CssRule rule,
        double borderRadiusTopLeftBottomRight,
        double borderRadiusTopRightBottomLeft
    ) =>
        rule.BorderRadius(
            Invariant($"{borderRadiusTopLeftBottomRight}rem"),
            Invariant($"{borderRadiusTopRightBottomLeft}rem")
        );

    /// <summary>
    /// Sets the border-radius CSS property with percentage values for opposite corners.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadiusTopLeftBottomRight">The border radius value as a percentage for top-left and bottom-right corners.</param>
    /// <param name="borderRadiusTopRightBottomLeft">The border radius value as a percentage for top-right and bottom-left corners.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusPercent(
        this CssRule rule,
        double borderRadiusTopLeftBottomRight,
        double borderRadiusTopRightBottomLeft
    ) =>
        rule.BorderRadius(
            Invariant($"{borderRadiusTopLeftBottomRight}%"),
            Invariant($"{borderRadiusTopRightBottomLeft}%")
        );

    /// <summary>
    /// Sets the border-radius CSS property with three values (top-left, top-right/bottom-left, bottom-right).
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadiusTopLeft">The border radius value for the top-left corner.</param>
    /// <param name="borderRadiusTopRightBottomLeft">The border radius value for the top-right and bottom-left corners.</param>
    /// <param name="borderRadiusBottomRight">The border radius value for the bottom-right corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadius(
        this CssRule rule,
        string borderRadiusTopLeft,
        string borderRadiusTopRightBottomLeft,
        string borderRadiusBottomRight
    ) =>
        rule.Set(
            "border-radius",
            Invariant($"{borderRadiusTopLeft} {borderRadiusTopRightBottomLeft} {borderRadiusBottomRight}")
        );

    /// <summary>
    /// Sets the border-radius CSS property with three pixel values (top-left, top-right/bottom-left, bottom-right).
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadiusTopLeft">The border radius value in pixels for the top-left corner.</param>
    /// <param name="borderRadiusTopRightBottomLeft">The border radius value in pixels for the top-right and bottom-left corners.</param>
    /// <param name="borderRadiusBottomRight">The border radius value in pixels for the bottom-right corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusPx(
        this CssRule rule,
        double borderRadiusTopLeft,
        double borderRadiusTopRightBottomLeft,
        double borderRadiusBottomRight
    ) =>
        rule.BorderRadius(
            Invariant($"{borderRadiusTopLeft}px"),
            Invariant($"{borderRadiusTopRightBottomLeft}px"),
            Invariant($"{borderRadiusBottomRight}px")
        );

    /// <summary>
    /// Sets the border-radius CSS property with three em values (top-left, top-right/bottom-left, bottom-right).
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadiusTopLeft">The border radius value in em units for the top-left corner.</param>
    /// <param name="borderRadiusTopRightBottomLeft">The border radius value in em units for the top-right and bottom-left corners.</param>
    /// <param name="borderRadiusBottomRight">The border radius value in em units for the bottom-right corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusEm(
        this CssRule rule,
        double borderRadiusTopLeft,
        double borderRadiusTopRightBottomLeft,
        double borderRadiusBottomRight
    ) =>
        rule.BorderRadius(
            Invariant($"{borderRadiusTopLeft}em"),
            Invariant($"{borderRadiusTopRightBottomLeft}em"),
            Invariant($"{borderRadiusBottomRight}em")
        );

    /// <summary>
    /// Sets the border-radius CSS property with three rem values (top-left, top-right/bottom-left, bottom-right).
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadiusTopLeft">The border radius value in rem units for the top-left corner.</param>
    /// <param name="borderRadiusTopRightBottomLeft">The border radius value in rem units for the top-right and bottom-left corners.</param>
    /// <param name="borderRadiusBottomRight">The border radius value in rem units for the bottom-right corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusRem(
        this CssRule rule,
        double borderRadiusTopLeft,
        double borderRadiusTopRightBottomLeft,
        double borderRadiusBottomRight
    ) =>
        rule.BorderRadius(
            Invariant($"{borderRadiusTopLeft}rem"),
            Invariant($"{borderRadiusTopRightBottomLeft}rem"),
            Invariant($"{borderRadiusBottomRight}rem")
        );

    /// <summary>
    /// Sets the border-radius CSS property with three percentage values (top-left, top-right/bottom-left, bottom-right).
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadiusTopLeft">The border radius value as a percentage for the top-left corner.</param>
    /// <param name="borderRadiusTopRightBottomLeft">The border radius value as a percentage for the top-right and bottom-left corners.</param>
    /// <param name="borderRadiusBottomRight">The border radius value as a percentage for the bottom-right corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusPercent(
        this CssRule rule,
        double borderRadiusTopLeft,
        double borderRadiusTopRightBottomLeft,
        double borderRadiusBottomRight
    ) =>
        rule.BorderRadius(
            Invariant($"{borderRadiusTopLeft}%"),
            Invariant($"{borderRadiusTopRightBottomLeft}%"),
            Invariant($"{borderRadiusBottomRight}%")
        );

    /// <summary>
    /// Sets the border-radius CSS property with four individual values for each corner.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadiusTopLeft">The border radius value for the top-left corner.</param>
    /// <param name="borderRadiusTopRight">The border radius value for the top-right corner.</param>
    /// <param name="borderRadiusBottomRight">The border radius value for the bottom-right corner.</param>
    /// <param name="borderRadiusBottomLeft">The border radius value for the bottom-left corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadius(
        this CssRule rule,
        string borderRadiusTopLeft,
        string borderRadiusTopRight,
        string borderRadiusBottomRight,
        string borderRadiusBottomLeft
    ) =>
        rule.Set(
            "border-radius",
            Invariant(
                $"{borderRadiusTopLeft} {borderRadiusTopRight} {borderRadiusBottomRight} {borderRadiusBottomLeft}"
            )
        );

    /// <summary>
    /// Sets the border-radius CSS property with four pixel values for each corner.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadiusTopLeft">The border radius value in pixels for the top-left corner.</param>
    /// <param name="borderRadiusTopRight">The border radius value in pixels for the top-right corner.</param>
    /// <param name="borderRadiusBottomRight">The border radius value in pixels for the bottom-right corner.</param>
    /// <param name="borderRadiusBottomLeft">The border radius value in pixels for the bottom-left corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusPx(
        this CssRule rule,
        double borderRadiusTopLeft,
        double borderRadiusTopRight,
        double borderRadiusBottomRight,
        double borderRadiusBottomLeft
    ) =>
        rule.BorderRadius(
            Invariant($"{borderRadiusTopLeft}px"),
            Invariant($"{borderRadiusTopRight}px"),
            Invariant($"{borderRadiusBottomRight}px"),
            Invariant($"{borderRadiusBottomLeft}px")
        );

    /// <summary>
    /// Sets the border-radius CSS property with four em values for each corner.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadiusTopLeft">The border radius value in em units for the top-left corner.</param>
    /// <param name="borderRadiusTopRight">The border radius value in em units for the top-right corner.</param>
    /// <param name="borderRadiusBottomRight">The border radius value in em units for the bottom-right corner.</param>
    /// <param name="borderRadiusBottomLeft">The border radius value in em units for the bottom-left corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusEm(
        this CssRule rule,
        double borderRadiusTopLeft,
        double borderRadiusTopRight,
        double borderRadiusBottomRight,
        double borderRadiusBottomLeft
    ) =>
        rule.BorderRadius(
            Invariant($"{borderRadiusTopLeft}em"),
            Invariant($"{borderRadiusTopRight}em"),
            Invariant($"{borderRadiusBottomRight}em"),
            Invariant($"{borderRadiusBottomLeft}em")
        );

    /// <summary>
    /// Sets the border-radius CSS property with four rem values for each corner.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadiusTopLeft">The border radius value in rem units for the top-left corner.</param>
    /// <param name="borderRadiusTopRight">The border radius value in rem units for the top-right corner.</param>
    /// <param name="borderRadiusBottomRight">The border radius value in rem units for the bottom-right corner.</param>
    /// <param name="borderRadiusBottomLeft">The border radius value in rem units for the bottom-left corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusRem(
        this CssRule rule,
        double borderRadiusTopLeft,
        double borderRadiusTopRight,
        double borderRadiusBottomRight,
        double borderRadiusBottomLeft
    ) =>
        rule.BorderRadius(
            Invariant($"{borderRadiusTopLeft}rem"),
            Invariant($"{borderRadiusTopRight}rem"),
            Invariant($"{borderRadiusBottomRight}rem"),
            Invariant($"{borderRadiusBottomLeft}rem")
        );

    /// <summary>
    /// Sets the border-radius CSS property with four percentage values for each corner.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadiusTopLeft">The border radius value as a percentage for the top-left corner.</param>
    /// <param name="borderRadiusTopRight">The border radius value as a percentage for the top-right corner.</param>
    /// <param name="borderRadiusBottomRight">The border radius value as a percentage for the bottom-right corner.</param>
    /// <param name="borderRadiusBottomLeft">The border radius value as a percentage for the bottom-left corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusPercent(
        this CssRule rule,
        double borderRadiusTopLeft,
        double borderRadiusTopRight,
        double borderRadiusBottomRight,
        double borderRadiusBottomLeft
    ) =>
        rule.BorderRadius(
            Invariant($"{borderRadiusTopLeft}%"),
            Invariant($"{borderRadiusTopRight}%"),
            Invariant($"{borderRadiusBottomRight}%"),
            Invariant($"{borderRadiusBottomLeft}%")
        );

    /// <summary>
    /// Sets the border-top-left-radius CSS property.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value for the top-left corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusTopLeft(this CssRule rule, string borderRadius) =>
        rule.Set("border-top-left-radius", borderRadius);

    /// <summary>
    /// Sets the border-top-left-radius CSS property with a pixel value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value in pixels for the top-left corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusTopLeftPx(this CssRule rule, double borderRadius) =>
        rule.BorderRadiusTopLeft(Invariant($"{borderRadius}px"));

    /// <summary>
    /// Sets the border-top-left-radius CSS property with an em value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value in em units for the top-left corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusTopLeftEm(this CssRule rule, double borderRadius) =>
        rule.BorderRadiusTopLeft(Invariant($"{borderRadius}em"));

    /// <summary>
    /// Sets the border-top-left-radius CSS property with a rem value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value in rem units for the top-left corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusTopLeftRem(this CssRule rule, double borderRadius) =>
        rule.BorderRadiusTopLeft(Invariant($"{borderRadius}rem"));

    /// <summary>
    /// Sets the border-top-left-radius CSS property with a percentage value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value as a percentage for the top-left corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusTopLeftPercent(this CssRule rule, double borderRadius) =>
        rule.BorderRadiusTopLeft(Invariant($"{borderRadius}%"));

    /// <summary>
    /// Sets the border-top-right-radius CSS property.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value for the top-right corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusTopRight(this CssRule rule, string borderRadius) =>
        rule.Set("border-top-right-radius", borderRadius);

    /// <summary>
    /// Sets the border-top-right-radius CSS property with a pixel value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value in pixels for the top-right corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusTopRightPx(this CssRule rule, double borderRadius) =>
        rule.BorderRadiusTopRight(Invariant($"{borderRadius}px"));

    /// <summary>
    /// Sets the border-top-right-radius CSS property with an em value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value in em units for the top-right corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusTopRightEm(this CssRule rule, double borderRadius) =>
        rule.BorderRadiusTopRight(Invariant($"{borderRadius}em"));

    /// <summary>
    /// Sets the border-top-right-radius CSS property with a rem value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value in rem units for the top-right corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusTopRightRem(this CssRule rule, double borderRadius) =>
        rule.BorderRadiusTopRight(Invariant($"{borderRadius}rem"));

    /// <summary>
    /// Sets the border-top-right-radius CSS property with a percentage value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value as a percentage for the top-right corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusTopRightPercent(this CssRule rule, double borderRadius) =>
        rule.BorderRadiusTopRight(Invariant($"{borderRadius}%"));

    /// <summary>
    /// Sets the border-bottom-right-radius CSS property.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value for the bottom-right corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusBottomRight(this CssRule rule, string borderRadius) =>
        rule.Set("border-bottom-right-radius", borderRadius);

    /// <summary>
    /// Sets the border-bottom-right-radius CSS property with a pixel value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value in pixels for the bottom-right corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusBottomRightPx(this CssRule rule, double borderRadius) =>
        rule.BorderRadiusBottomRight(Invariant($"{borderRadius}px"));

    /// <summary>
    /// Sets the border-bottom-right-radius CSS property with an em value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value in em units for the bottom-right corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusBottomRightEm(this CssRule rule, double borderRadius) =>
        rule.BorderRadiusBottomRight(Invariant($"{borderRadius}em"));

    /// <summary>
    /// Sets the border-bottom-right-radius CSS property with a rem value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value in rem units for the bottom-right corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusBottomRightRem(this CssRule rule, double borderRadius) =>
        rule.BorderRadiusBottomRight(Invariant($"{borderRadius}rem"));

    /// <summary>
    /// Sets the border-bottom-right-radius CSS property with a percentage value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value as a percentage for the bottom-right corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusBottomRightPercent(this CssRule rule, double borderRadius) =>
        rule.BorderRadiusBottomRight(Invariant($"{borderRadius}%"));

    /// <summary>
    /// Sets the border-bottom-left-radius CSS property.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value for the bottom-left corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusBottomLeft(this CssRule rule, string borderRadius) =>
        rule.Set("border-bottom-left-radius", borderRadius);

    /// <summary>
    /// Sets the border-bottom-left-radius CSS property with a pixel value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value in pixels for the bottom-left corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusBottomLeftPx(this CssRule rule, double borderRadius) =>
        rule.BorderRadiusBottomLeft(Invariant($"{borderRadius}px"));

    /// <summary>
    /// Sets the border-bottom-left-radius CSS property with an em value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value in em units for the bottom-left corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusBottomLeftEm(this CssRule rule, double borderRadius) =>
        rule.BorderRadiusBottomLeft(Invariant($"{borderRadius}em"));

    /// <summary>
    /// Sets the border-bottom-left-radius CSS property with a rem value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value in rem units for the bottom-left corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusBottomLeftRem(this CssRule rule, double borderRadius) =>
        rule.BorderRadiusBottomLeft(Invariant($"{borderRadius}rem"));

    /// <summary>
    /// Sets the border-bottom-left-radius CSS property with a percentage value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="borderRadius">The border radius value as a percentage for the bottom-left corner.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule BorderRadiusBottomLeftPercent(this CssRule rule, double borderRadius) =>
        rule.BorderRadiusBottomLeft(Invariant($"{borderRadius}%"));
}
