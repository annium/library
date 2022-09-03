using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data;
using Annium.Blazor.Charts.Domain;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Internal.Data;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;
using Demo.Blazor.Charts.Domain;
using Microsoft.AspNetCore.Components;
using NodaTime;

namespace Demo.Blazor.Charts.Pages.Home;

public partial class Page
{
    [Inject]
    private ITimeProvider TimeProvider { get; set; } = default!;

    [Inject]
    private IChartContext ChartContext { get; set; } = default!;

    [Inject]
    private IHttpRequestFactory Api { get; set; } = default!;

    [Inject]
    private IIndex<SerializerKey, ISerializer<string>> Serializers { get; set; } = default!;

    [Inject]
    private ISeriesSourceFactory SeriesSourceFactory { get; set; } = default!;

    [Inject]
    private IMapper Mapper { get; set; } = default!;

    [Inject]
    private Style Styles { get; set; } = default!;

    [Inject]
    public ILogger<Page> Logger { get; set; } = default!;

    private ISeriesSource<ICandle> _candleSeries = default!;

    private ISeriesSource<IValue> _openSeries = default!;

    protected override void OnInitialized()
    {
        ChartContext.Configure(ImmutableArray.Create(1, 2, 4, 8, 16), ImmutableArray.Create(1, 5, 15, 30));

        // ChartContext.SetMoment(SystemClock.Instance.GetCurrentInstant() - Duration.FromDays(1000));

        _candleSeries = SeriesSourceFactory.Create(ChartContext.Resolution, LoadCandles, new SeriesSourceCacheOptions(true));
        _openSeries = SeriesSourceFactory.Create(_candleSeries, x => new LineValue(x.Moment, x.Open) as IValue);
    }

    private async Task<IReadOnlyList<ICandle>> LoadCandles(
        Duration resolution,
        Instant start,
        Instant end
    )
    {
        var response = await Api.New("https://finnhub.io")
            .Get("/api/v1/crypto/candle")
            .Param("symbol", "BINANCE:BTCUSDT")
            .Param("resolution", resolution.TotalMinutes.FloorInt32())
            .Param("from", start.ToUnixTimeSeconds())
            .Param("to", end.ToUnixTimeSeconds())
            .Param("token", "cc76b1qad3i1sic4jej0")
            .AsAsync<CandleResponse>();

        var candles = Mapper.Map<IReadOnlyList<Candle>>(response);

        return candles;
    }

    private string GetLabel(ICandle x)
    {
        var absChange = Math.Abs(x.Close - x.Open);
        var percentChange = (absChange / x.Open * 100).Round(2);
        var changeSign = x.Close > x.Open ? "+" : "-";

        return $"O: {x.Open} H: {x.High} L: {x.Low} C: {x.Close}  [ {changeSign}{absChange} ({changeSign}{percentChange:F2}) ]";
    }
}