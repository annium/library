using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Blazor.Interop.Internal.Extensions;
using Annium.Core.Primitives;

namespace Annium.Blazor.Interop.Internal;

internal sealed record InteropEvent<T> : IDisposable
    where T : notnull
{
    private readonly IInteropContext _ctx = InteropContext.Instance;
    private readonly IObject _target;
    private readonly object _netRef;
    private readonly IDictionary<int, Action<T>> _handlers = new Dictionary<int, Action<T>>();
    private readonly IList<Action> _disposers = new List<Action>();
    private readonly string _binderName;
    private readonly string _unbinderName;
    private readonly string _callbackName;

    public InteropEvent(
        string bindContext,
        IObject target,
        object netRef
    )
    {
        var callContext = bindContext.CamelCase();
        _binderName = $"{callContext}.on{typeof(T).Name}";
        _unbinderName = $"{callContext}.offEvent";
        _callbackName = $"{bindContext}.Handle{typeof(T).Name}";
        _target = target;
        _netRef = netRef;
    }

    public Action Register<TKey>(TKey eventKey, Action<T> handle, params object[] args)
        where TKey : notnull
    {
        var callbackId = _ctx.Invoke<int>(_binderName, new[] { _target.Id, eventKey.ToString(), _netRef, _callbackName }.Append(args).ToArray());
        if (!_handlers.TryAdd(callbackId, handle))
            throw OperationException($"failed to add handler {callbackId}");

        void Disposer()
        {
            Trace($"remove {callbackId}");
            _ctx.InvokeVoid(_unbinderName, _target.Id, eventKey.ToString(), callbackId);
            if (!_handlers.Remove(callbackId))
                throw OperationException($"failed to remove handler {callbackId}");
        }

        Trace($"register {callbackId}");
        _disposers.Add(Disposer);

        return () =>
        {
            Trace($"unregister {callbackId}");
            if (_disposers.Remove(Disposer))
                Disposer();
        };
    }

    public void Handle(int callbackId, T data)
    {
        if (!_handlers.TryGetValue(callbackId, out var handle))
            throw OperationException($"failed to find handler {callbackId}");

        handle(data);
    }

    public void Dispose()
    {
        Trace("dispose start");
        foreach (var dispose in _disposers)
            dispose();
        _disposers.Clear();
        Trace("dispose done");
    }

    private void Trace(string message) =>
        Console.WriteLine(Message(message));

    private InvalidOperationException OperationException(string message) =>
        new(Message(message));

    private string Message(string message) =>
        $"Interop event for {_target.Id}: {typeof(T).FriendlyName()} {message}";
}