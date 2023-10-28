using Microsoft.AspNetCore.Components;

// ReSharper disable once CheckNamespace
namespace Annium.Blazor.Interop;

public sealed record Input(ElementReference Reference) : ReferenceElement(Reference)
{
    public static implicit operator Input(ElementReference reference) => new(reference);
}
