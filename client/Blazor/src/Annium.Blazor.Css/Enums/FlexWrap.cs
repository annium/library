using Annium.Blazor.Css.Internal;

namespace Annium.Blazor.Css
{
    public class FlexWrap : ImplicitString<FlexWrap>
    {
        public static readonly FlexWrap NoWrap = new FlexWrap("nowrap");
        public static readonly FlexWrap Wrap = new FlexWrap("wrap");
        public static readonly FlexWrap WrapReverse = new FlexWrap("wrap-reverse");

        private FlexWrap(string type) : base(type)
        {
        }
    }
}