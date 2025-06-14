using System;
using Microsoft.JSInterop;

namespace Annium.Blazor.State.Internal;

/// <summary>
/// Implementation of session storage interface that provides temporary browser storage functionality that persists only during the session
/// </summary>
internal class SessionStorage : StorageBase, ISessionStorage
{
    /// <summary>
    /// Initializes a new instance of the SessionStorage class
    /// </summary>
    /// <param name="sp">Service provider for dependency resolution</param>
    /// <param name="js">JavaScript runtime for browser storage operations</param>
    public SessionStorage(IServiceProvider sp, IJSRuntime js)
        : base(sp, js, "sessionStorage") { }
}
