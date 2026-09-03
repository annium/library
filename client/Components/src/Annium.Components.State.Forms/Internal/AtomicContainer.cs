using System.Collections.Generic;
using Annium.Components.State.Core;
using Annium.Logging;

namespace Annium.Components.State.Forms.Internal;

/// <summary>
/// Represents a state container for managing atomic values with change tracking and validation status.
/// </summary>
/// <typeparam name="T">The type of the atomic value.</typeparam>
internal class AtomicContainer<T> : ObservableState, IAtomicContainer<T>, ILogSubject
{
    /// <summary>
    /// Gets the current value of the atomic container.
    /// </summary>
    public T Value { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the atomic value has changed from its initial value.
    /// </summary>
    public bool HasChanged => !EqualityComparer<T>.Default.Equals(Value, _initialValue);

    /// <summary>
    /// Gets a value indicating whether the atomic value has been touched (modified).
    /// </summary>
    public bool HasBeenTouched { get; private set; }

    /// <summary>
    /// Gets the current validation status of the atomic value.
    /// </summary>
    public Status Status { get; private set; }

    /// <summary>
    /// Gets the status message associated with the current validation status.
    /// </summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the logger instance for this container.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The initial value of the atomic container.
    /// </summary>
    private T _initialValue;

    /// <summary>
    /// Initializes a new instance of the AtomicContainer class.
    /// </summary>
    /// <param name="initialValue">The initial value of the atomic container.</param>
    /// <param name="logger">The logger instance.</param>
    public AtomicContainer(T initialValue, ILogger logger)
    {
        Logger = logger;
        Value = _initialValue = initialValue;
    }

    /// <summary>
    /// Initializes the atomic container with a new value, resetting the touched state and status.
    /// </summary>
    /// <param name="value">The new value to initialize with.</param>
    public void Init(T value)
    {
        Value = _initialValue = value;
        HasBeenTouched = false;
        Status = Status.None;
        NotifyChanged();
    }

    /// <summary>
    /// Sets a new value for the atomic container, marking it as touched if the value changes.
    /// </summary>
    /// <param name="value">The new value to set.</param>
    /// <returns>True if the value was changed, false otherwise.</returns>
    public bool Set(T value)
    {
        if (EqualityComparer<T>.Default.Equals(value, Value))
            return false;

        Value = value;
        HasBeenTouched = true;
        NotifyChanged();

        return true;
    }

    /// <summary>
    /// Resets the atomic container to its initial value.
    /// </summary>
    public void Reset() => Init(_initialValue);

    /// <summary>
    /// Sets the validation status of the atomic container.
    /// </summary>
    /// <param name="status">The status to set.</param>
    public void SetStatus(Status status) => SetStatus(status, string.Empty);

    /// <summary>
    /// Sets the validation status and message of the atomic container.
    /// </summary>
    /// <param name="status">The status to set.</param>
    /// <param name="message">The status message to set.</param>
    public void SetStatus(Status status, string message)
    {
        if (status == Status && message == Message)
            return;

        Status = status;
        Message = message;
        NotifyChanged();
    }

    /// <summary>
    /// Checks if the atomic container has any of the specified statuses.
    /// </summary>
    /// <param name="statuses">The statuses to check for.</param>
    /// <returns>True if the container has any of the specified statuses, false otherwise.</returns>
    public bool IsStatus(params Status[] statuses)
    {
        foreach (var status in statuses)
            if (Status == status)
                return true;

        return false;
    }

    /// <summary>
    /// Checks if the atomic container has any of the specified statuses.
    /// </summary>
    /// <param name="statuses">The statuses to check for.</param>
    /// <returns>True if the container has any of the specified statuses, false otherwise.</returns>
    public bool HasStatus(params Status[] statuses)
    {
        foreach (var status in statuses)
            if (Status == status)
                return true;

        return false;
    }
}
