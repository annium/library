using Annium.Components.State.Core;

namespace Annium.Components.State.Forms;

/// <summary>
/// Interface for a tracked state that can be modified and reset.
/// </summary>
public interface ITrackedState : IReadOnlyTrackedState
{
    /// <summary>
    /// Resets the tracked state to its initial state.
    /// </summary>
    void Reset();
}

/// <summary>
/// Interface for a read-only tracked state that provides change tracking and status information.
/// </summary>
public interface IReadOnlyTrackedState : IObservableState
{
    /// <summary>
    /// Gets a value indicating whether the state has been changed from its initial value.
    /// </summary>
    bool HasChanged { get; }

    /// <summary>
    /// Gets a value indicating whether the state has been interacted with (touched).
    /// </summary>
    bool HasBeenTouched { get; }

    /// <summary>
    /// Determines whether the current status exactly matches any of the specified statuses.
    /// </summary>
    /// <param name="statuses">The statuses to check against.</param>
    /// <returns>True if the current status matches any of the specified statuses; otherwise, false.</returns>
    bool IsStatus(params Status[] statuses);

    /// <summary>
    /// Determines whether the current status is contained within any of the specified statuses.
    /// </summary>
    /// <param name="statuses">The statuses to check against.</param>
    /// <returns>True if the current status is contained within any of the specified statuses; otherwise, false.</returns>
    bool HasStatus(params Status[] statuses);
}
