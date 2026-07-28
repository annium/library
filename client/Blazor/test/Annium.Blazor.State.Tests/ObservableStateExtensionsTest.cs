using System;
using System.Collections.Generic;
using Annium.Components.State.Core;
using Annium.Testing;
using Xunit;

namespace Annium.Blazor.State.Tests;

/// <summary>
/// Tests for <see cref="ObservableStateExtensions"/> — the <c>Notify</c> subscription helpers that bridge
/// <see cref="IObservableState.Changed"/> to plain handlers, both for a single state and for a collection of
/// states. The <c>ComponentBase</c>-targeted overloads are not covered here: exercising them would require either
/// a real <c>ComponentBase</c> render pipeline (bUnit) or reflection-invoking a protected framework method on a
/// bare subclass, neither of which pins behavior beyond "the static <c>StateHasChanged</c> lookup resolved",
/// which every other test in this class already exercises implicitly at type-init time.
/// </summary>
public class ObservableStateExtensionsTest
{
    /// <summary>
    /// The parameterless <c>Notify(Action)</c> overload invokes the handler when the state changes.
    /// </summary>
    [Fact]
    public void Notify_Action_StateChanges_InvokesHandler()
    {
        var state = new Node();
        var count = 0;

        using var _ = state.Notify(() => count++);
        state.Fire();

        count.Is(1);
    }

    /// <summary>
    /// The parameterless <c>Notify(Action)</c> overload stops invoking the handler once the subscription is
    /// disposed.
    /// </summary>
    [Fact]
    public void Notify_Action_AfterDispose_StopsInvokingHandler()
    {
        var state = new Node();
        var count = 0;
        var subscription = state.Notify(() => count++);

        subscription.Dispose();
        state.Fire();

        count.Is(0);
    }

    /// <summary>
    /// The <c>Notify(Action&lt;T&gt;)</c> overload invokes the handler with the state instance itself.
    /// </summary>
    [Fact]
    public void Notify_ActionOfState_StateChanges_InvokesHandlerWithState()
    {
        var state = new Node();
        Node? received = null;

        using var _ = state.Notify(s => received = s);
        state.Fire();

        received!.Is(state);
    }

    /// <summary>
    /// The <c>Notify(Action&lt;T&gt;)</c> overload stops invoking the handler once the subscription is disposed.
    /// </summary>
    [Fact]
    public void Notify_ActionOfState_AfterDispose_StopsInvokingHandler()
    {
        var state = new Node();
        var count = 0;
        var subscription = state.Notify(_ => count++);

        subscription.Dispose();
        state.Fire();

        count.Is(0);
    }

    /// <summary>
    /// The <c>IEnumerable&lt;T&gt;.Notify(Action)</c> overload subscribes each state independently: firing one
    /// state invokes the handler once, firing another adds another invocation, and each subscription is a
    /// separate disposable.
    /// </summary>
    [Fact]
    public void Notify_EnumerableAction_EachStateChanges_InvokesHandlerPerState()
    {
        var first = new Node();
        var second = new Node();
        var count = 0;

        List<IDisposable> subscriptions = [.. new[] { first, second }.Notify(() => count++)];

        first.Fire();
        var afterFirst = count;
        second.Fire();

        subscriptions.Has(2);
        afterFirst.Is(1);
        count.Is(2);
    }

    /// <summary>
    /// The <c>IEnumerable&lt;T&gt;.Notify(Action)</c> overload's subscriptions are independently disposable:
    /// disposing one state's subscription leaves the other state's notifications active.
    /// </summary>
    [Fact]
    public void Notify_EnumerableAction_OneSubscriptionDisposed_OtherStateStillNotifies()
    {
        var first = new Node();
        var second = new Node();
        var count = 0;

        List<IDisposable> subscriptions = [.. new[] { first, second }.Notify(() => count++)];
        subscriptions[0].Dispose();

        first.Fire();
        second.Fire();

        count.Is(1);
    }

    /// <summary>
    /// The <c>IEnumerable&lt;T&gt;.Notify(Action&lt;T&gt;)</c> overload invokes the handler with the specific
    /// state instance that changed.
    /// </summary>
    [Fact]
    public void Notify_EnumerableActionOfState_StateChanges_InvokesHandlerWithChangedState()
    {
        var first = new Node();
        var second = new Node();
        var received = new List<Node>();

        using var subscriptions = new CompositeDisposable(new[] { first, second }.Notify(s => received.Add(s)));
        second.Fire();

        received.Has(1);
        received[0].Is(second);
    }

    /// <summary>
    /// Minimal <see cref="ObservableState"/> exposing a public trigger for its protected change notification, used
    /// to drive <see cref="IObservableState.Changed"/> deterministically without a real Blazor state.
    /// </summary>
    private sealed class Node : ObservableState
    {
        /// <summary>
        /// Raises a change notification.
        /// </summary>
        public void Fire() => NotifyChanged();
    }

    /// <summary>
    /// Aggregates several disposables so a test can dispose them all via a single <c>using</c> declaration.
    /// </summary>
    private sealed class CompositeDisposable : IDisposable
    {
        /// <summary>
        /// The wrapped disposables.
        /// </summary>
        private readonly IReadOnlyCollection<IDisposable> _disposables;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeDisposable"/> class.
        /// </summary>
        /// <param name="disposables">The disposables to aggregate.</param>
        public CompositeDisposable(IEnumerable<IDisposable> disposables)
        {
            _disposables = [.. disposables];
        }

        /// <summary>
        /// Disposes every wrapped disposable.
        /// </summary>
        public void Dispose()
        {
            foreach (var disposable in _disposables)
                disposable.Dispose();
        }
    }
}
