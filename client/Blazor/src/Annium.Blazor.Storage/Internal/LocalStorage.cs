using Annium.Serialization.Abstractions;
using Microsoft.JSInterop;

namespace Annium.Blazor.Storage.Internal
{
    internal class LocalStorage : StorageBase, ILocalStorage
    {
        public LocalStorage(
            IJSInProcessRuntime js,
            ISerializer<string> serializer
        ) : base(js, serializer, "localStorage")
        {
        }
    }
}