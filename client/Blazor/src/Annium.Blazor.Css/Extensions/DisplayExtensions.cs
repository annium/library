// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Css;

public static class DisplayExtensions
{
    public static CssRule DisplayBlock(this CssRule rule) =>
        rule.Display("block");

    public static CssRule DisplayFlex(this CssRule rule) =>
        rule.Display("flex");

    public static CssRule DisplayInline(this CssRule rule) =>
        rule.Display("inline");

    public static CssRule DisplayInlineBlock(this CssRule rule) =>
        rule.Display("inline-block");

    public static CssRule DisplayInlineFlex(this CssRule rule) =>
        rule.Display("inline-flex");

    private static CssRule Display(this CssRule rule, string display) =>
        rule.Set("display", display);
}