using System.Collections.Generic;

namespace Annium.XRest.Clients.Csharp.Views.Models;

internal sealed record EnumView(
    IReadOnlyList<string> Usages,
    string Namespace,
    string Name,
    Dictionary<string, long> Values
) : IModelView;
