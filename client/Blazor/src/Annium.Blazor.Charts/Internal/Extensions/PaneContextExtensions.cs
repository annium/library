using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Extensions;
using Annium.Core.Primitives;

namespace Annium.Blazor.Charts.Internal.Extensions;

internal static class PaneContextExtensions
{
    private const int DefaultBlockSize = 40;

    public static IReadOnlyDictionary<int, decimal> GetHorizontalLines(this IPaneContext context)
    {
        var lines = new Dictionary<int, decimal>();

        var (min, max) = context.View;
        if (min == max || context.DotPerPx == 0)
            return lines;

        var alignment = (DefaultBlockSize * context.DotPerPx).ToPretty(0.5m);

        var value = min.CeilTo(alignment);

        while (value <= max)
        {
            var line = context.ToY(value);
            lines[line] = value;
            value += alignment;
        }

        return lines;
    }
}