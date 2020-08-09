using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Annium.Components.State.Internal
{
    internal class ObservableContainer
    {
        public IObservable<Unit> Changed { get; }
        private event Action StateChanged = () => { };

        protected ObservableContainer()
        {
            Changed = Observable.FromEvent(
                handle => StateChanged += handle,
                handle => StateChanged -= handle
            );
        }

        protected void OnChanged() => StateChanged.Invoke();
    }
}