using Annium.Blazor.Css.Internal;

namespace Annium.Blazor.Css;

public class FlexWrap : ImplicitString<FlexWrap>
{
    public static readonly FlexWrap NoWrap = new("nowrap");
    public static readonly FlexWrap Wrap = new("wrap");
    public static readonly FlexWrap WrapReverse = new("wrap-reverse");

    private FlexWrap(string type) : base(type)
    {
    }
}