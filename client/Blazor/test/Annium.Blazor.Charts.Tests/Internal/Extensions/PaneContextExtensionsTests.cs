using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Extensions;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Charts.Internal.Extensions;
using Annium.Blazor.Interop;
using Annium.Core.Runtime.Time;
using Annium.Data.Models;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Blazor.Charts.Tests.Internal.Extensions;

/// <summary>
/// Tests for the internal PaneContextExtensions.GetHorizontalLines grid-line calculation
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
    /// Tests that horizontal grid lines are produced at a constant value spacing across the view, and that each
    /// line's Y position matches converting its reported value back through ToY
    /// </summary>
    [Fact]
    public void GetHorizontalLines_ReturnsEvenlySpacedAlignedLines()
    {
        // arrange
        var pane = CreatePane(min: 0m, max: 100m, height: 120m);

        // act
        var lines = pane.GetHorizontalLines();

        // assert
        lines.Count.IsGreater(1);
        var sorted = lines.OrderBy(kv => kv.Key).ToArray();
        var step = sorted[1].Value - sorted[0].Value;
        for (var i = 2; i < sorted.Length; i++)
            (sorted[i].Value - sorted[i - 1].Value).Is(step);
        foreach (var (y, value) in lines)
            pane.ToY(value).Is(y);
    }

    /// <summary>
    /// Tests that no grid lines are produced when the pane's view has equal min and max (a degenerate,
    /// zero-height view), even though DotPerPx is non-zero
    /// </summary>
    [Fact]
    public void GetHorizontalLines_MinEqualsMax_ReturnsEmpty()
    {
        // arrange
        var pane = new FakePaneContext { View = ValueRange.Create(5m, 5m), DotPerPx = 2m };

        // assert
        pane.GetHorizontalLines().IsEmpty();
    }

    /// <summary>
    /// Tests that no grid lines are produced when the pane's DotPerPx is zero, even though the view has a
    /// non-degenerate range
    /// </summary>
    [Fact]
    public void GetHorizontalLines_DotPerPxZero_ReturnsEmpty()
    {
        // arrange
        var pane = new FakePaneContext { View = ValueRange.Create(0m, 100m), DotPerPx = 0m };

        // assert
        pane.GetHorizontalLines().IsEmpty();
    }

    /// <summary>
    /// Creates and configures a chart and pane context, adjusting the pane's range to establish a concrete view
    /// and DotPerPx
    /// </summary>
    /// <param name="min">The minimum value to adjust the pane's range to</param>
    /// <param name="max">The maximum value to adjust the pane's range to</param>
    /// <param name="height">The pixel height of the pane's rectangle, used to compute DotPerPx</param>
    /// <returns>A configured, managed pane context with an established view</returns>
    private IManagedPaneContext CreatePane(decimal min, decimal max, decimal height)
    {
        Get<ITimeManager>().SetNow(_now);

        var chart = (IManagedChartContext)Get<IChartContext>();
        chart.Configure([1], [1]);
        chart.SetMoment(_now);
        chart.SetRect(new DomRect { Width = 300m });
        chart.Update();

        var pane = Get<IManagedPaneContext>();
        pane.Init(chart);
        pane.SetRect(new DomRect { Height = height });

        var source = new FakeSeriesSource();
        _ = pane.RegisterSource(source);
        pane.AdjustRange(source, min, max);

        return pane;
    }

    /// <summary>
    /// A minimal series source fake used solely as a registration key for pane range adjustment
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

    /// <summary>
    /// A minimal IPaneContext fake exposing only a settable View and DotPerPx, used to independently exercise
    /// each branch of GetHorizontalLines' degenerate-input guard
    /// </summary>
    private sealed class FakePaneContext : IPaneContext
    {
        /// <summary>
        /// Occurs when the time bounds of the pane change; unused by this fake
        /// </summary>
        public event Action<ValueRange<Instant>> OnBoundsChange
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Gets the parent chart context; unused by this fake
        /// </summary>
        public IChartContext Chart => throw new NotSupportedException();

        /// <summary>
        /// Gets the collection of series data sources; always empty for this fake
        /// </summary>
        public IReadOnlyCollection<ISeriesSource> Sources => [];

        /// <summary>
        /// Gets the series context for drawing series data; always null for this fake
        /// </summary>
        public ISeriesContext? Series => null;

        /// <summary>
        /// Gets the bottom horizontal side context; always null for this fake
        /// </summary>
        public IHorizontalSideContext? Bottom => null;

        /// <summary>
        /// Gets the right vertical side context; always null for this fake
        /// </summary>
        public IVerticalSideContext? Right => null;

        /// <summary>
        /// Gets the DOM rectangle bounds of the pane; unused by this fake
        /// </summary>
        public DomRect Rect => default;

        /// <summary>
        /// Gets or initializes the number of dots per pixel for value scaling
        /// </summary>
        public required decimal DotPerPx { get; init; }

        /// <summary>
        /// Gets a value indicating whether the pane is locked; always false for this fake
        /// </summary>
        public bool IsLocked => false;

        /// <summary>
        /// Gets the time bounds of the pane data; unused by this fake
        /// </summary>
        public ValueRange<Instant> Bounds => ValueRange.Create(NodaConstants.UnixEpoch, NodaConstants.UnixEpoch);

        /// <summary>
        /// Gets or initializes the currently visible value range
        /// </summary>
        public required ValueRange<decimal> View { get; init; }

        /// <summary>
        /// Gets the full value range of the pane data; mirrors View for this fake
        /// </summary>
        public ValueRange<decimal> Range => View;

        /// <summary>
        /// Adjusts the value range for a specific series source; not supported by this fake
        /// </summary>
        /// <param name="source">The series source to adjust range for</param>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <returns>Never returns; always throws</returns>
        public bool AdjustRange(ISeriesSource source, decimal min, decimal max) => throw new NotSupportedException();

        /// <summary>
        /// Registers a series source with the pane; not supported by this fake
        /// </summary>
        /// <param name="source">The series source to register</param>
        /// <returns>Never returns; always throws</returns>
        public IDisposable RegisterSource(ISeriesSource source) => throw new NotSupportedException();

        /// <summary>
        /// Sets the series context for the pane; not supported by this fake
        /// </summary>
        /// <param name="series">The series context to set, or null to clear</param>
        public void SetSeries(ISeriesContext? series) => throw new NotSupportedException();

        /// <summary>
        /// Sets the bottom horizontal side context; not supported by this fake
        /// </summary>
        /// <param name="bottom">The bottom context to set, or null to clear</param>
        public void SetBottom(IHorizontalSideContext? bottom) => throw new NotSupportedException();

        /// <summary>
        /// Sets the right vertical side context; not supported by this fake
        /// </summary>
        /// <param name="right">The right context to set, or null to clear</param>
        public void SetRight(IVerticalSideContext? right) => throw new NotSupportedException();
    }
}
