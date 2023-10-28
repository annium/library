using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Microsoft.JSInterop;

namespace Annium.Blazor.State.Internal;

internal class SessionStorage : StorageBase, ISessionStorage
{
    public SessionStorage(IJSRuntime js, IIndex<SerializerKey, ISerializer<string>> serializers)
        : base(js, serializers, "sessionStorage") { }
}
