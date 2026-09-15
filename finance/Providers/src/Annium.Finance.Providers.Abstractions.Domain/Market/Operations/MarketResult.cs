using System.Diagnostics.CodeAnalysis;
using Annium.Finance.Providers.Abstractions.Domain.Shared.Operations;

namespace Annium.Finance.Providers.Abstractions.Domain.Market.Operations;

/// <summary>
/// Represents the outcome of a market data provider operation that returns no data.
/// </summary>
/// <remarks>
/// A value type, for the reasons written out on <c>UserResult</c> — the other half of this pair, which this
/// one matches field for field. Notably this one is also yielded per page from
/// <c>IMarketProvider.LoadCandlesAsync</c>, so a history fetch allocated one result per page and now
/// allocates none.
/// </remarks>
public readonly record struct MarketResult : IBaseResult
{
    /// <summary>Creates a successful result carrying no message.</summary>
    /// <returns>A <see cref="MarketResult"/> with <see cref="MarketOperationStatus.Ok"/> status.</returns>
    public static MarketResult Ok() => new(MarketOperationStatus.Ok, string.Empty);

    /// <summary>Creates a result with the given status and no message.</summary>
    /// <param name="status">The outcome status of the operation.</param>
    /// <returns>A <see cref="MarketResult"/> with the given status.</returns>
    public static MarketResult New(MarketOperationStatus status) => new(status, string.Empty);

    /// <summary>Creates a result with the given status and message.</summary>
    /// <param name="status">The outcome status of the operation.</param>
    /// <param name="message">A message describing the outcome, typically an error detail.</param>
    /// <returns>A <see cref="MarketResult"/> with the given status and message.</returns>
    public static MarketResult New(MarketOperationStatus status, string message) => new(status, message);

    /// <summary>Creates a data-less result carrying the status and message of a data-bearing result, discarding its data.</summary>
    /// <typeparam name="T">The type of data carried by the source result.</typeparam>
    /// <param name="result">The data-bearing result to take the status and message from.</param>
    /// <returns>A <see cref="MarketResult"/> with the same status and message as <paramref name="result"/>.</returns>
    public static MarketResult From<T>(MarketResult<T> result) => new(result.Status, result.Message);

    /// <summary>Creates a successful result carrying the given data.</summary>
    /// <typeparam name="T">The type of data returned by the operation.</typeparam>
    /// <param name="data">The data returned by the operation.</param>
    /// <returns>A <see cref="MarketResult{T}"/> with <see cref="MarketOperationStatus.Ok"/> status.</returns>
    public static MarketResult<T> Ok<T>(T data) => new(MarketOperationStatus.Ok, data, string.Empty);

    /// <summary>Creates a data-bearing result with the given status and no message.</summary>
    /// <typeparam name="T">The type of data returned by the operation.</typeparam>
    /// <param name="status">The outcome status of the operation.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <returns>A <see cref="MarketResult{T}"/> with the given status and data.</returns>
    public static MarketResult<T> New<T>(MarketOperationStatus status, T data) => new(status, data, string.Empty);

    /// <summary>Creates a data-bearing result with the given status, data and message.</summary>
    /// <typeparam name="T">The type of data returned by the operation.</typeparam>
    /// <param name="status">The outcome status of the operation.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <param name="error">A message describing the outcome, typically an error detail.</param>
    /// <returns>A <see cref="MarketResult{T}"/> with the given status, data and message.</returns>
    public static MarketResult<T> New<T>(MarketOperationStatus status, T data, string error) =>
        new(status, data, error);

    /// <summary>Creates a data-bearing result carrying the status and message of a data-less result, attaching new data.</summary>
    /// <typeparam name="T">The type of data to attach.</typeparam>
    /// <param name="result">The data-less result to take the status and message from.</param>
    /// <param name="data">The data to attach to the new result.</param>
    /// <returns>A <see cref="MarketResult{T}"/> with the same status and message as <paramref name="result"/>.</returns>
    public static MarketResult<T> From<T>(MarketResult result, T data) => new(result.Status, data, result.Message);

    /// <summary>Creates a data-bearing result carrying the status and message of another data-bearing result, replacing its data.</summary>
    /// <typeparam name="TSource">The type of data carried by the source result.</typeparam>
    /// <typeparam name="T">The type of data to attach to the new result.</typeparam>
    /// <param name="result">The result to take the status and message from.</param>
    /// <param name="data">The data to attach to the new result.</param>
    /// <returns>A <see cref="MarketResult{T}"/> with the same status and message as <paramref name="result"/>, carrying <paramref name="data"/>.</returns>
    public static MarketResult<T> From<TSource, T>(MarketResult<TSource> result, T data) =>
        new(result.Status, data, result.Message);

    /// <summary>Gets a value indicating whether the operation failed because of a network-level error.</summary>
    public bool IsNetworkError => Status is MarketOperationStatus.NetworkError;

    /// <summary>Gets a value indicating whether the operation was aborted before it could complete.</summary>
    public bool IsAborted => Status is MarketOperationStatus.Aborted;

    /// <summary>Gets a value indicating whether the operation completed successfully.</summary>
    public bool IsSuccess => Status is MarketOperationStatus.Ok;

    /// <summary>Gets a value indicating whether the operation failed for a reason other than a network error or abort.</summary>
    public bool IsFailure => !IsNetworkError && !IsAborted && !IsSuccess;

    /// <summary>Gets the outcome status of the operation.</summary>
    public MarketOperationStatus Status { get; }

    /// <summary>Gets the message describing the outcome, typically an error detail; empty on success.</summary>
    /// <remarks>
    /// Reads through a null check because this is a value type: on an instance nothing constructed the
    /// backing field is null, and the declared type says it is not.
    /// </remarks>
    public string Message
    {
        get => field ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MarketResult"/> struct.
    /// </summary>
    /// <param name="status">The outcome status of the operation.</param>
    /// <param name="message">A message describing the outcome, typically an error detail.</param>
    private MarketResult(MarketOperationStatus status, string message)
    {
        Status = status;
        Message = message;
    }

    /// <summary>Returns the status and message as a string.</summary>
    /// <returns>A string in the form "Status (Message)".</returns>
    public override string ToString() => $"{Status} ({Message})";
}

/// <summary>
/// Represents the outcome of a market data provider operation that returns data of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of data returned by the operation.</typeparam>
/// <remarks>See <see cref="MarketResult"/> for why this is a value type and what <c>default</c> reads as.</remarks>
public readonly record struct MarketResult<T> : IBaseResult<T>
{
    /// <summary>Gets a value indicating whether the operation failed because of a network-level error.</summary>
    public bool IsNetworkError => Status is MarketOperationStatus.NetworkError;

    /// <summary>Gets a value indicating whether the operation was aborted before it could complete.</summary>
    public bool IsAborted => Status is MarketOperationStatus.Aborted;

    /// <summary>Gets a value indicating whether the operation completed successfully and <see cref="Data"/> is populated.</summary>
    [MemberNotNullWhen(true, nameof(Data))]
    public bool IsSuccess => Status is MarketOperationStatus.Ok;

    /// <summary>Gets a value indicating whether the operation failed for a reason other than a network error or abort.</summary>
    public bool IsFailure => !IsNetworkError && !IsAborted && !IsSuccess;

    /// <summary>Gets the outcome status of the operation.</summary>
    public MarketOperationStatus Status { get; }

    /// <summary>Gets the data returned by the operation; set when <see cref="IsSuccess"/> is true.</summary>
    public T? Data { get; }

    /// <summary>Gets the message describing the outcome, typically an error detail; empty on success.</summary>
    /// <remarks>
    /// Reads through a null check because this is a value type: on an instance nothing constructed the
    /// backing field is null, and the declared type says it is not.
    /// </remarks>
    public string Message
    {
        get => field ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MarketResult{T}"/> struct.
    /// </summary>
    /// <param name="status">The outcome status of the operation.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <param name="message">A message describing the outcome, typically an error detail.</param>
    internal MarketResult(MarketOperationStatus status, T? data, string message)
    {
        Status = status;
        Data = data;
        Message = message;
    }

    /// <summary>Returns the status and message as a string.</summary>
    /// <returns>A string in the form "Status (Message)".</returns>
    public override string ToString() => $"{Status} ({Message})";
}
