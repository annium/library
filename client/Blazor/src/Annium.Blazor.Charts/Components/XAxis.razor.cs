using System;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Blazor.Interop;
using Annium.Logging;
using Annium.NodaTime.Extensions;
using Microsoft.AspNetCore.Components;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Components;

/// <summary>
/// Horizontal axis component that renders time-based labels and grid lines for charts.
/// </summary>
public partial class XAxis : ILogSubject, IAsyncDisposable
{
    /// <summary>
    /// Gets or sets the font family for axis labels.
    /// </summary>
    [Parameter]
    public string LabelFontFamily { get; set; } = AxisLabelFontFamily;

    /// <summary>
    /// Gets or sets the font size for axis labels.
    /// </summary>
    [Parameter]
    public int LabelFontSize { get; set; } = AxisLabelFontSize;

    /// <summary>
    /// Gets or sets the style (color) for axis labels.
    /// </summary>
    [Parameter]
    public string LabelStyle { get; set; } = AxisLabelStyle;

    /// <summary>
    /// Gets or sets the chart context from the parent chart component.
    /// </summary>
    [CascadingParameter]
    public IChartContext Chart { get; set; } = null!;

    /// <summary>
    /// Gets or sets the horizontal side context for rendering the axis.
    /// </summary>
    [CascadingParameter]
    public IHorizontalSideContext SideContext { get; set; } = null!;

    /// <summary>
    /// Gets or sets the logger instance for this component.
    /// </summary>
    [Inject]
    public ILogger Logger { get; set; } = null!;

    /// <summary>
    /// Container for managing disposable resources.
    /// </summary>
    private AsyncDisposableBox _disposable = Disposable.AsyncBox(VoidLogger.Instance);

    /// <summary>
    /// Called after the component has been rendered. Subscribes to chart updates on first render.
    /// </summary>
    /// <param name="firstRender">True if this is the first time the component is being rendered.</param>
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        _disposable += Chart.OnUpdate(Draw);
    }

    /// <summary>
    /// Draws the X-axis labels and formatting on the canvas.
    /// </summary>
    private void Draw()
    {
        var ctx = SideContext.Canvas;

        ctx.Save();

        ctx.Font = $"{LabelFontSize}px {LabelFontFamily}";
        ctx.FillStyle = LabelStyle;
        ctx.TextBaseline = CanvasTextBaseline.middle;

        var baseline = (SideContext.Rect.Height / 2).FloorInt32();
        foreach (var (x, m) in Chart.GetVerticalLines())
        {
            if (m.IsMidnight())
            {
                var text = m.ToString("dd.MM", null);
                var offset = ((double)ctx.MeasureTextWidth(text) / 2).CeilInt32();

                ctx.Font = $"bold {LabelFontSize}px {LabelFontFamily}";
                ctx.FillText(text, x - offset, baseline);
                ctx.Font = $"{LabelFontSize}px {LabelFontFamily}";
            }
            else
            {
                var text = m.ToString("HH:mm", null);
                var offset = ((double)ctx.MeasureTextWidth(text) / 2).CeilInt32();

                ctx.FillText(text, x - offset, baseline);
            }
        }

        ctx.Restore();
    }

    /// <summary>
    /// Disposes of the component's resources asynchronously.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous disposal operation.</returns>
    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }
}
