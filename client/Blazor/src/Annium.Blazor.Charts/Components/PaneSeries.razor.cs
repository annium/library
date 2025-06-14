using System;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Blazor.Core.Tools;
using Annium.Blazor.Css;
using Annium.Blazor.Interop;
using Annium.Logging;
using Microsoft.AspNetCore.Components;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Components;

/// <summary>
/// Series pane component that provides the main charting area for displaying data series
/// </summary>
public partial class PaneSeries : ILogSubject, IAsyncDisposable
{
    /// <summary>
    /// Additional CSS class to apply to the component
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// The child content to render within the pane
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = null!;

    /// <summary>
    /// The chart context providing access to chart-level data and operations
    /// </summary>
    [CascadingParameter]
    internal IChartContext ChartContext { get; set; } = null!;

    /// <summary>
    /// The managed pane context providing access to pane-level data and operations
    /// </summary>
    [CascadingParameter]
    internal IManagedPaneContext PaneContext { get; set; } = null!;

    /// <summary>
    /// The managed series context for this series pane
    /// </summary>
    [Inject]
    private IManagedSeriesContext SeriesContext { get; set; } = null!;

    /// <summary>
    /// The CSS styles for the component
    /// </summary>
    [Inject]
    private Style Styles { get; set; } = null!;

    /// <summary>
    /// Logger for the component
    /// </summary>
    [Inject]
    public ILogger Logger { get; set; } = null!;

    /// <summary>
    /// The computed CSS class string for the component
    /// </summary>
    private string Class => ClassBuilder.With(Styles.Block).With(CssClass).Build();

    /// <summary>
    /// The main block element reference
    /// </summary>
    private Div _block = null!;

    /// <summary>
    /// The canvas element for drawing the series content
    /// </summary>
    private Canvas _canvas = null!;

    /// <summary>
    /// The overlay canvas element for interactive content
    /// </summary>
    private Canvas _overlay = null!;

    /// <summary>
    /// Container for managing disposable resources
    /// </summary>
    private AsyncDisposableBox _disposable = Disposable.AsyncBox(VoidLogger.Instance);

    /// <summary>
    /// Called after the component has been rendered
    /// </summary>
    /// <param name="firstRender">True if this is the first render</param>
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        SetSize();
        SeriesContext.Init(_canvas, _overlay);
        PaneContext.SetSeries(SeriesContext);
        _disposable += () => PaneContext.SetSeries(null);

        _disposable += ChartContext.OnUpdate(Draw);
        _disposable += _block;
        _disposable += _canvas;
        _disposable += _overlay;
        _disposable += Window.OnResize(_ => SetSize());
        _disposable += _block.OnResize(_ => SetSize());
    }

    /// <summary>
    /// Sets the size of the canvas elements based on the block's bounding rectangle
    /// </summary>
    private void SetSize()
    {
        var rect = _block.GetBoundingClientRect();
        _overlay.Width = _canvas.Width = rect.Width.CeilInt32();
        _overlay.Height = _canvas.Height = rect.Height.CeilInt32();
        PaneContext.SetRect(rect);
        SeriesContext.SetRect(rect);
    }

    /// <summary>
    /// Draws the series pane boundaries, grid lines, and chart axes
    /// </summary>
    private void Draw()
    {
        var ctx = _canvas;
        var width = SeriesContext.Rect.Width.CeilInt32();
        var height = SeriesContext.Rect.Height.CeilInt32();

        ctx.Save();

        ctx.ClearRect(0, 0, width, height);

        ctx.StrokeStyle = GridStyle;
        ctx.LineWidth = GridLine;

        // boundaries
        ctx.BeginPath();
        ctx.MoveTo(GridHalfLine, GridHalfLine);
        ctx.LineTo(width - GridHalfLine, GridHalfLine);
        ctx.LineTo(width - GridHalfLine, height - GridHalfLine);
        ctx.LineTo(GridHalfLine, height - GridHalfLine);
        ctx.LineTo(GridHalfLine, GridHalfLine);
        ctx.Stroke();
        ctx.ClosePath();

        foreach (var y in PaneContext.GetHorizontalLines().Keys)
        {
            ctx.BeginPath();
            ctx.MoveTo(0, y - 0.5f);
            ctx.LineTo(width, y - 0.5f);
            ctx.Stroke();
            ctx.ClosePath();
        }

        foreach (var x in ChartContext.GetVerticalLines().Keys)
        {
            ctx.BeginPath();
            ctx.MoveTo(x - 0.5f, 0);
            ctx.LineTo(x - 0.5f, height);
            ctx.Stroke();
            ctx.ClosePath();
        }

        ctx.Restore();
    }

    /// <summary>
    /// Disposes of the component's resources asynchronously
    /// </summary>
    /// <returns>A task representing the disposal operation</returns>
    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }

    /// <summary>
    /// CSS styles for the PaneSeries component
    /// </summary>
    public class Style : RuleSet
    {
        /// <summary>
        /// CSS rule for the main block element
        /// </summary>
        public readonly CssRule Block = Rule.Class()
            .DisplayFlex()
            .FlexColumn(AlignItems.Start, JustifyContent.Start)
            .PositionRelative()
            .Set("grid-area", "1 / 1 / 2 / 2")
            .Set("line-height", "0");

        /// <summary>
        /// CSS rule for canvas elements
        /// </summary>
        public readonly CssRule Canvas = Rule.Class().PositionAbsolute().LeftPx(0).RightPx(0).TopPx(0).BottomPx(0);
    }
}
