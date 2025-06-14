using Annium.Blazor.Css.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Represents CSS flex-direction property values for controlling the direction of flex items in a flex container.
/// </summary>
public class FlexDirection : ImplicitString<FlexDirection>
{
    /// <summary>
    /// Places flex items in a row (left to right in left-to-right languages).
    /// </summary>
    public static readonly FlexDirection Row = new("row");

    /// <summary>
    /// Places flex items in a column (top to bottom).
    /// </summary>
    public static readonly FlexDirection Column = new("column");

    /// <summary>
    /// Places flex items in a row but in reverse order (right to left in left-to-right languages).
    /// </summary>
    public static readonly FlexDirection RowReverse = new("row-reverse");

    /// <summary>
    /// Places flex items in a column but in reverse order (bottom to top).
    /// </summary>
    public static readonly FlexDirection ColumnReverse = new("column-reverse");

    /// <summary>
    /// Initializes a new instance of the FlexDirection class.
    /// </summary>
    /// <param name="type">The CSS flex-direction value.</param>
    private FlexDirection(string type)
        : base(type) { }
}
