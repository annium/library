using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Blazor.Charts.Internal.Extensions;
using Microsoft.AspNetCore.Components;
using NodaTime;

namespace Annium.Blazor.Charts.Components;

/// <summary>
/// Provides label functionality for displaying information about a single time series data point at the current lookup position
/// </summary>
/// <typeparam name="T">The time series type that implements ITimeSeries</typeparam>
public partial class Label<T> : LabelBase<T>
    where T : ITimeSeries
{
    /// <summary>
    /// Gets or sets the source for retrieving time series data
    /// </summary>
    [Parameter, EditorRequired]
    public ISeriesSource<T> Source { get; set; } = null!;

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
    /// Handles lookup events to display label information for the data point at the specified moment
    /// </summary>
    /// <param name="moment">The moment in time to look up data for</param>
    /// <param name="_">The screen point (unused in this implementation)</param>
    private void HandleLookup(Instant? moment, Point? _)
    {
        if (moment is null)
            return;

        var item = Source.GetItem(moment.Value, Match);
        if (item is not null)
            RenderItems(moment.Value, [item]);
    }
}
