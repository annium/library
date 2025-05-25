using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Blazor.Charts.Internal.Extensions;
using Microsoft.AspNetCore.Components;
using NodaTime;

namespace Annium.Blazor.Charts.Components;

public partial class MultiLabel<TValue, TItem> : LabelBase<TItem>
    where TValue : IMultiValue<TItem>
{
    [Parameter, EditorRequired]
    public ISeriesSource<TValue> Source { get; set; } = null!;

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        if (!firstRender)
            return;

        Disposable += ChartContext.OnLookupChanged(HandleLookup);
    }

    private void HandleLookup(Instant? moment, Point? _)
    {
        if (moment is null)
            return;

        var item = Source.GetItem(moment.Value, Match);
        if (item is not null)
            RenderItems(moment.Value, item.Items);
    }
}
