using System.Collections.Generic;
using Annium.Blazor.Charts.Components;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Domain.Models;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Blazor.Charts.Tests.Components;

/// <summary>
/// Tests for the GetBounds calculation of the series components, pinning that min/max are computed from the
/// correct fields (e.g. candle Low/High are not swapped) and that an empty item list yields the sentinel
/// (decimal.MaxValue, decimal.MinValue) result rather than throwing or defaulting to zero
/// </summary>
public class SeriesBoundsTests : TestBase
{
    /// <summary>
    /// A fixed timestamp representing the current time for tests
    /// </summary>
    private readonly Instant _now = new LocalDateTime(2020, 1, 15, 14, 20).InUtc().ToInstant();

    /// <summary>
    /// Initializes a new instance of the SeriesBoundsTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public SeriesBoundsTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddCharts());
    }

    /// <summary>
    /// Tests that CandleSeries.GetBounds returns the sentinel (decimal.MaxValue, decimal.MinValue) for an empty item list
    /// </summary>
    [Fact]
    public void CandleSeries_GetBounds_Empty_ReturnsSentinel()
    {
        // arrange
        var series = new ExposedCandleSeries<TestCandle>();

        // act
        var (min, max) = series.Bounds([]);

        // assert
        min.Is(decimal.MaxValue);
        max.Is(decimal.MinValue);
    }

    /// <summary>
    /// Tests that CandleSeries.GetBounds returns the minimum Low and the maximum High across items, without swapping them
    /// </summary>
    [Fact]
    public void CandleSeries_GetBounds_NonEmpty_ReturnsMinLowAndMaxHigh()
    {
        // arrange
        var series = new ExposedCandleSeries<TestCandle>();
        var items = new List<TestCandle>
        {
            new(_now, Open: 10m, High: 15m, Low: 8m, Close: 12m),
            new(_now.Plus(Duration.FromMinutes(1)), Open: 12m, High: 20m, Low: 11m, Close: 18m),
            new(_now.Plus(Duration.FromMinutes(2)), Open: 18m, High: 19m, Low: 5m, Close: 9m),
        };

        // act
        var (min, max) = series.Bounds(items);

        // assert
        min.Is(5m);
        max.Is(20m);
    }

    /// <summary>
    /// Tests that LineSeries.GetBounds returns the sentinel (decimal.MaxValue, decimal.MinValue) for an empty item list
    /// </summary>
    [Fact]
    public void LineSeries_GetBounds_Empty_ReturnsSentinel()
    {
        // arrange
        var series = new ExposedLineSeries<PointValue>();

        // act
        var (min, max) = series.Bounds([]);

        // assert
        min.Is(decimal.MaxValue);
        max.Is(decimal.MinValue);
    }

    /// <summary>
    /// Tests that LineSeries.GetBounds returns the minimum and maximum Value across items
    /// </summary>
    [Fact]
    public void LineSeries_GetBounds_NonEmpty_ReturnsMinAndMaxValue()
    {
        // arrange
        var series = new ExposedLineSeries<PointValue>();
        var items = new List<PointValue>
        {
            new(_now, 3m),
            new(_now.Plus(Duration.FromMinutes(1)), -7m),
            new(_now.Plus(Duration.FromMinutes(2)), 4m),
        };

        // act
        var (min, max) = series.Bounds(items);

        // assert
        min.Is(-7m);
        max.Is(4m);
    }

    /// <summary>
    /// Tests that NodeSeries.GetBounds returns the sentinel (decimal.MaxValue, decimal.MinValue) for an empty item list
    /// </summary>
    [Fact]
    public void NodeSeries_GetBounds_Empty_ReturnsSentinel()
    {
        // arrange
        var series = new ExposedNodeSeries<PointValue>();

        // act
        var (min, max) = series.Bounds([]);

        // assert
        min.Is(decimal.MaxValue);
        max.Is(decimal.MinValue);
    }

    /// <summary>
    /// Tests that NodeSeries.GetBounds returns the minimum and maximum Value across items
    /// </summary>
    [Fact]
    public void NodeSeries_GetBounds_NonEmpty_ReturnsMinAndMaxValue()
    {
        // arrange
        var series = new ExposedNodeSeries<PointValue>();
        var items = new List<PointValue>
        {
            new(_now, 2m),
            new(_now.Plus(Duration.FromMinutes(1)), 9m),
            new(_now.Plus(Duration.FromMinutes(2)), -1m),
        };

        // act
        var (min, max) = series.Bounds(items);

        // assert
        min.Is(-1m);
        max.Is(9m);
    }

    /// <summary>
    /// Tests that MultiBlockSeries.GetBounds returns the sentinel (decimal.MaxValue, decimal.MinValue) for an empty item list
    /// </summary>
    [Fact]
    public void MultiBlockSeries_GetBounds_Empty_ReturnsSentinel()
    {
        // arrange
        var series = new ExposedMultiBlockSeries<MultiValue<RangeItem>, RangeItem>();

        // act
        var (min, max) = series.Bounds([]);

        // assert
        min.Is(decimal.MaxValue);
        max.Is(decimal.MinValue);
    }

    /// <summary>
    /// Tests that MultiBlockSeries.GetBounds returns the minimum Low and maximum High across all range items of all
    /// multi-value items, without swapping them
    /// </summary>
    [Fact]
    public void MultiBlockSeries_GetBounds_NonEmpty_ReturnsMinLowAndMaxHigh()
    {
        // arrange
        var series = new ExposedMultiBlockSeries<MultiValue<RangeItem>, RangeItem>();
        var items = new List<MultiValue<RangeItem>>
        {
            new(_now, [new RangeItem(Low: 4m, High: 10m), new RangeItem(Low: 2m, High: 6m)]),
            new(_now.Plus(Duration.FromMinutes(1)), [new RangeItem(Low: 5m, High: 14m)]),
        };

        // act
        var (min, max) = series.Bounds(items);

        // assert
        min.Is(2m);
        max.Is(14m);
    }

    /// <summary>
    /// Tests that MultiLineSeries.GetBounds returns the sentinel (decimal.MaxValue, decimal.MinValue) for an empty item list
    /// </summary>
    [Fact]
    public void MultiLineSeries_GetBounds_Empty_ReturnsSentinel()
    {
        // arrange
        var series = new ExposedMultiLineSeries<MultiValue<PointItem>, PointItem>();

        // act
        var (min, max) = series.Bounds([]);

        // assert
        min.Is(decimal.MaxValue);
        max.Is(decimal.MinValue);
    }

    /// <summary>
    /// Tests that MultiLineSeries.GetBounds returns the minimum and maximum Value across all point items of all
    /// multi-value items
    /// </summary>
    [Fact]
    public void MultiLineSeries_GetBounds_NonEmpty_ReturnsMinAndMaxValue()
    {
        // arrange
        var series = new ExposedMultiLineSeries<MultiValue<PointItem>, PointItem>();
        var items = new List<MultiValue<PointItem>>
        {
            new(_now, [new PointItem(6m), new PointItem(-3m)]),
            new(_now.Plus(Duration.FromMinutes(1)), [new PointItem(11m)]),
        };

        // act
        var (min, max) = series.Bounds(items);

        // assert
        min.Is(-3m);
        max.Is(11m);
    }

    /// <summary>
    /// Tests that MultiNodeSeries.GetBounds returns the sentinel (decimal.MaxValue, decimal.MinValue) for an empty item list
    /// </summary>
    [Fact]
    public void MultiNodeSeries_GetBounds_Empty_ReturnsSentinel()
    {
        // arrange
        var series = new ExposedMultiNodeSeries<MultiValue<PointItem>, PointItem>();

        // act
        var (min, max) = series.Bounds([]);

        // assert
        min.Is(decimal.MaxValue);
        max.Is(decimal.MinValue);
    }

    /// <summary>
    /// Tests that MultiNodeSeries.GetBounds returns the minimum and maximum Value across all point items of all
    /// multi-value items
    /// </summary>
    [Fact]
    public void MultiNodeSeries_GetBounds_NonEmpty_ReturnsMinAndMaxValue()
    {
        // arrange
        var series = new ExposedMultiNodeSeries<MultiValue<PointItem>, PointItem>();
        var items = new List<MultiValue<PointItem>>
        {
            new(_now, [new PointItem(1m), new PointItem(8m)]),
            new(_now.Plus(Duration.FromMinutes(1)), [new PointItem(-5m)]),
        };

        // act
        var (min, max) = series.Bounds(items);

        // assert
        min.Is(-5m);
        max.Is(8m);
    }

    /// <summary>
    /// Tests that MultiRangeSeries.GetBounds returns the sentinel (decimal.MaxValue, decimal.MinValue) for an empty item list
    /// </summary>
    [Fact]
    public void MultiRangeSeries_GetBounds_Empty_ReturnsSentinel()
    {
        // arrange
        var series = new ExposedMultiRangeSeries<MultiValue<RangeItem>, RangeItem>();

        // act
        var (min, max) = series.Bounds([]);

        // assert
        min.Is(decimal.MaxValue);
        max.Is(decimal.MinValue);
    }

    /// <summary>
    /// Tests that MultiRangeSeries.GetBounds returns the minimum Low and maximum High across all range items of all
    /// multi-value items, without swapping them
    /// </summary>
    [Fact]
    public void MultiRangeSeries_GetBounds_NonEmpty_ReturnsMinLowAndMaxHigh()
    {
        // arrange
        var series = new ExposedMultiRangeSeries<MultiValue<RangeItem>, RangeItem>();
        var items = new List<MultiValue<RangeItem>>
        {
            new(_now, [new RangeItem(Low: 3m, High: 9m)]),
            new(
                _now.Plus(Duration.FromMinutes(1)),
                [new RangeItem(Low: 1m, High: 7m), new RangeItem(Low: 6m, High: 16m)]
            ),
        };

        // act
        var (min, max) = series.Bounds(items);

        // assert
        min.Is(1m);
        max.Is(16m);
    }

    /// <summary>
    /// A test candle record implementing ICandle, used to exercise CandleSeries.GetBounds without any rendering infrastructure
    /// </summary>
    /// <param name="Moment">The specific moment in time for this candle</param>
    /// <param name="Open">The opening price for the time period</param>
    /// <param name="High">The highest price during the time period</param>
    /// <param name="Low">The lowest price during the time period</param>
    /// <param name="Close">The closing price for the time period</param>
    private sealed record TestCandle(Instant Moment, decimal Open, decimal High, decimal Low, decimal Close) : ICandle;

    /// <summary>
    /// A test subclass of CandleSeries exposing the protected GetBounds method for direct testing
    /// </summary>
    /// <typeparam name="T">The candle item type</typeparam>
    private sealed class ExposedCandleSeries<T> : CandleSeries<T>
        where T : ICandle
    {
        /// <summary>
        /// Invokes the protected GetBounds method
        /// </summary>
        /// <param name="items">The candle items to analyze</param>
        /// <returns>A tuple containing the minimum and maximum values</returns>
        public (decimal min, decimal max) Bounds(IReadOnlyList<T> items) => GetBounds(items);
    }

    /// <summary>
    /// A test subclass of LineSeries exposing the protected GetBounds method for direct testing
    /// </summary>
    /// <typeparam name="T">The point value item type</typeparam>
    private sealed class ExposedLineSeries<T> : LineSeries<T>
        where T : IPointValue
    {
        /// <summary>
        /// Invokes the protected GetBounds method
        /// </summary>
        /// <param name="items">The point items to analyze</param>
        /// <returns>A tuple containing the minimum and maximum values</returns>
        public (decimal min, decimal max) Bounds(IReadOnlyList<T> items) => GetBounds(items);
    }

    /// <summary>
    /// A test subclass of NodeSeries exposing the protected GetBounds method for direct testing
    /// </summary>
    /// <typeparam name="T">The point value item type</typeparam>
    private sealed class ExposedNodeSeries<T> : NodeSeries<T>
        where T : IPointValue
    {
        /// <summary>
        /// Invokes the protected GetBounds method
        /// </summary>
        /// <param name="items">The point items to analyze</param>
        /// <returns>A tuple containing the minimum and maximum values</returns>
        public (decimal min, decimal max) Bounds(IReadOnlyList<T> items) => GetBounds(items);
    }

    /// <summary>
    /// A test subclass of MultiBlockSeries exposing the protected GetBounds method for direct testing
    /// </summary>
    /// <typeparam name="TM">The multi-value type</typeparam>
    /// <typeparam name="TI">The range item type</typeparam>
    private sealed class ExposedMultiBlockSeries<TM, TI> : MultiBlockSeries<TM, TI>
        where TM : IMultiValue<TI>
        where TI : IRangeItem
    {
        /// <summary>
        /// Invokes the protected GetBounds method
        /// </summary>
        /// <param name="items">The multi-value items to analyze</param>
        /// <returns>A tuple containing the minimum and maximum values</returns>
        public (decimal min, decimal max) Bounds(IReadOnlyList<TM> items) => GetBounds(items);
    }

    /// <summary>
    /// A test subclass of MultiLineSeries exposing the protected GetBounds method for direct testing
    /// </summary>
    /// <typeparam name="TM">The multi-value type</typeparam>
    /// <typeparam name="TI">The point item type</typeparam>
    private sealed class ExposedMultiLineSeries<TM, TI> : MultiLineSeries<TM, TI>
        where TM : IMultiValue<TI>
        where TI : IPointItem
    {
        /// <summary>
        /// Invokes the protected GetBounds method
        /// </summary>
        /// <param name="items">The multi-value items to analyze</param>
        /// <returns>A tuple containing the minimum and maximum values</returns>
        public (decimal min, decimal max) Bounds(IReadOnlyList<TM> items) => GetBounds(items);
    }

    /// <summary>
    /// A test subclass of MultiNodeSeries exposing the protected GetBounds method for direct testing
    /// </summary>
    /// <typeparam name="TM">The multi-value type</typeparam>
    /// <typeparam name="TI">The point item type</typeparam>
    private sealed class ExposedMultiNodeSeries<TM, TI> : MultiNodeSeries<TM, TI>
        where TM : IMultiValue<TI>
        where TI : IPointItem
    {
        /// <summary>
        /// Invokes the protected GetBounds method
        /// </summary>
        /// <param name="items">The multi-value items to analyze</param>
        /// <returns>A tuple containing the minimum and maximum values</returns>
        public (decimal min, decimal max) Bounds(IReadOnlyList<TM> items) => GetBounds(items);
    }

    /// <summary>
    /// A test subclass of MultiRangeSeries exposing the protected GetBounds method for direct testing
    /// </summary>
    /// <typeparam name="TM">The multi-value type</typeparam>
    /// <typeparam name="TI">The range item type</typeparam>
    private sealed class ExposedMultiRangeSeries<TM, TI> : MultiRangeSeries<TM, TI>
        where TM : IMultiValue<TI>
        where TI : IRangeItem
    {
        /// <summary>
        /// Invokes the protected GetBounds method
        /// </summary>
        /// <param name="items">The multi-value items to analyze</param>
        /// <returns>A tuple containing the minimum and maximum values</returns>
        public (decimal min, decimal max) Bounds(IReadOnlyList<TM> items) => GetBounds(items);
    }
}
