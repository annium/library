using System;
using System.IO;

namespace XRest.Clients.Dotnet.Helpers
{
    internal static class WriterHelper
    {
        public static string GetOutputPath(string rootDir, string rootNs, string ns)
        {
            if (!ns.Contains(rootNs))
                throw new InvalidOperationException($"Can't resolve relative path when namespace '{ns}' is not containing root namespace '{rootNs}'");

            if (ns == rootNs)
                return rootDir;

            var nsParts = ns.Substring(rootNs.Length + 1).Split('.');
            var relativePath = Path.Combine(nsParts);

            return Path.Combine(rootDir, relativePath);
        }
    }
}