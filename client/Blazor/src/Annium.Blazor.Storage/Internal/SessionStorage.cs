using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Microsoft.JSInterop;

namespace Annium.Blazor.Storage.Internal
{
    internal class SessionStorage : StorageBase, ISessionStorage
    {
        public SessionStorage(
            IJSRuntime js,
            IIndex<string, ISerializer<string>> serializers
        ) : base(js, serializers, "sessionStorage")
        {
        }
    }
}