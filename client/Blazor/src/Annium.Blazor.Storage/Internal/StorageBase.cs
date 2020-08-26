using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Serialization.Abstractions;
using Microsoft.JSInterop;

namespace Annium.Blazor.Storage.Internal
{
    internal class StorageBase : IStorageBase
    {
        private readonly IJSRuntime _js;
        private readonly ISerializer<string> _serializer;

        private readonly string _storage;

        public StorageBase(
            IJSRuntime js,
            ISerializer<string> serializer,
            string storage
        )
        {
            _js = js;
            _serializer = serializer;
            _storage = storage;
        }

        public async ValueTask<IReadOnlyCollection<string>> GetKeysAsync()
        {
            var length = await _js.InvokeAsync<int>("eval", $"{_storage}.length");

            if (length == 0)
                return Array.Empty<string>();

            var keys = await Task.WhenAll(
                Enumerable.Range(0, length)
                    .Select(async i => await _js.InvokeAsync<string>($"{_storage}.key", i))
            );

            return keys;
        }

        public ValueTask<bool> HasKeyAsync(string key)
        {
            return _js.InvokeAsync<bool>($"{_storage}.hasOwnProperty", key);
        }

        public async ValueTask<T> GetAsync<T>(string key)
        {
            var raw = await _js.InvokeAsync<string>($"{_storage}.getItem", key);

            if (string.IsNullOrWhiteSpace(raw))
                throw new KeyNotFoundException($"Key {key} is not found in {_storage}");

            var value = _serializer.Deserialize<T>(raw);

            return value;
        }

        public async ValueTask<bool> SetAsync<T>(string key, T value)
        {
            var raw = _serializer.Serialize<T>(value);
            var hasKey = await HasKeyAsync(key);
            await _js.InvokeVoidAsync($"{_storage}.setItem", key, raw);

            return !hasKey;
        }

        public async ValueTask<bool> RemoveAsync(string key)
        {
            var hasKey = await HasKeyAsync(key);
            await _js.InvokeVoidAsync($"{_storage}.removeItem", key);

            return hasKey;
        }

        public ValueTask ClearAsync()
        {
            return _js.InvokeVoidAsync($"{_storage}.clear");
        }
    }
}