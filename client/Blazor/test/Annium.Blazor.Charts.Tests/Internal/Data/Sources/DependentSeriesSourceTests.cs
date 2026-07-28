using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Core.Runtime.Time;
using Annium.Data.Models;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Blazor.Charts.Tests.Internal.Data.Sources;

/// <summary>
/// Tests for the DependentSeriesSource functionality
/// </summary>
public class DependentSeriesSourceTests : TestBase
{
    /// <summary>
    /// A fixed timestamp representing the current time for tests
    /// </summary>
    private readonly Instant _now = new LocalDateTime(2020, 1, 15, 14, 20).InUtc().ToInstant();

    /// <summary>
    /// Initializes a new instance of the DependentSeriesSourceTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public DependentSeriesSourceTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddCharts());
    }

    /// <summary>
    /// Tests that GetItems returns false and empty data when the underlying source has no data for the range
    /// </summary>
    [Fact]
    public void GetItems_SourceMissing_ReturnsFalse()
    {
        // arrange
        var source = CreateSource(Array.Empty<SourceItem>);
        var dependent = CreateDependent(source, Transform);

        // act
        var result = dependent.GetItems(_now, _now + M(4), out var data);

        // assert
        result.IsFalse();
        data.IsEmpty();
    }

    /// <summary>
    /// Tests that GetItems transforms and caches source data on first call when the cache is empty but the source has data
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task GetItems_SourceHasData_CacheEmpty_TransformsAndFillsCache()
    {
        // arrange
        var source = CreateSource(() => Items(_now, 0, 1, 2, 3, 4));
        await LoadAndWaitAsync(source, _now, _now + M(4));
        var dependent = CreateDependent(source, Transform);

        // act
        var result = dependent.GetItems(_now, _now + M(4), out var data);

        // assert
        result.IsTrue();
        data.Select(x => x.Moment).ToArray().IsEqual(Items(_now, 0, 1, 2, 3, 4).Select(x => x.Moment).ToArray());
    }

    /// <summary>
    /// Tests that a second call for the same range is served from cache without re-invoking the transform function
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task GetItems_CacheHit_DoesNotRecomputeTransform()
    {
        // arrange
        var source = CreateSource(() => Items(_now, 0, 1, 2, 3, 4));
        await LoadAndWaitAsync(source, _now, _now + M(4));
        var calls = 0;
        var dependent = CreateDependent(
            source,
            (chunk, resolution, start, end) =>
            {
                calls++;
                return Transform(chunk, resolution, start, end);
            }
        );

        // act
        var first = dependent.GetItems(_now, _now + M(4), out var firstData);
        var second = dependent.GetItems(_now, _now + M(4), out var secondData);

        // assert
        first.IsTrue();
        second.IsTrue();
        calls.Is(1);
        secondData.Select(x => x.Moment).ToArray().IsEqual(firstData.Select(x => x.Moment).ToArray());
    }

    /// <summary>
    /// Tests that GetItem delegates lookups to the internal cache after data has been transformed and cached
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task GetItem_DelegatesToCache()
    {
        // arrange
        var source = CreateSource(() => Items(_now, 0, 1, 2, 3, 4));
        await LoadAndWaitAsync(source, _now, _now + M(4));
        var dependent = CreateDependent(source, Transform);
        dependent.GetItems(_now, _now + M(4), out _);

        // assert
        dependent.GetItem(_now + M(2))!.Moment.Is(_now + M(2));
        dependent.GetItem(_now - M(1)).IsDefault();
    }

    /// <summary>
    /// Tests that SetResolution forwards to both the underlying source and the cache, clearing previously cached data
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SetResolution_ForwardsToSourceAndCache()
    {
        // arrange
        var source = CreateSource(() => Items(_now, 0, 1, 2, 3, 4));
        await LoadAndWaitAsync(source, _now, _now + M(4));
        var dependent = CreateDependent(source, Transform);
        dependent.GetItems(_now, _now + M(4), out _);

        // act
        dependent.SetResolution(M(2));

        // assert
        dependent.Resolution.Is(M(2));
        dependent.GetItems(_now, _now + M(4), out var data).IsFalse();
        data.IsEmpty();
    }

    /// <summary>
    /// Tests that Clear removes data from both the underlying source and the cache
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Clear_ClearsSourceAndCache()
    {
        // arrange
        var source = CreateSource(() => Items(_now, 0, 1, 2, 3, 4));
        await LoadAndWaitAsync(source, _now, _now + M(4));
        var dependent = CreateDependent(source, Transform);
        dependent.GetItems(_now, _now + M(4), out _);

        // act
        dependent.Clear();

        // assert
        dependent.GetItems(_now, _now + M(4), out var data).IsFalse();
        data.IsEmpty();
    }

    /// <summary>
    /// Tests that the Loaded event of the underlying source is relayed through the dependent source
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Loaded_RelaysFromSource()
    {
        // arrange
        var source = CreateSource(() => Items(_now, 0, 1, 2, 3, 4));
        var dependent = CreateDependent(source, Transform);
        var tcs = new TaskCompletionSource();
        dependent.Loaded += () => tcs.TrySetResult();

        // act
        dependent.LoadItems(_now, _now + M(4));
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // assert
        tcs.Task.IsCompletedSuccessfully.IsTrue();
    }

    /// <summary>
    /// Tests that the OnBoundsChange event of the cache is relayed through the dependent source when data is first cached
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task OnBoundsChange_RelaysFromCache()
    {
        // arrange
        var source = CreateSource(() => Items(_now, 0, 1, 2, 3, 4));
        await LoadAndWaitAsync(source, _now, _now + M(4));
        var dependent = CreateDependent(source, Transform);
        ValueRange<Instant>? reported = null;
        dependent.OnBoundsChange += bounds => reported = bounds;

        // act
        dependent.GetItems(_now, _now + M(4), out _);

        // assert
        reported.IsNotNull();
        reported!.Start.Is(_now);
        reported.End.Is(_now + M(4));
    }

    /// <summary>
    /// Creates a checked series source for testing, backed by a synchronous item provider
    /// </summary>
    /// <param name="getItems">Function that provides the items for the source</param>
    /// <returns>A configured series source for testing</returns>
    private ISeriesSource<SourceItem> CreateSource(Func<IReadOnlyList<SourceItem>> getItems)
    {
        Get<ITimeManager>().SetNow(_now);

        var sourceFactory = Get<ISeriesSourceFactory>();

        return sourceFactory.CreateChecked(M(1), (_, _, _) => Task.FromResult(getItems()));
    }

    /// <summary>
    /// Creates a dependent series source wrapping the given source with the given transform
    /// </summary>
    /// <param name="source">The underlying source to depend on</param>
    /// <param name="getValues">The transform function from source items to destination items</param>
    /// <returns>A configured dependent series source for testing</returns>
    private ISeriesSource<Item> CreateDependent(
        ISeriesSource<SourceItem> source,
        Func<IReadOnlyList<SourceItem>, Duration, Instant, Instant, IReadOnlyCollection<Item>> getValues
    ) => Get<ISeriesSourceFactory>().CreateChecked(source, getValues);

    /// <summary>
    /// Triggers a load on the given source and asynchronously waits for its Loaded event, deterministically
    /// </summary>
    /// <param name="source">The source to load</param>
    /// <param name="start">The start of the range to load</param>
    /// <param name="end">The end of the range to load</param>
    /// <returns>A task that completes once the load has finished.</returns>
    private static async Task LoadAndWaitAsync(ISeriesSource<SourceItem> source, Instant start, Instant end)
    {
        var tcs = new TaskCompletionSource();
        void OnLoaded() => tcs.TrySetResult();
        source.Loaded += OnLoaded;
        try
        {
            source.LoadItems(start, end);
            await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        }
        finally
        {
            source.Loaded -= OnLoaded;
        }
    }

    /// <summary>
    /// A one-to-one transform from source items to destination items, preserving the moment
    /// </summary>
    /// <param name="chunk">The chunk of source items available for the requested range</param>
    /// <param name="resolution">The resolution of the source</param>
    /// <param name="start">The start of the requested range</param>
    /// <param name="end">The end of the requested range</param>
    /// <returns>The transformed destination items</returns>
    private static IReadOnlyCollection<Item> Transform(
        IReadOnlyList<SourceItem> chunk,
        Duration resolution,
        Instant start,
        Instant end
    ) => chunk.Select(x => new Item(x.Moment)).ToArray();

    /// <summary>
    /// Creates a list of test source items with specified time offsets
    /// </summary>
    /// <param name="start">The starting instant</param>
    /// <param name="offsets">Array of minute offsets from start</param>
    /// <returns>A list of test source items</returns>
    private static IReadOnlyList<SourceItem> Items(Instant start, params int[] offsets)
    {
        var items = new List<SourceItem>();

        foreach (var offset in offsets)
            items.Add(new SourceItem(start + M(offset)));

        return items;
    }

    /// <summary>
    /// Creates a Duration from minutes
    /// </summary>
    /// <param name="minutes">The number of minutes</param>
    /// <returns>A Duration representing the specified minutes</returns>
    private static Duration M(int minutes) => Duration.FromMinutes(minutes);

    /// <summary>
    /// A test source item that implements ITimeSeries for testing purposes
    /// </summary>
    /// <param name="Moment">The timestamp of the item</param>
    private sealed record SourceItem(Instant Moment) : ITimeSeries;

    /// <summary>
    /// A test destination item that implements ITimeSeries for testing purposes
    /// </summary>
    /// <param name="Moment">The timestamp of the item</param>
    private sealed record Item(Instant Moment) : ITimeSeries;
}
