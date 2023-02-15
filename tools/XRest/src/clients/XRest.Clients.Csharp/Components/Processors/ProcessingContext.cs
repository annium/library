using System;
using System.Collections.Generic;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;

namespace XRest.Clients.Csharp.Components.Processors;

internal sealed record ProcessingContext(Namespace ModelsNamespace)
{
    public IReadOnlyCollection<Namespace> Namespaces => _namespaces;
    private readonly HashSet<Namespace> _namespaces = new();

    public void TrackNamespace(Namespace @namespace) => _namespaces.Add(@namespace);
    public void TrackNamespace(Type type) => _namespaces.Add(type.Namespace!.ToNamespace());
}