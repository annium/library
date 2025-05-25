using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Annium.Components.State.Core;

public abstract class ObservableState : IObservableState
{
    public IObservable<Unit> Changed { get; }
    private event Action StateChanged = delegate { };
    private bool _isMuted;

    protected ObservableState()
    {
        Changed = Observable.FromEvent(handle => StateChanged += handle, handle => StateChanged -= handle);
    }

    public IDisposable Mute()
    {
        _isMuted = true;

        return new MuteScope(Unmute);
    }

    protected void NotifyChanged()
    {
        if (!_isMuted)
            StateChanged.Invoke();
    }

    private void Unmute() => _isMuted = false;
}

file readonly struct MuteScope : IDisposable
{
    private readonly Action _unmute;

    public MuteScope(Action unmute)
    {
        _unmute = unmute;
    }

    public void Dispose() => _unmute();
}
