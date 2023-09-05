using System;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Blazor.Core.Tools;
using Annium.Blazor.Css;
using Annium.Logging;
using Microsoft.AspNetCore.Components;
using NodaTime;

namespace Annium.Blazor.Charts.Components;

public partial class Resolution : ILogSubject, IAsyncDisposable
{
    [Parameter]
    public string? CssClass { get; set; }

    [CascadingParameter]
    public IChartContext ChartContext { get; set; } = default!;

    [Inject]
    private Style Styles { get; set; } = default!;

    [Inject]
    public ILogger Logger { get; set; } = default!;

    private string Class => ClassBuilder.With(Styles.Container).With(CssClass).Build();
    private AsyncDisposableBox _disposable = Disposable.AsyncBox(VoidLogger.Instance);

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender) return;

        _disposable += ChartContext.OnUpdate(StateHasChanged);
    }

    private Action HandleResolutionSelect(Duration resolution) => () => ChartContext.SetResolution(resolution);

    private string Humanize(Duration resolution) => resolution.TotalMinutes switch
    {
        < 60   => $"{resolution.TotalMinutes}m",
        < 1440 => $"{resolution.TotalHours}h",
        _      => $"{resolution.TotalDays}d",
    };

    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }

    public class Style : RuleSet
    {
        public readonly CssRule Container = Rule.Class()
            .PositionAbsolute()
            .FlexRow(AlignItems.Stretch, JustifyContent.Stretch);

        public readonly CssRule Resolution = Rule.Class()
            .FlexRow(AlignItems.Center, JustifyContent.Center)
            .MarginPx(0, 5)
            .Set("cursor", "pointer");

        public readonly CssRule ActiveResolution = Rule.Class()
            .FontWeightBold();
    }
}