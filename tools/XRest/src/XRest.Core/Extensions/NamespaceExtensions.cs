using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Models;

namespace XRest.Core.Extensions;

public static class NamespaceExtensions
{
    public static Namespace ToNamespace(this string ns) => Namespace.New(ns.ToNamespaceArray());

    public static string[] ToNamespaceArray(this string ns) => ns switch
    {
        "" => Array.Empty<string>(),
        _  => ns.Split('.').ToArray().EnsureValidNamespace()
    };

    private static string ToNamespaceString(this IEnumerable<string> ns) => string.Join('.', ns);

    private static T EnsureValidNamespace<T>(this T ns)
        where T : IEnumerable<string>
    {
        if (ns.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException($"Namespace {ns.ToNamespaceString()} contains empty parts");

        return ns;
    }
}