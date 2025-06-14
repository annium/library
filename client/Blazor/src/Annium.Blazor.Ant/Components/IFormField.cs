using System;
using Annium.Components.State.Forms;

namespace Annium.Blazor.Ant.Components;

/// <summary>
/// Defines the contract for a form field component that manages state for a specific value type.
/// </summary>
/// <typeparam name="TValue">The type of value managed by the form field.</typeparam>
public interface IFormField<TValue>
    where TValue : IEquatable<TValue>
{
    /// <summary>
    /// Gets the atomic container that manages the state of the form field.
    /// </summary>
    IAtomicContainer<TValue> State { get; }
}
