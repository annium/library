using System;
using System.Collections.Generic;
using Annium.Core.Primitives.Collections.Generic;

namespace Annium.Blazor.Interop.Internal;

internal record ElementInteropEvent<T> : InteropEventBase<T>
    where T : notnull
{
    private readonly IObject _target;

    public ElementInteropEvent(
        IObject target
    ) : base("element", new Lazy<string>(() => target.Id))
    {
        _target = target;
    }

    protected override IEnumerable<object> GetSharedBindArgs() => _target.Id.Yield();
}