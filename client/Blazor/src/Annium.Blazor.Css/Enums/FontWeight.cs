using Annium.Blazor.Css.Internal;

namespace Annium.Blazor.Css
{
    public class FontWeight : ImplicitString<FontWeight>
    {
        public static readonly FontWeight W100 = new FontWeight("100");
        public static readonly FontWeight W200 = new FontWeight("200");
        public static readonly FontWeight W300 = new FontWeight("300");
        public static readonly FontWeight W400 = new FontWeight("400");
        public static readonly FontWeight W500 = new FontWeight("500");
        public static readonly FontWeight W600 = new FontWeight("600");
        public static readonly FontWeight W700 = new FontWeight("700");
        public static readonly FontWeight W800 = new FontWeight("800");
        public static readonly FontWeight W900 = new FontWeight("900");

        private FontWeight(string type) : base(type)
        {
        }
    }
}