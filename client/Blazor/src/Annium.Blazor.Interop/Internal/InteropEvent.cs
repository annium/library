using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Annium.Blazor.Interop.Internal.Extensions;
using Annium.Linq;
using Microsoft.JSInterop;

namespace Annium.Blazor.Interop.Internal;

internal sealed record InteropEvent<T> : IInteropEvent<T>
    where T : notnull
{
    private const string HandleMethod = $"{nameof(InteropEvent<T>)}.{nameof(Handle)}";
    private static IInteropContext Ctx => InteropContext.Instance;
    private static readonly IReadOnlyList<Type> ConstructorTypes = typeof(T).GetConstructors().Single().GetParameters().Select(x => x.ParameterType).ToArray();

    public static IInteropEvent<T> Static(string context, string target) => new InteropEvent<T>(
        context,
        new Lazy<string>(target),
        new Lazy<IEnumerable<object>>(Array.Empty<string>())
    );

    public static IInteropEvent<T> Element(IObject target) => new InteropEvent<T>(
        "element",
        new Lazy<string>(() => target.Id),
        new Lazy<IEnumerable<object>>(() => target.Id.Yield())
    );

    private readonly Lazy<string> _target;
    private readonly Lazy<IEnumerable<object>> _sharedBindArgs;
    private readonly object _netRef;
    private readonly IDictionary<int, Action<T>> _handlers = new Dictionary<int, Action<T>>();
    private readonly IList<Action> _disposers = new List<Action>();
    private readonly string _binderName;
    private readonly string _unbinderName;

    private InteropEvent(
        string context,
        Lazy<string> target,
        Lazy<IEnumerable<object>> sharedBindArgs
    )
    {
        _binderName = $"{context}.on{typeof(T).Name}";
        _unbinderName = $"{context}.off{typeof(T).Name}";
        _target = target;
        _sharedBindArgs = sharedBindArgs;
        _netRef = DotNetObjectReference.Create(this);
    }

    public Action Register<TKey>(TKey eventKey, Action<T> handle, params object[] args)
        where TKey : notnull
    {
        var callbackId = Ctx.Apply<int>(_binderName, _sharedBindArgs.Value.Concat(new[] { eventKey.ToString(), _netRef, HandleMethod }).Concat(args).ToArray());
        if (!_handlers.TryAdd(callbackId, handle))
            throw OperationException($"failed to add handler {callbackId}");

        void Disposer()
        {
            Ctx.Apply(_unbinderName, _sharedBindArgs.Value.Concat(new object[] { eventKey.ToString()!, callbackId }).ToArray());
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

    [JSInvokable(HandleMethod)]
    public void Handle(int callbackId, JsonElement[] args)
    {
        if (!_handlers.TryGetValue(callbackId, out var handle))
            throw OperationException($"failed to find handler {callbackId}");

        var values = args.Select((x, i) => x.Deserialize(ConstructorTypes[i])).ToArray();
        var data = (T) Activator.CreateInstance(typeof(T), values)!;
        handle(data);
    }

    public void Dispose()
    {
        foreach (var dispose in _disposers)
            dispose();
        _disposers.Clear();
    }

    private InvalidOperationException OperationException(string message) =>
        new(Message(message));

    private string Message(string message) =>
        $"Interop event for {_target}: {typeof(T).FriendlyName()} {message}";
}

internal interface IInteropEvent<T> : IDisposable
    where T : notnull
{
    Action Register<TKey>(TKey eventKey, Action<T> handle, params object[] args)
        where TKey : notnull;
}