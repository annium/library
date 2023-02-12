using System;
using Annium.Net.Types.Models;

namespace XRest.Clients.Csharp.Helpers;

internal static class WriterHelper
{
    public static string GetOutputPath(string rootDir, Namespace rootNs, Namespace ns)
    {
        if (!ns.StartsWith(rootNs))
            throw new InvalidOperationException($"Can't resolve relative path when namespace '{ns}' is not containing root namespace '{rootNs}'");

        if (ns == rootNs)
            return rootDir;

        return ns.From(rootNs).ToPath(rootDir);
    }
}