using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Components.State.Forms.Internal;
using Annium.Data.Operations;
using Annium.Extensions.Validation;

namespace Annium.Components.State.Forms.Extensions;

/// <summary>
/// Provides extension methods for adding validation support to object containers.
/// </summary>
public static class ObjectContainerValidationExtensions
{
    /// <summary>
    /// Adds validation to an object container using the specified validator.
    /// Validation is triggered immediately when the container value changes.
    /// </summary>
    /// <typeparam name="T">The type of object being validated.</typeparam>
    /// <param name="state">The object container to add validation to.</param>
    /// <param name="validator">The validator to use for validation.</param>
    /// <returns>The same object container instance with validation enabled.</returns>
    public static IObjectContainer<T> UseValidator<T>(this IObjectContainer<T> state, IValidator<T> validator)
        where T : notnull, new()
    {
        return state.Changed.SubscribeValidator(state, validator);
    }

    /// <summary>
    /// Adds validation to an object container using the specified validator with throttling.
    /// Validation is triggered after the specified delay when the container value changes.
    /// </summary>
    /// <typeparam name="T">The type of object being validated.</typeparam>
    /// <param name="state">The object container to add validation to.</param>
    /// <param name="validator">The validator to use for validation.</param>
    /// <param name="dueTime">The delay before validation is triggered after value changes.</param>
    /// <returns>The same object container instance with throttled validation enabled.</returns>
    public static IObjectContainer<T> UseValidator<T>(
        this IObjectContainer<T> state,
        IValidator<T> validator,
        TimeSpan dueTime
    )
        where T : notnull, new()
    {
        return state.Changed.Throttle(dueTime).SubscribeValidator(state, validator);
    }

    /// <summary>
    /// Subscribes a validator to an observable stream to perform validation when events occur.
    /// </summary>
    /// <typeparam name="T">The type of object being validated.</typeparam>
    /// <param name="observable">The observable stream to subscribe to.</param>
    /// <param name="state">The object container to validate.</param>
    /// <param name="validator">The validator to use for validation.</param>
    /// <returns>The same object container instance.</returns>
    private static IObjectContainer<T> SubscribeValidator<T>(
        this IObservable<Unit> observable,
        IObjectContainer<T> state,
        IValidator<T> validator
    )
        where T : notnull, new()
    {
        var holder = new CtsHolder();
        observable.Subscribe(change =>
        {
            // atomically swap in a fresh token source and cancel the previous run (Interlocked guards the
            // reassign against overlapping notifications that could otherwise clobber each other's token).
            var next = new CancellationTokenSource();
            var previous = Interlocked.Exchange(ref holder.Cts, next);
            previous.Cancel();
            previous.Dispose();

            // fire-and-forget: do NOT block the (single-threaded, UI-bound) Changed callback on an awaitable —
            // that would deadlock a genuinely-async validator on Blazor WASM. ValidateAsync catches the
            // validator itself, so the discarded task cannot fault unobserved.
            _ = state.ValidateAsync(validator, next.Token);
        });

        return state;
    }

    /// <summary>
    /// Runs the validator against the container's value and updates child statuses. Sets children to
    /// <see cref="Status.Validating"/> up front; on completion (unless cancelled) applies labeled errors to their
    /// matching child and plain (unlabeled) errors — e.g. a thrown validator — to every child. A synchronous
    /// validator completes inline; a genuinely-async one resolves later without blocking the caller.
    /// </summary>
    /// <typeparam name="T">The type of object being validated.</typeparam>
    /// <param name="state">The object container to validate.</param>
    /// <param name="validator">The validator to use for validation.</param>
    /// <param name="ct">Cancellation token; if cancelled after the validator returns, the result is discarded.</param>
    /// <returns>A task that completes when validation has been applied (or discarded on cancellation).</returns>
    private static async Task ValidateAsync<T>(
        this IObjectContainer<T> state,
        IValidator<T> validator,
        CancellationToken ct
    )
        where T : notnull, new()
    {
        using (state.Mute())
            SetValidating(state.Children);

        IResult result;
        try
        {
            result = await validator.ValidateAsync(state.Value);
        }
        catch (Exception exception)
        {
            result = Result.Create().Error(exception.Message);
        }

        if (ct.IsCancellationRequested)
            return;

        // plain (unlabeled) errors — e.g. a thrown validator — are not tied to a specific child, so apply them
        // to every atomic descendant rather than silently dropping them.
        var plainMessage = result.PlainErrors.Count > 0 ? string.Join("; ", result.PlainErrors) : null;

        using (state.Mute())
            ApplyStatuses(state.Children, result.LabeledErrors, plainMessage, string.Empty);
    }

