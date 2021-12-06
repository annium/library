using System.Collections.Generic;

namespace Annium.Blazor.Storage;

public interface IStorageBase
{
    IReadOnlyCollection<string> GetKeys();

    bool HasKey(string key);

    T Get<T>(string key);

    bool Set<T>(string key, T value);

    bool Remove(string key);

    void Clear();
}