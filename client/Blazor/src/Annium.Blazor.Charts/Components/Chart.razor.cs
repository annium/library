using System;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Blazor.Core.Tools;
using Annium.Blazor.Css;
using Annium.Blazor.Interop;
using Annium.Extensions.Jobs;
using Annium.Logging;
using Microsoft.AspNetCore.Components;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Components;

/// <summary>
/// Main chart component that provides interactive charting functionality with zoom, pan, and overlay capabilities.
/// </summary>
public partial class Chart : ILogSubject, IAsyncDisposable
{
    /// <summary>
    /// Gets or sets the chart context that manages the chart's data and rendering state.
    /// </summary>
    [Parameter, EditorRequired]
    public IChartContext ChartContext { get; set; } = null!;

    /// <summary>
    /// Gets or sets the CSS class to apply to the chart container.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Gets or sets the child content to render within the chart.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = null!;

    /// <summary>
    /// Gets or sets the logger instance for this component.
    /// </summary>
    [Inject]
    public ILogger Logger { get; set; } = null!;

    /// <summary>
    /// Gets or sets the CSS styles for the chart component.
    /// </summary>
    [Inject]
    private Style Styles { get; set; } = null!;

    /// <summary>
    /// Gets the computed CSS class string for the chart container.
    /// </summary>
    private string Class => ClassBuilder.With(Styles.Container).With(CssClass).Build();

    /// <summary>
    /// The HTML div element that contains the chart.
    /// </summary>
    private Div _container = null!;

    /// <summary>
    /// The managed chart context instance used internally.
    /// </summary>
    private IManagedChartContext _chartContext = null!;

    /// <summary>
    /// Container for managing disposable resources.
    /// </summary>
    private AsyncDisposableBox _disposable = Disposable.AsyncBox(VoidLogger.Instance);

    /// <summary>
    /// Called when the component parameters are set. Triggers a chart redraw request.
    /// </summary>
    protected override void OnParametersSet()
    {
        this.Trace("request draw");
        _chartContext = (IManagedChartContext)ChartContext;
        _chartContext.RequestDraw();
    }

    /// <summary>
    /// Called after the component has been rendered. Sets up event handlers and initialization on first render.
    /// </summary>
    /// <param name="firstRender">True if this is the first time the component is being rendered.</param>
    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        _chartContext.SetRect(_container.GetBoundingClientRect());

        _disposable += Timer.Start(CheckState, AnimationFrameMs, AnimationFrameMs);
        _disposable += _container;
        _disposable += Window.OnResize(_ => _chartContext.SetRect(_container.GetBoundingClientRect()));
        _disposable += _container.OnResize(_ => _chartContext.SetRect(_container.GetBoundingClientRect()));
        _disposable += Window.OnKeyDown(HandleKeyboard, false);
        _container.OnWheel(HandleWheel);
        _container.OnMouseMove(HandlePointerMove);
        _container.OnMouseOut(HandlePointerOut);
    }

    /// <summary>
    /// Handles keyboard events for zoom control (Alt + minus/plus keys).
    /// </summary>
    /// <param name="e">The keyboard event arguments.</param>
    private void HandleKeyboard(KeyboardEvent e)
    {
        if (e is { AltKey: true } && (e.Key == "-" || e.Code == "Minus"))
            ChangeZoom(-1);

        if (e is { AltKey: true } && (e.Key == "=" || e.Code == "Equal"))
            ChangeZoom(1);
    }

    /// <summary>
    /// Handles mouse wheel events for zoom and scroll functionality.
    /// </summary>
    /// <param name="e">The wheel event arguments.</param>
    private void HandleWheel(WheelEvent e)
    {
        if (_chartContext.IsLocked)
            return;

        var changed = e.CtrlKey ? HandleZoomDelta(e.DeltaY) : HandleScrollDelta(e.DeltaX);

        if (changed)
            _chartContext.RequestDraw();
    }

    /// <summary>
    /// Handles mouse pointer movement for overlay updates.
    /// </summary>
    /// <param name="e">The mouse event arguments.</param>
    private void HandlePointerMove(MouseEvent e) => _chartContext.RequestOverlay(new Point(e.X, e.Y));

    /// <summary>
    /// Handles mouse pointer leaving the chart area.
    /// </summary>
    /// <param name="_">The mouse event arguments (unused).</param>
    private void HandlePointerOut(MouseEvent _) => _chartContext.RequestOverlay(null);

    /// <summary>
    /// Periodically checks the chart state and updates drawing and overlays as needed.
    /// </summary>
    private void CheckState()
    {
        if (_chartContext.TryDraw())
            _chartContext.Update();

        if (_chartContext.TryOverlay(out var point))
        {
            _chartContext.ClearOverlays();

            if (point == default)
                _chartContext.SetLookup(null, null);
            else
                _chartContext.SetLookup(_chartContext.FromX(point.X), point);
        }
    }

    /// <summary>
    /// Handles zoom changes based on wheel delta.
    /// </summary>
    /// <param name="delta">The wheel delta value.</param>
    /// <returns>True if the zoom level was changed, false otherwise.</returns>
    private bool HandleZoomDelta(decimal delta)
    {
        return ChangeZoom(delta < 0 ? 1 : -1);
    }

    /// <summary>
    /// Changes the zoom level by the specified delta.
    /// </summary>
    /// <param name="delta">The zoom level change (positive for zoom in, negative for zoom out).</param>
    /// <returns>True if the zoom level was changed, false otherwise.</returns>
    private bool ChangeZoom(int delta)
    {
        var zoomIndex = _chartContext.ResolveZoomIndex();

        var newZoomIndex = (zoomIndex + delta).Within(0, _chartContext.Zooms.Count - 1);
        if (newZoomIndex == zoomIndex)
            return false;

        _chartContext.SetZoom(_chartContext.Zooms[newZoomIndex]);

        return true;
    }

    /// <summary>
    /// Handles horizontal scrolling based on wheel delta.
    /// </summary>
    /// <param name="delta">The scroll delta value.</param>
    /// <returns>True if the scroll position was changed, false otherwise.</returns>
    private bool HandleScrollDelta(decimal delta)
    {
        var change = (delta * ScrollMultiplier).FloorInt32();
        if (change == 0)
            return false;

        var (start, end) = _chartContext.View;
        var size = end - start;

        // block scrolling to left of chart bounds
        if (change < 0 && end - _chartContext.Bounds.Start <= size / 2)
            return false;

        // block scrolling to right of chart bounds
        if (change > 0 && _chartContext.Bounds.End - start <= size / 2)
            return false;

        var moment = _chartContext.FromX(_chartContext.ToX(_chartContext.Moment) + change);
        _chartContext.SetMoment(moment);

        return true;
    }

    /// <summary>
    /// Disposes of the component's resources asynchronously.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous disposal operation.</returns>
    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }

    /// <summary>
    /// CSS styles for the Chart component.
    /// </summary>
    public class Style : RuleSet
    {
        /// <summary>
        /// CSS rule for the chart container element.
        /// </summary>
        public readonly CssRule Container = Rule.Class()
            .PositionRelative()
            .FlexColumn(AlignItems.FlexStart, JustifyContent.Stretch);
    }
}
