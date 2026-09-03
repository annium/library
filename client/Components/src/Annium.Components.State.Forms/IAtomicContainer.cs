namespace Annium.Components.State.Forms;

/// <summary>
/// Represents a container for atomic values that can be tracked and validated.
/// </summary>
/// <typeparam name="T">The type of the atomic value.</typeparam>
public interface IAtomicContainer<T> : IValueTrackedState<T>, IStatusContainer { }
