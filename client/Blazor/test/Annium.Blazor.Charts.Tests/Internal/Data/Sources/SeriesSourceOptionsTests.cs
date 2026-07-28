using System;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Blazor.Charts.Tests.Internal.Data.Sources;

/// <summary>
/// Tests for SeriesSourceOptionsBuilder and the resulting SeriesSourceOptions
/// </summary>
public class SeriesSourceOptionsTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the SeriesSourceOptionsTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public SeriesSourceOptionsTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddCharts());
    }

    /// <summary>
    /// Tests that the Init/Set/Build chain produces options that resolve the smallest configured resolution
    /// </summary>
    [Fact]
    public void Builder_InitSetBuild_ResolvesSmallestConfiguredResolution()
    {
        // arrange
        var options = SeriesSourceOptionsBuilder
            .Init(M(1), 1.3m, 1.5m)
            .Set(M(5), 0.9m, 1.1m)
            .Set(M(15), 0.5m, 0.7m)
            .Build();

        // act
        var result = options.GetForResolution(M(1));

        // assert
        result.Is(new SeriesSourceResolutionOptions(1.3m, 1.5m));
    }

    /// <summary>
    /// Tests that the well-known Default options resolve the exact resolution they were configured for
    /// </summary>
    [Fact]
    public void Default_GetForResolution_ExactBaseResolution_ReturnsConfiguredOptions()
    {
        // act
        var result = SeriesSourceOptionsBuilder.Default.GetForResolution(Duration.FromMinutes(1));

        // assert
        result.Is(new SeriesSourceResolutionOptions(1.3m, 1.5m));
    }

    /// <summary>
    /// Tests that GetForResolution returns the options configured exactly for a non-smallest resolution
    /// </summary>
    [Fact]
    public void GetForResolution_ExactMatchForNonSmallestResolution_ReturnsConfiguredOptions()
    {
        // arrange
        var options = SeriesSourceOptionsBuilder
            .Init(M(1), 1.3m, 1.5m)
            .Set(M(5), 0.9m, 1.1m)
            .Set(M(15), 0.5m, 0.7m)
            .Build();

        // act
        var result = options.GetForResolution(M(5));

        // assert
        result.Is(new SeriesSourceResolutionOptions(0.9m, 1.1m));
    }

    /// <summary>
    /// Tests that GetForResolution falls back to the closest configured resolution at or below the requested one
    /// </summary>
    [Fact]
    public void GetForResolution_NoExactMatch_FallsBackToClosestLesserConfiguredResolution()
    {
        // arrange
        var options = SeriesSourceOptionsBuilder
            .Init(M(1), 1.3m, 1.5m)
            .Set(M(5), 0.9m, 1.1m)
            .Set(M(15), 0.5m, 0.7m)
            .Build();

        // act
        var result = options.GetForResolution(M(10));

        // assert
        result.Is(new SeriesSourceResolutionOptions(0.9m, 1.1m));
    }

    /// <summary>
    /// Tests that GetForResolution throws when no configured resolution is at or below the requested one
    /// </summary>
    [Fact]
    public void GetForResolution_NoResolutionAtOrBelowTarget_Throws()
    {
        // arrange
        var options = SeriesSourceOptionsBuilder.Init(M(15), 0.5m, 0.7m).Build();

        // act & assert
        Wrap.It(() => options.GetForResolution(M(1))).Throws<InvalidOperationException>().Reports("configuration");
    }

    /// <summary>
    /// Tests that Build snapshots the builder's configuration: a later Set on the builder must not mutate an
    /// already-built options instance.
    /// </summary>
    [Fact]
    public void Build_SnapshotsOptions_LaterBuilderSetDoesNotMutateBuiltInstance()
    {
        // arrange
        var builder = SeriesSourceOptionsBuilder.Init(M(1), 1.3m, 1.5m);
        var built = builder.Build();

        // act — mutate the builder after building
        builder.Set(M(5), 0.9m, 1.1m);

        // assert — the already-built snapshot has no M(5) entry, so it falls back to M(1)'s options,
        // while a freshly built instance from the mutated builder does resolve M(5)
        built.GetForResolution(M(5)).Is(new SeriesSourceResolutionOptions(1.3m, 1.5m));
        builder.Build().GetForResolution(M(5)).Is(new SeriesSourceResolutionOptions(0.9m, 1.1m));
    }

    /// <summary>
    /// Creates a Duration from minutes
    /// </summary>
    /// <param name="minutes">The number of minutes</param>
    /// <returns>A Duration representing the specified minutes</returns>
    private static Duration M(int minutes) => Duration.FromMinutes(minutes);
}
