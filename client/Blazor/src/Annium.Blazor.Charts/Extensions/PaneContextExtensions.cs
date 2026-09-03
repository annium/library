using System.Runtime.CompilerServices;
using Annium.Blazor.Charts.Domain.Contexts;
using NodaTime;

namespace Annium.Blazor.Charts.Extensions;

/// <summary>
/// Extension methods for IPaneContext to provide coordinate transformation utilities
/// </summary>
public static class PaneContextExtensions
{
    /// <summary>
    /// Converts a time instant to X coordinate position within the pane using the chart context
    /// </summary>
    /// <param name="ctx">The pane context</param>
    /// <param name="moment">The time instant to convert</param>
    /// <returns>The X coordinate position</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToX(this IPaneContext ctx, Instant moment) => ctx.Chart.ToX(moment);

    /// <summary>
    /// Converts an X coordinate position to a time instant within the pane using the chart context
    /// </summary>
    /// <param name="ctx">The pane context</param>
    /// <param name="x">The X coordinate position</param>
    /// <returns>The corresponding time instant</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant FromX(this IPaneContext ctx, int x) => ctx.Chart.FromX(x);

    /// <summary>
    /// Converts a decimal value to Y coordinate position within the pane
    /// </summary>
    /// <param name="ctx">The pane context</param>
    /// <param name="value">The decimal value to convert</param>
    /// <returns>The Y coordinate position</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToY(this IPaneContext ctx, decimal value) =>
        ctx.DotPerPx == 0 ? 0 : ((ctx.View.End - value) / ctx.DotPerPx).RoundInt32();

    /// <summary>
    /// Converts a Y coordinate position to a decimal value within the pane
    /// </summary>
    /// <param name="ctx">The pane context</param>
    /// <param name="y">The Y coordinate position</param>
    /// <returns>The corresponding decimal value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal FromY(this IPaneContext ctx, int y) => ctx.View.End - y * ctx.DotPerPx;
}
