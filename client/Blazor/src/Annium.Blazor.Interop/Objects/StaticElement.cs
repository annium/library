// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Interop;

public record StaticElement(string Id) : Element
{
    public override string Id { get; } = Id;
}
