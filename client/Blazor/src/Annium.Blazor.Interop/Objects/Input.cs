using Microsoft.AspNetCore.Components;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Interop;

/// <summary>
/// Represents an HTML input element with JavaScript interop capabilities
/// </summary>
/// <param name="Reference">The Blazor element reference</param>
public sealed record Input(ElementReference Reference) : ReferenceElement(Reference)
{
    /// <summary>
    /// Implicitly converts an ElementReference to an Input instance
    /// </summary>
    /// <param name="reference">The element reference to convert</param>
    /// <returns>A new Input instance</returns>
    public static implicit operator Input(ElementReference reference) => new(reference);
}
