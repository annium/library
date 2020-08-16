using static System.FormattableString;
using Annium.Blazor.Css.Internal;

namespace Annium.Blazor.Css
{
    public static class RuleFontExtensions
    {
        public static CssRule FontFamily(this CssRule rule, string fontFamily) => rule.Set("font-family", fontFamily);
        public static CssRule FontSize(this CssRule rule, string fontSize) => rule.Set("font-size", fontSize);
        public static CssRule FontSizePx(this CssRule rule, double fontSize) => rule.Set("font-size", Invariant($"{fontSize}px"));
        public static CssRule FontSizeEm(this CssRule rule, double fontSize) => rule.Set("font-size", Invariant($"{fontSize}em"));
        public static CssRule FontSizeRem(this CssRule rule, double fontSize) => rule.Set("font-size", Invariant($"{fontSize}rem"));
        public static CssRule FontWeight(this CssRule rule, FontWeight fontWeight) => rule.Set("font-weight", fontWeight);
        public static CssRule FontWeightNormal(this CssRule rule) => rule.Set("font-weight", Internal.FontWeight.W400);
        public static CssRule FontWeightBold(this CssRule rule) => rule.Set("font-weight", Internal.FontWeight.W700);
    }
}