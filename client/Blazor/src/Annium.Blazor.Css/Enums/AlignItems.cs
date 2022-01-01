using Annium.Blazor.Css.Internal;

namespace Annium.Blazor.Css;

public class AlignItems : ImplicitString<AlignItems>
{
    public static readonly AlignItems Baseline = new("baseline");
    public static readonly AlignItems Center = new("center");
    public static readonly AlignItems Start = new("start");
    public static readonly AlignItems End = new("end");
    public static readonly AlignItems FlexStart = new("flex-start");
    public static readonly AlignItems FlexEnd = new("flex-end");
    public static readonly AlignItems SelfStart = new("self-start");
    public static readonly AlignItems SelfEnd = new("self-end");
    public static readonly AlignItems Revert = new("revert");
    public static readonly AlignItems Stretch = new("stretch");

    private AlignItems(string type) : base(type)
    {
    }
}