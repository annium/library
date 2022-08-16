using System;
using System.Globalization;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Blazor.Interop;
using Annium.Core.Primitives;
using Microsoft.AspNetCore.Components;
using NodaTime;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Components;

public partial class Crosshair : IAsyncDisposable
{
    [Parameter]
    public string LineStyle { get; set; } = CrosshairLineStyle;

    [Parameter]
    public string LabelBackground { get; set; } = CrosshairLabelBackground;

    [Parameter]
    public string LabelFontFamily { get; set; } = CrosshairLabelFontFamily;

    [Parameter]
    public int LabelFontSize { get; set; } = CrosshairLabelFontSize;

    [Parameter]
    public string LabelStyle { get; set; } = CrosshairLabelStyle;

    [CascadingParameter]
    public IChartContext ChartContext { get; set; } = default!;

    private AsyncDisposableBox _disposable = Disposable.AsyncBox();

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender) return;

        _disposable += ChartContext.OnLookupChanged(HandleLookup);
    }

    protected override void OnParametersSet()
    {
        ChartContext.RequestDraw();
    }

    private void HandleLookup(Instant? m, Point? p)
    {
        if (m is null || p is null)
            return;

        var moment = m.Value;
        var point = p.Value;
        var msPerPx = ChartContext.MsPerPx;
        var x = ((moment - ChartContext.View.Start).TotalMilliseconds.FloorInt64() / (decimal) msPerPx).FloorInt32() + 0.5f;

        foreach (var pane in ChartContext.Panes)
        {
            // render crosshair at series
            {
                var ctx = pane.Series.Overlay;
                var rect = pane.Series.Rect;
                var ctxY = rect.Y.FloorInt32();
                var ctxWidth = rect.Width.CeilInt32();
                var ctxHeight = rect.Height.CeilInt32();

                ctx.Save();

                ctx.StrokeStyle = LineStyle;
                ctx.LineWidth = 1;
                ctx.LineDash = new[] { 6, 6 };

                var y = point.Y - ctxY + 0.5f;

                ctx.ClearRect(0, 0, ctxWidth, ctxHeight);

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
                var ctxWidth = rect.Width.CeilInt32();
                var ctxHeight = rect.Height.CeilInt32();

                ctx.Save();

                ctx.ClearRect(0, 0, ctxWidth, ctxHeight);

                ctx.Font = $"{LabelFontSize}px {LabelFontFamily}";
                ctx.FillStyle = LabelStyle;

                var dateTime = moment.InZone(ChartContext.TimeZone);
                var text = dateTime.ToString("dd.MM.yy HH:mm", null);
                var textSize = ctx.MeasureText(text);

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
                var ctxHeight = rect.Height.CeilInt32();

                ctx.Save();

                ctx.ClearRect(0, 0, ctxWidth, ctxHeight);

                var backOffset = (LabelFontSize * 0.85d).CeilInt32();
                ctx.FillStyle = LabelBackground;
                ctx.FillRect(0, point.Y - ctxY - backOffset, ctxWidth, backOffset * 2);

                var value = pane.View.Start + (ctx.Height - point.Y + ctxY) * pane.DotPerPx;
                var text = value.ToString(CultureInfo.InvariantCulture);
                ctx.Font = $"{LabelFontSize}px {LabelFontFamily}";
                ctx.FillStyle = LabelStyle;
                ctx.TextBaseline = CanvasTextBaseline.middle;
                ctx.FillText(text, 3, point.Y - ctxY);

                ctx.Restore();
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }
}