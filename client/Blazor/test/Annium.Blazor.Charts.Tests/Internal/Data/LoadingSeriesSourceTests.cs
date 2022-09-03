using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Internal.Data;
using Annium.Core.Runtime.Time;
using Annium.Testing;
using Annium.Testing.Lib;
using NodaTime;
using Xunit;

namespace Annium.Blazor.Charts.Tests.Internal.Data;

public class LoadingSeriesSourceTests : TestBase
{
    private readonly Instant _now = new LocalDateTime(2020, 1, 15, 14, 20).InUtc().ToInstant();

    public LoadingSeriesSourceTests()
    {
        Register(container => container.AddCharts());
    }

    [Fact]
    public void GetItems_Empty()
    {
        // arrange
        var source = CreateSource(Array.Empty<Item>);

        // act
        var result = source.GetItems(_now - Duration.FromMinutes(5), _now, out var items);

        // assert
        result.IsFalse();
        items.IsEmpty();
    }

    private ISeriesSource<Item> CreateSource(Func<IReadOnlyList<Item>> getItems)
    {
        Get<ITimeManager>().SetNow(_now);

        var sourceFactory = Get<ISeriesSourceFactory>();
        var options = new SeriesSourceOptions(1, 2, 3);
        var cacheOptions = new SeriesSourceCacheOptions(true);
        var source = sourceFactory.Create(Duration.FromMinutes(1), (_, _, _) => Task.FromResult(getItems()), options, cacheOptions);

        return source;
    }

    private sealed record Item(Instant Moment) : ITimeSeries;
}