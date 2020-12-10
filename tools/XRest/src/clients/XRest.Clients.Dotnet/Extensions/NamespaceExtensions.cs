using System.Collections.Generic;
using System.Linq;
using XRest.Core.Models;

namespace XRest.Clients.Dotnet.Extensions
{
    public static class NamespaceExtensions
    {
        public static IReadOnlyCollection<Namespace> ToUsagesFrom(
            this IEnumerable<Namespace> references,
            Namespace target
        ) => references
            .Where(x => !target.StartsWith(x))
            .Distinct()
            .OrderBy(x => x.FirstOrDefault() != "System").ThenBy(x => x.ToString())
            .ToArray();

        public static IReadOnlyCollection<string> ToUsageStrings(
            this IEnumerable<Namespace> references
        ) => references
            .Select(x => x.ToString())
            .ToArray();
    }
}