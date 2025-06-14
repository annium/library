using System;

namespace Annium.Blazor.Charts.Domain.Contexts;

/// <summary>
/// Extension methods for IChartContext providing convenient zoom operations
/// </summary>
public static class ChartContextExtensions
{
    /// <summary>
    /// Increases the zoom level by one step
    /// </summary>
    /// <param name="ctx">The chart context to zoom in</param>
    public static void ZoomIn(this IChartContext ctx)
    {
        ctx.ChangeZoom(1);
    }

    /// <summary>
    /// Decreases the zoom level by one step
    /// </summary>
    /// <param name="ctx">The chart context to zoom out</param>
    public static void ZoomOut(this IChartContext ctx)
    {
        ctx.ChangeZoom(-1);
    }

    /// <summary>
    /// Changes the zoom level by the specified delta
    /// </summary>
    /// <param name="ctx">The chart context to modify</param>
    /// <param name="delta">The zoom delta to apply (positive for zoom in, negative for zoom out)</param>
    public static void ChangeZoom(this IChartContext ctx, int delta)
    {
        var index = ctx.ResolveZoomIndex();
        var newIndex = (index + delta).Within(0, ctx.Zooms.Count - 1);
        var zoom = ctx.Zooms[newIndex];

        ctx.SetZoom(zoom);
    }

    /// <summary>
    /// Resolves the current zoom index in the available zooms collection
    /// </summary>
    /// <param name="ctx">The chart context to resolve zoom index for</param>
    /// <returns>The index of the current zoom level</returns>
    public static int ResolveZoomIndex(this IChartContext ctx)
    {
        var index = 0;
        foreach (var zoom in ctx.Zooms)
        {
            if (zoom == ctx.Zoom)
                return index;
            index++;
        }

        throw new InvalidOperationException("Failed to resolve zoom index");
    }
}
