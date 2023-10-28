using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Microsoft.JSInterop;

namespace Annium.Blazor.State.Internal;

internal class LocalStorage : StorageBase, ILocalStorage
{
    public LocalStorage(IJSRuntime js, IIndex<SerializerKey, ISerializer<string>> serializers)
        : base(js, serializers, "localStorage") { }
}