    /// <summary>
    /// Recursively marks every atomic (<see cref="IStatusContainer"/>) descendant of the given children as
    /// <see cref="Status.Validating"/>.
    /// </summary>
    /// <param name="children">The named child states to descend.</param>
    private static void SetValidating(IEnumerable<KeyValuePair<string, ITrackedState>> children)
    {
        foreach (var (_, child) in children)
        {
            if (child is IStatusContainer atomic)
                atomic.SetStatus(Status.Validating);
            else
                // mute the intermediate composite so setting its atomic descendants' statuses does not storm
                // its aggregate Changed once per descendant (nested mutes compose via the depth counter).
                using (child.Mute())
                    SetValidating(NamedChildrenOf(child));
        }
    }

    /// <summary>
    /// Recursively applies validation results to atomic descendants, routing a dotted-path labeled error
    /// (e.g. <c>Address.City</c>) into the matching nested child. A plain (unlabeled) error is applied to every
    /// atomic descendant; a child with neither is cleared to <see cref="Status.None"/>.
    /// </summary>
    /// <param name="children">The named child states at the current level.</param>
    /// <param name="labeledErrors">The validation labeled errors keyed by dotted path.</param>
    /// <param name="plainMessage">The joined plain-error message, or null when there are none.</param>
    /// <param name="prefix">The dotted-path prefix accumulated from ancestor segments.</param>
    private static void ApplyStatuses(
        IEnumerable<KeyValuePair<string, ITrackedState>> children,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> labeledErrors,
        string? plainMessage,
        string prefix
    )
    {
        foreach (var (name, child) in children)
        {
            var path = prefix.Length == 0 ? name : $"{prefix}.{name}";
            if (child is IStatusContainer atomic)
            {
                if (labeledErrors.TryGetValue(path, out var errors))
                    atomic.SetStatus(Status.Error, string.Join("; ", errors));
                else if (plainMessage is not null)
                    atomic.SetStatus(Status.Error, plainMessage);
                else
                    atomic.SetStatus(Status.None);
            }
            else
            {
                // labeled errors keyed exactly to a composite child (e.g. "Address" rather than "Address.City")
                // have no atomic status surface to land on and are intentionally not applied here — validation
                // targets leaf fields; the composite is descended with the accumulated path prefix. Mute the
                // intermediate so its aggregate Changed is not stormed per descendant.
                using (child.Mute())
                    ApplyStatuses(NamedChildrenOf(child), labeledErrors, plainMessage, path);
            }
        }
    }

    /// <summary>
    /// Gets the named child states of a composite container, or an empty sequence for a leaf/atomic state.
    /// </summary>
    /// <param name="state">The tracked state to inspect.</param>
    /// <returns>The named children, or an empty sequence if the state has none.</returns>
    private static IEnumerable<KeyValuePair<string, ITrackedState>> NamedChildrenOf(ITrackedState state) =>
        state is INamedChildStates named ? named.NamedChildren : [];

    /// <summary>
    /// Mutable holder for the current validation cancellation source, enabling an atomic swap via
    /// <see cref="Interlocked.Exchange{T}(ref T, T)"/> (a captured local cannot be passed by ref).
    /// </summary>
    private sealed class CtsHolder
    {
        /// <summary>
        /// The cancellation source for the in-flight validation run; swapped atomically on each change.
        /// </summary>
        public CancellationTokenSource Cts = new();
    }
}
