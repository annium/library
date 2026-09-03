using Annium.Data.Operations;

namespace Annium.Components.State.Operations;

/// <summary>
/// Represents an operation state with typed data that can be started, succeeded, failed, or reset
/// </summary>
/// <typeparam name="T">The type of data associated with the operation</typeparam>
public interface IOperationState<T> : IOperationStateBase, IDataResultBase<T>
{
    /// <summary>
    /// Starts the operation by setting it to loading state
    /// </summary>
    void Start();

    /// <summary>
    /// Marks the operation as successfully completed with the provided data
    /// </summary>
    /// <param name="data">The data to associate with the successful operation</param>
    void Succeed(T data);

    /// <summary>
    /// Marks the operation as failed with the provided data result.
    /// </summary>
    /// <remarks>
    /// Keeps <see cref="IDataResultBase{T}.Data"/> from the supplied result (the failed operation still carries
    /// data). Contrast with the <see cref="Fail(IResultBase)"/> overload, which clears Data.
    /// </remarks>
    /// <param name="result">The data result containing error information and data</param>
    void Fail(IDataResultBase<T> result);

    /// <summary>
    /// Marks the operation as failed with the provided result.
    /// </summary>
    /// <remarks>
    /// Clears Data to <c>default(T)</c> because no data was supplied. Contrast with the
    /// <see cref="Fail(IDataResultBase{T})"/> overload, which keeps Data.
    /// </remarks>
    /// <param name="result">The result containing error information</param>
    void Fail(IResultBase result);

    /// <summary>
    /// Resets the operation state to its initial state
    /// </summary>
    void Reset();
}
