using Annium.Components.State.Core;

namespace Annium.Components.State.Forms;

public interface ITrackedState : IObservableState
{
    bool HasChanged { get; }
    bool HasBeenTouched { get; }
    void Reset();
    bool IsStatus(params Status[] statuses);
    bool HasStatus(params Status[] statuses);
}