using Annium.Components.State.Core;

namespace Annium.Components.State.Forms;

public interface ITrackedState : IReadOnlyTrackedState
{
    void Reset();
}

public interface IReadOnlyTrackedState : IObservableState
{
    bool HasChanged { get; }
    bool HasBeenTouched { get; }
    bool IsStatus(params Status[] statuses);
    bool HasStatus(params Status[] statuses);
}
