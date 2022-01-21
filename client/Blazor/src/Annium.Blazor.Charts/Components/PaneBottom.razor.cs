using System;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Blazor.Core.Tools;
using Annium.Blazor.Css;
using Annium.Blazor.Interop;
using Annium.Core.Primitives;
using Microsoft.AspNetCore.Components;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Components;

public partial class PaneBottom : IAsyncDisposable
{
    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    [CascadingParameter]
    internal IChartContext ChartContext { get; set; } = default!;

    [CascadingParameter]
    internal IPaneContext PaneContext { get; set; } = default!;

    [Inject]
    private IManagedHorizontalSideContext SideContext { get; set; } = default!;

    private string Class => ClassBuilder.With(_style.Block).With(CssClass).Build();

    private Div _block = default!;
    private Canvas _canvas = default!;
    private Canvas _overlay = default!;
    private AsyncDisposableBox _disposable = Disposable.AsyncBox();

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender) return;

        var rect = _block.GetBoundingClientRect();
        _overlay.Width = _canvas.Width = rect.Width.CeilInt32();
        _overlay.Height = _canvas.Height = rect.Height.CeilInt32();
        SideContext.Init(_canvas, _overlay, rect);
        PaneContext.SetBottom(SideContext);

        _disposable += ChartContext.OnUpdate(Draw);
        _disposable += _block;
        _disposable += _canvas;
        _disposable += _overlay;
    }

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

    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }

    public class Style : RuleSet
    {
        public readonly CssRule Block = Rule.Class()
            .PositionRelative()
            .Set("grid-area", "2 / 1 / 3 / 2")
            .Set("line-height", "0");

        public readonly CssRule Canvas = Rule.Class()
            .PositionAbsolute()
            .LeftPx(0)
            .RightPx(0)
            .TopPx(0)
            .BottomPx(0);
    }
}