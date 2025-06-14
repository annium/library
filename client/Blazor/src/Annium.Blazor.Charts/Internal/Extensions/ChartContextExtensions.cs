using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Interop;
using Annium.NodaTime.Extensions;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Extensions;

/// <summary>
/// Extension methods for <see cref="IChartContext"/> to provide additional chart functionality.
/// </summary>
internal static class ChartContextExtensions
{
    /// <summary>
    /// Default block size in pixels used for alignment calculations.
    /// </summary>
    private const int DefaultBlockSize = 80;

    /// <summary>
    /// Subscribes to chart context update events and returns an unsubscription action.
    /// </summary>
    /// <param name="context">The chart context to subscribe to.</param>
    /// <param name="draw">The action to execute when the chart is updated.</param>
    /// <returns>An action that unsubscribes the draw action from the chart updates.</returns>
    public static Action OnUpdate(this IChartContext context, Action draw)
    {
        context.Updated += draw;

        return () => context.Updated -= draw;
    }

    /// <summary>
    /// Subscribes to chart context lookup changed events and returns an unsubscription action.
    /// </summary>
    /// <param name="context">The chart context to subscribe to.</param>
    /// <param name="handle">The action to execute when the lookup changes, receiving instant and point information.</param>
    /// <returns>An action that unsubscribes the handle action from the lookup changed events.</returns>
    public static Action OnLookupChanged(this IChartContext context, Action<Instant?, Point?> handle)
    {
        context.LookupChanged += handle;

        return () => context.LookupChanged -= handle;
    }

    /// <summary>
    /// Calculates vertical grid lines for the chart based on time alignment within the current view.
    /// </summary>
    /// <param name="context">The chart context containing view and timezone information.</param>
    /// <returns>A dictionary mapping X-coordinate positions to their corresponding local date times.</returns>
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

    /// <summary>
    /// Clears all overlay contexts for all panes in the chart, including series, bottom labels, and right labels.
    /// </summary>
    /// <param name="context">The chart context containing panes to clear.</param>
    public static void ClearOverlays(this IChartContext context)
    {
        foreach (var pane in context.Panes)
        {
            // clear crosshair at series
            if (pane.Series is not null)
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

    /// <summary>
    /// Calculates the appropriate duration alignment for grid lines based on chart resolution and zoom level.
    /// </summary>
    /// <param name="context">The chart context containing resolution and zoom information.</param>
    /// <returns>A duration representing the optimal alignment interval for the current chart scale.</returns>
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
            _ => Duration.FromMinutes(block > 5 ? 5 : 3),
        };
    }
}
