using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Blazor.Core.Tools;
using Annium.Blazor.Css;
using Annium.Blazor.Interop;
using Annium.Blazor.Interop.Domain;
using Annium.Extensions.Jobs;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Components;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Components;

public partial class Chart : ILogSubject<Chart>, IAsyncDisposable
{
    [Parameter, EditorRequired]
    public IChartContext ChartContext { get; set; } = default!;

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    [Inject]
    public ILogger<Chart> Logger { get; set; } = default!;

    [Inject]
    private Style Styles { get; set; } = default!;

    private string Class => ClassBuilder.With(Styles.Container).With(CssClass).Build();
    private Div _container = default!;
    private IManagedChartContext _chartContext = default!;
    private decimal _rawZoom;
    private AsyncDisposableBox _disposable = Disposable.AsyncBox();

    protected override void OnParametersSet()
    {
        this.Log().Trace("request draw");
        _chartContext = (IManagedChartContext) ChartContext;
        _chartContext.RequestDraw();
        _rawZoom = _chartContext.Zoom;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        _chartContext.SetRect(_container.GetBoundingClientRect());

        _disposable += Timer.Start(CheckState, AnimationFrameMs, AnimationFrameMs);
        _disposable += _container;
        _disposable += Window.OnResize(_ => _chartContext.SetRect(_container.GetBoundingClientRect()));
        _disposable += _container.OnResize(_ => _chartContext.SetRect(_container.GetBoundingClientRect()));
        _container.OnWheel(HandleWheel);
        _container.OnMouseMove(HandlePointerMove);
        _container.OnMouseOut(HandlePointerOut);
    }

    private void HandleWheel(WheelEvent e)
    {
        if (_chartContext.IsLocked)
            return;

        var changed = e.CtrlKey
            ? HandleZoomDelta(e.DeltaY)
            : HandleScrollDelta(e.DeltaX);

        if (changed)
            _chartContext.RequestDraw();
    }

    private void HandlePointerMove(MouseEvent e) =>
        _chartContext.RequestOverlay(new Point(e.X, e.Y));

    private void HandlePointerOut(MouseEvent _) =>
        _chartContext.RequestOverlay(null);

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

    private bool HandleZoomDelta(decimal delta)
    {
        var zooms = _chartContext.Zooms.ToList();
        var zoomIndex = zooms.IndexOf(_chartContext.Zoom);

        var newZoomIndex = (delta < 0 ? zoomIndex + 1 : zoomIndex - 1).Within(0, zooms.Count - 1);
        if (newZoomIndex == zoomIndex)
            return false;

        _chartContext.SetZoom(zooms[newZoomIndex]);

        return true;
    }

    private bool HandleScrollDelta(decimal delta)
    {
        var change = (delta * ScrollMultiplier).FloorInt32();
        if (change == 0)
            return false;

        var (start, end) = _chartContext.View;
        var size = end - start;
        if (change < 0 && end - _chartContext.Bounds.Start <= size / 2)
            return false;

        if (change > 0 && _chartContext.Bounds.End - start <= size / 2)
            return false;

        var moment = _chartContext.FromX(_chartContext.ToX(_chartContext.Moment) + change);
        _chartContext.SetMoment(moment);

        return true;
    }

    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }

    public class Style : RuleSet
    {
        public readonly CssRule Container = Rule.Class()
            .PositionRelative()
            .FlexColumn(AlignItems.FlexStart, JustifyContent.Stretch);
    }
}