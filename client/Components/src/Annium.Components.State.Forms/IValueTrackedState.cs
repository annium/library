namespace Annium.Components.State.Forms;

public interface IValueTrackedState<T> : IReadOnlyValueTrackedState<T>, ITrackedState
{
    bool Set(T value);
}

public interface IReadOnlyValueTrackedState<T> : IReadOnlyTrackedState
{
    T Value { get; }
}