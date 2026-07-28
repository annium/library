using System;
using Annium.Data.Models.Extensions;

namespace Annium.Components.State.Forms.Internal;

/// <summary>
/// Represents a reference to a value-tracked state with its change notification subscription.
/// Shared by containers (array, map) that keep a collection of child item states.
/// </summary>
/// <typeparam name="TItem">The type of value tracked by the referenced state.</typeparam>
internal class StateReference<TItem>
{
    /// <summary>
    /// Gets the state reference.
    /// </summary>
    public IValueTrackedState<TItem> Ref { get; }

    /// <summary>
    /// Gets the subscription for change notifications.
    /// </summary>
    public IDisposable Subscription { get; }

    /// <summary>
    /// Initializes a new instance of the StateReference class.
    /// </summary>
    /// <param name="ref">The state reference.</param>
    /// <param name="subscription">The change notification subscription.</param>
    public StateReference(IValueTrackedState<TItem> @ref, IDisposable subscription)
    {
        Ref = @ref;
        Subscription = subscription;
    }

    /// <summary>
    /// Returns a string representation of the state reference.
    /// </summary>
    /// <returns>A friendly name of the state type.</returns>
    public override string ToString() => Ref.GetType().FriendlyName();
}
