using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Internal.Data.Cache;
using Annium.Data.Models;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Blazor.Charts.Tests.Internal.Data;

/// <summary>
/// Tests for the CheckedSeriesSourceCache functionality
/// </summary>
public class CheckedSeriesSourceCacheTests : TestBase
{
    /// <summary>
    /// A fixed moment in time used as a reference point for tests
    /// </summary>
    private readonly Instant _moment = new LocalDateTime(2020, 1, 15, 14, 20).InUtc().ToInstant();

    /// <summary>
    /// Initializes a new instance of the CheckedSeriesSourceCacheTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public CheckedSeriesSourceCacheTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddCharts());
    }

    /// <summary>
    /// Tests that the cache correctly reports its empty state
    /// </summary>
    [Fact]
    public void IsEmpty()
    {
        // arrange
        var cache = CreateCache();

        // assert
        cache.IsEmpty.IsTrue();

        // act
        var (start, end) = Range(_moment, 2);
        var items = Items(start, 0, 1, 2);
        cache.AddData(start, end, items);

        // assert
        cache.IsEmpty.IsFalse();
    }

    /// <summary>
    /// Tests that the cache correctly reports its bounds
    /// </summary>
    [Fact]
    public void Bounds()
    {
        // arrange
        var cache = CreateCache();

        // assert
        cache.Bounds.Start.Is(NodaConstants.UnixEpoch);
        cache.Bounds.End.Is(NodaConstants.UnixEpoch);

        // act
        var (start, end) = Range(_moment, 2);
        var items = Items(start, 0, 1, 2);
        cache.AddData(start, end, items);

        // assert
        cache.Bounds.Start.Is(start);
        cache.Bounds.End.Is(end);
    }

    /// <summary>
    /// Tests that the cache correctly reports whether it has data for specific ranges
    /// </summary>
    [Fact]
    public void HasData()
    {
        // arrange
        var cache = CreateCache();
        var (start1, end1) = Range(_moment, 2);
        var items1 = Items(start1, 0, 1, 2);
        cache.AddData(start1, end1, items1);
        var (start2, end2) = Range(end1 + M(2), 2);
        var items2 = Items(start2, 0, 1, 2);
        cache.AddData(start2, end2, items2);

        // assert
        cache.HasData(start1, end1).IsTrue();
        cache.HasData(start2, end2).IsTrue();
        cache.HasData(start1, start1 + M(1)).IsTrue();
        cache.HasData(start1, start2).IsFalse();
        cache.HasData(start1, end2).IsFalse();
        cache.HasData(end1, start2).IsFalse();
    }

    /// <summary>
    /// Tests that the cache correctly retrieves data for specific ranges
    /// </summary>
    [Fact]
    public void GetData()
    {
        // arrange
        var cache = CreateCache();
        var (start1, end1) = Range(_moment, 2);
        var items1 = Items(start1, 0, 1, 2);
        cache.AddData(start1, end1, items1);
        var (start2, end2) = Range(end1 + M(2), 2);
        var items2 = Items(start2, 0, 1, 2);
        cache.AddData(start2, end2, items2);

        // assert
        cache.GetData(start1, end1).IsEqual(items1);
        cache.GetData(start2, end2).IsEqual(items2);
        cache.GetData(start1, start1 + M(1)).IsEqual(items1.Take(2).ToArray());
        cache.GetData(start1, start2).IsEmpty();
        cache.GetData(start1, end2).IsEmpty();
        cache.GetData(end1, start2).IsEmpty();
    }

    /// <summary>
    /// Tests that the cache correctly retrieves individual items by timestamp
    /// </summary>
    [Fact]
    public void GetItem()
    {
        // arrange
        var cache = CreateCache();
        // range 1
        var (start1, end1) = Range(_moment, 20);
        var items1 = Items(start1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20);
        cache.AddData(start1, end1, items1);

        // assert
        // range 1
        cache.GetItem(start1 - M(1)).IsDefault();
        cache.GetItem(start1).Is(items1[0]);
        cache.GetItem(start1 + M(1)).Is(items1[1]);
        cache.GetItem(start1 + M(6)).Is(items1[6]);
        cache.GetItem(start1 + M(10)).Is(items1[10]);
        cache.GetItem(start1 + M(14)).Is(items1[14]);
        cache.GetItem(end1 - M(1)).Is(items1[^2]);
        cache.GetItem(end1).Is(items1[^1]);
        cache.GetItem(end1 + M(1)).IsDefault();
    }

    /// <summary>
    /// Tests that the cache correctly identifies empty ranges within requested bounds
    /// </summary>
    [Fact]
    public void GetEmptyRanges()
    {
        // arrange
        var cache = CreateCache();

        // assert
        cache
            .GetEmptyRanges(_moment - M(2), _moment + M(2))
            .IsEqual(new[] { ValueRange.Create(_moment - M(2), _moment + M(2)) });

        // arrange
        var (start1, end1) = Range(_moment, 2);
        var items1 = Items(start1, 0, 1, 2);
        cache.AddData(start1, end1, items1);
        var (start2, end2) = Range(end1 + M(2), 2);
        var items2 = Items(start2, 0, 1, 2);
        cache.AddData(start2, end2, items2);

        // assert
        cache.GetEmptyRanges(start1, end1).IsEmpty();
        cache.GetEmptyRanges(start1 - M(2), end1).IsEqual(new[] { ValueRange.Create(start1 - M(2), start1 - M(1)) });
        cache.GetEmptyRanges(start1, end1 + M(2)).IsEqual(new[] { ValueRange.Create(end1 + M(1), end1 + M(2)) });
        cache
            .GetEmptyRanges(start1 - M(2), end2 + M(2))
            .IsEqual(
                new[]
                {
                    ValueRange.Create(start1 - M(2), start1 - M(1)),
                    ValueRange.Create(end1 + M(1), start2 - M(1)),
                    ValueRange.Create(end2 + M(1), end2 + M(2)),
                }
            );
    }

    /// <summary>
    /// Tests adding initial data to the cache
    /// </summary>
    [Fact]
    public void AddData_Init()
    {
        // arrange
        var cache = CreateCache();
        var (start1, end1) = Range(_moment, 2);
        var items1 = Items(start1, 0, 1, 2);
        var (start2, end2) = Range(end1 + M(2), 2);
        var items2 = Items(start2, 0, 1, 2);

        // act
        cache.AddData(start1, end1, items1);
        cache.AddData(start2, end2, items2);

        // assert
        cache.GetEmptyRanges(start1, end1).IsEmpty();
        cache.GetEmptyRanges(start1 - M(2), end1).IsEqual(new[] { ValueRange.Create(start1 - M(2), start1 - M(1)) });
        cache.GetEmptyRanges(start1, end1 + M(2)).IsEqual(new[] { ValueRange.Create(end1 + M(1), end1 + M(2)) });
        cache
            .GetEmptyRanges(start1 - M(2), end2 + M(2))
            .IsEqual(
                new[]
                {
                    ValueRange.Create(start1 - M(2), start1 - M(1)),
                    ValueRange.Create(end1 + M(1), start2 - M(1)),
                    ValueRange.Create(end2 + M(1), end2 + M(2)),
                }
            );
    }

    /// <summary>
    /// Tests that adding intersecting data ranges throws an exception
    /// </summary>
    [Fact]
    public void AddData_Intersection()
    {
        // arrange
        var cache = CreateCache();
        var (start1, end1) = Range(_moment, 2);
        var items1 = Items(start1, 0, 1, 2);
        var (start2, end2) = Range(end1, 2);
        var items2 = Items(start2, 0, 1, 2);

        // act
        cache.AddData(start1, end1, items1);
        Wrap.It(() => cache.AddData(start2, end2, items2)).Throws<InvalidOperationException>().Reports("intersect");
    }

    /// <summary>
    /// Tests adding data before existing data ranges
    /// </summary>
    [Fact]
    public void AddData_Before()
    {
        // arrange
        var cache = CreateCache();
        var (start1, end1) = Range(_moment, 2);
        var items1 = Items(start1, 0, 1, 2);
        var (start2, end2) = Range(end1 + M(1), 2);
        var items2 = Items(start2, 0, 1, 2);

        // act
        cache.AddData(start2, end2, items2);
        cache.AddData(start1, end1, items1);

        // assert
        cache
            .GetEmptyRanges(start1 - M(2), end2 + M(2))
            .IsEqual(
                new[] { ValueRange.Create(start1 - M(2), start1 - M(1)), ValueRange.Create(end2 + M(1), end2 + M(2)) }
            );
    }

    /// <summary>
    /// Tests adding data after existing data ranges
    /// </summary>
    [Fact]
    public void AddData_After()
    {
        // arrange
        var cache = CreateCache();
        var (start1, end1) = Range(_moment, 2);
        var items1 = Items(start1, 0, 1, 2);
        var (start2, end2) = Range(end1 + M(1), 2);
        var items2 = Items(start2, 0, 1, 2);

        // act
        cache.AddData(start1, end1, items1);
        cache.AddData(start2, end2, items2);

        // assert
        cache
            .GetEmptyRanges(start1 - M(2), end2 + M(2))
            .IsEqual(
                new[] { ValueRange.Create(start1 - M(2), start1 - M(1)), ValueRange.Create(end2 + M(1), end2 + M(2)) }
            );
    }

    /// <summary>
    /// Tests adding data with empty ranges between existing data
    /// </summary>
    [Fact]
    public void AddData_EmptyRange()
    {
        // arrange
        var cache = CreateCache();
        var (start1, end1) = Range(_moment, 2);
        var items1 = Items(start1, 0, 1, 2);
        var (start2, end2) = Range(end1 + M(2), 2);
        var items2 = Items(start2, 0, 1, 2);

        // act
        cache.AddData(start1, end1, items1);
        cache.AddData(start2, end2, items2);

        // assert
        cache
            .GetEmptyRanges(start1 - M(2), end2 + M(2))
            .IsEqual(
                new[]
                {
                    ValueRange.Create(start1 - M(2), start1 - M(1)),
                    ValueRange.Create(end1 + M(1), start2 - M(1)),
                    ValueRange.Create(end2 + M(1), end2 + M(2)),
                }
            );
    }

    /// <summary>
    /// Tests that changing the resolution clears the cache
    /// </summary>
    [Fact]
    public void SetResolution()
    {
        // arrange
        var cache = CreateCache();
        var (start, end) = Range(_moment, 2);
        var items = Items(start, 0, 1, 2);
        cache.AddData(start, end, items);

        // assert
        cache.IsEmpty.IsFalse();

        // act
        cache.SetResolution(M(2));

        // assert
        cache.IsEmpty.IsTrue();
    }

    /// <summary>
    /// Tests that clearing the cache empties it
    /// </summary>
    [Fact]
    public void Clear()
    {
        // arrange
        var cache = CreateCache();
        var (start, end) = Range(_moment, 2);
        var items = Items(start, 0, 1, 2);
        cache.AddData(start, end, items);

        // assert
        cache.IsEmpty.IsFalse();

        // act
        cache.Clear();

        // assert
        cache.IsEmpty.IsTrue();
    }

    /// <summary>
    /// Creates a new CheckedSeriesSourceCache instance for testing
    /// </summary>
    /// <returns>A new cache instance</returns>
    private ISeriesSourceCache<Item> CreateCache()
    {
        return new CheckedSeriesSourceCache<Item>(M(1));
    }

    /// <summary>
    /// Creates a time range starting from a given instant
    /// </summary>
    /// <param name="from">The starting instant</param>
    /// <param name="length">The length in minutes</param>
    /// <returns>A tuple containing start and end instants</returns>
    private (Instant start, Instant end) Range(Instant from, int length) => (from, from + M(length));

    /// <summary>
    /// Creates a list of test items with specified time offsets
    /// </summary>
    /// <param name="start">The starting instant</param>
    /// <param name="offsets">Array of minute offsets from start</param>
    /// <returns>A list of test items</returns>
    private IReadOnlyList<Item> Items(Instant start, params int[] offsets)
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
    private Duration M(int minutes) => Duration.FromMinutes(minutes);

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
