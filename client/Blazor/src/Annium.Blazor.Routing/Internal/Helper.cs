using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Blazor.Routing.Internal;

internal class Helper
{
    private const char Separator = '/';

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
