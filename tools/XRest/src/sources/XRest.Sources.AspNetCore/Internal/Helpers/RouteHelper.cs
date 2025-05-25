using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace XRest.Sources.AspNetCore.Internal.Helpers;

public static partial class RouteHelper
{
    [GeneratedRegex("\\{([A-z0-9]+)[^}]*\\}", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex MyRegex();

    private static readonly Regex _routeRe = MyRegex();

    public static string NormalizeRoute(string route)
    {
        return _routeRe.Replace(route, x => $"{{{x.Groups[1].Value}}}");
    }

    public static IReadOnlyCollection<string> ParseRouteParameters(string route)
    {
        var matches = _routeRe.Matches(route);

        return matches.Select(x => x.Groups[1].Value).ToArray();
    }
}
