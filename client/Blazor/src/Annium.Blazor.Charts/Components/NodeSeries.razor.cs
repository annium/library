using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Interop;
using Annium.Logging;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Charts.Components;

public partial class NodeSeries<T> : SeriesBase<T>, ILogSubject
    where T : IPointValue
{
    public delegate void Renderer(T item, Canvas ctx, int x, int y);

    [Parameter, EditorRequired]
    public Renderer RenderItem { get; set; } = delegate { };

    [Inject]
    public ILogger Logger { get; set; } = null!;

    protected override int MinValuesToRender => 1;

    protected override void RenderValues(IReadOnlyList<T> items)
    {
        foreach (var item in items)
            RenderItem(item, SeriesContext.Canvas, PaneContext.ToX(item.Moment), PaneContext.ToY(item.Value));
    }

    protected override (decimal min, decimal max) GetBounds(IReadOnlyList<T> items)
    {
        var min = decimal.MaxValue;
        var max = decimal.MinValue;

        foreach (var item in items)
        {
            min = Math.Min(min, item.Value);
            max = Math.Max(max, item.Value);
        }

        return (min, max);
    }
}
