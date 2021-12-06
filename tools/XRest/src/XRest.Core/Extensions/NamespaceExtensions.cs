using System;
using System.Collections.Generic;
using System.Linq;
using XRest.Core.Models;

namespace XRest.Core.Extensions;

public static class NamespaceExtensions
{
    public static Namespace ToNamespace(this string ns) => Namespace.New(ns);
    public static Namespace ToNamespace(this IEnumerable<string> ns) => Namespace.New(ns);

    public static string[] ToNamespaceArray(this string ns)
    {
        if (ns is null)
            throw new ArgumentException("Value cannot be null.", nameof(ns));

        if (ns == string.Empty)
            return Array.Empty<string>();

        return ns.Split('.').ToArray().EnsureValidNamespace();
    }

    public static string ToNamespaceString(this IEnumerable<string> ns) => string.Join('.', ns);

    internal static T EnsureValidNamespace<T>(this T ns)
        where T : IEnumerable<string>
    {
        if (ns is null)
            throw new ArgumentNullException(nameof(ns));

        if (ns.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException($"Namespace {ns.ToNamespaceString()} contains empty parts");

        return ns;
    }
}