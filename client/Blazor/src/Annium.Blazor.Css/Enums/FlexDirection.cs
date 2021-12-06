using Annium.Blazor.Css.Internal;

namespace Annium.Blazor.Css;

public class FlexDirection : ImplicitString<FlexDirection>
{
    public static readonly FlexDirection Row = new FlexDirection("row");
    public static readonly FlexDirection Column = new FlexDirection("column");
    public static readonly FlexDirection RowReverse = new FlexDirection("row-reverse");
    public static readonly FlexDirection ColumnReverse = new FlexDirection("column-reverse");

    private FlexDirection(string type) : base(type)
    {
    }
}