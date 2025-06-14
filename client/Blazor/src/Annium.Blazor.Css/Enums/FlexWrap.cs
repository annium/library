using Annium.Blazor.Css.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Css;

/// <summary>
/// Represents CSS flex-wrap property values for controlling whether flex items wrap onto new lines.
/// </summary>
public class FlexWrap : ImplicitString<FlexWrap>
{
    /// <summary>
    /// Flex items stay on a single line and may overflow the container.
    /// </summary>
    public static readonly FlexWrap NoWrap = new("nowrap");

    /// <summary>
    /// Flex items wrap onto new lines as needed.
    /// </summary>
    public static readonly FlexWrap Wrap = new("wrap");

    /// <summary>
    /// Flex items wrap onto new lines in reverse order.
    /// </summary>
    public static readonly FlexWrap WrapReverse = new("wrap-reverse");

    /// <summary>
    /// Initializes a new instance of the FlexWrap class.
    /// </summary>
    /// <param name="type">The CSS flex-wrap value.</param>
    private FlexWrap(string type)
        : base(type) { }
}
