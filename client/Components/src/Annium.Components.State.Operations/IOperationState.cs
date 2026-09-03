using Annium.Data.Operations;

namespace Annium.Components.State.Operations;

/// <summary>
/// Represents an operation state without data that can be started, succeeded, failed, or reset
/// </summary>
public interface IOperationState : IOperationStateBase
{
    /// <summary>
    /// Starts the operation by setting it to loading state
    /// </summary>
    void Start();

    /// <summary>
    /// Marks the operation as successfully completed
    /// </summary>
    void Succeed();

    /// <summary>
    /// Marks the operation as failed with the provided result
    /// </summary>
    /// <param name="result">The result containing error information</param>
    void Fail(IResultBase result);

    /// <summary>
    /// Resets the operation state to its initial state
    /// </summary>
    void Reset();
}
