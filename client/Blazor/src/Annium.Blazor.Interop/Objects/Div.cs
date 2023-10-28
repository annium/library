using Microsoft.AspNetCore.Components;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Interop;

public sealed record Div(ElementReference Reference) : ReferenceElement(Reference)
{
    public static implicit operator Div(ElementReference reference) => new(reference);
}
