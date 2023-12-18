using System;
using Microsoft.JSInterop;

namespace Annium.Blazor.State.Internal;

internal class SessionStorage : StorageBase, ISessionStorage
{
    public SessionStorage(IServiceProvider sp, IJSRuntime js)
        : base(sp, js, "sessionStorage") { }
}
