using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Blazor.Interop;
using Microsoft.JSInterop;

namespace Annium.Blazor.Interop.Tests.Fakes;

/// <summary>
/// A fake <see cref="IInteropContext"/> whose in-process runtime records every JS call and returns
/// deterministic callback ids, so interop plumbing (binder/unbinder names, arguments, callback lifecycle) can be
/// asserted without a browser or a loaded JS module.
/// </summary>
internal sealed class FakeInteropContext : IInteropContext
{
    /// <summary>
    /// Gets the fake in-process runtime backing this context.
    /// </summary>
    public FakeJsInProcessRuntime Runtime { get; } = new();

    /// <summary>
    /// Gets the in-process JavaScript runtime for synchronous JavaScript interop calls.
    /// </summary>
    public IJSInProcessRuntime InProcessRuntime => Runtime;

    /// <summary>
    /// Clears every recorded call and resets the callback-id counter.
    /// </summary>
    public void Reset() => Runtime.Reset();
}

/// <summary>
/// Records a single synchronous JS call routed through the interop layer.
/// </summary>
/// <param name="Identifier">The fully qualified JS function identifier.</param>
/// <param name="Args">The arguments passed to the call.</param>
internal readonly record struct JsCall(string Identifier, object?[] Args);

/// <summary>
/// A fake <see cref="IJSInProcessRuntime"/> that records synchronous calls and hands out incrementing callback ids
/// for <see cref="int"/>-returning invocations; async members are unsupported (interop plumbing is synchronous).
/// </summary>
internal sealed class FakeJsInProcessRuntime : IJSInProcessRuntime
{
    /// <summary>
    /// The synchronous calls recorded so far, in invocation order.
    /// </summary>
    public List<JsCall> Calls { get; } = new();

    /// <summary>
    /// Monotonic source of callback ids returned for <see cref="int"/>-typed invocations.
    /// </summary>
    private int _nextCallbackId;

    /// <summary>
    /// Clears recorded calls and resets the callback-id counter.
    /// </summary>
    public void Reset()
    {
        Calls.Clear();
        _nextCallbackId = 0;
    }

    /// <summary>
    /// Records the call and returns a deterministic value: an incrementing id for <see cref="int"/>, default otherwise.
    /// </summary>
    /// <typeparam name="TResult">The expected return type.</typeparam>
    /// <param name="identifier">The JS function identifier.</param>
    /// <param name="args">The call arguments.</param>
    /// <returns>An incrementing id when <typeparamref name="TResult"/> is <see cref="int"/>; otherwise default.</returns>
    public TResult Invoke<TResult>(string identifier, params object?[]? args)
    {
        Calls.Add(new JsCall(identifier, args ?? []));
        if (typeof(TResult) == typeof(int))
            return (TResult)(object)_nextCallbackId++;

        return default!;
    }

    /// <summary>
    /// Not supported — the interop layer under test uses only synchronous in-process calls.
    /// </summary>
    /// <typeparam name="TValue">The expected return type.</typeparam>
    /// <param name="identifier">The JS function identifier.</param>
    /// <param name="args">The call arguments.</param>
    /// <returns>Never returns; always throws.</returns>
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
        throw new NotSupportedException("async interop is not exercised by these tests");

    /// <summary>
    /// Not supported — the interop layer under test uses only synchronous in-process calls.
    /// </summary>
    /// <typeparam name="TValue">The expected return type.</typeparam>
    /// <param name="identifier">The JS function identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <param name="args">The call arguments.</param>
    /// <returns>Never returns; always throws.</returns>
    public ValueTask<TValue> InvokeAsync<TValue>(
        string identifier,
        CancellationToken cancellationToken,
        object?[]? args
    ) => throw new NotSupportedException("async interop is not exercised by these tests");
}
