using System;
using System.Collections.Generic;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;

namespace XRest.Clients.Csharp.Components.Processors;

internal sealed record ClientContext(
    Namespace ClientNamespace,
    Namespace ModelsNamespace
)
{
    public IReadOnlyCollection<Namespace> Usages => _usages;
    private readonly HashSet<Namespace> _usages = new();

    public void UseNamespace(Namespace @namespace) => _usages.Add(@namespace);
    public void UseNamespace(Type type) => _usages.Add(type.Namespace!.ToNamespace());
}