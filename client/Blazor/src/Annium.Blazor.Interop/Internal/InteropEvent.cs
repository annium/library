using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Annium.Blazor.Interop.Internal.Extensions;
using Annium.Core.Primitives;
using Microsoft.JSInterop;

namespace Annium.Blazor.Interop.Internal;

internal sealed record InteropEvent<TContext, TEvent> : IDisposable
    where TContext : IObject
    where TEvent : notnull
{
    private const string HandleMethod = $"{nameof(InteropEvent<TContext, TEvent>)}.{nameof(Handle)}";
    private static readonly string BindContext = typeof(TContext).FriendlyName().CamelCase();
    private static IInteropContext Ctx => InteropContext.Instance;
    private static readonly IReadOnlyList<Type> ConstructorTypes = typeof(TEvent).GetConstructors().Single().GetParameters().Select(x => x.ParameterType).ToArray();
    private readonly TContext _target;
    private readonly object _netRef;
    private readonly IDictionary<int, Action<TEvent>> _handlers = new Dictionary<int, Action<TEvent>>();
    private readonly IList<Action> _disposers = new List<Action>();
    private readonly string _binderName;
    private readonly string _unbinderName;

    public InteropEvent(
        TContext target
    )
    {
        _binderName = $"{BindContext}.on{typeof(TEvent).Name}";
        _unbinderName = $"{BindContext}.offEvent";
        _target = target;
        _netRef = DotNetObjectReference.Create(this);
    }

    public Action Register<TKey>(TKey eventKey, Action<TEvent> handle, params object[] args)
        where TKey : notnull
    {
        var callbackId = Ctx.Invoke<int>(_binderName, new[] { _target.Id, eventKey.ToString(), _netRef, HandleMethod }.Append(args).ToArray());
        if (!_handlers.TryAdd(callbackId, handle))
            throw OperationException($"failed to add handler {callbackId}");

        void Disposer()
        {
            Trace($"remove {callbackId}");
            Ctx.InvokeVoid(_unbinderName, _target.Id, eventKey.ToString(), callbackId);
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

    [JSInvokable(HandleMethod)]
    public void Handle(int callbackId, JsonElement[] args)
    {
        if (!_handlers.TryGetValue(callbackId, out var handle))
            throw OperationException($"failed to find handler {callbackId}");

        var values = args.Select((x, i) => x.Deserialize(ConstructorTypes[i])).ToArray();
        var data = (TEvent) Activator.CreateInstance(typeof(TEvent), values)!;
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
        $"Interop event for {_target.Id}: {typeof(TEvent).FriendlyName()} {message}";
}