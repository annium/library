using System;

namespace Annium.Blazor.Charts.Domain.Contexts;

public static class ChartContextExtensions
{
    public static void ZoomIn(this IChartContext ctx)
    {
        ctx.ChangeZoom(1);
    }

    public static void ZoomOut(this IChartContext ctx)
    {
        ctx.ChangeZoom(-1);
    }

    public static void ChangeZoom(this IChartContext ctx, int delta)
    {
        var index = ctx.ResolveZoomIndex();
        var newIndex = (index + delta).Within(0, ctx.Zooms.Count - 1);
        var zoom = ctx.Zooms[newIndex];

        ctx.SetZoom(zoom);
    }

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