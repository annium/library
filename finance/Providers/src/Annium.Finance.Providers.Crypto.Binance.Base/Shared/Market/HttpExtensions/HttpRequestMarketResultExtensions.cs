using System.Net;
using System.Threading.Tasks;
using Annium.Finance.Providers.Abstractions.Domain.Market.Operations;
using Annium.Finance.Providers.Core.Market.HttpExtensions;
using Annium.Finance.Providers.Crypto.Binance.Base.Shared.Contracts.Domain;
using Annium.Net.Http;
using OneOf;
using static Annium.Finance.Providers.Crypto.Binance.Base.Shared.HttpExtensions.HttpRequestHelper;

namespace Annium.Finance.Providers.Crypto.Binance.Base.Shared.Market.HttpExtensions;

/// <summary>
/// Adapts raw Binance market-data HTTP responses, that either deserialize to a payload or to an <see cref="OperationResult"/> error, into <see cref="MarketResult{T}"/>.
/// </summary>
/// <remarks>
/// One mapping for every Binance provider. Spot and USD-M futures each carried a copy of this, byte-similar
/// down to the comments, so a fix to this one was a fix to this one alone: the status a rate limit is
/// reported by was corrected here while the providers that actually serve traffic went on reporting the old
/// one.
/// </remarks>
public static class HttpRequestMarketResultExtensions
{
    /// <summary>Sends the request and maps its response into a <see cref="MarketResult{T}"/>, converting an unsuccessful Binance error response into the matching status.</summary>
    /// <typeparam name="T">The type of the expected success payload.</typeparam>
    /// <param name="request">The market-data HTTP request to send.</param>
    /// <returns>A task that resolves to the mapped market result.</returns>
    public static Task<MarketResult<T?>> AsMarketResultAsync<T>(this IHttpRequest request)
        where T : class
    {
        return request.AsMarketResultAsync<T, OperationResult>(GetFailureAsync, MapResponse);
    }

    /// <summary>Maps a response that resolved to either a success payload or an <see cref="OperationResult"/> error into a <see cref="MarketResult{T}"/>.</summary>
    /// <typeparam name="T">The type of the expected success payload.</typeparam>
    /// <param name="response">The HTTP response carrying either the payload or the Binance error.</param>
    /// <returns>The mapped market result.</returns>
    private static MarketResult<T?> MapResponse<T>(IHttpResponse<OneOf<T, OperationResult>> response)
    {
        // if response mapped to success
        if (response.Data.IsT0)
        {
            var data = response.Data.AsT0;

            // if response is successful - return Ok
            if (response.IsSuccess)
                return MarketResult.Ok<T?>(data);

            // otherwise response is mapped to success, but is failure - use response.StatusCode
            {
                var status = MapStatusCode(response.StatusCode);
                return MarketResult.New<T?>(status, data);
            }
        }

        // if response mapped to error, OperationResult - use it to construct response
        {
            var error = response.Data.AsT1;

            // a synthetic error says only what stopped us from reading the body; where the server did answer
            // with a status that means something specific, that is the better account of what happened. A
            // rate limit answered with a page we have no serializer for is a rate limit, not a parse failure
            var byStatus = MapStatusCode(response.StatusCode);
            var status =
                error.IsSynthetic && byStatus != MarketOperationStatus.UnknownError
                    ? byStatus
                    : MapOperationCode(error.Code);

            return MarketResult.New<T?>(status, default, error.Message);
        }
    }

    /// <summary>Maps an HTTP status code from an otherwise-successfully-parsed Binance response into a market operation status.</summary>
    /// <param name="code">The HTTP status code returned by Binance.</param>
    /// <returns>The equivalent market operation status.</returns>
    private static MarketOperationStatus MapStatusCode(HttpStatusCode code) =>
        code switch
        {
            (HttpStatusCode)418 or HttpStatusCode.TooManyRequests => MarketOperationStatus.TooManyRequests,
            HttpStatusCode.BadRequest => MarketOperationStatus.BadRequest,
            // 401/403/404 fell through to UnknownError here while the user mapping named all three. The
            // asymmetry was not a decision - this half simply had no Forbidden to map to until now
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => MarketOperationStatus.Forbidden,
            HttpStatusCode.NotFound => MarketOperationStatus.NotFound,
            _ => MarketOperationStatus.UnknownError,
        };

    /// <summary>Maps a Binance <see cref="OperationResult"/> error code into a market operation status.</summary>
    /// <param name="code">The error code returned by Binance in the operation result.</param>
    /// <returns>The equivalent market operation status.</returns>
    /// <remarks>
    /// The code list lives in <see cref="BinanceErrors"/> and is shared with the user mapping - these two
    /// were separate code lists once and drifted, which is the divergence sharing them makes impossible.
    /// <see cref="BinanceErrorClass.InsufficientFunds"/> has no vocabulary on this half and needs none:
    /// market endpoints do not spend anything. It is folded into the refusal case rather than given a
    /// meaningless status of its own.
    /// </remarks>
    private static MarketOperationStatus MapOperationCode(long code) =>
        BinanceErrors.Classify(code) switch
        {
            BinanceErrorClass.Transport => MarketOperationStatus.NetworkError,
            BinanceErrorClass.Aborted => MarketOperationStatus.Aborted,
            BinanceErrorClass.Unparsed => MarketOperationStatus.ParseError,
            BinanceErrorClass.RateLimited => MarketOperationStatus.TooManyRequests,
            BinanceErrorClass.Access => MarketOperationStatus.Forbidden,
            BinanceErrorClass.NotFound => MarketOperationStatus.NotFound,
            BinanceErrorClass.Refused or BinanceErrorClass.InsufficientFunds => MarketOperationStatus.BadRequest,
            _ => MarketOperationStatus.UnknownError,
        };
}
