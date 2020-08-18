using System;
using Annium.Components.State;
using Annium.Components.State.Form;

namespace Annium.Blazor.Ant.Components
{
    public interface IFormField<TValue>
        where TValue : IEquatable<TValue>
    {
        IAtomicContainer<TValue> State { get; }
    }
}