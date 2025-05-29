using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;

namespace Annium.XRest.Clients.Csharp.Extensions;

public static class NamespaceExtensions
{
    public static IReadOnlyList<Namespace> ToUsagesFrom(this IEnumerable<Namespace> references, Namespace target) =>
        references.Where(x => !target.StartsWith(x)).CleanUsages().ToArray();

    public static IEnumerable<Namespace> CleanUsages(this IEnumerable<Namespace> references) =>
        references.Distinct().OrderBy(x => x.FirstOrDefault() != "System").ThenBy(x => x.ToString());

    public static IReadOnlyList<string> ToUsageStrings(this IEnumerable<Namespace> references) =>
        references.Select(x => x.ToString()).ToArray();
}
