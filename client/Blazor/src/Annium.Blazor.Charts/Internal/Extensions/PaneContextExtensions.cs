using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Extensions;

namespace Annium.Blazor.Charts.Internal.Extensions;

/// <summary>
/// Extension methods for <see cref="IPaneContext"/> to provide additional pane functionality.
/// </summary>
internal static class PaneContextExtensions
{
    /// <summary>
    /// Default block size in pixels used for horizontal line calculations.
    /// </summary>
    private const int DefaultBlockSize = 40;

    /// <summary>
    /// Calculates horizontal grid lines for the pane based on value alignment within the current view.
    /// </summary>
    /// <param name="context">The pane context containing view and scale information.</param>
    /// <returns>A dictionary mapping Y-coordinate positions to their corresponding decimal values.</returns>
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
