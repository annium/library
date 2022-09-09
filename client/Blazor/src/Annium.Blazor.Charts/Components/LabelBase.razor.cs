using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Interop;
using Annium.Core.Primitives;
using Microsoft.AspNetCore.Components;
using NodaTime;
using OneOf;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Components;

public abstract partial class LabelBase<T> : IAsyncDisposable
    // where T : ITimeSeries
{
    [Parameter, EditorRequired]
    public OneOf<string, Func<T, string>, Func<Instant, T, string>> Text { get; set; }

    [Parameter]
    public LookupMatch Match { get; set; } = LookupMatch.Exact;

    [Parameter]
    public OneOf<int, Func<T, int>, Func<IPaneContext, Instant, T, int>>? Left { get; set; }

    [Parameter]
    public OneOf<int, Func<T, int>, Func<IPaneContext, Instant, T, int>>? Right { get; set; }

    [Parameter]
    public OneOf<int, Func<T, int>, Func<IPaneContext, T, int>>? Top { get; set; }

    [Parameter]
    public OneOf<int, Func<T, int>, Func<IPaneContext, T, int>>? Bottom { get; set; }

    [Parameter]
    public string FontFamily { get; set; } = SeriesLabelFontFamily;

    [Parameter]
    public int FontSize { get; set; } = SeriesLabelFontSize;

    [Parameter]
    public OneOf<string, Func<T, string>> GetColor { get; set; } = SeriesLabelStyle;

    [CascadingParameter]
    public IChartContext ChartContext { get; set; } = default!;

    [CascadingParameter]
    internal IPaneContext PaneContext { get; set; } = default!;

    [CascadingParameter]
    public ISeriesContext SeriesContext { get; set; } = default!;

    protected AsyncDisposableBox Disposable = Annium.Core.Primitives.Disposable.AsyncBox();

    protected override void OnParametersSet()
    {
        if (Left is null && Right is null)
            throw new ArgumentException($"Either {nameof(Left)} or {nameof(Right)} must be specified");

        if (Top is null && Bottom is null)
            throw new ArgumentException($"Either {nameof(Top)} or {nameof(Bottom)} must be specified");
    }

    protected void RenderItems(Instant moment, IReadOnlyCollection<T> items)
    {
        if (items.Count == 0)
            return;

        var ctx = SeriesContext.Overlay;
        var rect = SeriesContext.Rect;

        ctx.Save();

        foreach (var item in items)
        {
            var x = GetX(Left, moment, item) ?? rect.Width.FloorInt32() - GetX(Right, moment, item)!.Value;
            var y = GetY(Top, item) ?? rect.Height.FloorInt32() - GetY(Bottom, item)!.Value;

            var text = Text.Match(
                value => value,
                get => get(item),
                get => get(moment, item)
            );
            ctx.Font = $"{FontSize}px {FontFamily}";
            ctx.FillStyle = GetColor.Match(value => value, get => get(item));
            ctx.TextBaseline = CanvasTextBaseline.middle;
            ctx.FillText(text, x, y);
        }

        ctx.Restore();
    }

    private int? GetX(OneOf<int, Func<T, int>, Func<IPaneContext, Instant, T, int>>? getter, Instant moment, T item)
    {
        if (!getter.HasValue)
            return null;

        return getter.Value.Match(
            value => value,
            get => get(item),
            get => get(PaneContext, moment, item)
        );
    }

    private int? GetY(OneOf<int, Func<T, int>, Func<IPaneContext, T, int>>? getter, T item)
    {
        if (!getter.HasValue)
            return null;

        return getter.Value.Match(
            value => value,
            get => get(item),
            get => get(PaneContext, item)
        );
    }

    public ValueTask DisposeAsync()
    {
        return Disposable.DisposeAsync();
    }
}