using System;

namespace Annium.Components.State.Form
{
    public interface IAtomicContainer<T> : IState<T>, IStatusContainer
        where T : IEquatable<T>
    {
    }
}