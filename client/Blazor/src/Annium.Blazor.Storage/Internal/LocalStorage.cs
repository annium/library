using Annium.Serialization.Abstractions;
using Microsoft.JSInterop;

namespace Annium.Blazor.Storage.Internal
{
    internal class LocalStorage : StorageBase, ILocalStorage
    {
        public LocalStorage(
            IJSRuntime js,
            ISerializer<string> serializer
        ) : base(js, serializer, "localStorage")
        {
        }
    }
}