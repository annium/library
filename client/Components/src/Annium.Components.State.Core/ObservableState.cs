using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Annium.Components.State.Core;

public class ObservableState : IObservableState
{
    public IObservable<Unit> Changed { get; }
    private event Action StateChanged = () => { };
    private bool _isMuted;

    protected ObservableState()
    {
        Changed = Observable.FromEvent(
            handle => StateChanged += handle,
            handle => StateChanged -= handle
        );
    }

    public IDisposable Mute()
    {
        _isMuted = true;

        return new MuteScope(this);
    }

    protected void NotifyChanged()
    {
        if (!_isMuted)
            StateChanged.Invoke();
    }

    private void Unmute() => _isMuted = false;

    private class MuteScope : IDisposable
    {
        private readonly ObservableState _state;

        public MuteScope(
            ObservableState state
        )
        {
            _state = state;
        }

        public void Dispose()
        {
            _state.Unmute();
        }
    }
}