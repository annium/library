using System;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Domain.Interfaces.Contexts;
using Annium.Blazor.Charts.Internal.Domain.Models.Contexts;
using Annium.Blazor.Interop;
using Annium.Core.Runtime.Time;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Blazor.Charts.Tests.Internal.Domain.Models.Contexts;

/// <summary>
/// Tests for the double-init guards of PaneContext, SeriesContext, HorizontalSideContext and VerticalSideContext.
/// Each Init() method uses an Interlocked.CompareExchange guard and throws InvalidOperationException on a second
/// call; this pins the intended behavior that the thrown message names the actual type being initialized
/// </summary>
public class ContextInitGuardTests : TestBase
{
    /// <summary>
    /// A fixed timestamp representing the current time for tests
    /// </summary>
    private readonly Instant _now = new LocalDateTime(2020, 1, 15, 14, 20).InUtc().ToInstant();

    /// <summary>
    /// Initializes a new instance of the ContextInitGuardTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public ContextInitGuardTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddCharts());
    }

    /// <summary>
    /// Tests that calling PaneContext.Init a second time throws, with a message naming PaneContext (the correct,
    /// non-copy-pasted case)
    /// </summary>
    [Fact]
    public void PaneContext_Init_CalledTwice_SecondCallThrowsWithCorrectTypeName()
    {
        // arrange
        Get<ITimeManager>().SetNow(_now);
        var chart = (IManagedChartContext)Get<IChartContext>();
        var pane = Get<IManagedPaneContext>();
        pane.Init(chart);

        // act & assert
        Wrap.It(() => pane.Init(chart)).Throws<InvalidOperationException>().Reports(nameof(PaneContext));
    }

    /// <summary>
    /// Tests that calling SeriesContext.Init a second time throws with a message naming SeriesContext (regression
    /// guard for a former copy-paste bug where the message named VerticalSideContext).
    /// </summary>
    [Fact]
    public void SeriesContext_Init_CalledTwice_SecondCallThrowsWithCorrectTypeName()
    {
        // arrange
        var series = Get<IManagedSeriesContext>();
        var canvas = new Canvas(default);
        series.Init(canvas, canvas);

        // act & assert
        Wrap.It(() => series.Init(canvas, canvas)).Throws<InvalidOperationException>().Reports(nameof(SeriesContext));
    }

    /// <summary>
    /// Tests that calling HorizontalSideContext.Init a second time throws with a message naming
    /// HorizontalSideContext (regression guard for a former copy-paste bug where the message named
    /// VerticalSideContext).
    /// </summary>
    [Fact]
    public void HorizontalSideContext_Init_CalledTwice_SecondCallThrowsWithCorrectTypeName()
    {
        // arrange
        var horizontal = Get<IManagedHorizontalSideContext>();
        var canvas = new Canvas(default);
        horizontal.Init(canvas, canvas);

        // act & assert
        Wrap.It(() => horizontal.Init(canvas, canvas))
            .Throws<InvalidOperationException>()
            .Reports(nameof(HorizontalSideContext));
    }

    /// <summary>
    /// Tests that calling VerticalSideContext.Init a second time throws, with a message naming
    /// VerticalSideContext. This one happens to be correct, since VerticalSideContext.Init is the source that the
    /// SeriesContext/HorizontalSideContext guards were copy-pasted from.
    /// </summary>
    [Fact]
    public void VerticalSideContext_Init_CalledTwice_SecondCallThrowsWithCorrectTypeName()
    {
        // arrange
        var vertical = Get<IManagedVerticalSideContext>();
        var canvas = new Canvas(default);
        vertical.Init(canvas, canvas);

        // act & assert
        Wrap.It(() => vertical.Init(canvas, canvas))
            .Throws<InvalidOperationException>()
            .Reports(nameof(VerticalSideContext));
    }
}
