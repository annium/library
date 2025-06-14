// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Interop;

/// <summary>
/// Represents a DOM element with a static identifier
/// </summary>
/// <param name="Id">The static identifier of the element</param>
public record StaticElement(string Id) : Element
{
    /// <summary>
    /// Gets the unique identifier of the element
    /// </summary>
    public override string Id { get; } = Id;
}
