using System;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Blazor.Core.Tools;
using Annium.Blazor.Css;
using Annium.Logging;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Charts.Components;

public partial class Zoom : ILogSubject, IAsyncDisposable
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
        if (!firstRender)
            return;

        _disposable += ChartContext.OnUpdate(StateHasChanged);
    }

    private void HandleZoomIn() => ChartContext.ZoomIn();
    private void HandleZoomOut() => ChartContext.ZoomOut();

    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }

    public class Style : RuleSet
    {
        public readonly CssRule Container = Rule.Class()
            .PositionAbsolute()
            .FlexColumn(AlignItems.Stretch, JustifyContent.Stretch)
            .Set("user-select", "none");

        public readonly CssRule Button = Rule.Class()
            .FlexRow(AlignItems.Center, JustifyContent.Center)
            .WidthRem(2)
            .HeightRem(2)
            .FontSizeRem(1.5)
            .Border("1px solid black")
            .Set("cursor", "pointer");
    }
}