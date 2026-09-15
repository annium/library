using System.Diagnostics.CodeAnalysis;
using Annium.Finance.Providers.Abstractions.Domain.Shared.Operations;

namespace Annium.Finance.Providers.Abstractions.Domain.User.Operations;

/// <summary>
/// Represents the outcome of a user (account/trading) provider operation that returns no data.
/// </summary>
/// <remarks>
/// <para>
/// A value type: an outcome is a handful of fields describing what happened, and every operation produces
/// one, so paying for an object to carry it is paying per operation for nothing. Measured at 40 bytes each
/// as a record class; as a struct returned through a <c>ValueTask</c> that completes synchronously, an
/// entire operation allocates nothing at all.
/// </para>
/// <para>
/// The four flags are computed from <see cref="Status"/> rather than stored. They were stored while this
/// was a class, which put the same fact in five places; the struct is the same size either way, because the
/// enum and the flags share one alignment slot.
/// </para>
/// <para>
/// Being a value type, this has an instance nothing constructed — <c>default</c> — which a class did not.
/// <see cref="UserOperationStatus.None"/> is zero so that instance reads as a failure, and
/// <see cref="Message"/> reads through a null check for the same reason.
/// </para>
/// </remarks>
public readonly record struct UserResult : IBaseResult
{
    /// <summary>Creates a successful result carrying no message.</summary>
    /// <returns>A <see cref="UserResult"/> with <see cref="UserOperationStatus.Ok"/> status.</returns>
    public static UserResult Ok() => new(UserOperationStatus.Ok, string.Empty);

    /// <summary>Creates a result with the given status and no message.</summary>
    /// <param name="status">The outcome status of the operation.</param>
    /// <returns>A <see cref="UserResult"/> with the given status.</returns>
    public static UserResult New(UserOperationStatus status) => new(status, string.Empty);

    /// <summary>Creates a result with the given status and message.</summary>
    /// <param name="status">The outcome status of the operation.</param>
    /// <param name="message">A message describing the outcome, typically an error detail.</param>
    /// <returns>A <see cref="UserResult"/> with the given status and message.</returns>
    public static UserResult New(UserOperationStatus status, string message) => new(status, message);

    /// <summary>Creates a data-less result carrying the status and message of a data-bearing result, discarding its data.</summary>
    /// <typeparam name="T">The type of data carried by the source result.</typeparam>
    /// <param name="result">The data-bearing result to take the status and message from.</param>
    /// <returns>A <see cref="UserResult"/> with the same status and message as <paramref name="result"/>.</returns>
    public static UserResult From<T>(UserResult<T> result) => new(result.Status, result.Message);

    /// <summary>Creates a successful result carrying the given data.</summary>
    /// <typeparam name="T">The type of data returned by the operation.</typeparam>
    /// <param name="data">The data returned by the operation.</param>
    /// <returns>A <see cref="UserResult{T}"/> with <see cref="UserOperationStatus.Ok"/> status.</returns>
    public static UserResult<T> Ok<T>(T data) => new(UserOperationStatus.Ok, data, string.Empty);

    /// <summary>Creates a data-bearing result with the given status and no message.</summary>
    /// <typeparam name="T">The type of data returned by the operation.</typeparam>
    /// <param name="status">The outcome status of the operation.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <returns>A <see cref="UserResult{T}"/> with the given status and data.</returns>
    public static UserResult<T> New<T>(UserOperationStatus status, T data) => new(status, data, string.Empty);

    /// <summary>Creates a data-bearing result with the given status, data and message.</summary>
    /// <typeparam name="T">The type of data returned by the operation.</typeparam>
    /// <param name="status">The outcome status of the operation.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <param name="error">A message describing the outcome, typically an error detail.</param>
    /// <returns>A <see cref="UserResult{T}"/> with the given status, data and message.</returns>
    public static UserResult<T> New<T>(UserOperationStatus status, T data, string error) => new(status, data, error);

    /// <summary>Creates a data-bearing result carrying the status and message of a data-less result, attaching new data.</summary>
    /// <typeparam name="T">The type of data to attach.</typeparam>
    /// <param name="result">The data-less result to take the status and message from.</param>
    /// <param name="data">The data to attach to the new result.</param>
    /// <returns>A <see cref="UserResult{T}"/> with the same status and message as <paramref name="result"/>.</returns>
    public static UserResult<T> From<T>(UserResult result, T data) => new(result.Status, data, result.Message);

    /// <summary>Creates a data-bearing result carrying the status and message of another data-bearing result, replacing its data.</summary>
    /// <typeparam name="TSource">The type of data carried by the source result.</typeparam>
    /// <typeparam name="T">The type of data to attach to the new result.</typeparam>
    /// <param name="result">The result to take the status and message from.</param>
    /// <param name="data">The data to attach to the new result.</param>
    /// <returns>A <see cref="UserResult{T}"/> with the same status and message as <paramref name="result"/>, carrying <paramref name="data"/>.</returns>
    public static UserResult<T> From<TSource, T>(UserResult<TSource> result, T data) =>
        new(result.Status, data, result.Message);

    /// <summary>Gets a value indicating whether the operation failed because of a network-level error.</summary>
    public bool IsNetworkError => Status is UserOperationStatus.NetworkError;

    /// <summary>Gets a value indicating whether the operation completed successfully.</summary>
    public bool IsSuccess => Status is UserOperationStatus.Ok;

    /// <summary>Gets a value indicating whether the operation was aborted before it could complete.</summary>
    public bool IsAborted => Status is UserOperationStatus.Aborted;

    /// <summary>Gets a value indicating whether the operation failed for a reason other than a network error or abort.</summary>
    public bool IsFailure => !IsNetworkError && !IsAborted && !IsSuccess;

    /// <summary>Gets the outcome status of the operation.</summary>
    public UserOperationStatus Status { get; }

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
    /// Initializes a new instance of the <see cref="UserResult"/> struct.
    /// </summary>
    /// <param name="status">The outcome status of the operation.</param>
    /// <param name="message">A message describing the outcome, typically an error detail.</param>
    private UserResult(UserOperationStatus status, string message)
    {
        Status = status;
        Message = message;
    }

    /// <summary>Returns the status and message as a string.</summary>
    /// <returns>A string in the form "Status (Message)".</returns>
    public override string ToString() => $"{Status} ({Message})";
}

