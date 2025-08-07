using System;
using System.Collections.Generic;
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
