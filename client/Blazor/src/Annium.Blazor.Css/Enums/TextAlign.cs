using Annium.Blazor.Css.Internal;

namespace Annium.Blazor.Css;

public class TextAlign : ImplicitString<TextAlign>
{
    public static readonly TextAlign Center = new("center");
    public static readonly TextAlign Start = new("start");
    public static readonly TextAlign End = new("end");
    public static readonly TextAlign Left = new("left");
    public static readonly TextAlign Right = new("right");
    public static readonly TextAlign Revert = new("revert");
    public static readonly TextAlign Justify = new("justify");

    private TextAlign(string type) : base(type)
    {
    }
}