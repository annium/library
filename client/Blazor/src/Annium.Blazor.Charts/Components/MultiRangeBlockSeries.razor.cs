using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Interop;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Components;
using NodaTime;

namespace Annium.Blazor.Charts.Components;

public partial class MultiRangeBlockSeries<TM, TI> : ILogSubject<MultiRangeBlockSeries<TM, TI>>, IAsyncDisposable
    where TM : MultiRangeValue<TI>
    where TI : RangeItem
{
    [Parameter, EditorRequired]
    public ISeriesSource<TM> Source { get; set; } = default!;

    [Parameter, EditorRequired]
    public Func<TI, string> GetItemColor { get; set; } = delegate { return string.Empty; };

    [CascadingParameter]
    public IChartContext ChartContext { get; set; } = default!;

    [CascadingParameter]
    internal IPaneContext PaneContext { get; set; } = default!;

    [CascadingParameter]
    internal ISeriesContext SeriesContext { get; set; } = default!;

    [Inject]
    public ILogger<MultiRangeBlockSeries<TM, TI>> Logger { get; set; } = default!;

    private Action _unregisterSource = delegate { };
    private Action _unregisterRender = delegate { };

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        var source = Source;

        await base.SetParametersAsync(parameters);

        if (Source != source)
        {
            this.Log().Trace("update {oldSource} -> {newSource}", source.GetFullId(), Source.GetFullId());
            _unregisterSource();
            _unregisterSource = PaneContext.RegisterSource(Source);
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;

        this.Log().Trace("register Draw");
        _unregisterRender = Source.RenderTo(ChartContext, Render);
    }

    private void Render(IReadOnlyList<TM> items)
    {
        if (items.Count == 0)
        {
            this.Log().Trace("No items to render");
            return;
        }

        var (min, max) = GetBounds(items);

        // if range is changed, redraw will be triggered
        if (PaneContext.AdjustRange(Source, min, max))
        {
            this.Log().Trace("adjusted to range {min} - {max}, wait for redraw", min, max);
            return;
        }

        this.Log().Trace("render {count} in range {min} - {max}", items.Count, min, max);
        var ctx = SeriesContext.Canvas;

        ctx.Save();

        for (var i = 0; i < items.Count - 1; i++)
            RenderItem(ctx, items[i], items[i + 1].Moment);
        RenderItem(ctx, items[^1], ChartContext.View.End);

        ctx.Restore();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RenderItem(
        Canvas ctx,
        TM item,
        Instant to
    )
    {
        var left = PaneContext.ToX(item.Moment);
        var right = PaneContext.ToX(to);
        var width = right - left;


        foreach (var range in item.Ranges)
        {
            ctx.FillStyle = GetItemColor(range);
            var low = PaneContext.ToY(range.Low);
            var high = PaneContext.ToY(range.High);

            ctx.FillRect(left, high, width, low - high);
        }
    }

    private (decimal min, decimal max) GetBounds(IReadOnlyList<TM> items)
    {
        var min = decimal.MaxValue;
        var max = decimal.MinValue;

        foreach (var item in items)
        {
            min = Math.Min(min, item.Ranges.Min(x => x.Low));
            max = Math.Max(max, item.Ranges.Max(x => x.High));
        }

        return (min, max);
    }

    public ValueTask DisposeAsync()
    {
        _unregisterSource();
        _unregisterRender();

        return ValueTask.CompletedTask;
    }
}