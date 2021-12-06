using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Blazor.Routing.Internal.Implementations;

internal class Helper
{
    private const char Separator = '/';

    public static IReadOnlyList<string> ParseTemplateParts(string template)
    {
        if (template is null || string.IsNullOrWhiteSpace(template) && template.Length > 0)
            throw new ArgumentNullException(nameof(template));

        template = template.StartsWith(Separator) ? template[1..] : template;

        var parts = template == string.Empty ? Array.Empty<string>() : template.Split('/');
        if (parts.Any(x => x is null || x == string.Empty || x.Contains(' ')))
            throw new ArgumentException($"Template '{template}' can't contain empty parts or whitespace");

        return parts;
    }
}