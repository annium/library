using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace XRest.Core.Helpers
{
    internal static class RouteHelper
    {
        private static readonly Regex RouteRe = new Regex(@"\{([A-z0-9]+)[^}]*\}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string BuildRoute(
            string controllerName,
            string? controllerRoute,
            string actionName,
            string? actionRoute
        )
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(controllerRoute))
                sb.Append(controllerRoute.Replace("[controller]", controllerName, StringComparison.InvariantCultureIgnoreCase));

            if (!string.IsNullOrWhiteSpace(actionRoute))
            {
                if (sb.Length > 0)
                    sb.Append("/");

                sb.Append(actionRoute.Replace("[action]", actionName, StringComparison.InvariantCultureIgnoreCase));
            }

            return RouteRe.Replace(sb.ToString(), x => $"{{{x.Groups[1].Value}}}");
        }

        public static IReadOnlyCollection<string> ParseRouteParameters(string route)
        {
            var matches = RouteRe.Matches(route);

            return matches.Select(x => x.Groups[1].Value).ToArray();
        }
    }
}