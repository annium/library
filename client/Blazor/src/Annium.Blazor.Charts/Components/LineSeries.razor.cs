using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Extensions;
using Annium.Logging;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Charts.Components;

/// <summary>
/// Blazor component for rendering line series charts with customizable color, width, and dash patterns.
/// </summary>
/// <typeparam name="T">The type implementing IPointValue interface representing point data.</typeparam>
public partial class LineSeries<T> : SeriesBase<T>, ILogSubject
    where T : IPointValue
{
    /// <summary>
    /// Gets or sets the color of the line series.
    /// </summary>
    [Parameter]
    public string Color { get; set; } = "black";

    /// <summary>
    /// Gets or sets the width of the line in pixels.
    /// </summary>
    [Parameter]
    public int Width { get; set; } = 1;

    /// <summary>
    /// Gets or sets the dash pattern for the line. Null for solid line.
    /// </summary>
    [Parameter]
    public int[]? Dash { get; set; }

    /// <summary>
    /// Gets or sets the logger instance for this component.
    /// </summary>
    [Inject]
    public ILogger Logger { get; set; } = null!;

    /// <summary>
    /// Gets the minimum number of values required to render the series.
    /// </summary>
    protected override int MinValuesToRender => 1;

    /// <summary>
    /// Renders the line series values on the canvas.
    /// </summary>
    /// <param name="items">The collection of point values to render.</param>
    protected override void RenderValues(IReadOnlyList<T> items)
    {
        var ctx = SeriesContext.Canvas;

        ctx.StrokeStyle = Color;
        ctx.LineWidth = Width;
        if (Dash is not null)
            ctx.LineDash = Dash;

        ctx.BeginPath();

        ctx.MoveTo(0, PaneContext.ToY(items[0].Value));

        foreach (var item in items)
        {
            var x = PaneContext.ToX(item.Moment);
            var y = PaneContext.ToY(item.Value);

            ctx.LineTo(x, y);
        }

        ctx.LineTo((float)PaneContext.Rect.Width, PaneContext.ToY(items[^1].Value));

        ctx.Stroke();
        ctx.ClosePath();
    }

    /// <summary>
    /// Calculates the minimum and maximum values from the point data for chart scaling.
    /// </summary>
    /// <param name="items">The collection of point items to analyze.</param>
    /// <returns>A tuple containing the minimum and maximum values.</returns>
    protected override (decimal min, decimal max) GetBounds(IReadOnlyList<T> items)
    {
        var min = decimal.MaxValue;
        var max = decimal.MinValue;

        foreach (var item in items)
        {
            min = Math.Min(min, item.Value);
            max = Math.Max(max, item.Value);
        }

        return (min, max);
    }
}
