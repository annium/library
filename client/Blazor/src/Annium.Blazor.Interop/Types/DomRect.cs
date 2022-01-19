namespace Annium.Blazor.Interop;

public readonly record struct DomRect
{
    public decimal X { get; init; }
    public decimal Y { get; init; }
    public decimal Width { get; init; }
    public decimal Height { get; init; }
}