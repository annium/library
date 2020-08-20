using System;
using Annium.Components.State.Core;

namespace Annium.Components.State.Forms.Internal
{
    internal class AtomicContainer<T> : ObservableState, IAtomicContainer<T>
        where T : IEquatable<T>
    {
        public T Value { get; private set; }
        public bool HasChanged => !Value.Equals(_initialValue);
        public bool HasBeenTouched { get; private set; }
        public Status Status { get; private set; }
        public string Message { get; private set; } = string.Empty;
        private readonly T _initialValue;

        public AtomicContainer(T initialValue)
        {
            Value = _initialValue = initialValue;
        }

        public bool Set(T value)
        {
            if (value.Equals(Value))
                return false;

            Value = value;
            HasBeenTouched = true;
            NotifyChanged();

            return true;
        }

        public void Reset()
        {
            Value = _initialValue;
            HasBeenTouched = false;
            Status = Status.None;
            NotifyChanged();
        }

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
}