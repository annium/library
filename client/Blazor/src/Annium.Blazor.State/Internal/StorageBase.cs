using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Microsoft.JSInterop;

namespace Annium.Blazor.State.Internal;

/// <summary>
/// Base class for browser storage implementations providing common functionality for both local and session storage
/// </summary>
internal class StorageBase : IStorageBase
{
    /// <summary>
    /// JavaScript runtime for interacting with browser storage APIs
    /// </summary>
    private readonly IJSInProcessRuntime _js;

    /// <summary>
    /// Serializer for converting objects to and from JSON strings
    /// </summary>
    private readonly ISerializer<string> _serializer;

    /// <summary>
    /// Name of the storage type (localStorage or sessionStorage)
    /// </summary>
    private readonly string _storage;

    /// <summary>
    /// Initializes a new instance of the StorageBase class
    /// </summary>
    /// <param name="sp">Service provider for dependency resolution</param>
    /// <param name="js">JavaScript runtime for browser storage operations</param>
    /// <param name="storage">Name of the storage type to use</param>
    protected StorageBase(IServiceProvider sp, IJSRuntime js, string storage)
    {
        _js = (IJSInProcessRuntime)js;
        var serializerKey = SerializerKey.CreateDefault(MediaTypeNames.Application.Json);
        _serializer = sp.ResolveKeyed<ISerializer<string>>(serializerKey);
        _storage = storage;
    }

    /// <summary>
    /// Gets all keys currently stored in the storage
    /// </summary>
    /// <returns>A read-only collection of all storage keys</returns>
    public IReadOnlyCollection<string> GetKeys()
    {
        var length = _js.Invoke<int>("eval", $"{_storage}.length");

        if (length == 0)
            return [];

        var keys = Enumerable.Range(0, length).Select(i => _js.Invoke<string>($"{_storage}.key", i)).ToArray();

        return keys;
    }

    /// <summary>
    /// Checks if a key exists in the storage
    /// </summary>
    /// <param name="key">The key to check for</param>
    /// <returns>True if the key exists, false otherwise</returns>
    public bool HasKey(string key)
    {
        return _js.Invoke<bool>($"{_storage}.hasOwnProperty", key);
    }

    /// <summary>
    /// Attempts to get a value from storage and deserialize it to the specified type
    /// </summary>
    /// <typeparam name="T">The type to deserialize the value to</typeparam>
    /// <param name="key">The key to retrieve the value for</param>
    /// <param name="value">The deserialized value if successful, default value otherwise</param>
    /// <returns>True if the value was successfully retrieved and deserialized, false otherwise</returns>
    public bool TryGet<T>(string key, out T? value)
    {
        if (TryGetString(key, out var raw))
        {
            value = _serializer.Deserialize<T>(raw!);
            return true;
        }

        value = default;

        return false;
    }

    /// <summary>
    /// Attempts to get a string value from storage
    /// </summary>
    /// <param name="key">The key to retrieve the value for</param>
    /// <param name="value">The string value if successful, null otherwise</param>
    /// <returns>True if the value was successfully retrieved, false otherwise</returns>
    public bool TryGetString(string key, out string? value)
    {
        value = _js.Invoke<string>($"{_storage}.getItem", key);

        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Gets a value from storage and deserializes it to the specified type
    /// </summary>
    /// <typeparam name="T">The type to deserialize the value to</typeparam>
    /// <param name="key">The key to retrieve the value for</param>
    /// <returns>The deserialized value</returns>
    public T Get<T>(string key)
    {
        return _serializer.Deserialize<T>(GetString(key));
    }

    /// <summary>
    /// Gets a string value from storage
    /// </summary>
    /// <param name="key">The key to retrieve the value for</param>
    /// <returns>The string value</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the key is not found in storage</exception>
    public string GetString(string key)
    {
        var raw = _js.Invoke<string>($"{_storage}.getItem", key);

        if (string.IsNullOrWhiteSpace(raw))
            throw new KeyNotFoundException($"Key {key} is not found in {_storage}");

        return raw;
    }

    /// <summary>
    /// Sets a value in storage by serializing it to JSON
    /// </summary>
    /// <typeparam name="T">The type of the value to store</typeparam>
    /// <param name="key">The key to store the value under</param>
    /// <param name="value">The value to store</param>
    /// <returns>True if the key was newly created, false if it was updated</returns>
    public bool Set<T>(string key, T value)
    {
        return SetString(key, _serializer.Serialize(value));
    }

    /// <summary>
    /// Sets a string value in storage
    /// </summary>
    /// <param name="key">The key to store the value under</param>
    /// <param name="value">The string value to store</param>
    /// <returns>True if the key was newly created, false if it was updated</returns>
    public bool SetString(string key, string value)
    {
        var hasKey = HasKey(key);
        _js.InvokeVoid($"{_storage}.setItem", key, value);

        return !hasKey;
    }

    /// <summary>
    /// Removes a key and its value from storage
    /// </summary>
    /// <param name="key">The key to remove</param>
    /// <returns>True if the key existed and was removed, false if it didn't exist</returns>
    public bool Remove(string key)
    {
        var hasKey = HasKey(key);
        _js.InvokeVoid($"{_storage}.removeItem", key);

        return hasKey;
    }

    /// <summary>
    /// Clears all keys and values from storage
    /// </summary>
    public void Clear()
    {
        _js.InvokeVoid($"{_storage}.clear");
    }
}
