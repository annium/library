using System;
using Annium.Blazor.Charts.Data.Comparers;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Internal.Data.Comparers;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Blazor.Charts.Tests.Internal.Data.Comparers;

/// <summary>
/// Tests for ItemComparer (type-keyed comparer cache) and TimeSeriesComparer (moment-based comparer)
/// </summary>
public class ComparersTests : TestBase
{
    /// <summary>
    /// A fixed moment in time used as a reference point for tests
    /// </summary>
    private readonly Instant _moment = new LocalDateTime(2020, 1, 15, 14, 20).InUtc().ToInstant();

    /// <summary>
    /// Initializes a new instance of the ComparersTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public ComparersTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddCharts());
    }

    /// <summary>
    /// Tests that ItemComparer.For returns the same cached comparer instance for repeated calls with the same type,
    /// even when a different comparison delegate is passed
    /// </summary>
    [Fact]
    public void ItemComparerFor_SameType_ReturnsSameCachedInstance()
    {
        // act
        var first = ItemComparer.For<ItemA>((a, b) => a.Moment.CompareTo(b.Moment));
        var second = ItemComparer.For<ItemA>((a, b) => b.Moment.CompareTo(a.Moment));

        // assert
        ReferenceEquals(first, second).IsTrue();
    }

    /// <summary>
    /// Tests that ItemComparer.For returns distinct comparer instances for distinct types
    /// </summary>
    [Fact]
    public void ItemComparerFor_DifferentTypes_ReturnsDifferentInstances()
    {
        // act
        var forA = ItemComparer.For<ItemA>((a, b) => a.Moment.CompareTo(b.Moment));
        var forB = ItemComparer.For<ItemB>((a, b) => a.Moment.CompareTo(b.Moment));

        // assert
        ReferenceEquals(forA, forB).IsFalse();
    }

    /// <summary>
    /// Tests that the comparer returned by ItemComparer.For throws when comparing a null argument
    /// </summary>
    [Fact]
    public void ItemComparerFor_CompareWithNullArgument_Throws()
    {
        // arrange
        var comparer = ItemComparer.For<ItemA>((a, b) => a.Moment.CompareTo(b.Moment));
        var item = new ItemA(_moment);

        // act & assert
        Wrap.It(() => comparer.Compare(null, item)).Throws<ArgumentNullException>();
        Wrap.It(() => comparer.Compare(item, null)).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that TimeSeriesComparer.Default returns the same singleton instance for repeated access
    /// </summary>
    [Fact]
    public void TimeSeriesComparerDefault_ReturnsSameInstance()
    {
        // act
        var first = TimeSeriesComparer<ItemA>.Default;
        var second = TimeSeriesComparer<ItemA>.Default;

        // assert
        ReferenceEquals(first, second).IsTrue();
    }

    /// <summary>
    /// Tests that TimeSeriesComparer orders items by their moment
    /// </summary>
    [Fact]
    public void TimeSeriesComparer_Compare_OrdersByMoment()
    {
        // arrange
        var comparer = TimeSeriesComparer<ItemA>.Default;
        var earlier = new ItemA(_moment);
        var later = new ItemA(_moment + Duration.FromMinutes(1));

        // assert
        (comparer.Compare(earlier, later) < 0).IsTrue();
        (comparer.Compare(later, earlier) > 0).IsTrue();
        comparer.Compare(earlier, earlier).Is(0);
    }

    /// <summary>
    /// Tests that TimeSeriesComparer throws when comparing a null argument
    /// </summary>
    [Fact]
    public void TimeSeriesComparer_CompareWithNullArgument_Throws()
    {
        // arrange
        var comparer = TimeSeriesComparer<ItemA>.Default;
        var item = new ItemA(_moment);

        // act & assert
        Wrap.It(() => comparer.Compare(null, item)).Throws<ArgumentNullException>();
        Wrap.It(() => comparer.Compare(item, null)).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// A test item type implementing ITimeSeries, used to key a distinct cached comparer
    /// </summary>
    /// <param name="Moment">The timestamp of the item</param>
    private sealed record ItemA(Instant Moment) : ITimeSeries;

    /// <summary>
    /// A second, distinct test item type implementing ITimeSeries, used to key a distinct cached comparer
    /// </summary>
    /// <param name="Moment">The timestamp of the item</param>
    private sealed record ItemB(Instant Moment) : ITimeSeries;
}
