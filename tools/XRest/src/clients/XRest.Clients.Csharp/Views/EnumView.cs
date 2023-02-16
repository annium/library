using System.Collections.Generic;

namespace XRest.Clients.Csharp.Views;

internal sealed record EnumView(
    IReadOnlyList<string> Usages,
    string Namespace,
    string Name,
    Dictionary<string, long> Values
) : IModelView;