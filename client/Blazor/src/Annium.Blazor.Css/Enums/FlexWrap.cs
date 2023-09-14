using Annium.Blazor.Css.Internal;

// ReSharper disable once CheckNamespace
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