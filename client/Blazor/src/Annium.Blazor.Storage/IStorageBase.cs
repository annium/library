using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annium.Blazor.Storage
{
    public interface IStorageBase
    {
        ValueTask<IReadOnlyCollection<string>> GetKeysAsync();

        ValueTask<bool> HasKeyAsync(string key);

        ValueTask<T> GetAsync<T>(string key);

        ValueTask<bool> SetAsync<T>(string key, T value);

        ValueTask<bool> RemoveAsync(string key);

        ValueTask ClearAsync();
    }
}