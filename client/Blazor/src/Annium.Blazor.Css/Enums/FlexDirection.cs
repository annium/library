using Annium.Blazor.Css.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

public class FlexDirection : ImplicitString<FlexDirection>
{
    public static readonly FlexDirection Row = new("row");
    public static readonly FlexDirection Column = new("column");
    public static readonly FlexDirection RowReverse = new("row-reverse");
    public static readonly FlexDirection ColumnReverse = new("column-reverse");

    private FlexDirection(string type) : base(type)
    {
    }
}