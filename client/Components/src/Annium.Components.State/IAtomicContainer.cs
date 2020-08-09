using System;

namespace Annium.Components.State
{
    public interface IAtomicContainer<T> : IState<T>
        where T : IEquatable<T>
    {
        void SetStatus(Status status);
    }
}