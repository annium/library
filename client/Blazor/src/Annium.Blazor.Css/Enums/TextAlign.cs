using Annium.Blazor.Css.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Represents CSS text-align property values for controlling horizontal text alignment.
/// </summary>
public class TextAlign : ImplicitString<TextAlign>
{
    /// <summary>
    /// Centers text horizontally within its container.
    /// </summary>
    public static readonly TextAlign Center = new("center");

    /// <summary>
    /// Aligns text to the start of the text direction (left for left-to-right languages).
    /// </summary>
    public static readonly TextAlign Start = new("start");

    /// <summary>
    /// Aligns text to the end of the text direction (right for left-to-right languages).
    /// </summary>
    public static readonly TextAlign End = new("end");

    /// <summary>
    /// Aligns text to the left edge of the container.
    /// </summary>
    public static readonly TextAlign Left = new("left");

    /// <summary>
    /// Aligns text to the right edge of the container.
    /// </summary>
    public static readonly TextAlign Right = new("right");

    /// <summary>
    /// Reverts the property to the value established by the user-agent stylesheet.
    /// </summary>
    public static readonly TextAlign Revert = new("revert");

    /// <summary>
    /// Stretches text to fill the full width of the container by adjusting spacing.
    /// </summary>
    public static readonly TextAlign Justify = new("justify");

    /// <summary>
    /// Initializes a new instance of the TextAlign class.
    /// </summary>
    /// <param name="type">The CSS text-align value.</param>
    private TextAlign(string type)
        : base(type) { }
}
