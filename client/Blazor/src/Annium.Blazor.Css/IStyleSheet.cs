namespace Annium.Blazor.Css;

/// <summary>
/// Interface for stylesheet objects that provide CSS content
/// </summary>
public interface IStyleSheet
{
    /// <summary>
    /// Gets the CSS content as a string
    /// </summary>
    string Css { get; }
}
