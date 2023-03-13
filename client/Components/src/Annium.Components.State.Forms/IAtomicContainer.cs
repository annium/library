using System;

namespace Annium.Components.State.Forms;

public interface IAtomicContainer<T> : IValueTrackedState<T>, IStatusContainer
    where T : IEquatable<T>
{
}