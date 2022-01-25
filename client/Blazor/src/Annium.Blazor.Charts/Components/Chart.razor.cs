using System;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain;
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
    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public Instant Moment { get; set; } = SystemClock.Instance.GetCurrentInstant().FloorToMinute();

    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    [Inject]
    private IManagedChartContext ChartContext { get; set; } = default!;

    private string Class => ClassBuilder.With(_style.Container).With(CssClass).Build();
    private Div _container = default!;
    private AsyncDisposableBox _disposable = Disposable.AsyncBox();

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        ChartContext.Init(_container);

        _disposable += _container.OnWheel(HandleWheel);
        _disposable += ChartContext.Container.OnMouseMove(HandlePointerMove);
        _disposable += ChartContext.Container.OnMouseOut(HandlePointerOut);
        _disposable += Timer.Start(Draw, AnimationFrameMs, AnimationFrameMs);
        _disposable += Timer.Start(Overlay, AnimationFrameMs, AnimationFrameMs);
        _disposable += _container;
    }

    protected override void OnParametersSet()
    {
        ChartContext.RequestDraw();
    }

    private void HandleWheel(bool ctrlKey, decimal deltaX, decimal deltaY)
    {
        if (ChartContext.IsLocked)
            return;

        var changed = ctrlKey
            ? ChartContext.ChangeZoom(deltaY)
            : ChartContext.ChangeScroll(deltaX);

        if (changed)
            ChartContext.RequestDraw();
    }

    private void HandlePointerMove(int x, int y) =>
        ChartContext.RequestOverlay(new Point(x, y));

    private void HandlePointerOut(int x, int y) =>
        ChartContext.RequestOverlay(null);

    private void Draw()
    {
        if (!ChartContext.TryDraw())
            return;

        ChartContext.Adjust(Moment);
        ChartContext.SendUpdate();
    }

    private void Overlay()
    {
        if (!ChartContext.TryOverlay(out var point))
            return;

        ClearOverlays();

        if (point == default)
            ChartContext.SendLookupChanged(null, null);
        else
        {
            var lookupMoment = (ChartContext.View.Start + Duration.FromMilliseconds(point.X * ChartContext.MsPerPx)).RoundToMinute();

            ChartContext.SendLookupChanged(lookupMoment, point);
        }
    }

    private void ClearOverlays()
    {
        foreach (var pane in ChartContext.Panes)
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