using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Blazor.Core.Tools;
using Annium.Blazor.Css;
using Annium.Blazor.Interop;
using Annium.Core.Primitives;
using Annium.Extensions.Jobs;
using Annium.NodaTime.Extensions;
using Microsoft.AspNetCore.Components;
using NodaTime;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Components;

public partial class Chart : IAsyncDisposable
{
    [Parameter, EditorRequired]
    public IChartContext ChartContext { get; set; } = default!;

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    private string Class => ClassBuilder.With(_style.Container).With(CssClass).Build();
    private Div _container = default!;
    private IManagedChartContext _chartContext = default!;
    private decimal _rawZoom;
    private AsyncDisposableBox _disposable = Disposable.AsyncBox();

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        _chartContext.Init(_container);

        _disposable += _container.OnWheel(HandleWheel);
        _disposable += _container.OnMouseMove(HandlePointerMove);
        _disposable += _container.OnMouseOut(HandlePointerOut);
        _disposable += Timer.Start(CheckState, AnimationFrameMs, AnimationFrameMs);
        _disposable += _container;
    }

    protected override void OnParametersSet()
    {
        _chartContext = (IManagedChartContext)ChartContext;
        _chartContext.RequestDraw();
        _rawZoom = _chartContext.Zoom;
    }

    private void HandleWheel(bool ctrlKey, decimal deltaX, decimal deltaY)
    {
        if (_chartContext.IsLocked)
            return;

        var changed = ctrlKey
            ? HandleZoomDelta(deltaY)
            : HandleScrollDelta(deltaX);

        if (changed)
            _chartContext.RequestDraw();
    }

    private void HandlePointerMove(int x, int y) =>
        _chartContext.RequestOverlay(new Point(x, y));

    private void HandlePointerOut(int x, int y) =>
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
            {
                var lookupMoment = (_chartContext.View.Start + Duration.FromMilliseconds(point.X * _chartContext.MsPerPx))
                    .RoundTo(_chartContext.Resolution);

                _chartContext.SetLookup(lookupMoment, point);
            }
        }
    }

    private bool HandleZoomDelta(decimal delta)
    {
        var zooms = _chartContext.Zooms;
        _rawZoom = (_rawZoom * (1 - delta * ZoomMultiplier)).Within(zooms[0], zooms[^1]);

        var value = zooms.OrderBy(x => _rawZoom.DiffFrom(x)).First();
        if (value == _chartContext.Zoom)
            return false;

        _chartContext.SetZoom(value);

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

        var duration = Duration.FromMilliseconds(_chartContext.MsPerPx * Math.Abs(change));
        var moment = change > 0 ? _chartContext.Moment + duration : _chartContext.Moment - duration;
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