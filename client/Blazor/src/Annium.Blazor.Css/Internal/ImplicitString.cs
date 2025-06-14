namespace Annium.Blazor.Css.Internal;

/// <summary>
/// Base class for types that can be implicitly converted to string.
/// </summary>
/// <typeparam name="T">The derived type that inherits from this class.</typeparam>
public abstract class ImplicitString<T>
    where T : ImplicitString<T>
{
    /// <summary>
    /// The string value that this instance represents.
    /// </summary>
    private readonly string _type;

    /// <summary>
    /// Initializes a new instance of the ImplicitString class with the specified string value.
    /// </summary>
    /// <param name="type">The string value to represent.</param>
    protected ImplicitString(string type)
    {
        _type = type;
    }

    /// <summary>
    /// Returns the string representation of this instance.
    /// </summary>
    /// <returns>The string value.</returns>
    public override string ToString() => _type;

    /// <summary>
    /// Implicitly converts an ImplicitString instance to its string representation.
    /// </summary>
    /// <param name="rule">The ImplicitString instance to convert.</param>
    /// <returns>The string representation of the instance.</returns>
    public static implicit operator string(ImplicitString<T> rule) => rule.ToString();
}
