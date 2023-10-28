namespace Annium.Components.State.Forms;

public interface IValueTrackedState<T> : IReadOnlyValueTrackedState<T>, ITrackedState
{
    void Init(T value);
    bool Set(T value);
}

public interface IReadOnlyValueTrackedState<T> : IReadOnlyTrackedState
{
    T Value { get; }
}
