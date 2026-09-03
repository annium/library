using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Blazor.Charts.Internal.Extensions;
using Microsoft.AspNetCore.Components;
using NodaTime;

namespace Annium.Blazor.Charts.Components;

/// <summary>
/// Provides label functionality for displaying information about multiple items from a multi-value data point at the current lookup position
/// </summary>
/// <typeparam name="TValue">The multi-value type that implements IMultiValue</typeparam>
/// <typeparam name="TItem">The item type contained within the multi-value</typeparam>
public partial class MultiLabel<TValue, TItem> : LabelBase<TItem>
    where TValue : IMultiValue<TItem>
{
    /// <summary>
    /// Gets or sets the source for retrieving multi-value data
    /// </summary>
    [Parameter, EditorRequired]
    public ISeriesSource<TValue> Source { get; set; } = null!;

    /// <summary>
    /// Called after the component has been rendered
    /// </summary>
    /// <param name="firstRender">True if this is the first time the component is being rendered</param>
    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        if (!firstRender)
            return;

        Disposable += ChartContext.OnLookupChanged(HandleLookup);
    }

    /// <summary>
    /// Handles lookup events to display label information for all items in the multi-value data point at the specified moment
    /// </summary>
    /// <param name="moment">The moment in time to look up data for</param>
    /// <param name="_">The screen point (unused in this implementation)</param>
    private void HandleLookup(Instant? moment, Point? _)
    {
        if (moment is null)
            return;

        var item = Source.GetItem(moment.Value, Match);
        if (item is not null)
            RenderItems(moment.Value, item.Items);
    }
}
