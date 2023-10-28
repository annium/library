using Annium.Blazor.Css.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

public class JustifyContent : ImplicitString<JustifyContent>
{
    public static readonly JustifyContent Left = new("left");
    public static readonly JustifyContent Right = new("right");
    public static readonly JustifyContent Center = new("center");
    public static readonly JustifyContent Start = new("start");
    public static readonly JustifyContent End = new("end");
    public static readonly JustifyContent FlexStart = new("flex-start");
    public static readonly JustifyContent FlexEnd = new("flex-end");
    public static readonly JustifyContent SpaceAround = new("space-around");
    public static readonly JustifyContent SpaceBetween = new("space-between");
    public static readonly JustifyContent SpaceEvenly = new("space-evenly");
    public static readonly JustifyContent Stretch = new("stretch");

    private JustifyContent(string type)
        : base(type) { }
}
