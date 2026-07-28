using System;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Core.Runtime.Time;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Blazor.Charts.Tests.Internal.Domain.Models.Contexts;

/// <summary>
/// Tests for ChartContext's Configure/SetZoom/SetResolution validation and RegisterPane's duplicate-registration
/// guard
/// </summary>
public class ChartContextTests : TestBase
{
    /// <summary>
    /// A fixed timestamp representing the current time for tests
    /// </summary>
    private readonly Instant _now = new LocalDateTime(2020, 1, 15, 14, 20).InUtc().ToInstant();

    /// <summary>
    /// Initializes a new instance of the ChartContextTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public ChartContextTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddCharts());
    }

    /// <summary>
    /// Tests that Configure rejects an empty zooms list
    /// </summary>
    [Fact]
    public void Configure_EmptyZooms_Throws()
    {
        // arrange
        var chart = CreateChart();

        // act & assert
        Wrap.It(() => chart.Configure([], [1])).Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that Configure rejects an empty resolutions list
    /// </summary>
    [Fact]
    public void Configure_EmptyResolutions_Throws()
    {
        // arrange
        var chart = CreateChart();

        // act & assert
        Wrap.It(() => chart.Configure([1], [])).Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that Configure rejects a non-positive zoom with a clear ArgumentException instead of letting it reach
    /// UpdateUnits and throw an opaque OverflowException (MsPerPx division by Zoom=0 → Infinity → Int32 overflow).
    /// </summary>
    [Fact]
    public void Configure_ZoomZero_ThrowsArgumentException()
    {
        // arrange
        var chart = CreateChart();

        // act & assert
        Wrap.It(() => chart.Configure([0], [1])).Throws<ArgumentException>().Reports("positive");
    }

    /// <summary>
    /// Tests that Configure selects the middle entry of the configured zooms as the current zoom
    /// </summary>
    [Fact]
    public void Configure_SelectsMiddleZoomAsCurrent()
    {
        // arrange
        var chart = CreateChart();

        // act
        chart.Configure([1, 2, 4, 8], [1]);

        // assert - (4 zooms / 2) floors to index 2
        chart.Zoom.Is(4);
    }

    /// <summary>
    /// Tests that SetZoom rejects a zoom level that is not present in the configured zooms
    /// </summary>
    [Fact]
    public void SetZoom_ValueNotConfigured_Throws()
    {
        // arrange
        var chart = CreateChart();
        chart.Configure([1, 2, 4], [1]);

        // act & assert
        Wrap.It(() => chart.SetZoom(99)).Throws<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Tests that SetZoom accepts a configured zoom level and updates Zoom
    /// </summary>
    [Fact]
    public void SetZoom_ConfiguredValue_UpdatesZoom()
    {
        // arrange
        var chart = CreateChart();
        chart.Configure([1, 2, 4], [1]);

        // act
        chart.SetZoom(2);

        // assert
        chart.Zoom.Is(2);
    }

    /// <summary>
    /// Tests that SetResolution rejects a resolution that is not present in the configured resolutions
    /// </summary>
    [Fact]
    public void SetResolution_ValueNotConfigured_Throws()
    {
        // arrange
        var chart = CreateChart();
        chart.Configure([1], [1, 5]);

        // act & assert
        Wrap.It(() => chart.SetResolution(Duration.FromMinutes(15))).Throws<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Tests that SetResolution accepts a configured resolution and updates Resolution
    /// </summary>
    [Fact]
    public void SetResolution_ConfiguredValue_UpdatesResolution()
    {
        // arrange
        var chart = CreateChart();
        chart.Configure([1], [1, 5]);

        // act
        chart.SetResolution(Duration.FromMinutes(5));

        // assert
        chart.Resolution.Is(Duration.FromMinutes(5));
    }

    /// <summary>
    /// Tests that registering the same pane twice throws
    /// </summary>
    [Fact]
    public void RegisterPane_SamePaneTwice_Throws()
    {
        // arrange
        var chart = CreateChart();
        var pane = Get<IManagedPaneContext>();
        pane.Init(chart);
        chart.RegisterPane(pane);

        // act & assert
        Wrap.It(() => chart.RegisterPane(pane)).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Tests that the action returned by RegisterPane, when invoked twice, throws on the second call since the
    /// pane is no longer registered
    /// </summary>
    [Fact]
    public void RegisterPane_UnregisterCalledTwice_SecondCallThrows()
    {
        // arrange
        var chart = CreateChart();
        var pane = Get<IManagedPaneContext>();
        pane.Init(chart);
        var unregister = chart.RegisterPane(pane);
        unregister();

        // act & assert
        Wrap.It(() => unregister()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Creates a chart context for validation tests
    /// </summary>
    /// <returns>An un-configured, managed chart context</returns>
    private IManagedChartContext CreateChart()
    {
        Get<ITimeManager>().SetNow(_now);

        return (IManagedChartContext)Get<IChartContext>();
    }
}
