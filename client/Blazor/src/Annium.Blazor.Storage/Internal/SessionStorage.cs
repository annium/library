using Annium.Serialization.Abstractions;
using Microsoft.JSInterop;

namespace Annium.Blazor.Storage.Internal
{
    internal class SessionStorage : StorageBase, ISessionStorage
    {
        public SessionStorage(
            IJSInProcessRuntime js,
            ISerializer<string> serializer
        ) : base(js, serializer, "sessionStorage")
        {
        }
    }
}