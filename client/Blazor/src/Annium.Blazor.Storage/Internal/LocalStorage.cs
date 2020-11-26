using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Microsoft.JSInterop;

namespace Annium.Blazor.Storage.Internal
{
    internal class LocalStorage : StorageBase, ILocalStorage
    {
        public LocalStorage(
            IJSRuntime js,
            IIndex<string, ISerializer<string>> serializers
        ) : base(js, serializers, "localStorage")
        {
        }
    }
}