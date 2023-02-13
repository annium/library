namespace XRest.Clients.Csharp.Views;

internal sealed record ParameterView(
    string Type,
    string Name
)
{
    public override string ToString() => $"{Type} {Name}";
}