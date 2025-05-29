using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;

namespace Annium.XRest.Clients.Csharp.Components.Processors;

internal sealed record ProcessingContext(Namespace ModelsNamespace, IReadOnlyCollection<IModel> Models)
{
    public IReadOnlyCollection<Namespace> Usages => _usages;
    private readonly HashSet<Namespace> _usages = new();

    public void UseNamespace(Namespace @namespace) => _usages.Add(@namespace);

    public void UseNamespace(Type type) => _usages.Add(type.Namespace!.ToNamespace());

    public bool HasModelFor(IModelRef reference) => Models.Any(reference.IsFor);
}
