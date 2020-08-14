using System;

namespace Annium.Components.State
{
    public interface IAtomicContainer<T> : IState<T>, IStatusContainer
        where T : IEquatable<T>
    {
    }
}