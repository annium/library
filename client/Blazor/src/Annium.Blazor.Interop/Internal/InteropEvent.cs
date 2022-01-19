using System;

namespace Annium.Blazor.Interop.Internal;

internal sealed record InteropEvent<T1, T2, T3, T4> : InteropEventBase
{
    public event Action<T1, T2, T3, T4> Event = delegate { };
    public bool HasListeners => Event.GetInvocationList().Length > 1;
    public void Handle(T1 x1, T2 x2, T3 x3, T4 x4) => Event(x1, x2, x3, x4);
}

internal sealed record InteropEvent<T1, T2, T3> : InteropEventBase
{
    public event Action<T1, T2, T3> Event = delegate { };
    public bool HasListeners => Event.GetInvocationList().Length > 1;
    public void Handle(T1 x1, T2 x2, T3 x3) => Event(x1, x2, x3);
}

internal sealed record InteropEvent<T1, T2> : InteropEventBase
{
    public event Action<T1, T2> Event = delegate { };
    public bool HasListeners => Event.GetInvocationList().Length > 1;
    public void Handle(T1 x1, T2 x2) => Event(x1, x2);
}

internal sealed record InteropEvent<T> : InteropEventBase
{
    public event Action<T> Event = delegate { };
    public bool HasListeners => Event.GetInvocationList().Length > 1;
    public void Handle(T x) => Event(x);
}

internal abstract record InteropEventBase
{
    public int? CallbackId { get; private set; }

    public void SetCallbackId(int cid)
    {
        if (CallbackId.HasValue)
            throw new InvalidOperationException("Callback id is already set");

        CallbackId = cid;
    }

    public void ResetCallbackId()
    {
        if (!CallbackId.HasValue)
            throw new InvalidOperationException("Callback id is already reset");

        CallbackId = null;
    }
}