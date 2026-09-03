using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;

namespace Annium.XRest.Clients.Csharp.Components.Processors;

internal sealed record ProcessingContext(Namespace ModelsNamespace, IReadOnlyCollection<IModel> Models)
{
    /// <summary>
    /// Names already taken by generated container types. A model of the same name would otherwise
    /// resolve to the container — a type declared in the file's own namespace wins over one a `using`
    /// brings in, and the compiler says nothing about it.
    /// </summary>
    public IReadOnlyCollection<string> ReservedNames { get; init; } = [];

    public IReadOnlyCollection<Namespace> Usages => _usages;
    private readonly HashSet<Namespace> _usages = new();

    public void UseNamespace(Namespace @namespace) => _usages.Add(@namespace);

    public void UseNamespace(Type type) => _usages.Add(type.Namespace!.ToNamespace());

    public bool HasModelFor(IModelRef reference) => Models.Any(reference.IsFor);

    /// <summary>
    /// Whether writing this model's bare name would resolve to something else: another model of the
    /// same name from a different namespace, or a generated container.
    /// </summary>
    /// <param name="reference">The model reference about to be written.</param>
    /// <returns>True when the reference has to be written out in full.</returns>
    public bool IsAmbiguous(IModelRef reference) =>
        ReservedNames.Contains(reference.Name) || Models.Count(x => x.Name == reference.Name) > 1;
}
