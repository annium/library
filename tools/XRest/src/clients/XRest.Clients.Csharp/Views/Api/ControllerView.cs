using System.Collections.Generic;
using Annium.Net.Types.Models;

namespace XRest.Clients.Csharp.Views.Api;

internal sealed record ControllerView(
    IReadOnlyCollection<Namespace> Usages,
    Namespace Namespace,
    string Name,
    IReadOnlyCollection<ActionView> Actions
)
{
    public override string ToString() => Name;
}
