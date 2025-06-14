using Annium.Blazor.Css.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Represents CSS align-items property values for controlling cross-axis alignment in flexbox layouts.
/// </summary>
public class AlignItems : ImplicitString<AlignItems>
{
    /// <summary>
    /// Aligns items to the baseline of the text.
    /// </summary>
    public static readonly AlignItems Baseline = new("baseline");

    /// <summary>
    /// Centers items along the cross axis.
    /// </summary>
    public static readonly AlignItems Center = new("center");

    /// <summary>
    /// Aligns items to the start of the cross axis.
    /// </summary>
    public static readonly AlignItems Start = new("start");

    /// <summary>
    /// Aligns items to the end of the cross axis.
    /// </summary>
    public static readonly AlignItems End = new("end");

    /// <summary>
    /// Aligns items to the start of the flex container's cross axis.
    /// </summary>
    public static readonly AlignItems FlexStart = new("flex-start");

    /// <summary>
    /// Aligns items to the end of the flex container's cross axis.
    /// </summary>
    public static readonly AlignItems FlexEnd = new("flex-end");

    /// <summary>
    /// Aligns items to the start of their own box.
    /// </summary>
    public static readonly AlignItems SelfStart = new("self-start");

    /// <summary>
    /// Aligns items to the end of their own box.
    /// </summary>
    public static readonly AlignItems SelfEnd = new("self-end");

    /// <summary>
    /// Reverts the property to the value established by the user-agent stylesheet.
    /// </summary>
    public static readonly AlignItems Revert = new("revert");

    /// <summary>
    /// Stretches items to fill the container along the cross axis.
    /// </summary>
    public static readonly AlignItems Stretch = new("stretch");

    /// <summary>
    /// Initializes a new instance of the AlignItems class.
    /// </summary>
    /// <param name="type">The CSS align-items value.</param>
    private AlignItems(string type)
        : base(type) { }
}
