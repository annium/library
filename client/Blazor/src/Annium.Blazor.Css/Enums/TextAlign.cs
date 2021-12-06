using Annium.Blazor.Css.Internal;

namespace Annium.Blazor.Css;

public class TextAlign : ImplicitString<TextAlign>
{
    public static readonly TextAlign Center = new TextAlign("center");
    public static readonly TextAlign Start = new TextAlign("start");
    public static readonly TextAlign End = new TextAlign("end");
    public static readonly TextAlign Left = new TextAlign("left");
    public static readonly TextAlign Right = new TextAlign("right");
    public static readonly TextAlign Revert = new TextAlign("revert");
    public static readonly TextAlign Justify = new TextAlign("justify");

    private TextAlign(string type) : base(type)
    {
    }
}