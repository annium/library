// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Css;

/// <summary>
/// Provides extension methods for CSS flexbox properties.
/// </summary>
public static class FlexboxExtensions
{
    /// <summary>
    /// Sets up a flexbox container with row direction.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="alignItems">The align-items value.</param>
    /// <param name="justifyContent">The justify-content value.</param>
    /// <param name="inline">Whether to use inline-flex instead of flex display.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule FlexRow(
        this CssRule rule,
        AlignItems alignItems,
        JustifyContent justifyContent,
        bool inline = false
    ) => rule.FlexBox(Css.FlexDirection.Row, alignItems, justifyContent, inline);

    /// <summary>
    /// Sets up a flexbox container with column direction.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="alignItems">The align-items value.</param>
    /// <param name="justifyContent">The justify-content value.</param>
    /// <param name="inline">Whether to use inline-flex instead of flex display.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule FlexColumn(
        this CssRule rule,
        AlignItems alignItems,
        JustifyContent justifyContent,
        bool inline = false
    ) => rule.FlexBox(Css.FlexDirection.Column, alignItems, justifyContent, inline);

    /// <summary>
    /// Sets up a flexbox container with row-reverse direction.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="alignItems">The align-items value.</param>
    /// <param name="justifyContent">The justify-content value.</param>
    /// <param name="inline">Whether to use inline-flex instead of flex display.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule FlexRowReverse(
        this CssRule rule,
        AlignItems alignItems,
        JustifyContent justifyContent,
        bool inline = false
    ) => rule.FlexBox(Css.FlexDirection.RowReverse, alignItems, justifyContent, inline);

    /// <summary>
    /// Sets up a flexbox container with column-reverse direction.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="alignItems">The align-items value.</param>
    /// <param name="justifyContent">The justify-content value.</param>
    /// <param name="inline">Whether to use inline-flex instead of flex display.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule FlexColumnReverse(
        this CssRule rule,
        AlignItems alignItems,
        JustifyContent justifyContent,
        bool inline = false
    ) => rule.FlexBox(Css.FlexDirection.ColumnReverse, alignItems, justifyContent, inline);

    /// <summary>
    /// Sets the flex property with the same grow and shrink values.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="growAndShrink">The value to use for both flex-grow and flex-shrink.</param>
    /// <param name="basis">The flex-basis value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule Flex(this CssRule rule, int growAndShrink, string basis = "auto") =>
        rule.FlexGrow(growAndShrink).FlexShrink(growAndShrink).FlexBasis(basis);

    /// <summary>
    /// Sets the flex property with separate grow and shrink values.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="grow">The flex-grow value.</param>
    /// <param name="shrink">The flex-shrink value.</param>
    /// <param name="basis">The flex-basis value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule Flex(this CssRule rule, int grow, int shrink, string basis = "auto") =>
        rule.FlexGrow(grow).FlexShrink(shrink).FlexBasis(basis);

    /// <summary>
    /// Sets the flex-direction property.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="direction">The flex direction value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule FlexDirection(this CssRule rule, FlexDirection direction) =>
        rule.Set("flex-direction", direction);

    /// <summary>
    /// Sets the align-items property.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="alignItems">The align-items value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule AlignItems(this CssRule rule, AlignItems alignItems) => rule.Set("align-items", alignItems);

    /// <summary>
    /// Sets the align-self property.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="alignItems">The align-self value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule AlignSelf(this CssRule rule, AlignItems alignItems) => rule.Set("align-self", alignItems);

    /// <summary>
    /// Sets the justify-content property.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="justifyContent">The justify-content value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule JustifyContent(this CssRule rule, JustifyContent justifyContent) =>
        rule.Set("justify-content", justifyContent);

    /// <summary>
    /// Sets the flex-grow property.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="grow">The flex-grow value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule FlexGrow(this CssRule rule, int grow) => rule.Set("flex-grow", $"{grow}");

    /// <summary>
    /// Sets the flex-shrink property.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="shrink">The flex-shrink value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule FlexShrink(this CssRule rule, int shrink) => rule.Set("flex-shrink", $"{shrink}");

    /// <summary>
    /// Sets the flex-basis property with a string value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="basis">The flex-basis value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule FlexBasis(this CssRule rule, string basis) => rule.Set("flex-basis", basis);

    /// <summary>
    /// Sets the flex-basis property with an integer value.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="basis">The flex-basis value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule FlexBasis(this CssRule rule, int basis) => rule.Set("flex-basis", $"{basis}");

    /// <summary>
    /// Sets the flex-basis property (note: method name suggests flex-wrap but sets flex-basis).
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="basis">The flex-basis value.</param>
    /// <returns>The modified CSS rule.</returns>
    public static CssRule FlexWrap(this CssRule rule, int basis) => rule.Set("flex-basis", $"{basis}");

    /// <summary>
    /// Configures a flexbox container with the specified properties.
    /// </summary>
    /// <param name="rule">The CSS rule to modify.</param>
    /// <param name="direction">The flex direction.</param>
    /// <param name="alignItems">The align-items value.</param>
    /// <param name="justifyContent">The justify-content value.</param>
    /// <param name="inline">Whether to use inline-flex instead of flex display.</param>
    /// <returns>The modified CSS rule.</returns>
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

        return rule.FlexDirection(direction).AlignItems(alignItems).JustifyContent(justifyContent);
    }
}
