using System.Collections.Generic;
using System.Linq;
using XRest.Core.Models;

namespace XRest.Core.Extensions
{
    public static class NamespaceExtensions
    {
        public static Namespace ToNamespace(this string ns) => Namespace.New(ns);
        public static Namespace ToNamespace(this IEnumerable<string> ns) => Namespace.New(ns.ToArray());
        public static string[] ToNamespaceArray(this string ns) => ns.Split('.').ToArray();
        public static string ToNamespaceString(this IEnumerable<string> ns) => string.Join('.', ns);
    }
}