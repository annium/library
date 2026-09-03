using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Interop;
using Annium.Core.Runtime.Time;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Blazor.Charts.Tests.Extensions;

/// <summary>
/// Tests for ChartContextExtensions ToX/FromX coordinate transformation helpers
/// </summary>
public class ChartContextExtensionsTests : TestBase
{
    /// <summary>
    /// A fixed timestamp representing the current time for tests
    /// </summary>
    private readonly Instant _now = new LocalDateTime(2020, 1, 15, 14, 20).InUtc().ToInstant();

    /// <summary>
    /// Initializes a new instance of the ChartContextExtensionsTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public ChartContextExtensionsTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddCharts());
    }

    /// <summary>
    /// Tests that converting an X coordinate to a moment and back yields the original X coordinate
    /// </summary>
    [Fact]
    public void ToX_FromX_RoundTrip_ForPixelAlignedMoment()
    {
        // arrange
        var chart = CreateChart(zooms: [1], resolutionMinutes: [1], width: 300m);

        // act
        var moment = chart.FromX(150);
        var x = chart.ToX(moment);

        // assert
        x.Is(150);
    }

    /// <summary>
    /// Tests that the start of the view maps to X coordinate zero
    /// </summary>
    [Fact]
    public void ToX_AtViewStart_ReturnsZero()
    {
        // arrange
        var chart = CreateChart(zooms: [1], resolutionMinutes: [1], width: 300m);

        // assert
        chart.ToX(chart.View.Start).Is(0);
    }

    /// <summary>
    /// Tests that the end of the view maps to an X coordinate equal to the chart width in pixels
    /// </summary>
    [Fact]
    public void ToX_AtViewEnd_ReturnsWidthInPixels()
    {
        // arrange
        var chart = CreateChart(zooms: [1], resolutionMinutes: [1], width: 300m);

        // assert
        chart.ToX(chart.View.End).Is(300);
    }

    /// <summary>
    /// Tests that ToX returns zero regardless of the requested moment when MsPerPx floors to zero (a zoom level
    /// large enough, relative to the resolution, that milliseconds-per-pixel rounds down to zero)
    /// </summary>
    [Fact]
    public void ToX_MsPerPxZero_ReturnsZeroRegardlessOfMoment()
    {
        // arrange
        var chart = CreateChart(zooms: [1, 70000], resolutionMinutes: [1], width: 300m);

        // assert
        chart.MsPerPx.Is(0);
        chart.ToX(_now).Is(0);
        chart.ToX(_now + Duration.FromDays(1)).Is(0);
    }

    /// <summary>
    /// Creates and configures a chart context for coordinate math tests
    /// </summary>
    /// <param name="zooms">The available zoom levels to configure; the chart selects the middle entry as current</param>
    /// <param name="resolutionMinutes">The available resolutions, in minutes, to configure</param>
    /// <param name="width">The pixel width of the chart's rectangle, used to compute the view</param>
    /// <returns>A configured, managed chart context with an established view</returns>
    private IManagedChartContext CreateChart(int[] zooms, int[] resolutionMinutes, decimal width)
    {
        Get<ITimeManager>().SetNow(_now);

        var chart = (IManagedChartContext)Get<IChartContext>();
        chart.Configure(zooms, resolutionMinutes);
        chart.SetMoment(_now);
        chart.SetRect(new DomRect { Width = width, Height = 0m });
        chart.Update();

        return chart;
    }
}
