using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using Annium.Linq;

namespace Annium.Data.Operations.Internal;

/// <summary>
/// Abstract base class for all result types, providing common error handling functionality.
/// This class implements thread-safe error collection and manipulation operations.
/// </summary>
/// <typeparam name="T">The concrete result type that inherits from this base class.</typeparam>
/// <remarks>
/// The error collections and the lock guarding them live in a bag created on the first error, so a
/// result that never carries one allocates nothing beyond itself. Every result in a successful path is
/// such a result, and every application built on Annium produces them by the million, which is what
/// makes the deferral worth its null checks.
/// </remarks>
internal abstract record ResultBase<T> : IResultBase<T>, IResultBase, ICopyable<T>
    where T : class, IResultBase<T>
{
    /// <summary>
    /// What <see cref="ErrorState"/> reports for a result that never carried an error — the same text the
    /// builder produced, assembled once.
    /// </summary>
    private static readonly string _noErrorState = new StringBuilder()
        .AppendLine("no plain errors")
        .AppendLine("no labeled errors")
        .ToString();

    /// <summary>
    /// The errors and their lock, or null while there are none.
    /// </summary>
    private ErrorBag? _bag;

    /// <summary>
    /// Gets a snapshot of all plain error messages that are not associated with any specific label.
    /// </summary>
    public IReadOnlyCollection<string> PlainErrors
    {
        get
        {
            var bag = Volatile.Read(ref _bag);
            if (bag is null)
                return [];

            lock (bag.Gate)
                return bag.Plain.ToArray();
        }
    }

    /// <summary>
    /// Gets a concatenated string of all plain errors separated by "; ".
    /// </summary>
    public string PlainError
    {
        get
        {
            var bag = Volatile.Read(ref _bag);
            if (bag is null)
                return string.Empty;

            lock (bag.Gate)
                return bag.Plain.Join("; ");
        }
    }

    /// <summary>
    /// Gets a snapshot dictionary of labeled errors, where each label maps to a snapshot collection of error messages.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> LabeledErrors
    {
        get
        {
            var bag = Volatile.Read(ref _bag);
            if (bag is null)
                return ReadOnlyDictionary<string, IReadOnlyCollection<string>>.Empty;

            lock (bag.Gate)
                return bag.Labeled.ToImmutableDictionary(
                    pair => pair.Key,
                    pair => (IReadOnlyCollection<string>)pair.Value.ToArray()
                );
        }
    }

    /// <summary>
    /// Gets a value indicating whether this result has no errors (both plain and labeled).
    /// </summary>
    public bool IsOk => !HasErrors;

    /// <summary>
    /// Gets a value indicating whether this result has any errors (either plain or labeled).
    /// </summary>
    public bool HasErrors
    {
        get
        {
            var bag = Volatile.Read(ref _bag);
            if (bag is null)
                return false;

            lock (bag.Gate)
                return bag.Plain.Count > 0 || bag.Labeled.Count > 0;
        }
    }

    /// <summary>
    /// Creates a deep copy of this result instance.
    /// </summary>
    /// <returns>A new instance of type T that is a copy of this result.</returns>
    public abstract T Copy();

    /// <summary>
    /// Clears all errors (both plain and labeled) from this result.
    /// </summary>
    /// <returns>This result instance for method chaining.</returns>
    public T Clear()
    {
        // nothing was ever added, so there is nothing to clear and no reason to create the bag
        var bag = Volatile.Read(ref _bag);
        if (bag is null)
            return Self;

        lock (bag.Gate)
        {
            bag.Plain.Clear();
            bag.Labeled.Clear();
        }

        return Self;
    }

    /// <summary>
    /// Adds a plain error message to this result.
    /// </summary>
    /// <param name="error">The error message to add.</param>
    /// <returns>This result instance for method chaining.</returns>
    public T Error(string error)
    {
        var bag = GetBag();

        lock (bag.Gate)
            bag.Plain.Add(error);

        return Self;
    }

    /// <summary>
    /// Adds a labeled error message to this result.
    /// </summary>
    /// <param name="label">The label to associate with the error.</param>
    /// <param name="error">The error message to add.</param>
    /// <returns>This result instance for method chaining.</returns>
    public T Error(string label, string error)
    {
        var bag = GetBag();

        lock (bag.Gate)
            bag.LabeledFor(label).Add(error);

        return Self;
    }

    /// <summary>
    /// Adds multiple plain error messages to this result.
    /// </summary>
    /// <param name="errors">The error messages to add.</param>
    /// <returns>This result instance for method chaining.</returns>
    public T Errors(params string[] errors) => Errors((IReadOnlyCollection<string>)errors);

    /// <summary>
    /// Adds multiple plain error messages from a collection to this result.
    /// </summary>
    /// <param name="errors">The collection of error messages to add.</param>
    /// <returns>This result instance for method chaining.</returns>
    public T Errors(IReadOnlyCollection<string> errors)
    {
        // an empty batch must not create the bag: Copy() and Join() pass the errors of a successful
        // result through here, and those are the calls the deferral exists for
        if (errors.Count == 0)
            return Self;

        var bag = GetBag();

        lock (bag.Gate)
            foreach (var error in errors)
                bag.Plain.Add(error);

        return Self;
    }

    /// <summary>
    /// Adds multiple labeled error groups to this result.
    /// </summary>
    /// <param name="errors">An array of tuples where each tuple contains a label and its associated error messages.</param>
    /// <returns>This result instance for method chaining.</returns>
    public T Errors(params ValueTuple<string, IReadOnlyCollection<string>>[] errors)
    {
        if (errors.Length == 0)
            return Self;

        var bag = GetBag();

        lock (bag.Gate)
            foreach (var (label, labelErrors) in errors)
            {
                var target = bag.LabeledFor(label);
                foreach (var error in labelErrors)
                    target.Add(error);
            }

        return Self;
    }

    /// <summary>
    /// Adds multiple labeled error groups from a collection of key-value pairs to this result.
    /// </summary>
    /// <param name="errors">A collection of key-value pairs where each key is a label and each value is a collection of error messages.</param>
    /// <returns>This result instance for method chaining.</returns>
    public T Errors(IReadOnlyCollection<KeyValuePair<string, IReadOnlyCollection<string>>> errors)
    {
        if (errors.Count == 0)
            return Self;

        var bag = GetBag();

        lock (bag.Gate)
            foreach (var (label, labelErrors) in errors)
            {
                var target = bag.LabeledFor(label);
                foreach (var error in labelErrors)
                    target.Add(error);
            }

        return Self;
    }

    /// <summary>
    /// Joins the errors from multiple result instances into this result.
    /// </summary>
    /// <param name="results">The result instances whose errors should be added to this result.</param>
    /// <returns>This result instance for method chaining.</returns>
    public T Join(params IResultBase[] results)
    {
        foreach (var result in results)
        {
            Errors(result.PlainErrors);
            Errors(result.LabeledErrors);
        }

        return Self;
    }

    /// <summary>
    /// Joins the errors from a collection of result instances into this result.
    /// </summary>
    /// <param name="results">The collection of result instances whose errors should be added to this result.</param>
    /// <returns>This result instance for method chaining.</returns>
    public T Join(IReadOnlyCollection<IResultBase> results)
    {
        foreach (var result in results)
        {
            Errors(result.PlainErrors);
            Errors(result.LabeledErrors);
        }

        return Self;
    }

    /// <summary>
    /// Gets a detailed string representation of all errors in this result for debugging purposes.
    /// </summary>
    /// <returns>A formatted string containing all plain and labeled errors.</returns>
    public string ErrorState()
    {
        var bag = Volatile.Read(ref _bag);
        if (bag is null)
            return _noErrorState;

        lock (bag.Gate)
        {
            var sb = new StringBuilder();

            if (bag.Plain.Count > 0)
            {
                sb.AppendLine($"{bag.Plain.Count} plain errors:");
                foreach (var error in bag.Plain)
                    sb.AppendLine($"- {error}");
            }
            else
                sb.AppendLine("no plain errors");

            if (bag.Labeled.Count > 0)
            {
                sb.AppendLine($"{bag.Labeled.Count} labeled errors:");
                foreach (var (label, errors) in bag.Labeled)
                {
                    sb.AppendLine($"- {label}:");
                    foreach (var error in errors)
                        sb.AppendLine($"-- {error}");
                }
            }
            else
                sb.AppendLine("no labeled errors");

            return sb.ToString();
        }
    }

    /// <summary>
    /// Copies all errors from this result to the specified clone result instance.
    /// </summary>
    /// <param name="clone">The target result instance to copy errors to.</param>
    protected void CloneTo(T clone)
    {
        clone.Errors(PlainErrors);
        clone.Errors(LabeledErrors);
    }

    /// <summary>
    /// This instance as the concrete result type, for the fluent returns.
    /// </summary>
    private T Self => (this as T)!;

    /// <summary>
    /// Returns the error bag, creating it on the first error and reusing it afterwards.
    /// </summary>
    /// <returns>The bag.</returns>
    /// <remarks>
    /// Two writers racing here both build one and the first to publish wins, so every writer ends up
    /// locking the same gate; the interlocked exchange is what makes a bag's collections visible to a
    /// thread that sees the reference.
    /// </remarks>
    private ErrorBag GetBag()
    {
        var bag = Volatile.Read(ref _bag);
        if (bag is not null)
            return bag;

        return Interlocked.CompareExchange(ref _bag, bag = new ErrorBag(), null) ?? bag;
    }

    /// <summary>
    /// The errors of one result, together with the lock guarding them. Allocated on the first error, so
    /// a successful result never pays for it.
    /// </summary>
    private sealed class ErrorBag
    {
        /// <summary>
        /// Guards both collections.
        /// </summary>
        public readonly Lock Gate = new();

        /// <summary>
        /// Error messages with no label.
        /// </summary>
        public readonly HashSet<string> Plain = new();

        /// <summary>
        /// Error messages by label.
        /// </summary>
        public readonly Dictionary<string, HashSet<string>> Labeled = new();

        /// <summary>
        /// Returns the set of errors under a label, adding an empty one when the label is new. Must be
        /// called while holding <see cref="Gate"/>.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <returns>The label's set of errors.</returns>
        public HashSet<string> LabeledFor(string label)
        {
            if (!Labeled.TryGetValue(label, out var errors))
                Labeled[label] = errors = new HashSet<string>();

            return errors;
        }
    }
}
