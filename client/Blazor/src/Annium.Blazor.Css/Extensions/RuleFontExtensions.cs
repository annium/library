using Annium.Blazor.Css.Internal;

namespace Annium.Blazor.Css
{
    public static class RuleFontExtensions
    {
        public static CssRule FontSize(this CssRule rule, string fontSize) => rule.Set("font-size", fontSize);
        public static CssRule FontSizePx(this CssRule rule, double fontSize) => rule.Set("font-size", $"{fontSize}px");
        public static CssRule FontSizeEm(this CssRule rule, double fontSize) => rule.Set("font-size", $"{fontSize}em");
        public static CssRule FontSizeRem(this CssRule rule, double fontSize) => rule.Set("font-size", $"{fontSize}rem");
        public static CssRule FontWeight(this CssRule rule, FontWeight fontWeight) => rule.Set("font-weight", fontWeight);
        public static CssRule FontWeightNormal(this CssRule rule) => rule.Set("font-weight", Internal.FontWeight.W400);
        public static CssRule FontWeightBold(this CssRule rule) => rule.Set("font-weight", Internal.FontWeight.W700);
    }
}