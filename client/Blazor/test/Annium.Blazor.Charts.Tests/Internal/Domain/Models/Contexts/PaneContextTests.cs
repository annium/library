using System;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Interop;
using Annium.Core.Runtime.Time;
using Annium.Data.Models;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Blazor.Charts.Tests.Internal.Domain.Models.Contexts;

/// <summary>
/// Tests for PaneContext.AdjustRange's view/range padding branches and RegisterSource's tracking behavior
/// </summary>
public class PaneContextTests : TestBase
{
    /// <summary>
    /// A fixed timestamp representing the current time for tests
    /// </summary>
    private readonly Instant _now = new LocalDateTime(2020, 1, 15, 14, 20).InUtc().ToInstant();

    /// <summary>
    /// Initializes a new instance of the PaneContextTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public PaneContextTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddCharts());
    }

    /// <summary>
    /// Tests that AdjustRange rejects a range whose minimum exceeds its maximum
    /// </summary>
    [Fact]
    public void AdjustRange_MinGreaterThanMax_Throws()
    {
        // arrange
        var pane = CreatePane();
        var source = new FakeSeriesSource();
        _ = pane.RegisterSource(source);

        // act & assert
        Wrap.It(() => pane.AdjustRange(source, 10m, 5m)).Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that a non-degenerate range (min &lt; max) pads the view by 10% of the range's size on each side
    /// </summary>
    [Fact]
    public void AdjustRange_SizeGreaterThanZero_PadsViewByTenPercent()
    {
        // arrange
        var pane = CreatePane();
        var source = new FakeSeriesSource();
        _ = pane.RegisterSource(source);

        // act
        var changed = pane.AdjustRange(source, 0m, 100m);

        // assert
        changed.IsTrue();
        pane.Range.Start.Is(0m);
        pane.Range.End.Is(100m);
        pane.View.Start.Is(-10m);
        pane.View.End.Is(110m);
    }

    /// <summary>
    /// Tests that a single-value range (min == max) at zero produces a fixed, symmetric +/-0.5 view
    /// </summary>
    [Fact]
    public void AdjustRange_MinEqualsMaxAtZero_SetsSymmetricView()
    {
        // arrange
        var pane = CreatePane();
        var source = new FakeSeriesSource();
        _ = pane.RegisterSource(source);

        // act
        var changed = pane.AdjustRange(source, 0m, 0m);

        // assert
        changed.IsTrue();
        pane.View.Start.Is(-0.5m);
        pane.View.End.Is(0.5m);
    }

    /// <summary>
    /// Tests that a single-value, non-zero NEGATIVE range (min == max == -5) pads the view symmetrically by 10% of
    /// the magnitude and keeps it ordered (Start &lt;= End): -5 → (-5.5, -4.5). Negative values are a normal case
    /// (e.g. P&amp;L series), so the view must not invert.
    /// </summary>
    [Fact]
    public void AdjustRange_MinEqualsMaxNegative_PadsSymmetricallyAndStaysOrdered()
    {
        // arrange
        var pane = CreatePane();
        var source = new FakeSeriesSource();
        _ = pane.RegisterSource(source);

        // act
        var changed = pane.AdjustRange(source, -5m, -5m);

        // assert
        changed.IsTrue();
        pane.View.Start.Is(-5.5m);
        pane.View.End.Is(-4.5m);
    }

    /// <summary>
    /// Tests that adjusting a registered source to the sentinel (unrendered) range leaves the pane's range and
    /// view unchanged and reports no change
    /// </summary>
    [Fact]
    public void AdjustRange_SentinelRange_ReturnsFalse()
    {
        // arrange
        var pane = CreatePane();
        var source = new FakeSeriesSource();
        _ = pane.RegisterSource(source);

        // act
        var changed = pane.AdjustRange(source, decimal.MinValue, decimal.MaxValue);

        // assert
        changed.IsFalse();
        pane.Range.Start.Is(decimal.MinValue);
        pane.Range.End.Is(decimal.MaxValue);
    }

    /// <summary>
    /// Tests that registering the same source twice does not track it twice
    /// </summary>
    [Fact]
    public void RegisterSource_CalledTwiceWithSameSource_TracksOnce()
    {
        // arrange
        var pane = CreatePane();
        var source = new FakeSeriesSource();

        // act
        _ = pane.RegisterSource(source);
        _ = pane.RegisterSource(source);

        // assert
        pane.Sources.Count.Is(1);
    }

    /// <summary>
    /// Tests that disposing the registration of the last remaining source resets the pane's range and view to
    /// (0, 0)
    /// </summary>
    [Fact]
    public void RegisterSource_DisposeLastSource_ResetsRangeAndView()
    {
        // arrange
        var pane = CreatePane();
        var source = new FakeSeriesSource();
        var registration = pane.RegisterSource(source);
        pane.AdjustRange(source, 0m, 100m);

        // act
        registration.Dispose();

        // assert
        pane.Sources.Count.Is(0);
        pane.Range.Start.Is(0m);
        pane.Range.End.Is(0m);
        pane.View.Start.Is(0m);
        pane.View.End.Is(0m);
    }

    /// <summary>
    /// Creates and initializes a pane context, with its parent chart configured and its rectangle sized
    /// </summary>
    /// <returns>A configured, managed pane context</returns>
    private IManagedPaneContext CreatePane()
    {
        Get<ITimeManager>().SetNow(_now);

        var chart = (IManagedChartContext)Get<IChartContext>();
        chart.Configure([1], [1]);
        chart.SetMoment(_now);
        chart.SetRect(new DomRect { Width = 300m });
        chart.Update();

        var pane = Get<IManagedPaneContext>();
        pane.Init(chart);
        pane.SetRect(new DomRect { Height = 120m });

        return pane;
    }

    /// <summary>
    /// A minimal series source fake used solely as a registration key for pane range adjustment tests
    /// </summary>
    private sealed class FakeSeriesSource : ISeriesSource
    {
        /// <summary>
        /// Occurs when data has been loaded; unused by this fake
        /// </summary>
        public event Action Loaded
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Occurs when the bounds of the data change; unused by this fake
        /// </summary>
        public event Action<ValueRange<Instant>> OnBoundsChange
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Gets the resolution of the fake data series
        /// </summary>
        public Duration Resolution => Duration.Zero;

        /// <summary>
        /// Gets the time bounds of the fake data series
        /// </summary>
        public ValueRange<Instant> Bounds => ValueRange.Create(NodaConstants.UnixEpoch, NodaConstants.UnixEpoch);

        /// <summary>
        /// Gets a value indicating whether data is currently being loaded; always false for this fake
        /// </summary>
        public bool IsLoading => false;

        /// <summary>
        /// Sets the resolution for the fake data series; a no-op
        /// </summary>
        /// <param name="resolution">The new resolution to set</param>
        public void SetResolution(Duration resolution) { }

        /// <summary>
        /// Clears all loaded data; a no-op
        /// </summary>
        public void Clear() { }
    }
}
