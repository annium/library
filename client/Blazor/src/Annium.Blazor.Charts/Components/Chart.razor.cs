using System;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
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
    private AsyncDisposableBox _disposable = Disposable.AsyncBox();

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        _chartContext.Init(_container);

        _disposable += _container.OnWheel(HandleWheel);
        _disposable += _chartContext.Container.OnMouseMove(HandlePointerMove);
        _disposable += _chartContext.Container.OnMouseOut(HandlePointerOut);
        _disposable += Timer.Start(CheckState, AnimationFrameMs, AnimationFrameMs);
        _disposable += _container;
    }

    protected override void OnParametersSet()
    {
        _chartContext = (IManagedChartContext)ChartContext;
        _chartContext.RequestDraw();
    }

    private void HandleWheel(bool ctrlKey, decimal deltaX, decimal deltaY)
    {
        if (_chartContext.IsLocked)
            return;

        var changed = ctrlKey
            ? _chartContext.ChangeZoom(deltaY)
            : _chartContext.ChangeScroll(deltaX);

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
        {
            _chartContext.Adjust(_chartContext.Moment);
            _chartContext.SendUpdate();
        }

        if (_chartContext.TryOverlay(out var point))
        {
            ClearOverlays();

            if (point == default)
                _chartContext.SendLookupChanged(null, null);
            else
            {
                var lookupMoment = (_chartContext.View.Start + Duration.FromMilliseconds(point.X * _chartContext.MsPerPx)).RoundToMinute();

                _chartContext.SendLookupChanged(lookupMoment, point);
            }
        }
    }

    private void ClearOverlays()
    {
        foreach (var pane in _chartContext.Panes)
        {
            // clear crosshair at series
            ClearContext(pane.Series.Overlay, pane.Series.Rect);

            // clear bottom label
            if (pane.Bottom is not null)
                ClearContext(pane.Bottom.Overlay, pane.Bottom.Rect);

            // clear right label
            if (pane.Right is not null)
                ClearContext(pane.Right.Overlay, pane.Right.Rect);
        }

        static void ClearContext(Canvas ctx, DomRect rect) =>
            ctx.ClearRect(0, 0, rect.Width.CeilInt32(), rect.Height.CeilInt32());
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