using Annium.Components.State.Operations.Internal;

namespace Annium.Components.State.Operations;

/// <summary>
/// Factory class for creating operation state instances
/// </summary>
public static class OperationState
{
    /// <summary>
    /// Creates a new operation state without data
    /// </summary>
    /// <returns>A new instance of IOperationState</returns>
    public static IOperationState New() => new Internal.OperationState();

    /// <summary>
    /// Creates a new operation state with typed data
    /// </summary>
    /// <typeparam name="T">The type of data associated with the operation</typeparam>
    /// <returns>A new instance of IOperationState&lt;T&gt;</returns>
    public static IOperationState<T> New<T>() => new OperationState<T>();
}
