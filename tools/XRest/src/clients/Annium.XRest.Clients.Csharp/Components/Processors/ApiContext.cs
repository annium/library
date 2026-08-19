using System.Collections.Generic;
using Annium.Net.Types.Models;

namespace Annium.XRest.Clients.Csharp.Components.Processors;

internal sealed record ApiContext(
    Namespace ClientsNamespace,
    Namespace ModelsNamespace,
    IReadOnlyCollection<IModel> Models
)
{
    /// <summary>
    /// Names taken by generated container types, which a model of the same name must not be written
    /// short against.
    /// </summary>
    public IReadOnlyCollection<string> ReservedNames { get; init; } = [];
}
