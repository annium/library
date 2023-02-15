using System.Collections.Generic;

namespace XRest.Clients.Csharp.Views;

internal sealed record ClientContainerView(
    IReadOnlyCollection<string> Usages,
    string Namespace,
    string Name,
    string Type,
    IReadOnlyCollection<IClientView> Clients
) : IClientView
{
    public override string ToString() => Name;
}