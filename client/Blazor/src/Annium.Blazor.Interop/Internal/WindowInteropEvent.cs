using System;
using System.Collections.Generic;

namespace Annium.Blazor.Interop.Internal;

internal sealed record WindowInteropEvent<T>() : InteropEventBase<T>("window", new Lazy<string>("window"))
    where T : notnull
{
    protected override IEnumerable<object> GetSharedBindArgs() => Array.Empty<object>();
}