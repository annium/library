using System;
using System.Globalization;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Blazor.Interop;
using Annium.Logging;
using Microsoft.AspNetCore.Components;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Components;

/// <summary>
/// Vertical axis component that renders value-based labels and grid lines for chart panes.
/// </summary>
public partial class YAxis : ILogSubject, IAsyncDisposable
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
    public IChartContext ChartContext { get; set; } = null!;

    /// <summary>
    /// Gets or sets the pane context from the parent pane component.
    /// </summary>
    [CascadingParameter]
    public IPaneContext PaneContext { get; set; } = null!;

    /// <summary>
    /// Gets or sets the vertical side context for rendering the axis.
    /// </summary>
    [CascadingParameter]
    public IVerticalSideContext SideContext { get; set; } = null!;

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

        _disposable += ChartContext.OnUpdate(Draw);
    }

    /// <summary>
    /// Draws the Y-axis labels and formatting on the canvas.
    /// </summary>
    private void Draw()
    {
        var ctx = SideContext.Canvas;

        ctx.Save();

        ctx.Font = $"{LabelFontSize}px {LabelFontFamily}";
        ctx.FillStyle = LabelStyle;
        ctx.TextBaseline = CanvasTextBaseline.middle;

        var x = 3;
        foreach (var (y, m) in PaneContext.GetHorizontalLines())
        {
            var text = m.ToString(CultureInfo.InvariantCulture);

            ctx.FillText(text, x, y);
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
