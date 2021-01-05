using static System.FormattableString;

namespace Annium.Blazor.Css
{
    public static class RightExtensions
    {
        public static CssRule Right(this CssRule rule, string right) => rule.Set("right", right);
        public static CssRule RightPx(this CssRule rule, int right) => rule.Right(Invariant($"{right}px"));
        public static CssRule RightEm(this CssRule rule, int right) => rule.Right(Invariant($"{right}em"));
        public static CssRule RightRem(this CssRule rule, int right) => rule.Right(Invariant($"{right}rem"));
        public static CssRule RightPercent(this CssRule rule, int right) => rule.Right(Invariant($"{right}%"));
    }
}