using System;

namespace Annium.Components.State
{
    public interface IAtomicContainer<T> : IState<T>
        where T : IEquatable<T>
    {
        Status Status { get; }
        string Message { get; }
        void SetStatus(Status status);
        void SetStatus(Status status, string message);
    }
}