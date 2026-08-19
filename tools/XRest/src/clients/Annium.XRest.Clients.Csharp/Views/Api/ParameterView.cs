namespace Annium.XRest.Clients.Csharp.Views.Api;

/// <summary>
/// A parameter of a generated call. <paramref name="Name"/> is what the server knows it by;
/// <paramref name="Argument"/> is the C# identifier it is declared as, which differs only where the
/// name is already taken by another parameter of the same call.
/// </summary>
/// <param name="Type">The rendered parameter type.</param>
/// <param name="Name">The name the server knows the parameter by.</param>
/// <param name="Argument">The C# identifier the parameter is declared as.</param>
internal sealed record ParameterView(string Type, string Name, string Argument)
{
    public ParameterView(string type, string name)
        : this(type, name, name) { }

    public override string ToString() => $"{Type} {Name}";
}
