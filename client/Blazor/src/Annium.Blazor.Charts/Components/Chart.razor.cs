using System;
using System.Threading.Tasks;
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
        _disposable += Timer.Start(Draw, AnimationFrameMs, AnimationFrameMs);
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

    private void Draw()
    {
        if (!ChartContext.TryDraw())
            return;

        ChartContext.Adjust(Moment);
        ChartContext.SendUpdate();
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