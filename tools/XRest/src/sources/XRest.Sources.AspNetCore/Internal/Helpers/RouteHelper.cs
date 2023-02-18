using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace XRest.Sources.AspNetCore.Internal.Helpers;

public static partial class RouteHelper
{
    [GeneratedRegex("\\{([A-z0-9]+)[^}]*\\}", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex MyRegex();

    private static readonly Regex RouteRe = MyRegex();

    public static string NormalizeRoute(string route)
    {
        return RouteRe.Replace(route, x => $"{{{x.Groups[1].Value}}}");
    }

    public static IReadOnlyCollection<string> ParseRouteParameters(string route)
    {
        var matches = RouteRe.Matches(route);

        return matches.Select(x => x.Groups[1].Value).ToArray();
    }
}