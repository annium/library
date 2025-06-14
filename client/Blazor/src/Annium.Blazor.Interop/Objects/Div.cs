using Microsoft.AspNetCore.Components;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Interop;

/// <summary>
/// Represents an HTML div element
/// </summary>
/// <param name="Reference">The ElementReference to the HTML div element</param>
public sealed record Div(ElementReference Reference) : ReferenceElement(Reference)
{
    /// <summary>
    /// Implicitly converts an ElementReference to a Div
    /// </summary>
    /// <param name="reference">The ElementReference to convert</param>
    /// <returns>A new Div instance</returns>
    public static implicit operator Div(ElementReference reference) => new(reference);
}
