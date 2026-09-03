using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Core.Runtime.Time;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Blazor.Charts.Tests.Internal.Data;

/// <summary>
/// Tests for the LoadingSeriesSource functionality
/// </summary>
public class LoadingSeriesSourceTests : TestBase
{
    /// <summary>
    /// A fixed timestamp representing the current time for tests
    /// </summary>
    private readonly Instant _now = new LocalDateTime(2020, 1, 15, 14, 20).InUtc().ToInstant();

    /// <summary>
    /// Initializes a new instance of the LoadingSeriesSourceTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public LoadingSeriesSourceTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddCharts());
    }

    /// <summary>
    /// Tests that GetItems returns false and empty items when the source has no data
    /// </summary>
    [Fact]
    public void GetItems_Empty()
    {
        // arrange
        var source = CreateSource(Array.Empty<Item>);

        // act
        var result = source.GetItems(_now - Duration.FromMinutes(5), _now, out var items);

        // assert
        result.IsFalse();
        items.IsEmpty();
    }

    /// <summary>
    /// Tests that GetItems returns true with data once a load has covered the requested range plus its buffer zone
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task GetItems_AllDataAvailable_ReturnsTrueWithData()
    {
        // arrange
        var source = CreateSource(() => Items(_now, 0, 1, 2, 3, 4, 5));
        await LoadAndWaitAsync(source, _now, _now + M(5));

        // act
        var result = source.GetItems(_now, _now + M(5), out var items);

        // assert
        result.IsTrue();
        items.IsEqual(Items(_now, 0, 1, 2, 3, 4, 5));
    }

    /// <summary>
    /// Tests that GetItems returns false when the cache has some data but not enough to cover the requested
    /// range plus its buffer zone
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task GetItems_SomeDataMissing_ReturnsFalse()
    {
        // arrange
        var source = CreateSource(() => Items(_now, 0, 1, 2, 3, 4, 5));
        await LoadAndWaitAsync(source, _now, _now + M(5));

        // act
        var result = source.GetItems(_now - M(10), _now + M(20), out var items);

        // assert
        result.IsFalse();
        items.IsEmpty();
    }

    /// <summary>
    /// Tests that LoadItems asynchronously populates the cache and raises the Loaded event once complete
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task LoadItems_PopulatesCacheAndRaisesLoaded()
    {
        // arrange
        var source = CreateSource(() => Items(_now, 0, 1, 2, 3, 4, 5));
        var loadedTcs = new TaskCompletionSource();
        source.Loaded += () => loadedTcs.TrySetResult();

        // act
        source.LoadItems(_now, _now + M(5));
        await loadedTcs.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // assert
        source.GetItems(_now, _now + M(5), out var items).IsTrue();
        items.IsEqual(Items(_now, 0, 1, 2, 3, 4, 5));
    }

    /// <summary>
    /// Tests that GetItem delegates lookups to the internal cache after data has been loaded
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task GetItem_DelegatesToCache()
    {
        // arrange
        var source = CreateSource(() => Items(_now, 0, 1, 2, 3, 4, 5));
        await LoadAndWaitAsync(source, _now, _now + M(5));

        // assert
        source.GetItem(_now + M(2))!.Moment.Is(_now + M(2));
        source.GetItem(_now - M(100)).IsDefault();
    }

    /// <summary>
    /// Tests that SetResolution with a different resolution updates Resolution and clears previously cached data
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SetResolution_DifferentResolution_ChangesResolutionAndClearsCache()
    {
        // arrange
        var source = CreateSource(() => Items(_now, 0, 1, 2, 3, 4, 5));
        await LoadAndWaitAsync(source, _now, _now + M(5));

        // act
        source.SetResolution(M(15));

        // assert
        source.Resolution.Is(M(15));
        source.GetItems(_now, _now + M(5), out var items).IsFalse();
        items.IsEmpty();
    }

    /// <summary>
    /// Tests that SetResolution with the current resolution is a no-op and does not clear cached data
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SetResolution_SameResolution_IsNoOp()
    {
        // arrange
        var source = CreateSource(() => Items(_now, 0, 1, 2, 3, 4, 5));
        await LoadAndWaitAsync(source, _now, _now + M(5));

        // act
        source.SetResolution(M(1));

        // assert
        source.Resolution.Is(M(1));
        source.GetItems(_now, _now + M(5), out var items).IsTrue();
        items.IsEqual(Items(_now, 0, 1, 2, 3, 4, 5));
    }

    /// <summary>
    /// Tests that IsLoading is true synchronously once LoadItems has been triggered and reverts to false once
    /// the load completes and Loaded has fired
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task IsLoading_TransitionsFromFalseToTrueToFalse()
    {
        // arrange
        Get<ITimeManager>().SetNow(_now);
        var sourceFactory = Get<ISeriesSourceFactory>();
        var gate = new TaskCompletionSource<IReadOnlyList<Item>>();
#pragma warning disable VSTHRD003 // gate.Task is a manually-completed synchronization primitive, not started work
        var source = sourceFactory.CreateChecked<Item>(M(1), (_, _, _) => gate.Task);
#pragma warning restore VSTHRD003
        var loadedTcs = new TaskCompletionSource();
        source.Loaded += () => loadedTcs.TrySetResult();

        // assert - not loading yet
        source.IsLoading.IsFalse();

        // act
        source.LoadItems(_now, _now + M(4));

        // assert - loading synchronously, since the load function is gated and hasn't completed yet
        source.IsLoading.IsTrue();

        // act
        gate.SetResult(Items(_now, 0, 1, 2, 3, 4));
        await loadedTcs.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // assert - loading completed
        source.IsLoading.IsFalse();
    }

    /// <summary>
    /// Tests that disposing an already-disposed source throws, and that a single Dispose call succeeds
    /// </summary>
    [Fact]
    public void Dispose_CalledTwice_SecondCallThrows()
    {
        // arrange
        var source = CreateSource(Array.Empty<Item>);

        // act
        source.Dispose();

        // assert
        Wrap.It(() => source.Dispose()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Candidate bug: <c>LoadDataAsync</c> sets <c>_isLoading</c> to 1 at the start and only resets it to 0 at the
    /// very end, without a try/finally. When the load function throws, the reset is skipped and IsLoading is left
    /// stuck at true forever. This test pins the intended behavior (IsLoading eventually returns to false even
    /// after a failed load).
    /// </summary>
    [Fact]
    public void LoadItems_LoadFails_ResetsIsLoading()
    {
        // arrange
        Get<ITimeManager>().SetNow(_now);
        var sourceFactory = Get<ISeriesSourceFactory>();
        var source = sourceFactory.CreateChecked<Item>(
            M(1),
            (_, _, _) => Task.FromException<IReadOnlyList<Item>>(new InvalidOperationException("boom"))
        );

        // act
        source.LoadItems(_now, _now + M(4));
        SpinWait.SpinUntil(() => Logs.Any(x => x.Exception is not null), TimeSpan.FromSeconds(5)).IsTrue();

        // assert
        source.IsLoading.IsFalse();
    }

    /// <summary>
    /// Candidate bug: two overlapping LoadItems calls, triggered before either completes, both compute their empty
    /// ranges against the same (still empty) cache snapshot. When both loads complete, the second AddData collides
    /// with the first and throws, so only one of the two loads actually succeeds. This test pins the intended
    /// behavior (both overlapping loads complete successfully).
    /// </summary>
    [Fact]
    public void LoadItems_ConcurrentOverlappingLoads_BothComplete()
    {
        // arrange
        Get<ITimeManager>().SetNow(_now);
        var sourceFactory = Get<ISeriesSourceFactory>();
        var calls = 0;
        var gate1 = new TaskCompletionSource<IReadOnlyList<Item>>();
        var gate2 = new TaskCompletionSource<IReadOnlyList<Item>>();
        var source = sourceFactory.CreateChecked<Item>(
            M(1),
            (_, _, _) => Interlocked.Increment(ref calls) == 1 ? gate1.Task : gate2.Task
        );
        var loadedCount = 0;
        source.Loaded += () => Interlocked.Increment(ref loadedCount);

        // act
        source.LoadItems(_now, _now + M(4));
        source.LoadItems(_now, _now + M(4));
        gate1.SetResult(Items(_now, 0, 1, 2, 3, 4));
        gate2.SetResult(Items(_now, 0, 1, 2, 3, 4));
        SpinWait.SpinUntil(() => loadedCount + Logs.Count(x => x.Exception is not null) >= 2, TimeSpan.FromSeconds(5));

        // assert
        loadedCount.Is(2);
    }

    /// <summary>
    /// Creates a test series source with the specified data provider
    /// </summary>
    /// <param name="getItems">Function that provides the items for the source</param>
    /// <returns>A configured series source for testing</returns>
    private ISeriesSource<Item> CreateSource(Func<IReadOnlyList<Item>> getItems)
    {
        Get<ITimeManager>().SetNow(_now);

        var sourceFactory = Get<ISeriesSourceFactory>();
        var source = sourceFactory.CreateChecked(Duration.FromMinutes(1), (_, _, _) => Task.FromResult(getItems()));

        return source;
    }

    /// <summary>
    /// Triggers a load on the given source and asynchronously waits for its Loaded event, deterministically
    /// </summary>
    /// <param name="source">The source to load</param>
    /// <param name="start">The start of the range to load</param>
    /// <param name="end">The end of the range to load</param>
    /// <returns>A task that completes once the load has finished.</returns>
    private static async Task LoadAndWaitAsync(ISeriesSource<Item> source, Instant start, Instant end)
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
    /// Creates a list of test items with specified time offsets
    /// </summary>
    /// <param name="start">The starting instant</param>
    /// <param name="offsets">Array of minute offsets from start</param>
    /// <returns>A list of test items</returns>
    private static IReadOnlyList<Item> Items(Instant start, params int[] offsets)
    {
        var items = new List<Item>();

        foreach (var offset in offsets)
            items.Add(new(start + M(offset)));

        return items;
    }

    /// <summary>
    /// Creates a Duration from minutes
    /// </summary>
    /// <param name="minutes">The number of minutes</param>
    /// <returns>A Duration representing the specified minutes</returns>
    private static Duration M(int minutes) => Duration.FromMinutes(minutes);

    /// <summary>
    /// A test item that implements ITimeSeries and IComparable for testing purposes
    /// </summary>
    /// <param name="Moment">The timestamp of the item</param>
    private sealed record Item(Instant Moment) : ITimeSeries, IComparable<Item>
    {
        /// <summary>
        /// Compares this item to another item by their timestamps
        /// </summary>
        /// <param name="other">The other item to compare to</param>
        /// <returns>A value indicating the relative order of the items</returns>
        public int CompareTo(Item? other) =>
            Moment.CompareTo(other?.Moment ?? throw new InvalidOperationException($"Can't compare {this} to null"));
    }
}
