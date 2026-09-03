using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Blazor.Routing.Internal;

/// <summary>
/// Provides helper methods for routing operations.
/// </summary>
internal static class Helper
{
    /// <summary>
    /// Parses a route template string into its individual parts.
    /// </summary>
    /// <param name="template">The template string to parse.</param>
    /// <returns>A list of template parts split by the separator character.</returns>
    public static IReadOnlyList<string> ParseTemplateParts(string template)
    {
        if (template is null || string.IsNullOrWhiteSpace(template) && template.Length > 0)
            throw new ArgumentNullException(nameof(template));

        template = template.StartsWith(Constants.Separator) ? template[1..] : template;

        var parts = template == string.Empty ? [] : template.Split(Constants.Separator);
        if (parts.Any(x => string.IsNullOrWhiteSpace(x) || x.Contains(' ')))
            throw new ArgumentException($"Template '{template}' can't contain empty parts or whitespace");

        return parts;
    }
}
