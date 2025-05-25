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

public partial class PaneSeries : ILogSubject, IAsyncDisposable
{
    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; } = null!;

    [CascadingParameter]
    internal IChartContext ChartContext { get; set; } = null!;

    [CascadingParameter]
    internal IManagedPaneContext PaneContext { get; set; } = null!;

    [Inject]
    private IManagedSeriesContext SeriesContext { get; set; } = null!;

    [Inject]
    private Style Styles { get; set; } = null!;

    [Inject]
    public ILogger Logger { get; set; } = null!;

    private string Class => ClassBuilder.With(Styles.Block).With(CssClass).Build();

    private Div _block = null!;
    private Canvas _canvas = null!;
    private Canvas _overlay = null!;
    private AsyncDisposableBox _disposable = Disposable.AsyncBox(VoidLogger.Instance);

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

    private void SetSize()
    {
        var rect = _block.GetBoundingClientRect();
        _overlay.Width = _canvas.Width = rect.Width.CeilInt32();
        _overlay.Height = _canvas.Height = rect.Height.CeilInt32();
        PaneContext.SetRect(rect);
        SeriesContext.SetRect(rect);
    }

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

    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }

    public class Style : RuleSet
    {
        public readonly CssRule Block = Rule.Class()
            .DisplayFlex()
            .FlexColumn(AlignItems.Start, JustifyContent.Start)
            .PositionRelative()
            .Set("grid-area", "1 / 1 / 2 / 2")
            .Set("line-height", "0");

        public readonly CssRule Canvas = Rule.Class().PositionAbsolute().LeftPx(0).RightPx(0).TopPx(0).BottomPx(0);
    }
}
