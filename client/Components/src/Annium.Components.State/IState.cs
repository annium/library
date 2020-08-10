using System;
using System.Reactive;

namespace Annium.Components.State
{
    public interface IState<T> : IState
    {
        T Value { get; }
        bool Set(T value);
    }

    public interface IState
    {
        IObservable<Unit> Changed { get; }
        bool HasChanged { get; }
        bool HasBeenTouched { get; }
        void Reset();
        bool IsStatus(params Status[] statuses);
        bool HasStatus(params Status[] statuses);
    }
}