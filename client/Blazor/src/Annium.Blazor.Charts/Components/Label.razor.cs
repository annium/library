using System;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Blazor.Interop;
using Annium.Core.Primitives;
using Microsoft.AspNetCore.Components;
using NodaTime;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Components;

public partial class Label<TData> : IAsyncDisposable
    where TData : ITimeSeries
{
    [Parameter, EditorRequired]
    public ISeriesSource<TData> Source { get; set; } = default!;

    [Parameter, EditorRequired]
    public Func<TData, string> GetText { get; set; } = default!;

    [Parameter]
    public int? Left { get; set; }

    [Parameter]
    public int? Right { get; set; }

    [Parameter]
    public int? Top { get; set; }

    [Parameter]
    public int? Bottom { get; set; }

    [Parameter, EditorRequired]
    public int Width { get; set; }

    [Parameter, EditorRequired]
    public int Height { get; set; }

    [Parameter]
    public string LabelFontFamily { get; set; } = SeriesLabelFontFamily;

    [Parameter]
    public int LabelFontSize { get; set; } = SeriesLabelFontSize;

    [Parameter]
    public string LabelStyle { get; set; } = SeriesLabelStyle;

    [CascadingParameter]
    public IChartContext ChartContext { get; set; } = default!;

    [CascadingParameter]
    public ISeriesContext SeriesContext { get; set; } = default!;

    private AsyncDisposableBox _disposable = Disposable.AsyncBox();

    protected override void OnParametersSet()
    {
        if (!Left.HasValue && !Right.HasValue)
            throw new ArgumentException($"Either {nameof(Left)} or {nameof(Right)} must be specified");

        if (!Top.HasValue && !Bottom.HasValue)
            throw new ArgumentException($"Either {nameof(Top)} or {nameof(Bottom)} must be specified");
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender) return;

        _disposable += ChartContext.OnLookupChanged(HandleLookup);
    }

    private void HandleLookup(Instant? moment, Point? _)
    {
        if (moment is null)
            return;

        var item = Source.GetItem(moment.Value);
        if (item is null)
            return;

        var ctx = SeriesContext.Overlay;
        var rect = SeriesContext.Rect;

        ctx.ClearRect(0, 0, Width, Height);

        var x = Left ?? rect.Width.FloorInt32() - Right!.Value;
        var y = Top ?? rect.Height.FloorInt32() - Bottom!.Value;

        ctx.Save();

        var text = GetText(item);
        ctx.Font = $"{LabelFontSize}px {LabelFontFamily}";
        ctx.FillStyle = LabelStyle;
        ctx.TextBaseline = CanvasTextBaseline.middle;
        ctx.FillText(text, x, y);

        ctx.Restore();
    }

    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }
}