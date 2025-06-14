using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Blazor.Routing.Internal;

/// <summary>
/// Provides helper methods for routing operations.
/// </summary>
internal class Helper
{
    /// <summary>
    /// The path separator character used in routing templates.
    /// </summary>
    private const char Separator = '/';

    /// <summary>
    /// Parses a route template string into its individual parts.
    /// </summary>
    /// <param name="template">The template string to parse.</param>
    /// <returns>A list of template parts split by the separator character.</returns>
    public static IReadOnlyList<string> ParseTemplateParts(string template)
    {
        if (template is null || string.IsNullOrWhiteSpace(template) && template.Length > 0)
            throw new ArgumentNullException(nameof(template));

        template = template.StartsWith(Separator) ? template[1..] : template;

        var parts = template == string.Empty ? [] : template.Split('/');
        if (parts.Any(x => string.IsNullOrWhiteSpace(x) || x.Contains(' ')))
            throw new ArgumentException($"Template '{template}' can't contain empty parts or whitespace");

        return parts;
    }
}
