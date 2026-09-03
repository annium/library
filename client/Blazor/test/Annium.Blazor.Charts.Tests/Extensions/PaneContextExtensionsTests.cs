using System;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Interop;
using Annium.Core.Runtime.Time;
using Annium.Data.Models;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Blazor.Charts.Tests.Extensions;

/// <summary>
/// Tests for PaneContextExtensions ToX/FromX/ToY/FromY coordinate transformation helpers
/// </summary>
public class PaneContextExtensionsTests : TestBase
{
    /// <summary>
    /// A fixed timestamp representing the current time for tests
    /// </summary>
    private readonly Instant _now = new LocalDateTime(2020, 1, 15, 14, 20).InUtc().ToInstant();

    /// <summary>
    /// Initializes a new instance of the PaneContextExtensionsTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public PaneContextExtensionsTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddCharts());
    }

    /// <summary>
    /// Tests that pane ToX/FromX delegate to the parent chart context's ToX/FromX
    /// </summary>
    [Fact]
    public void ToX_FromX_DelegateToChart()
    {
        // arrange
        var (chart, pane) = CreatePane();

        // assert
        pane.ToX(_now).Is(chart.ToX(_now));
        pane.FromX(100).Is(chart.FromX(100));
    }

    /// <summary>
    /// Tests that converting a Y coordinate to a value and back yields the original Y coordinate, once the pane
    /// has an established view and a non-zero DotPerPx
    /// </summary>
    [Fact]
    public void ToY_FromY_RoundTrip_ForNormalDotPerPx()
    {
        // arrange
        var (_, pane) = CreatePane();
        var source = new FakeSeriesSource();
        _ = pane.RegisterSource(source);
        pane.AdjustRange(source, 0m, 100m);

        // act
        var value = pane.FromY(60);
        var y = pane.ToY(value);

        // assert
        y.Is(60);
    }

    /// <summary>
    /// Tests that ToY returns zero when the pane's DotPerPx is zero (its default, un-configured value)
    /// </summary>
    [Fact]
    public void ToY_DotPerPxZero_ReturnsZero()
    {
        // arrange
        var pane = Get<IManagedPaneContext>();

        // assert
        pane.DotPerPx.Is(0m);
        pane.ToY(42m).Is(0);
    }

    /// <summary>
    /// Creates and initializes a chart and pane context pair for coordinate math tests
    /// </summary>
    /// <returns>The configured chart and pane context</returns>
    private (IManagedChartContext Chart, IManagedPaneContext Pane) CreatePane()
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

        return (chart, pane);
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
