using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace XRest.Core.Helpers
{
    public static class RouteHelper
    {
        private static readonly Regex RouteRe = new Regex(@"\{([A-z0-9]+)[^}]*\}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string BuildRoute(
            string? controllerArea,
            string controllerName,
            string? controllerRoute,
            string actionName,
            string? actionRoute
        )
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(controllerRoute))
            {
                var route = controllerRoute.Replace("[controller]", controllerName, StringComparison.InvariantCultureIgnoreCase);
                if (!string.IsNullOrWhiteSpace(controllerArea))
                    route = route.Replace("[area]", controllerArea, StringComparison.InvariantCultureIgnoreCase);
                sb.Append(route);
            }

            if (!string.IsNullOrWhiteSpace(actionRoute))
            {
                if (sb.Length > 0)
                    sb.Append("/");

                sb.Append(actionRoute.Replace("[action]", actionName, StringComparison.InvariantCultureIgnoreCase));
            }

            return NormalizeRoute(sb.ToString());
        }

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
}