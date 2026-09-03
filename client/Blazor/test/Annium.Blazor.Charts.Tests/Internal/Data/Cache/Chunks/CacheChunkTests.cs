using System;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Internal.Data.Cache.Chunks;
using Annium.Blazor.Charts.Internal.Data.Comparers;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Blazor.Charts.Tests.Internal.Data.Cache.Chunks;

/// <summary>
/// Tests for CacheChunkBase and CheckedCacheChunk boundary validation and merging
/// </summary>
public class CacheChunkTests : TestBase
{
    /// <summary>
    /// A fixed moment in time used as a reference point for tests
    /// </summary>
    private readonly Instant _moment = new LocalDateTime(2020, 1, 15, 14, 20).InUtc().ToInstant();

    /// <summary>
    /// Initializes a new instance of the CacheChunkTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public CacheChunkTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddCharts());
    }

    /// <summary>
    /// Tests that constructing a chunk whose start is after its end throws
    /// </summary>
    [Fact]
    public void Constructor_StartAfterEnd_Throws()
    {
        // act & assert
        Wrap.It(() => new UncheckedCacheChunk<Item>(_moment + M(1), _moment, [], TimeSeriesComparer<Item>.Default))
            .Throws<InvalidOperationException>()
            .Reports("invalid");
    }

    /// <summary>
    /// Tests that a CheckedCacheChunk with no items does not throw, since there is nothing to validate against bounds
    /// </summary>
    [Fact]
    public void CheckedCacheChunk_NoItems_DoesNotThrow()
    {
        // act
        var chunk = new CheckedCacheChunk<Item>(_moment, _moment + M(2), []);

        // assert
        chunk.Items.IsEmpty();
        chunk.Range.Start.Is(_moment);
        chunk.Range.End.Is(_moment + M(2));
    }

    /// <summary>
    /// Tests that a CheckedCacheChunk throws when an item's moment falls before the chunk's start
    /// </summary>
    [Fact]
    public void CheckedCacheChunk_ItemBeforeStart_Throws()
    {
        // arrange
        var items = new[] { new Item(_moment - M(1)), new Item(_moment + M(1)) };

        // act & assert
        Wrap.It(() => new CheckedCacheChunk<Item>(_moment, _moment + M(2), items))
            .Throws<InvalidOperationException>()
            .Reports("goes before start");
    }

    /// <summary>
    /// Tests that a CheckedCacheChunk throws when an item's moment falls after the chunk's end
    /// </summary>
    [Fact]
    public void CheckedCacheChunk_ItemAfterEnd_Throws()
    {
        // arrange
        var items = new[] { new Item(_moment), new Item(_moment + M(3)) };

        // act & assert
        Wrap.It(() => new CheckedCacheChunk<Item>(_moment, _moment + M(2), items))
            .Throws<InvalidOperationException>()
            .Reports("goes after end");
    }

    /// <summary>
    /// Tests that Append extends the chunk's range to the appended chunk's end and merges items in sorted order
    /// </summary>
    [Fact]
    public void Append_ExtendsRangeAndMergesItemsInSortedOrder()
    {
        // arrange
        var comparer = TimeSeriesComparer<Item>.Default;
        var first = new UncheckedCacheChunk<Item>(
            _moment,
            _moment + M(1),
            [new Item(_moment + M(1)), new Item(_moment)],
            comparer
        );
        var second = new UncheckedCacheChunk<Item>(
            _moment + M(2),
            _moment + M(3),
            [new Item(_moment + M(3)), new Item(_moment + M(2))],
            comparer
        );

        // act
        first.Append(second);

        // assert
        first.Range.Start.Is(_moment);
        first.Range.End.Is(_moment + M(3));
        first.Items.IsEqual(
            new[] { new Item(_moment), new Item(_moment + M(1)), new Item(_moment + M(2)), new Item(_moment + M(3)) }
        );
    }

    /// <summary>
    /// Creates a Duration from minutes
    /// </summary>
    /// <param name="minutes">The number of minutes</param>
    /// <returns>A Duration representing the specified minutes</returns>
    private static Duration M(int minutes) => Duration.FromMinutes(minutes);

    /// <summary>
    /// A test item that implements ITimeSeries for testing purposes
    /// </summary>
    /// <param name="Moment">The timestamp of the item</param>
    private sealed record Item(Instant Moment) : ITimeSeries;
}
