using Annium.Components.State.Core;

namespace Annium.Components.State.Forms;

public interface IValueTrackedState<T> : ITrackedState
{
    T Value { get; }
    bool Set(T value);
}
