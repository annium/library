using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Domain.Lookup;
using Annium.Core.Runtime.Time;
using Annium.Data.Models;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Blazor.Charts.Tests.Domain.Contexts;

/// <summary>
/// Tests for ChartZoomExtensions.ZoomIn/ZoomOut/ChangeZoom/ResolveZoomIndex
/// </summary>
public class ChartZoomExtensionsTests : TestBase
{
    /// <summary>
    /// A fixed timestamp representing the current time for tests
    /// </summary>
    private readonly Instant _now = new LocalDateTime(2020, 1, 15, 14, 20).InUtc().ToInstant();

    /// <summary>
    /// Initializes a new instance of the ChartZoomExtensionsTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public ChartZoomExtensionsTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddCharts());
    }

    /// <summary>
    /// Tests that ResolveZoomIndex returns the index of the current zoom within the configured zooms
    /// </summary>
    [Fact]
    public void ResolveZoomIndex_ReturnsIndexOfCurrentZoom()
    {
        // arrange
        var chart = CreateChart([1, 2, 4, 8]);
        chart.SetZoom(4);

        // assert
        chart.ResolveZoomIndex().Is(2);
    }

    /// <summary>
    /// Tests that ZoomIn moves to the next configured zoom level
    /// </summary>
    [Fact]
    public void ZoomIn_MovesToNextZoomLevel()
    {
        // arrange
        var chart = CreateChart([1, 2, 4, 8]);
        chart.SetZoom(2);

        // act
        chart.ZoomIn();

        // assert
        chart.Zoom.Is(4);
    }

    /// <summary>
    /// Tests that ZoomOut moves to the previous configured zoom level
    /// </summary>
    [Fact]
    public void ZoomOut_MovesToPreviousZoomLevel()
    {
        // arrange
        var chart = CreateChart([1, 2, 4, 8]);
        chart.SetZoom(4);

        // act
        chart.ZoomOut();

        // assert
        chart.Zoom.Is(2);
    }

    /// <summary>
    /// Tests that ChangeZoom clamps at the last configured zoom level when zooming in past the end
    /// </summary>
    [Fact]
    public void ChangeZoom_PastUpperBound_ClampsAtLastZoom()
    {
        // arrange
        var chart = CreateChart([1, 2, 4]);
        chart.SetZoom(4);

        // act
        chart.ChangeZoom(5);

        // assert
        chart.Zoom.Is(4);
    }

    /// <summary>
    /// Tests that ChangeZoom clamps at the first configured zoom level when zooming out past the start
    /// </summary>
    [Fact]
    public void ChangeZoom_PastLowerBound_ClampsAtFirstZoom()
    {
        // arrange
        var chart = CreateChart([1, 2, 4]);
        chart.SetZoom(1);

        // act
        chart.ChangeZoom(-5);

        // assert
        chart.Zoom.Is(1);
    }

    /// <summary>
    /// Tests that ResolveZoomIndex (and by extension ChangeZoom) throws when the chart's current zoom is not
    /// present in its configured zooms, an invariant that cannot be produced through ChartContext's own public
    /// API (Configure and SetZoom always keep the current zoom in sync with the configured list), so this uses
    /// a minimal fake to exercise the guard directly
    /// </summary>
    [Fact]
    public void ResolveZoomIndex_CurrentZoomNotInZooms_Throws()
    {
        // arrange
        var chart = new FakeChartContext { Zoom = 99, Zooms = [1, 2, 4] };

        // act & assert
        Wrap.It(() => chart.ResolveZoomIndex()).Throws<InvalidOperationException>();
        Wrap.It(() => chart.ChangeZoom(1)).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Creates and configures a chart context with the given zoom levels
    /// </summary>
    /// <param name="zooms">The available zoom levels to configure</param>
    /// <returns>A configured chart context</returns>
    private IChartContext CreateChart(int[] zooms)
    {
        Get<ITimeManager>().SetNow(_now);

        var chart = Get<IChartContext>();
        chart.Configure(zooms, [1]);

        return chart;
    }

    /// <summary>
    /// A minimal IChartContext fake exposing only a settable Zoom and Zooms, used to independently exercise the
    /// "current zoom not found" invariant of ResolveZoomIndex/ChangeZoom without needing to corrupt a real
    /// ChartContext's internally-managed state
    /// </summary>
    private sealed class FakeChartContext : IChartContext
    {
        /// <summary>
        /// Event triggered when lookup information changes; unused by this fake
        /// </summary>
        public event Action<Instant?, Point?> LookupChanged
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Event triggered when the chart context is updated; unused by this fake
        /// </summary>
        public event Action Updated
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Gets the current moment in time displayed on the chart; unused by this fake
        /// </summary>
        public Instant Moment => default;

        /// <summary>
        /// Gets or initializes the current zoom level
        /// </summary>
        public required int Zoom { get; init; }

        /// <summary>
        /// Gets or initializes the available zoom levels
        /// </summary>
        public required IReadOnlyList<int> Zooms { get; init; }

        /// <summary>
        /// Gets the current time resolution; unused by this fake
        /// </summary>
        public Duration Resolution => Duration.Zero;

        /// <summary>
        /// Gets the number of pixels per resolution unit; unused by this fake
        /// </summary>
        public int PxPerResolution => 0;

        /// <summary>
        /// Gets the available time resolutions; unused by this fake
        /// </summary>
        public IReadOnlyList<Duration> Resolutions => [];

        /// <summary>
        /// Gets a value indicating whether the chart is locked; always false for this fake
        /// </summary>
        public bool IsLocked => false;

        /// <summary>
        /// Gets the number of milliseconds per pixel; unused by this fake
        /// </summary>
        public int MsPerPx => 0;

        /// <summary>
        /// Gets the time zone used for chart display; unused by this fake
        /// </summary>
        public DateTimeZone TimeZone => DateTimeZone.Utc;

        /// <summary>
        /// Gets the time zone offset in minutes; unused by this fake
        /// </summary>
        public int TimeZoneOffset => 0;

        /// <summary>
        /// Gets the time bounds of the chart data; unused by this fake
        /// </summary>
        public ValueRange<Instant> Bounds => ValueRange.Create(NodaConstants.UnixEpoch, NodaConstants.UnixEpoch);

        /// <summary>
        /// Gets the currently visible time range; unused by this fake
        /// </summary>
        public ValueRange<Instant> View => ValueRange.Create(NodaConstants.UnixEpoch, NodaConstants.UnixEpoch);

        /// <summary>
        /// Gets the collection of pane contexts; always empty for this fake
        /// </summary>
        public IReadOnlyCollection<IPaneContext> Panes => [];

        /// <summary>
        /// Configures the chart with available zoom levels and resolutions; not supported by this fake
        /// </summary>
        /// <param name="zooms">The available zoom levels</param>
        /// <param name="resolutions">The available time resolutions</param>
        public void Configure(IReadOnlyList<int> zooms, IReadOnlyList<int> resolutions) =>
            throw new NotSupportedException();

        /// <summary>
        /// Sets the current moment in time; not supported by this fake
        /// </summary>
        /// <param name="moment">The moment to set</param>
        public void SetMoment(Instant moment) => throw new NotSupportedException();

        /// <summary>
        /// Sets the zoom level; not supported by this fake, since Zoom is fixed via the object initializer
        /// </summary>
        /// <param name="zoom">The zoom level to set</param>
        public void SetZoom(int zoom) => throw new NotSupportedException();

        /// <summary>
        /// Sets the time resolution; not supported by this fake
        /// </summary>
        /// <param name="resolution">The resolution to set</param>
        public void SetResolution(Duration resolution) => throw new NotSupportedException();

        /// <summary>
        /// Registers a pane context with the chart; not supported by this fake
        /// </summary>
        /// <param name="pane">The pane context to register</param>
        /// <returns>Never returns; always throws</returns>
        public Action RegisterPane(IPaneContext pane) => throw new NotSupportedException();

        /// <summary>
        /// Requests a redraw of the chart; a no-op for this fake
        /// </summary>
        public void RequestDraw() { }

        /// <summary>
        /// Requests an overlay at the specified point; a no-op for this fake
        /// </summary>
        /// <param name="point">The point for the overlay, or null to hide overlay</param>
        public void RequestOverlay(Point? point) { }
    }
}
