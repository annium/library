using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Annium.XRest.Sources.AspNetCore.Internal.Helpers;

internal static partial class RouteHelper
{
    /// <summary>
    /// Matches a route parameter placeholder, capturing its name without constraints or modifiers:
    /// <c>{id}</c>, <c>{id:int}</c>, <c>{id?}</c>, and the catch-all forms <c>{*slug}</c> / <c>{**slug}</c>.
    /// </summary>
    [GeneratedRegex(@"\{\*{0,2}([A-Za-z0-9_]+)[^}]*\}", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex RouteParameterRegex();

    private static readonly Regex _routeRe = RouteParameterRegex();

    /// <summary>
    /// Rewrites every placeholder to a bare <c>{name}</c>, preferring the declaring method's spelling of
    /// the name. The generated client interpolates the path as <c>$"..."</c> against its own parameters,
    /// so a route written as <c>{Id}</c> for a parameter named <c>id</c> must be emitted as <c>{id}</c> —
    /// route matching is case-insensitive, C# interpolation is not.
    /// </summary>
    public static string NormalizeRoute(string route, IReadOnlyCollection<string> parameterNames) =>
        _routeRe.Replace(
            route,
            match =>
            {
                var name = match.Groups[1].Value;
                var actual = parameterNames.FirstOrDefault(x =>
                    string.Equals(x, name, StringComparison.OrdinalIgnoreCase)
                );

                return $"{{{actual ?? name}}}";
            }
        );

    public static IReadOnlyCollection<string> ParseRouteParameters(string route) =>
        _routeRe.Matches(route).Select(x => x.Groups[1].Value).ToArray();
}
