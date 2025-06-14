using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Annium.Blazor.Interop.Internal.Extensions;
using Annium.Linq;
using Microsoft.JSInterop;

namespace Annium.Blazor.Interop.Internal;

/// <summary>
/// Represents an interop event that can bind to JavaScript events and handle callbacks.
/// </summary>
/// <typeparam name="T">The type of event data that will be received from JavaScript.</typeparam>
internal sealed record InteropEvent<T> : IInteropEvent<T>
    where T : notnull
{
    /// <summary>
    /// The method name used for JavaScript callback invocation.
    /// </summary>
    private const string HandleMethod = $"{nameof(InteropEvent<T>)}.{nameof(Handle)}";

    /// <summary>
    /// Gets the current interop context instance.
    /// </summary>
    private static IInteropContext Ctx => InteropContext.Instance;

    /// <summary>
    /// The parameter types for the constructor of the event data type T.
    /// </summary>
    private static readonly IReadOnlyList<Type> _constructorTypes = typeof(T)
        .GetConstructors()
        .Single()
        .GetParameters()
        .Select(x => x.ParameterType)
        .ToArray();

    /// <summary>
    /// Creates a static interop event for a specified context and target.
    /// </summary>
    /// <param name="context">The JavaScript context for the event.</param>
    /// <param name="target">The target identifier for the event.</param>
    /// <returns>A new interop event instance.</returns>
    public static IInteropEvent<T> Static(string context, string target) =>
        new InteropEvent<T>(context, new Lazy<string>(target), new Lazy<IEnumerable<object>>([]));

    /// <summary>
    /// Creates an element-based interop event for a specified object target.
    /// </summary>
    /// <param name="target">The object target that provides the element ID.</param>
    /// <returns>A new interop event instance for the element.</returns>
    public static IInteropEvent<T> Element(IObject target) =>
        new InteropEvent<T>(
            "element",
            new Lazy<string>(() => target.Id),
            new Lazy<IEnumerable<object>>(() => target.Id.Yield())
        );

    /// <summary>
    /// Lazy-loaded target identifier for the event.
    /// </summary>
    private readonly Lazy<string> _target;

    /// <summary>
    /// Lazy-loaded shared arguments used for binding events.
    /// </summary>
    private readonly Lazy<IEnumerable<object>> _sharedBindArgs;

    /// <summary>
    /// .NET object reference for JavaScript callbacks.
    /// </summary>
    private readonly object _netRef;

    /// <summary>
    /// Dictionary mapping callback IDs to their corresponding event handlers.
    /// </summary>
    private readonly IDictionary<int, Action<T>> _handlers = new Dictionary<int, Action<T>>();

    /// <summary>
    /// List of disposal actions to clean up event handlers.
    /// </summary>
    private readonly IList<Action> _disposers = new List<Action>();

    /// <summary>
    /// The JavaScript function name used to bind events.
    /// </summary>
    private readonly string _binderName;

    /// <summary>
    /// The JavaScript function name used to unbind events.
    /// </summary>
    private readonly string _unbinderName;

    /// <summary>
    /// Initializes a new instance of the InteropEvent class.
    /// </summary>
    /// <param name="context">The JavaScript context for the event.</param>
    /// <param name="target">Lazy-loaded target identifier.</param>
    /// <param name="sharedBindArgs">Lazy-loaded shared binding arguments.</param>
    private InteropEvent(string context, Lazy<string> target, Lazy<IEnumerable<object>> sharedBindArgs)
    {
        _binderName = $"{context}.on{typeof(T).Name}";
        _unbinderName = $"{context}.off{typeof(T).Name}";
        _target = target;
        _sharedBindArgs = sharedBindArgs;
        _netRef = DotNetObjectReference.Create(this);
    }

    /// <summary>
    /// Registers an event handler for a specific event key.
    /// </summary>
    /// <typeparam name="TKey">The type of the event key.</typeparam>
    /// <param name="eventKey">The key identifying the specific event to handle.</param>
    /// <param name="handle">The action to execute when the event occurs.</param>
    /// <param name="args">Additional arguments to pass to the JavaScript binder.</param>
    /// <returns>An action that can be called to unregister the event handler.</returns>
    public Action Register<TKey>(TKey eventKey, Action<T> handle, params object[] args)
        where TKey : notnull
    {
        var callbackId = Ctx.Apply<int>(
            _binderName,
            _sharedBindArgs.Value.Concat([eventKey.ToString(), _netRef, HandleMethod]).Concat(args).ToArray()
        );
        if (!_handlers.TryAdd(callbackId, handle))
            throw OperationException($"failed to add handler {callbackId}");

        void Disposer()
        {
            Ctx.Apply(_unbinderName, _sharedBindArgs.Value.Concat([eventKey.ToString()!, callbackId]).ToArray());
            if (!_handlers.Remove(callbackId))
                throw OperationException($"failed to remove handler {callbackId}");
        }

        _disposers.Add(Disposer);

        return () =>
        {
            if (_disposers.Remove(Disposer))
                Disposer();
        };
    }

    /// <summary>
    /// Handles JavaScript callback invocations for registered event handlers.
    /// </summary>
    /// <param name="callbackId">The ID of the callback to invoke.</param>
    /// <param name="args">The JSON arguments from JavaScript to deserialize and pass to the handler.</param>
    [JSInvokable(HandleMethod)]
    public void Handle(int callbackId, JsonElement[] args)
    {
        if (!_handlers.TryGetValue(callbackId, out var handle))
            throw OperationException($"failed to find handler {callbackId}");

        var values = args.Select((x, i) => x.Deserialize(_constructorTypes[i])).ToArray();
        var data = (T)Activator.CreateInstance(typeof(T), values)!;
        handle(data);
    }

    /// <summary>
    /// Disposes all registered event handlers and cleans up resources.
    /// </summary>
    public void Dispose()
    {
        foreach (var dispose in _disposers)
            dispose();
        _disposers.Clear();
    }

    /// <summary>
    /// Creates an InvalidOperationException with a formatted message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A new InvalidOperationException with the formatted message.</returns>
    private InvalidOperationException OperationException(string message) => new(Message(message));

    /// <summary>
    /// Formats an error message with context information.
    /// </summary>
    /// <param name="message">The base error message.</param>
    /// <returns>A formatted error message including target and type information.</returns>
    private string Message(string message) => $"Interop event for {_target}: {typeof(T).FriendlyName()} {message}";
}

/// <summary>
/// Defines the contract for interop events that can register event handlers and be disposed.
/// </summary>
/// <typeparam name="T">The type of event data.</typeparam>
internal interface IInteropEvent<T> : IDisposable
    where T : notnull
{
    /// <summary>
    /// Registers an event handler for a specific event key.
    /// </summary>
    /// <typeparam name="TKey">The type of the event key.</typeparam>
    /// <param name="eventKey">The key identifying the specific event to handle.</param>
    /// <param name="handle">The action to execute when the event occurs.</param>
    /// <param name="args">Additional arguments to pass to the JavaScript binder.</param>
    /// <returns>An action that can be called to unregister the event handler.</returns>
    Action Register<TKey>(TKey eventKey, Action<T> handle, params object[] args)
        where TKey : notnull;
}
