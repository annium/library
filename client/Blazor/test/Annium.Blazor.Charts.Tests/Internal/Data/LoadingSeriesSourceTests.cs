using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data;
using Annium.Blazor.Charts.Domain;
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
        var source = CreateSource(Array.Empty<Item>);

        // assert
        source.IsNotDefault();
    }

    private ISeriesSource<Item> CreateSource(Func<IReadOnlyList<Item>> getItems)
    {
        var sourceFactory = Get<ISeriesSourceFactory>();
        var source = sourceFactory.Create(Duration.FromMinutes(1), (_, _, _) => Task.FromResult(getItems()));

        return source;
    }

    private sealed record Item(Instant Moment, int Value) : ITimeSeries;
}