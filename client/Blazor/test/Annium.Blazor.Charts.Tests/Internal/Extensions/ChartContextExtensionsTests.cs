using System.Linq;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Blazor.Interop;
using Annium.Core.Runtime.Time;
using Annium.Testing;
using NodaTime;
using NodaTime.TimeZones;
using Xunit;

namespace Annium.Blazor.Charts.Tests.Internal.Extensions;

/// <summary>
/// Tests for the internal ChartContextExtensions.GetVerticalLines grid-line calculation, which also exercises
/// the private GetAlignmentDuration helper indirectly. GetAlignmentDuration's Zoom=0 division-by-zero branch
/// cannot be exercised from here: Zoom=0 never survives Configure, which rejects non-positive zooms - see
/// <c>Internal.Domain.Models.Contexts.ChartContextTests.Configure_ZoomZero_ThrowsArgumentException</c>.
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
    /// Tests that grid lines are produced at a constant spacing across the view, and that each line's X position
    /// matches converting its reported moment back through ToX. Rather than hard-coding the machine's local
    /// timezone offset (GetVerticalLines aligns to local time, and ChartContext resolves the system's default
    /// timezone at construction), this asserts structural invariants that hold regardless of the running machine's
    /// timezone
    /// </summary>
    [Fact]
    public void GetVerticalLines_ReturnsEvenlySpacedAlignedLines()
    {
        // arrange - Resolution=1min, Zoom=8 => alignment block (80*1/8=10) falls in the "block > 5" branch,
        // yielding a 5-minute grid alignment; a 10-minute-wide view should produce 2-3 lines
        var chart = CreateChart(zooms: [8], resolutionMinutes: [1], width: 80m);

        // act
        var lines = chart.GetVerticalLines();

        // assert
        lines.Count.IsGreater(1);
        var sorted = lines.OrderBy(kv => kv.Key).ToArray();
        var instants = sorted
            .Select(kv => kv.Value.InZone(chart.TimeZone, Resolvers.LenientResolver).ToInstant())
            .ToArray();
        var step = instants[1] - instants[0];
        for (var i = 2; i < instants.Length; i++)
            (instants[i] - instants[i - 1]).Is(step);
        foreach (var (x, moment) in lines)
            chart.ToX(moment.InZone(chart.TimeZone, Resolvers.LenientResolver).ToInstant()).Is(x);
    }

    /// <summary>
    /// Creates and configures a chart context for grid-line tests
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
