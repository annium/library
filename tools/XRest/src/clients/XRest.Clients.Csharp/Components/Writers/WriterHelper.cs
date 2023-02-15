using System;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;

namespace XRest.Clients.Csharp.Components.Writers;

internal static class WriterHelper
{
    public static string GetOutputPath(string rootDir, Namespace rootNs, Namespace ns)
    {
        if (!ns.StartsWith(rootNs))
            throw new InvalidOperationException($"Can't resolve relative path when namespace '{ns}' is not containing root namespace '{rootNs}'");

        return ns == rootNs ? rootDir : ns.From(rootNs).ToPath(rootDir);
    }
}