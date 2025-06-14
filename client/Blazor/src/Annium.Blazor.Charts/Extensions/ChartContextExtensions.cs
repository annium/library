using System.Runtime.CompilerServices;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.NodaTime.Extensions;
using NodaTime;

namespace Annium.Blazor.Charts.Extensions;

/// <summary>
/// Extension methods for IChartContext to provide coordinate transformation utilities
/// </summary>
internal static class ChartContextExtensions
{
    /// <summary>
    /// Converts a time instant to X coordinate position within the chart
    /// </summary>
    /// <param name="ctx">The chart context</param>
    /// <param name="moment">The time instant to convert</param>
    /// <returns>The X coordinate position</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToX(this IChartContext ctx, Instant moment) =>
        ctx.MsPerPx == 0
            ? 0
            : ((moment - ctx.View.Start).TotalMilliseconds.FloorInt64() / (decimal)ctx.MsPerPx).CeilInt32();

    /// <summary>
    /// Converts an X coordinate position to a time instant within the chart
    /// </summary>
    /// <param name="ctx">The chart context</param>
    /// <param name="x">The X coordinate position</param>
    /// <returns>The corresponding time instant</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant FromX(this IChartContext ctx, int x) =>
        (ctx.View.Start + x * Duration.FromMilliseconds(ctx.MsPerPx)).RoundTo(ctx.Resolution);
}
