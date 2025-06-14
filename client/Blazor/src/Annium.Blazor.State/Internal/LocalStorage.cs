using System;
using Microsoft.JSInterop;

namespace Annium.Blazor.State.Internal;

/// <summary>
/// Implementation of local storage interface that provides persistent browser storage functionality
/// </summary>
internal class LocalStorage : StorageBase, ILocalStorage
{
    /// <summary>
    /// Initializes a new instance of the LocalStorage class
    /// </summary>
    /// <param name="sp">Service provider for dependency resolution</param>
    /// <param name="js">JavaScript runtime for browser storage operations</param>
    public LocalStorage(IServiceProvider sp, IJSRuntime js)
        : base(sp, js, "localStorage") { }
}
