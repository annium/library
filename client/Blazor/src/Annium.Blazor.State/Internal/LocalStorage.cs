using System;
using Microsoft.JSInterop;

namespace Annium.Blazor.State.Internal;

internal class LocalStorage : StorageBase, ILocalStorage
{
    public LocalStorage(IServiceProvider sp, IJSRuntime js)
        : base(sp, js, "localStorage") { }
}
