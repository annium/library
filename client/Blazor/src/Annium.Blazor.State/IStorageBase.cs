using System.Collections.Generic;

namespace Annium.Blazor.State;

public interface IStorageBase
{
    IReadOnlyCollection<string> GetKeys();
    bool HasKey(string key);
    bool TryGet<T>(string key, out T? value);
    bool TryGetString(string key, out string? value);
    T Get<T>(string key);
    string GetString(string key);
    bool Set<T>(string key, T value);
    bool SetString(string key, string value);
    bool Remove(string key);
    void Clear();
}
