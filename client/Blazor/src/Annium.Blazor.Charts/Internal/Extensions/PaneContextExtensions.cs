using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Core.Primitives;

namespace Annium.Blazor.Charts.Internal.Extensions;

internal static class PaneContextExtensions
{
    private const int DefaultBlockSize = 40;

    public static IReadOnlyDictionary<int, decimal> GetHorizontalLines(this IPaneContext context)
    {
        var lines = new Dictionary<int, decimal>();

        var dpx = context.DotPerPx;
        if (dpx == 0)
            return lines;

        var (min, max) = context.View;
        var alignment = (DefaultBlockSize * dpx).ToPretty(0.5m);

        var value = min.CeilTo(alignment);

        while (value <= max)
        {
            var line = context.Height - ((value - min) / dpx).FloorInt32();
            lines[line] = value;
            value += alignment;
        }

        return lines;
    }
}