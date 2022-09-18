using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Interop;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Charts.Components;

public partial class MultiNodeSeries<TM, TI> : SeriesBase<TM>, ILogSubject<MultiNodeSeries<TM, TI>>
    where TM : IMultiValue<TI>
    where TI : IPointItem
{
    [Parameter, EditorRequired]
    public Action<Canvas, int, int> RenderItem { get; set; } = delegate { };

    [Inject]
    public ILogger<MultiNodeSeries<TM, TI>> Logger { get; set; } = default!;

    protected override int MinValuesToRender => 1;

    protected override void RenderValues(IReadOnlyList<TM> values)
    {
        foreach (var value in values)
        foreach (var item in value.Values)
            RenderItem(SeriesContext.Canvas, PaneContext.ToX(value.Moment), PaneContext.ToY(item.Value));
    }

    protected override (decimal min, decimal max) GetBounds(IReadOnlyList<TM> values)
    {
        var min = decimal.MaxValue;
        var max = decimal.MinValue;

        foreach (var value in values)
        {
            min = Math.Min(min, value.Values.Min(x => x.Value));
            max = Math.Max(max, value.Values.Max(x => x.Value));
        }

        return (min, max);
    }
}