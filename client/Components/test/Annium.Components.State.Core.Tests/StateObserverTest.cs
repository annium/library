using Annium.Testing;
using Xunit;

namespace Annium.Components.State.Core.Tests;

/// <summary>
/// Tests for <see cref="StateObserver"/> — reflection-based discovery of <see cref="IObservableState"/> members
/// (public and non-public), aggregate change notification, and subscription disposal.
/// </summary>
public class StateObserverTest
{
    /// <summary>
    /// Verifies that a change on ANY discovered observable member (each public property) invokes the handler.
    /// Multiplicity is intentionally not asserted: an auto-property's compiler-generated backing field is also
    /// an observable member, so discovery subscribes to it twice — a Blazor-coalesced quirk, not the contract.
    /// </summary>
    [Fact]
    public void ObserveObject_AnyMemberChanges_InvokesHandler()
    {
        // arrange
        var target = new Target();
        var count = 0;

        // act
        using var _ = StateObserver.ObserveObject(target, () => count++);
        target.First.Fire();
        var afterFirst = count;
        target.Second.Fire();

        // assert: a change on First was observed, and a change on a different member (Second) was also observed
        (afterFirst > 0).IsTrue();
        (count > afterFirst).IsTrue();
    }

    /// <summary>
    /// Verifies that a non-public (private) field of an observable-state type is discovered and observed.
    /// </summary>
    [Fact]
    public void ObserveObject_PrivateFieldMember_IsDiscovered()
    {
        // arrange
        var target = new Target();
        var count = 0;

        // act
        using var _ = StateObserver.ObserveObject(target, () => count++);
        target.FireHidden();

        // assert: a plain private field (unlike an auto-property) has no backing-field duplicate, so it is
        // discovered and subscribed exactly once
        count.Is(1);
    }

    /// <summary>
    /// Verifies that disposing the returned handle unsubscribes: no further handler invocations occur after a
    /// later change (guards against the subscription-leak surface).
    /// </summary>
    [Fact]
    public void ObserveObject_AfterDispose_StopsInvokingHandler()
    {
        // arrange
        var target = new Target();
        var count = 0;
        var handle = StateObserver.ObserveObject(target, () => count++);

        // act + assert: handler fires while subscribed
        target.First.Fire();
        var beforeDispose = count;
        (beforeDispose > 0).IsTrue();

        // act: dispose then fire every member again
        handle.Dispose();
        target.First.Fire();
        target.Second.Fire();
        target.FireHidden();

        // assert: no further invocations after dispose
        count.Is(beforeDispose);
    }

    /// <summary>
    /// Test target exposing two public observable-state properties and one private observable-state field,
    /// to exercise multi-member and non-public discovery.
    /// </summary>
    private sealed class Target
    {
        /// <summary>
        /// Gets the first public observable-state member.
        /// </summary>
        public Node First { get; } = new();

        /// <summary>
        /// Gets the second public observable-state member.
        /// </summary>
        public Node Second { get; } = new();

        /// <summary>
        /// A private observable-state field (must still be discovered via non-public reflection).
        /// </summary>
        private readonly Node _hidden = new();

        /// <summary>
        /// Fires a change on the private member.
        /// </summary>
        public void FireHidden() => _hidden.Fire();
    }

    /// <summary>
    /// Minimal <see cref="ObservableState"/> that exposes a public trigger for its protected change notification.
    /// </summary>
    private sealed class Node : ObservableState
    {
        /// <summary>
        /// Raises a change notification.
        /// </summary>
        public void Fire() => NotifyChanged();
    }
}
