using System;
using System.Globalization;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Blazor.Interop;
using Annium.Logging;
using Microsoft.AspNetCore.Components;
using NodaTime;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Components;

/// <summary>
/// Provides crosshair functionality that displays interactive lines and labels at the current mouse position on the chart
/// </summary>
public partial class Crosshair : ILogSubject, IAsyncDisposable
{
    /// <summary>
    /// Gets or sets the style for the crosshair lines
    /// </summary>
    [Parameter]
    public string LineStyle { get; set; } = CrosshairLineStyle;

    /// <summary>
    /// Gets or sets the background color for crosshair labels
    /// </summary>
    [Parameter]
    public string LabelBackground { get; set; } = CrosshairLabelBackground;

    /// <summary>
    /// Gets or sets the font family for crosshair labels
    /// </summary>
    [Parameter]
    public string LabelFontFamily { get; set; } = CrosshairLabelFontFamily;

    /// <summary>
    /// Gets or sets the font size for crosshair labels
    /// </summary>
    [Parameter]
    public int LabelFontSize { get; set; } = CrosshairLabelFontSize;

    /// <summary>
    /// Gets or sets the text style for crosshair labels
    /// </summary>
    [Parameter]
    public string LabelStyle { get; set; } = CrosshairLabelStyle;

    /// <summary>
    /// Gets or sets the chart context provided by the parent chart component
    /// </summary>
    [CascadingParameter]
    public IChartContext ChartContext { get; set; } = null!;

    /// <summary>
    /// Gets or sets the logger instance for this component
    /// </summary>
    [Inject]
    public ILogger Logger { get; set; } = null!;

    /// <summary>
    /// Holds disposable resources for cleanup
    /// </summary>
    private AsyncDisposableBox _disposable = Disposable.AsyncBox(VoidLogger.Instance);

    /// <summary>
    /// Called when parameters are set, requests a chart redraw
    /// </summary>
    protected override void OnParametersSet()
    {
        ChartContext.RequestDraw();
    }

    /// <summary>
    /// Called after the component has been rendered
    /// </summary>
    /// <param name="firstRender">True if this is the first time the component is being rendered</param>
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        _disposable += ChartContext.OnLookupChanged(HandleLookup);
    }

    /// <summary>
    /// Handles lookup events to render crosshair at the specified moment and point
    /// </summary>
    /// <param name="m">The moment in time for the crosshair</param>
    /// <param name="p">The screen point for the crosshair</param>
    private void HandleLookup(Instant? m, Point? p)
    {
        if (m is null || p is null)
            return;

        var moment = m.Value;
        var point = p.Value;
        var x = ChartContext.ToX(moment) - 0.5f;

        foreach (var pane in ChartContext.Panes)
        {
            // render crosshair at series
            if (pane.Series is not null)
            {
                var ctx = pane.Series.Overlay;
                var rect = pane.Series.Rect;
                var ctxY = rect.Y.FloorInt32();
                var ctxWidth = rect.Width.FloorInt32();
                var ctxHeight = rect.Height.FloorInt32();

                ctx.Save();

                ctx.StrokeStyle = LineStyle;
                ctx.LineWidth = 1;
                ctx.LineDash = [6, 6];

                var y = point.Y - ctxY + 0.5f;

                // crosshair
                ctx.BeginPath();

                // horizontal line is available only at active pane
                if (y > 0 && y < ctxHeight)
                {
                    ctx.MoveTo(0, y);
                    ctx.LineTo(ctxWidth, y);
                }

                ctx.MoveTo(x, 0);
                ctx.LineTo(x, ctxHeight);
                ctx.Stroke();
                ctx.ClosePath();

                ctx.Restore();
            }

            // render bottom label
            if (pane.Bottom is not null)
            {
                var ctx = pane.Bottom.Overlay;
                var rect = pane.Bottom.Rect;
                var ctxX = rect.X.FloorInt32();
                var ctxHeight = rect.Height.CeilInt32();

                ctx.Save();

                ctx.Font = $"{LabelFontSize}px {LabelFontFamily}";
                ctx.FillStyle = LabelStyle;

                var dateTime = moment.InZone(ChartContext.TimeZone);
                var text = dateTime.ToString("dd.MM.yy HH:mm", null);
                var textSize = ctx.MeasureTextWidth(text);

                var backOffset = (textSize / 1.7d).CeilInt32();
                ctx.FillStyle = LabelBackground;
                ctx.FillRect(point.X - ctxX - backOffset, 0, backOffset * 2, ctxHeight);

                var textOffset = (textSize / 2d).CeilInt32();
                var baseline = (ctxHeight / 2d).FloorInt32();
                ctx.FillStyle = LabelStyle;
                ctx.TextBaseline = CanvasTextBaseline.middle;
                ctx.FillText(text, point.X - ctxX - textOffset, baseline);

                ctx.Restore();
            }

            // render right label
            if (pane.Right is not null)
            {
                var ctx = pane.Right.Overlay;
                var rect = pane.Right.Rect;
                var ctxY = rect.Y.FloorInt32();
                var ctxWidth = rect.Width.CeilInt32();

                ctx.Save();

                var backOffset = (LabelFontSize * 0.85d).CeilInt32();
                ctx.FillStyle = LabelBackground;
                ctx.FillRect(0, point.Y - ctxY - backOffset, ctxWidth, backOffset * 2);

                var value = pane.FromY(point.Y - ctxY);
                var text = value.ToString(CultureInfo.InvariantCulture);
                ctx.Font = $"{LabelFontSize}px {LabelFontFamily}";
                ctx.FillStyle = LabelStyle;
                ctx.TextBaseline = CanvasTextBaseline.middle;
                ctx.FillText(text, 3, point.Y - ctxY);

                ctx.Restore();
            }
        }
    }

    /// <summary>
    /// Disposes of the component's resources asynchronously
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous dispose operation</returns>
    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }
}
