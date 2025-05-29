using System.Collections.Generic;
using Annium.XRest.Clients.Csharp.Views.Api;

namespace Annium.XRest.Clients.Csharp.Views.Client;

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
