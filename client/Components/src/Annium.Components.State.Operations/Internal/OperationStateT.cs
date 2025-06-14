using Annium.Data.Operations;

namespace Annium.Components.State.Operations.Internal;

/// <summary>
/// Internal implementation of an operation state with typed data
/// </summary>
/// <typeparam name="T">The type of data associated with the operation</typeparam>
internal class OperationState<T> : OperationStateBase, IOperationState<T>
{
    /// <summary>
    /// Gets the data associated with the operation
    /// </summary>
    public T Data { get; private set; } = default!;

    /// <summary>
    /// Marks the operation as successfully completed with the provided data
    /// </summary>
    /// <param name="data">The data to associate with the successful operation</param>
    public void Succeed(T data)
    {
        Data = data;
        SucceedInternal();
    }

    /// <summary>
    /// Marks the operation as failed with the provided data result
    /// </summary>
    /// <param name="result">The data result containing error information and data</param>
    public void Fail(IDataResultBase<T> result)
    {
        Data = result.Data;
        FailInternal(result);
    }

    /// <summary>
    /// Marks the operation as failed with the provided result
    /// </summary>
    /// <param name="result">The result containing error information</param>
    public void Fail(IResultBase result)
    {
        Data = default!;
        FailInternal(result);
    }

    /// <summary>
    /// Resets the operation state to its initial state
    /// </summary>
    public void Reset()
    {
        Data = default!;
        ResetInternal();
    }
}
