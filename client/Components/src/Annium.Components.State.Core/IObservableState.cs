using System;
using System.Reactive;

namespace Annium.Components.State.Core;

public interface IObservableState
{
    IObservable<Unit> Changed { get; }
    IDisposable Mute();
}
