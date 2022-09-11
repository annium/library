using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Extensions;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Components;
using OneOf;

namespace Annium.Blazor.Charts.Components;

public partial class MultiLineSeries<TM, TI> : ILogSubject<MultiLineSeries<TM, TI>>, IAsyncDisposable
    where TM : IMultiValue<TI>
    where TI : IPointItem
{
    [Parameter, EditorRequired]
    public ISeriesSource<TM> Source { get; set; } = default!;

    [Parameter, EditorRequired]
    public OneOf<string, Func<TI, string>> ItemColor { get; set; }

    [Parameter]
    public bool Centered { get; set; }

    [Parameter]
    public bool ContinueLast { get; set; }

    [CascadingParameter]
    public IChartContext ChartContext { get; set; } = default!;

    [CascadingParameter]
    internal IPaneContext PaneContext { get; set; } = default!;

    [CascadingParameter]
    internal ISeriesContext SeriesContext { get; set; } = default!;

    [Inject]
    public ILogger<MultiLineSeries<TM, TI>> Logger { get; set; } = default!;

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
        var width = GetWidth();
        var offset = Centered ? width == 1 ? 0 : ((double) width / 2).CeilInt32() : 0;
        var lastMoment = ContinueLast ? ChartContext.View.End : ChartContext.FromX(ChartContext.ToX(items[^1].Moment) + width);
        var ctx = SeriesContext.Canvas;

        ctx.Save();

        // for (var i = 0; i < items.Count - 1; i++)
        //     RenderItem(ctx, items[i], items[i + 1].Moment, offset);
        // RenderItem(ctx, items[^1], lastMoment, offset);

        ctx.Restore();
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // private void RenderItem(
    //     Canvas ctx,
    //     TM item,
    //     Instant to,
    //     int offset
    // )
    // {
    //     var left = PaneContext.ToX(item.Moment);
    //     var right = PaneContext.ToX(to);
    //     var width = right - left;
    //
    //     foreach (var range in item.Values)
    //     {
    //         ctx.FillStyle = ItemColor.Match(value => value, get => get(range));
    //         var low = PaneContext.ToY(range.Low);
    //         var high = PaneContext.ToY(range.High);
    //
    //         ctx.FillRect(left - offset, high, width, low - high);
    //     }
    // }

    private (decimal min, decimal max) GetBounds(IReadOnlyList<TM> items)
    {
        var min = decimal.MaxValue;
        var max = decimal.MinValue;

        foreach (var item in items)
        {
            min = Math.Min(min, item.Values.Min(x => x.Value));
            max = Math.Max(max, item.Values.Max(x => x.Value));
        }

        return (min, max);
    }

    private int GetWidth()
    {
        var width = ChartContext.PxPerResolution.Above(1);

        return width % 2 == 1 ? width : width - 1;
    }

    public ValueTask DisposeAsync()
    {
        _unregisterSource();
        _unregisterRender();

        return ValueTask.CompletedTask;
    }
}