using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Interop;
using Annium.Core.Primitives;
using Annium.NodaTime.Extensions;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Extensions;

internal static class ChartContextExtensions
{
    private const int DefaultBlockSize = 80;

    public static Action OnUpdate(this IChartContext context, Action draw)
    {
        context.Updated += draw;

        return () => context.Updated -= draw;
    }

    public static Action OnLookupChanged(this IChartContext context, Action<Instant?, Point?> handle)
    {
        context.LookupChanged += handle;

        return () => context.LookupChanged -= handle;
    }

    public static IReadOnlyDictionary<int, LocalDateTime> GetVerticalLines(this IChartContext context)
    {
        var lines = new Dictionary<int, LocalDateTime>();

        var alignment = context.GetAlignmentDuration();
        var (start, end) = context.View;

        var lineMoment = start.FloorTo(alignment);
        lineMoment -= Duration.FromMinutes(context.TimeZoneOffset);

        // align floors instant, so pick next period if start is not aligned
        if (lineMoment < start)
            lineMoment += alignment;

        while (lineMoment <= end)
        {
            var line = context.ToX(lineMoment);
            lines[line] = lineMoment.InZone(context.TimeZone).LocalDateTime;
            lineMoment += alignment;
        }

        return lines;
    }

    public static void ClearOverlays(this IChartContext context)
    {
        foreach (var pane in context.Panes)
        {
            // clear crosshair at series
            ClearContext(pane.Series.Overlay, pane.Series.Rect);

            // clear bottom label
            if (pane.Bottom is not null)
                ClearContext(pane.Bottom.Overlay, pane.Bottom.Rect);

            // clear right label
            if (pane.Right is not null)
                ClearContext(pane.Right.Overlay, pane.Right.Rect);
        }

        static void ClearContext(Canvas ctx, DomRect rect) =>
            ctx.ClearRect(0, 0, rect.Width.CeilInt32(), rect.Height.CeilInt32());
    }


    private static Duration GetAlignmentDuration(this IChartContext context)
    {
        var block = DefaultBlockSize * context.Resolution.TotalMinutes / context.Zoom;

        return block switch
        {
            > 11520 => Duration.FromDays(8),
            > 5760 => Duration.FromDays(4),
            > 2880 => Duration.FromDays(2),
            > 1440 => Duration.FromDays(1),
            > 720 => Duration.FromHours(12),
            > 360 => Duration.FromHours(6),
            > 240 => Duration.FromHours(4),
            > 120 => Duration.FromHours(2),
            > 60 => Duration.FromHours(1),
            > 30 => Duration.FromMinutes(30),
            > 15 => Duration.FromMinutes(15),
            _ => Duration.FromMinutes(block > 5 ? 5 : 3)
        };
    }
}