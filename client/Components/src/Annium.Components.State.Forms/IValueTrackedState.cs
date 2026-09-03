namespace Annium.Components.State.Forms;

/// <summary>
/// Interface for a value-tracked state that can be modified and provides value management capabilities.
/// </summary>
/// <typeparam name="T">The type of value being tracked.</typeparam>
public interface IValueTrackedState<T> : IReadOnlyValueTrackedState<T>, ITrackedState
{
    /// <summary>
    /// Initializes the tracked state with the specified value.
    /// </summary>
    /// <param name="value">The value to initialize the state with.</param>
    void Init(T value);

    /// <summary>
    /// Sets the value of the tracked state.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <returns>True if the value was successfully set and different from the current value; otherwise, false.</returns>
    bool Set(T value);
}

/// <summary>
/// Interface for a read-only value-tracked state that provides access to the tracked value.
/// </summary>
/// <typeparam name="T">The type of value being tracked.</typeparam>
public interface IReadOnlyValueTrackedState<T> : IReadOnlyTrackedState
{
    /// <summary>
    /// Gets the current value of the tracked state.
    /// </summary>
    T Value { get; }
}
