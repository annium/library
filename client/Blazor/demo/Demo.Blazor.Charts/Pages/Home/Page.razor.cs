using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium;
using Annium.Blazor.Charts.Data.Sources;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Blazor.Charts.Domain.Interfaces;
using Annium.Blazor.Charts.Domain.Models;
using Annium.Blazor.Charts.Extensions;
using Annium.Net.Http;
using Demo.Blazor.Charts.Domain;
using Microsoft.AspNetCore.Components;
using NodaTime;

namespace Demo.Blazor.Charts.Pages.Home;

/// <summary>
/// Home page component for the Demo.Blazor.Charts application demonstrating chart functionality.
/// </summary>
public partial class Page
{
    /// <summary>
    /// Gets or sets the chart context for managing chart state and configuration.
    /// </summary>
    [Inject]
    private IChartContext ChartContext { get; set; } = null!;

    /// <summary>
    /// Gets or sets the HTTP request factory for API calls.
    /// </summary>
    [Inject]
    private IHttpRequestFactory Api { get; set; } = null!;

    /// <summary>
    /// Gets or sets the series source factory for creating chart data sources.
    /// </summary>
    [Inject]
    private ISeriesSourceFactory SeriesSourceFactory { get; set; } = null!;

    /// <summary>
    /// Gets or sets the style definitions for the page.
    /// </summary>
    [Inject]
    private Style Styles { get; set; } = null!;

    /// <summary>
    /// The series source for candle data.
    /// </summary>
    private ISeriesSource<ICandle> _candleSeries = null!;

    /// <summary>
    /// The series source for open price points.
    /// </summary>
    private ISeriesSource<PointValue> _openSeries = null!;

    /// <summary>
    /// The series source for multi-range data.
    /// </summary>
    private ISeriesSource<MultiValue<RangeItem>> _multiRangeSeries = null!;

    /// <summary>
    /// The series source for multi-line point data.
    /// </summary>
    private ISeriesSource<MultiValue<LinePointItem>> _multiLineSeries = null!;

    /// <summary>
    /// Function delegate for generating candle label text.
    /// </summary>
    private Func<ICandle, string> _getCandleLabel = delegate
    {
        return string.Empty;
    };

    /// <summary>
    /// Function delegate for generating range label text.
    /// </summary>
    private Func<RangeItem, string> _getRangeLabelText = delegate
    {
        return string.Empty;
    };

    /// <summary>
    /// Function delegate for calculating the left position of range labels.
    /// </summary>
    private Func<IPaneContext, Instant, int> _getRangeLabelLeft = delegate
    {
        return 0;
    };

    /// <summary>
    /// Function delegate for calculating the top position of range labels.
    /// </summary>
    private Func<IPaneContext, RangeItem, int> _getRangeLabelTop = delegate
    {
        return 0;
    };

    /// <summary>
    /// Function delegate for generating line point label text.
    /// </summary>
    private Func<LinePointItem, string> _getLineLabelText = delegate
    {
        return string.Empty;
    };

    /// <summary>
    /// Function delegate for calculating the left position of line labels.
    /// </summary>
    private Func<IPaneContext, Instant, int> _getLineLabelLeft = delegate
    {
        return 0;
    };

    /// <summary>
    /// Function delegate for calculating the top position of line labels.
    /// </summary>
    private Func<IPaneContext, LinePointItem, int> _getLineLabelTop = delegate
    {
        return 0;
    };

    /// <summary>
    /// Initializes the component by configuring the chart context and setting up data sources and label functions.
    /// </summary>
    protected override void OnInitialized()
    {
        ChartContext.Configure([1, 2, 4, 8, 16], [1, 5, 15, 30]);

        _candleSeries = SeriesSourceFactory.CreateChecked(ChartContext.Resolution, LoadCandlesAsync);
        _openSeries = SeriesSourceFactory.CreateUnchecked(
            _candleSeries,
            x => x.Close > x.Open * 1.001m ? new PointValue(x.Moment, x.Open) : null
        );
        _multiRangeSeries = SeriesSourceFactory.CreateChecked(
            _candleSeries,
            x => new MultiValue<RangeItem>(
                x.Moment,
                [new RangeItem(2 * x.Low - x.High, x.Low), new RangeItem(x.High, 2 * x.High - x.Low)]
            )
        );
        _multiLineSeries = SeriesSourceFactory.CreateChecked(
            _candleSeries,
            x => new MultiValue<LinePointItem>(
                x.Moment,
                [new LinePointItem(false, x.Low * .98m), new LinePointItem(true, x.High * 1.02m)]
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

    /// <summary>
    /// Loads candle data from the Binance API for the specified time range and resolution.
    /// </summary>
    /// <param name="resolution">The time resolution for the candles.</param>
    /// <param name="start">The start time for the data range.</param>
    /// <param name="end">The end time for the data range.</param>
    /// <returns>A read-only list of candle data.</returns>
    private async Task<IReadOnlyList<ICandle>> LoadCandlesAsync(Duration resolution, Instant start, Instant end)
    {
        var response = await Api.New("https://api.binance.com")
            .Get("/api/v3/klines")
            .Param("symbol", "BTCUSDT")
            .Param("interval", $"{resolution.TotalMinutes.FloorInt32()}m")
            .Param("startTime", start.ToUnixTimeMilliseconds())
            .Param("endTime", end.ToUnixTimeMilliseconds())
            .AsAsync<Candle[]>();

        return response ?? [];
    }

    /// <summary>
    /// Represents a line point item with upper/lower indicator and value.
    /// </summary>
    /// <param name="IsUpper">A value indicating whether this is an upper point.</param>
    /// <param name="Value">The decimal value of the point.</param>
    private sealed record LinePointItem(bool IsUpper, decimal Value) : IPointItem;
}
