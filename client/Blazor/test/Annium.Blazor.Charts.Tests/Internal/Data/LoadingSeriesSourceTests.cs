using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Testing;
using Annium.Testing.Lib;
using NodaTime;
using Xunit;

namespace Annium.Blazor.Charts.Tests.Internal.Data;

public class LoadingSeriesSourceTests : TestBase
{
    public LoadingSeriesSourceTests()
    {
        Register(container => container.AddCharts());
    }

    [Fact]
    public void True_IsTrue()
    {
        // arrange
        var ctx = Get<IChartContext>();
        var sourceFactory = Get<ISeriesSourceFactory>();
        var source = sourceFactory.Create(ctx, LoadItems);

        // assert
        source.IsNotDefault();
    }

    private Task<IReadOnlyList<Item>> LoadItems(
        Duration resolution,
        Instant start,
        Instant end
    )
    {
        return Task.FromResult<IReadOnlyList<Item>>(Array.Empty<Item>());
    }

    private sealed record Item(Instant Moment, int Value) : ITimeSeries;
}