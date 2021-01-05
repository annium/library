using static System.FormattableString;

namespace Annium.Blazor.Css
{
    public static class OpacityExtensions
    {
        public static CssRule Opacity(this CssRule rule, double opacity) =>
            rule.Set("opacity", Invariant($"{opacity}"));
    }
}