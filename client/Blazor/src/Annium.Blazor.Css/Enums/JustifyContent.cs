using Annium.Blazor.Css.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Represents CSS justify-content property values for controlling main-axis alignment in flexbox layouts.
/// </summary>
public class JustifyContent : ImplicitString<JustifyContent>
{
    /// <summary>
    /// Aligns items to the left edge of the container.
    /// </summary>
    public static readonly JustifyContent Left = new("left");

    /// <summary>
    /// Aligns items to the right edge of the container.
    /// </summary>
    public static readonly JustifyContent Right = new("right");

    /// <summary>
    /// Centers items along the main axis.
    /// </summary>
    public static readonly JustifyContent Center = new("center");

    /// <summary>
    /// Aligns items to the start of the main axis.
    /// </summary>
    public static readonly JustifyContent Start = new("start");

    /// <summary>
    /// Aligns items to the end of the main axis.
    /// </summary>
    public static readonly JustifyContent End = new("end");

    /// <summary>
    /// Aligns items to the start of the flex container's main axis.
    /// </summary>
    public static readonly JustifyContent FlexStart = new("flex-start");

    /// <summary>
    /// Aligns items to the end of the flex container's main axis.
    /// </summary>
    public static readonly JustifyContent FlexEnd = new("flex-end");

    /// <summary>
    /// Distributes items evenly with equal space around each item.
    /// </summary>
    public static readonly JustifyContent SpaceAround = new("space-around");

    /// <summary>
    /// Distributes items evenly with equal space between items (no space at edges).
    /// </summary>
    public static readonly JustifyContent SpaceBetween = new("space-between");

    /// <summary>
    /// Distributes items evenly with equal space around all items including edges.
    /// </summary>
    public static readonly JustifyContent SpaceEvenly = new("space-evenly");

    /// <summary>
    /// Stretches items to fill the container along the main axis.
    /// </summary>
    public static readonly JustifyContent Stretch = new("stretch");

    /// <summary>
    /// Initializes a new instance of the JustifyContent class.
    /// </summary>
    /// <param name="type">The CSS justify-content value.</param>
    private JustifyContent(string type)
        : base(type) { }
}
