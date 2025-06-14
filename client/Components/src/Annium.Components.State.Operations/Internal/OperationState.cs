using Annium.Data.Operations;

namespace Annium.Components.State.Operations.Internal;

/// <summary>
/// Internal implementation of an operation state without data
/// </summary>
internal class OperationState : OperationStateBase, IOperationState
{
    /// <summary>
    /// Marks the operation as successfully completed
    /// </summary>
    public void Succeed()
    {
        SucceedInternal();
    }

    /// <summary>
    /// Marks the operation as failed with the provided result
    /// </summary>
    /// <param name="result">The result containing error information</param>
    public void Fail(IResultBase result)
    {
        FailInternal(result);
    }

    /// <summary>
    /// Resets the operation state to its initial state
    /// </summary>
    public void Reset()
    {
        ResetInternal();
    }
}
