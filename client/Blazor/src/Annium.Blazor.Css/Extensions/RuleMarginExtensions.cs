namespace Annium.Blazor.Css
{
    public static class RuleMarginExtensions
    {
        public static CssRule Margin(this CssRule rule, string margin) =>
            rule.Set("margin", margin);

        public static CssRule MarginPx(this CssRule rule, double margin) =>
            rule.Margin($"{margin}px");

        public static CssRule MarginEm(this CssRule rule, double margin) =>
            rule.Margin($"{margin}em");

        public static CssRule MarginRem(this CssRule rule, double margin) =>
            rule.Margin($"{margin}rem");

        public static CssRule MarginPercent(this CssRule rule, double margin) =>
            rule.Margin($"{margin}%");

        public static CssRule Margin(this CssRule rule, string marginVertical, string marginHorizontal)
            => rule.Set("margin", $"{marginVertical} {marginHorizontal}");

        public static CssRule MarginPx(this CssRule rule, double marginVertical, double marginHorizontal) =>
            rule.Margin($"{marginVertical}px", $"{marginHorizontal}px");

        public static CssRule MarginEm(this CssRule rule, double marginVertical, double marginHorizontal) =>
            rule.Margin($"{marginVertical}em", $"{marginHorizontal}em");

        public static CssRule MarginRem(this CssRule rule, double marginVertical, double marginHorizontal) =>
            rule.Margin($"{marginVertical}rem", $"{marginHorizontal}rem");

        public static CssRule MarginPercent(this CssRule rule, double marginVertical, double marginHorizontal) =>
            rule.Margin($"{marginVertical}%", $"{marginHorizontal}%");

        public static CssRule Margin(this CssRule rule, string marginTop, string marginHorizontal, string marginBottom) =>
            rule.Set("margin", $"{marginTop} {marginHorizontal} {marginBottom}");

        public static CssRule MarginPx(this CssRule rule, double marginTop, double marginHorizontal, double marginBottom) =>
            rule.Margin($"{marginTop}px", $"{marginHorizontal}px", $"{marginBottom}px");

        public static CssRule MarginEm(this CssRule rule, double marginTop, double marginHorizontal, double marginBottom) =>
            rule.Margin($"{marginTop}em", $"{marginHorizontal}em", $"{marginBottom}em");

        public static CssRule MarginRem(this CssRule rule, double marginTop, double marginHorizontal, double marginBottom) =>
            rule.Margin($"{marginTop}rem", $"{marginHorizontal}rem", $"{marginBottom}rem");

        public static CssRule MarginPercent(this CssRule rule, double marginTop, double marginHorizontal, double marginBottom) =>
            rule.Margin($"{marginTop}%", $"{marginHorizontal}%", $"{marginBottom}%");

        public static CssRule Margin(this CssRule rule, string marginTop, string marginRight, string marginBottom, string marginLeft)
            => rule.Set("margin", $"{marginTop} {marginRight} {marginBottom} {marginLeft}");

        public static CssRule MarginPx(this CssRule rule, double marginTop, double marginRight, double marginBottom, double marginLeft) =>
            rule.Margin($"{marginTop}px", $"{marginRight}px", $"{marginBottom}px", $"{marginLeft}px");

        public static CssRule MarginEm(this CssRule rule, double marginTop, double marginRight, double marginBottom, double marginLeft) =>
            rule.Margin($"{marginTop}em", $"{marginRight}em", $"{marginBottom}em", $"{marginLeft}em");

        public static CssRule MarginRem(this CssRule rule, double marginTop, double marginRight, double marginBottom, double marginLeft) =>
            rule.Margin($"{marginTop}rem", $"{marginRight}rem", $"{marginBottom}rem", $"{marginLeft}rem");

        public static CssRule MarginPercent(this CssRule rule, double marginTop, double marginRight, double marginBottom, double marginLeft) =>
            rule.Margin($"{marginTop}%", $"{marginRight}%", $"{marginBottom}%", $"{marginLeft}%");

        public static CssRule MarginLeft(this CssRule rule, string margin) =>
            rule.Set("margin-left", margin);

        public static CssRule MarginLeftPx(this CssRule rule, double margin) =>
            rule.MarginLeft($"{margin}px");

        public static CssRule MarginLeftEm(this CssRule rule, double margin) =>
            rule.MarginLeft($"{margin}em");

        public static CssRule MarginLeftRem(this CssRule rule, double margin) =>
            rule.MarginLeft($"{margin}rem");

        public static CssRule MarginLeftPercent(this CssRule rule, double margin) =>
            rule.MarginLeft($"{margin}%");

        public static CssRule MarginTop(this CssRule rule, string margin) =>
            rule.Set("margin-top", margin);

        public static CssRule MarginTopPx(this CssRule rule, double margin) =>
            rule.MarginTop($"{margin}px");

        public static CssRule MarginTopEm(this CssRule rule, double margin) =>
            rule.MarginTop($"{margin}em");

        public static CssRule MarginTopRem(this CssRule rule, double margin) =>
            rule.MarginTop($"{margin}rem");

        public static CssRule MarginTopPercent(this CssRule rule, double margin) =>
            rule.MarginTop($"{margin}%");

        public static CssRule MarginRight(this CssRule rule, string margin) =>
            rule.Set("margin-right", margin);

        public static CssRule MarginRightPx(this CssRule rule, double margin) =>
            rule.MarginRight($"{margin}px");

        public static CssRule MarginRightEm(this CssRule rule, double margin) =>
            rule.MarginRight($"{margin}em");

        public static CssRule MarginRightRem(this CssRule rule, double margin) =>
            rule.MarginRight($"{margin}rem");

        public static CssRule MarginRightPercent(this CssRule rule, double margin) =>
            rule.MarginRight($"{margin}%");

        public static CssRule MarginBottom(this CssRule rule, string margin) =>
            rule.Set("margin-bottom", margin);

        public static CssRule MarginBottomPx(this CssRule rule, double margin) =>
            rule.MarginBottom($"{margin}px");

        public static CssRule MarginBottomEm(this CssRule rule, double margin) =>
            rule.MarginBottom($"{margin}em");

        public static CssRule MarginBottomRem(this CssRule rule, double margin) =>
            rule.MarginBottom($"{margin}rem");

        public static CssRule MarginBottomPercent(this CssRule rule, double margin) =>
            rule.MarginBottom($"{margin}%");
    }
}