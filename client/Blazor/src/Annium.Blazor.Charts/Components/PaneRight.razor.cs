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
/// Right pane component that provides a vertical side area at the right of a chart pane
/// </summary>
public partial class PaneRight : ILogSubject, IAsyncDisposable
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
    /// The pane context providing access to pane-level data and operations
    /// </summary>
    [CascadingParameter]
    internal IPaneContext PaneContext { get; set; } = null!;

    /// <summary>
    /// The managed vertical side context for this right pane
    /// </summary>
    [Inject]
    private IManagedVerticalSideContext SideContext { get; set; } = null!;

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
    /// The canvas element for drawing the pane content
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
        SideContext.Init(_canvas, _overlay);
        PaneContext.SetRight(SideContext);
        _disposable += () => PaneContext.SetRight(null);

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
        SideContext.SetRect(rect);
    }

    /// <summary>
    /// Draws the pane boundaries and grid on the canvas
    /// </summary>
    private void Draw()
    {
        var ctx = _canvas;
        var width = SideContext.Rect.Width.CeilInt32();
        var height = SideContext.Rect.Height.CeilInt32();

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
    /// CSS styles for the PaneRight component
    /// </summary>
    public class Style : RuleSet
    {
        /// <summary>
        /// CSS rule for the main block element
        /// </summary>
        public readonly CssRule Block = Rule.Class()
            .PositionRelative()
            .Set("grid-area", "1 / 2 / 2 / 3")
            .Set("line-height", "0");

        /// <summary>
        /// CSS rule for canvas elements
        /// </summary>
        public readonly CssRule Canvas = Rule.Class().PositionAbsolute().LeftPx(0).RightPx(0).TopPx(0).BottomPx(0);
    }
}
