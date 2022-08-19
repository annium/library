namespace Annium.Blazor.Interop;

public record StaticElement : Element
{
    protected override string Id { get; }

    public StaticElement(
        string id
    )
    {
        Id = id;
    }
}