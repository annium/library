using System.Collections.Generic;
using Annium.Net.Types.Models;

namespace XRest.Clients.Csharp.Components.Processors;

internal sealed record ApiContext(
    Namespace RootNamespace,
    Namespace ModelsNamespace,
    IReadOnlyCollection<ModelBase> Models
);