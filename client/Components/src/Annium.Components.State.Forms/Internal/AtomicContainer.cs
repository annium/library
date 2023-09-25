using System.Collections.Generic;
using Annium.Components.State.Core;
using Annium.Logging;

namespace Annium.Components.State.Forms.Internal;

internal class AtomicContainer<T> : ObservableState, IAtomicContainer<T>, ILogSubject
{
    public T Value { get; private set; }
    public bool HasChanged => !EqualityComparer<T>.Default.Equals(Value, _initialValue);
    public bool HasBeenTouched { get; private set; }
    public Status Status { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public ILogger Logger { get; }
    private T _initialValue;

    public AtomicContainer(
        T initialValue,
        ILogger logger
    )
    {
        Logger = logger;
        Value = _initialValue = initialValue;
    }

    public void Init(T value)
    {
        Value = _initialValue = value;
        HasBeenTouched = false;
        Status = Status.None;
        NotifyChanged();
    }

    public bool Set(T value)
    {
        if (EqualityComparer<T>.Default.Equals(value, Value))
            return false;

        Value = value;
        HasBeenTouched = true;
        NotifyChanged();

        return true;
    }

    public void Reset() => Init(_initialValue);

    public void SetStatus(Status status) => SetStatus(status, string.Empty);

    public void SetStatus(Status status, string message)
    {
        if (status == Status && message == Message)
            return;

        Status = status;
        Message = message;
        NotifyChanged();
    }

    public bool IsStatus(params Status[] statuses)
    {
        foreach (var status in statuses)
            if (Status == status)
                return true;

        return false;
    }

    public bool HasStatus(params Status[] statuses)
    {
        foreach (var status in statuses)
            if (Status == status)
                return true;

        return false;
    }
}