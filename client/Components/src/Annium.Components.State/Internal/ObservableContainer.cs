using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Annium.Components.State.Internal
{
    internal class ObservableContainer
    {
        public IObservable<Unit> Changed { get; }
        private event Action StateChanged = () => { };
        private bool _isMuted;

        protected ObservableContainer()
        {
            Changed = Observable.FromEvent(
                handle => StateChanged += handle,
                handle => StateChanged -= handle
            );
        }

        protected void OnChanged()
        {
            if (!_isMuted)
                StateChanged.Invoke();
        }

        protected MuteScope Mute()
        {
            _isMuted = true;

            return new MuteScope(this);
        }

        private void Unmute() => _isMuted = false;

        internal class MuteScope : IDisposable
        {
            private readonly ObservableContainer _container;

            public MuteScope(
                ObservableContainer container
            )
            {
                _container = container;
            }

            public void Dispose()
            {
                _container.Unmute();
            }
        }
    }
}