/// <summary>
/// Represents the outcome of a user (account/trading) provider operation that returns data of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of data returned by the operation.</typeparam>
/// <remarks>See <see cref="UserResult"/> for why this is a value type and what <c>default</c> reads as.</remarks>
public readonly record struct UserResult<T> : IBaseResult<T>
{
    /// <summary>Gets a value indicating whether the operation failed because of a network-level error.</summary>
    public bool IsNetworkError => Status is UserOperationStatus.NetworkError;

    /// <summary>Gets a value indicating whether the operation was aborted before it could complete.</summary>
    public bool IsAborted => Status is UserOperationStatus.Aborted;

    /// <summary>Gets a value indicating whether the operation completed successfully and <see cref="Data"/> is populated.</summary>
    [MemberNotNullWhen(true, nameof(Data))]
    public bool IsSuccess => Status is UserOperationStatus.Ok;

    /// <summary>Gets a value indicating whether the operation failed for a reason other than a network error or abort.</summary>
    public bool IsFailure => !IsNetworkError && !IsAborted && !IsSuccess;

    /// <summary>Gets the outcome status of the operation.</summary>
    public UserOperationStatus Status { get; }

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
    /// Initializes a new instance of the <see cref="UserResult{T}"/> struct.
    /// </summary>
    /// <param name="status">The outcome status of the operation.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <param name="message">A message describing the outcome, typically an error detail.</param>
    internal UserResult(UserOperationStatus status, T? data, string message)
    {
        Status = status;
        Data = data;
        Message = message;
    }

    /// <summary>Returns the status and message as a string.</summary>
    /// <returns>A string in the form "Status (Message)".</returns>
    public override string ToString() => $"{Status} ({Message})";
}
