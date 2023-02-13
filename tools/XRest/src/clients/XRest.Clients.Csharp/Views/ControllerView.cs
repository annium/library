using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Models;

namespace XRest.Clients.Csharp.Views;

internal sealed record ControllerView(
    IReadOnlyCollection<Namespace> Usages,
    Namespace Namespace,
    string Type,
    string Name,
    IReadOnlyCollection<ActionView> Actions
)
{
    public override string ToString() => Name;

    public static explicit operator ClientView(ControllerView x) =>
        new(x.Usages.Select(y => y.ToString()).ToArray(), x.Namespace.ToString(), x.Name, x.Type, x.Actions);
}