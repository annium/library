using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Blazor.Interop;
using Annium.Core.Primitives;
using Microsoft.AspNetCore.Components;
using static Annium.Blazor.Charts.Internal.Constants;

namespace Annium.Blazor.Charts.Components;

public abstract partial class LabelBase<T> : IAsyncDisposable
    where T : ITimeSeries
{
    [Parameter, EditorRequired]
    public Func<T, string> GetText { get; set; } = default!;

    [Parameter]
    public LookupMatch Match { get; set; } = LookupMatch.Exact;

    [Parameter]
    public Func<T, int>? GetLeft { get; set; }

    [Parameter]
    public Func<T, int>? GetRight { get; set; }

    [Parameter]
    public Func<T, int>? GetTop { get; set; }

    [Parameter]
    public Func<T, int>? GetBottom { get; set; }

    [Parameter]
    public string FontFamily { get; set; } = SeriesLabelFontFamily;

    [Parameter]
    public int FontSize { get; set; } = SeriesLabelFontSize;

    [Parameter]
    public Func<T, string> GetColor { get; set; } = _ => SeriesLabelStyle;

    [CascadingParameter]
    public IChartContext ChartContext { get; set; } = default!;

    [CascadingParameter]
    public ISeriesContext SeriesContext { get; set; } = default!;

    protected AsyncDisposableBox Disposable = Annium.Core.Primitives.Disposable.AsyncBox();

    protected override void OnParametersSet()
    {
        if (GetLeft is null && GetRight is null)
            throw new ArgumentException($"Either {nameof(GetLeft)} or {nameof(GetRight)} must be specified");

        if (GetTop is null && GetBottom is null)
            throw new ArgumentException($"Either {nameof(GetTop)} or {nameof(GetBottom)} must be specified");
    }

    protected void RenderItems(IReadOnlyCollection<T> items)
    {
        if (items.Count == 0)
            return;

        var ctx = SeriesContext.Overlay;
        var rect = SeriesContext.Rect;

        ctx.Save();

        foreach (var item in items)
        {
            var x = GetLeft?.Invoke(item) ?? rect.Width.FloorInt32() - GetRight!(item);
            var y = GetTop?.Invoke(item) ?? rect.Width.FloorInt32() - GetBottom!(item);


            var text = GetText(item);
            ctx.Font = $"{FontSize}px {FontFamily}";
            ctx.FillStyle = GetColor(item);
            ctx.TextBaseline = CanvasTextBaseline.middle;
            ctx.FillText(text, x, y);
        }

        ctx.Restore();
    }

    public ValueTask DisposeAsync()
    {
        return Disposable.DisposeAsync();
    }
}