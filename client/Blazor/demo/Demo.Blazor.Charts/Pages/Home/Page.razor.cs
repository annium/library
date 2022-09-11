using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Domain.Models;
using Annium.Blazor.Charts.Extensions;
using Annium.Core.Mapper;
using Annium.Core.Primitives;
using Annium.Net.Http;
using Demo.Blazor.Charts.Domain;
using Microsoft.AspNetCore.Components;
using NodaTime;

namespace Demo.Blazor.Charts.Pages.Home;

public partial class Page
{
    [Inject]
    private IChartContext ChartContext { get; set; } = default!;

    [Inject]
    private IHttpRequestFactory Api { get; set; } = default!;

    [Inject]
    private ISeriesSourceFactory SeriesSourceFactory { get; set; } = default!;

    [Inject]
    private IMapper Mapper { get; set; } = default!;

    [Inject]
    private Style Styles { get; set; } = default!;

    private ISeriesSource<ICandle> _candleSeries = default!;
    private ISeriesSource<PointValue> _openSeries = default!;
    private ISeriesSource<MultiValue<RangeItem>> _multiRangeSeries = default!;
    private ISeriesSource<MultiValue<LinePointItem>> _multiLineSeries = default!;
    private Func<ICandle, string> _getCandleLabel = delegate { return string.Empty; };
    private Func<RangeItem, string> _getRangeLabelText = delegate { return string.Empty; };
    private Func<IPaneContext, Instant, int> _getRangeLabelLeft = delegate { return 0; };
    private Func<IPaneContext, RangeItem, int> _getRangeLabelTop = delegate { return 0; };
    private Func<LinePointItem, string> _getLineLabelText = delegate { return string.Empty; };
    private Func<IPaneContext, Instant, int> _getLineLabelLeft = delegate { return 0; };
    private Func<IPaneContext, LinePointItem, int> _getLineLabelTop = delegate { return 0; };

    protected override void OnInitialized()
    {
        ChartContext.Configure(ImmutableArray.Create(1, 2, 4, 8, 16), ImmutableArray.Create(1, 5, 15, 30));

        _candleSeries = SeriesSourceFactory.CreateChecked(ChartContext.Resolution, LoadCandles);
        _openSeries = SeriesSourceFactory.CreateChecked(_candleSeries, x => new PointValue(x.Moment, x.Open));
        _multiRangeSeries = SeriesSourceFactory.CreateChecked(
            _candleSeries,
            x => new MultiValue<RangeItem>(x.Moment, new[]
                {
                    new RangeItem(2 * x.Low - x.High, x.Low),
                    new RangeItem(x.High, 2 * x.High - x.Low)
                }
            )
        );
        _multiLineSeries = SeriesSourceFactory.CreateChecked(
            _candleSeries,
            x => new MultiValue<LinePointItem>(x.Moment, new[]
                {
                    new LinePointItem(false, x.Low * .98m),
                    new LinePointItem(true, x.High * 1.02m)
                }
            )
        );

        _getCandleLabel = x =>
        {
            var absChange = Math.Abs(x.Close - x.Open);
            var percentChange = (absChange / x.Open * 100).Round(2);
            var changeSign = x.Close > x.Open ? "+" : "-";

            return $"O: {x.Open} H: {x.High} L: {x.Low} C: {x.Close}  [ {changeSign}{absChange} ({changeSign}{percentChange:F2}) ]";
        };

        _getRangeLabelText = range => $"{range.Low:F2} - {range.High:F2}";
        _getRangeLabelLeft = (ctx, moment) => ctx.ToX(moment) + 5;
        _getRangeLabelTop = (ctx, range) => ctx.ToY((range.Low + range.High) / 2);

        _getLineLabelText = point => $"{point.Value:F2}";
        _getLineLabelLeft = (ctx, moment) => ctx.ToX(moment) + 5;
        _getLineLabelTop = (ctx, point) => ctx.ToY(point.Value);
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

    private sealed record LinePointItem(bool IsUpper, decimal Value) : IPointItem;
}