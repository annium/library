using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Charts.Internal.Data.Cache;
using Annium.Data.Models;
using Annium.Logging;
using NodaTime;

namespace Annium.Blazor.Charts.Internal.Data.Sources;

/// <summary>
/// A series source that derives its data from another series source through transformation
/// </summary>
/// <typeparam name="TS">The type of the source series data</typeparam>
/// <typeparam name="TD">The type of the derived/destination series data</typeparam>
internal class DependentSeriesSource<TS, TD> : ISeriesSource<TD>, ILogSubject
{
    /// <summary>
    /// Event raised when data loading is completed
    /// </summary>
    public event Action Loaded = delegate { };

    /// <summary>
    /// Event raised when the data bounds change
    /// </summary>
    public event Action<ValueRange<Instant>> OnBoundsChange = delegate { };

    /// <summary>
    /// Gets the logger instance for this series source
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Gets the time resolution of the series data
    /// </summary>
    public Duration Resolution => _source.Resolution;

    /// <summary>
    /// Gets a value indicating whether the series source is currently loading data
    /// </summary>
    public bool IsLoading => _source.IsLoading;

    /// <summary>
    /// Gets the time bounds of the available data
    /// </summary>
    public ValueRange<Instant> Bounds => _source.Bounds;

    /// <summary>
    /// The underlying source series that provides the base data
    /// </summary>
    private readonly ISeriesSource<TS> _source;

    /// <summary>
    /// The cache for storing transformed data
    /// </summary>
    private readonly ISeriesSourceCache<TD> _cache;

    /// <summary>
    /// Function that transforms source data into destination data
    /// </summary>
    private readonly Func<IReadOnlyList<TS>, Duration, Instant, Instant, IReadOnlyCollection<TD>> _getValues;

    /// <summary>
    /// Initializes a new instance of the DependentSeriesSource class
    /// </summary>
    /// <param name="source">The source series to derive data from</param>
    /// <param name="cache">The cache for storing transformed data</param>
    /// <param name="getValues">Function to transform source data into destination data</param>
    /// <param name="logger">Logger instance for this series source</param>
    public DependentSeriesSource(
        ISeriesSource<TS> source,
        ISeriesSourceCache<TD> cache,
        Func<IReadOnlyList<TS>, Duration, Instant, Instant, IReadOnlyCollection<TD>> getValues,
        ILogger logger
    )
    {
        Logger = logger;
        _source = source;
        _cache = cache;
        _getValues = getValues;
        _source.Loaded += TriggerLoaded;
        _cache.OnBoundsChange += TriggerBoundsChanged;
    }

    /// <summary>
    /// Gets data items within the specified time range
    /// </summary>
    /// <param name="start">The start time of the range</param>
    /// <param name="end">The end time of the range</param>
    /// <param name="data">The retrieved data items</param>
    /// <returns>True if data was successfully retrieved, false otherwise</returns>
    public bool GetItems(Instant start, Instant end, out IReadOnlyList<TD> data)
    {
        if (_cache.HasData(start, end))
        {
            this.Trace<string, string>("get data in {start} - {end}: found in cache", start.S(), end.S());
            data = _cache.GetData(start, end);

            return true;
        }

        if (!_source.GetItems(start, end, out _))
        {
            this.Trace<string, string>("get data in {start} - {end}: missing in source", start.S(), end.S());
            data = [];

            return false;
        }

        this.Trace<string, string>("get data in {start} - {end}: found in source, fill cache", start.S(), end.S());

        var emptyRanges = _cache.GetEmptyRanges(start, end);
        foreach (var range in emptyRanges)
        {
            if (!_source.GetItems(range.Start, range.End, out var rangeSource))
                throw new InvalidOperationException(
                    $"Series source {_source} invalid behavior: expected to get data in range {range.S()}"
                );

            var rangeData = _getValues(rangeSource, _source.Resolution, range.Start, range.End);
            this.Trace<int, int, string>(
                "save {rangeDataCount} item(s) ({rangeSourceCount} sourced) in {range} to cache",
                rangeData.Count,
                rangeSource.Count,
                range.S()
            );
            _cache.AddData(range.Start, range.End, rangeData);
        }

        data = _cache.GetData(start, end);
        this.Trace<string, string>("get data in {start} - {end}: served from cache", start.S(), end.S());

        return true;
    }

    /// <summary>
    /// Gets a single data item at the specified moment in time
    /// </summary>
    /// <param name="moment">The moment in time to look up</param>
    /// <param name="match">The lookup match strategy</param>
    /// <returns>The data item if found, null otherwise</returns>
    public TD? GetItem(Instant moment, LookupMatch match = LookupMatch.Exact) => _cache.GetItem(moment, match);

    /// <summary>
    /// Initiates loading of data items within the specified time range
    /// </summary>
    /// <param name="start">The start time of the range to load</param>
    /// <param name="end">The end time of the range to load</param>
    public void LoadItems(Instant start, Instant end)
    {
        // this.Trace($"get data in {start} - {end}");
        _source.LoadItems(start, end);
    }

    /// <summary>
    /// Sets the time resolution for the series data
    /// </summary>
    /// <param name="resolution">The new resolution to set</param>
    public void SetResolution(Duration resolution)
    {
        _source.SetResolution(resolution);
        _cache.SetResolution(resolution);
    }

    /// <summary>
    /// Clears all cached data from the series source
    /// </summary>
    public void Clear()
    {
        _source.Clear();
        _cache.Clear();
    }

    /// <summary>
    /// Triggers the Loaded event
    /// </summary>
    private void TriggerLoaded() => Loaded();

    /// <summary>
    /// Triggers the OnBoundsChange event with the specified bounds
    /// </summary>
    /// <param name="bounds">The new bounds to report</param>
    private void TriggerBoundsChanged(ValueRange<Instant> bounds) => OnBoundsChange(bounds);

    /// <summary>
    /// Disposes the series source and cleans up resources
    /// </summary>
    public void Dispose()
    {
        _source.Clear();
        _cache.Clear();
        _source.Loaded -= TriggerLoaded;
        _cache.OnBoundsChange -= TriggerBoundsChanged;
    }
}
