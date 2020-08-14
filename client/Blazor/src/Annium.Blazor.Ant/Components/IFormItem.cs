using System;
using Annium.Components.State;

namespace Annium.Blazor.Ant.Components
{
    public interface IFormItem<TValue>
        where TValue : IEquatable<TValue>
    {
        IAtomicContainer<TValue> State { get; }
    }
}