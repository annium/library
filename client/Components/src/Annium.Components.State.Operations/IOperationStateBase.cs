using Annium.Components.State.Core;
using Annium.Data.Operations;

namespace Annium.Components.State.Operations;

/// <summary>
/// Base interface for operation states that provides loading status and result tracking
/// </summary>
public interface IOperationStateBase : IObservableState, IResultBase
{
    /// <summary>
    /// Gets a value indicating whether the operation is currently loading
    /// </summary>
    bool IsLoading { get; }

    /// <summary>
    /// Gets a value indicating whether the operation has completed (either succeeded or failed)
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// Gets a value indicating whether the operation has succeeded
    /// </summary>
    bool HasSucceed { get; }

    /// <summary>
    /// Gets a value indicating whether the operation has failed
    /// </summary>
    bool HasFailed { get; }
}
