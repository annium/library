using System.Collections.Generic;

namespace XRest.Clients.Csharp.Views;

internal sealed record ClientView(
    IReadOnlyCollection<string> Usages,
    string Namespace,
    string Name,
    string Type,
    IReadOnlyCollection<ActionView> Actions
) : IClientView
{
    public override string ToString() => Name;
}