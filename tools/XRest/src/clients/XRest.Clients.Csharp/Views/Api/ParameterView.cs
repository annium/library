namespace XRest.Clients.Csharp.Views.Api;

internal sealed record ParameterView(string Type, string Name)
{
    public override string ToString() => $"{Type} {Name}";
}
