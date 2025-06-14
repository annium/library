using System.Collections.Generic;

namespace Annium.Blazor.State;

/// <summary>
/// Base interface for browser storage implementations providing common storage operations
/// </summary>
public interface IStorageBase
{
    /// <summary>
    /// Gets all keys currently stored in the storage
    /// </summary>
    /// <returns>A read-only collection of all storage keys</returns>
    IReadOnlyCollection<string> GetKeys();

    /// <summary>
    /// Determines whether the storage contains the specified key
    /// </summary>
    /// <param name="key">The key to check for</param>
    /// <returns>True if the key exists in storage, false otherwise</returns>
    bool HasKey(string key);

    /// <summary>
    /// Attempts to retrieve a value from storage and deserialize it to the specified type
    /// </summary>
    /// <typeparam name="T">The type to deserialize the value to</typeparam>
    /// <param name="key">The key to retrieve</param>
    /// <param name="value">The deserialized value if successful, default value otherwise</param>
    /// <returns>True if the value was successfully retrieved and deserialized, false otherwise</returns>
    bool TryGet<T>(string key, out T? value);

    /// <summary>
    /// Attempts to retrieve a string value from storage
    /// </summary>
    /// <param name="key">The key to retrieve</param>
    /// <param name="value">The string value if successful, null otherwise</param>
    /// <returns>True if the value was successfully retrieved, false otherwise</returns>
    bool TryGetString(string key, out string? value);

    /// <summary>
    /// Retrieves a value from storage and deserializes it to the specified type
    /// </summary>
    /// <typeparam name="T">The type to deserialize the value to</typeparam>
    /// <param name="key">The key to retrieve</param>
    /// <returns>The deserialized value</returns>
    T Get<T>(string key);

    /// <summary>
    /// Retrieves a string value from storage
    /// </summary>
    /// <param name="key">The key to retrieve</param>
    /// <returns>The string value</returns>
    string GetString(string key);

    /// <summary>
    /// Stores a value in storage by serializing it
    /// </summary>
    /// <typeparam name="T">The type of the value to store</typeparam>
    /// <param name="key">The key to store the value under</param>
    /// <param name="value">The value to store</param>
    /// <returns>True if the value was successfully stored, false otherwise</returns>
    bool Set<T>(string key, T value);

    /// <summary>
    /// Stores a string value in storage
    /// </summary>
    /// <param name="key">The key to store the value under</param>
    /// <param name="value">The string value to store</param>
    /// <returns>True if the value was successfully stored, false otherwise</returns>
    bool SetString(string key, string value);

    /// <summary>
    /// Removes a value from storage
    /// </summary>
    /// <param name="key">The key of the value to remove</param>
    /// <returns>True if the value was successfully removed, false otherwise</returns>
    bool Remove(string key);

    /// <summary>
    /// Removes all values from storage
    /// </summary>
    void Clear();
}
