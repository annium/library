using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Serialization.Abstractions;
using Microsoft.JSInterop;

namespace Annium.Blazor.Storage.Internal
{
    internal class StorageBase : IStorageBase
    {
        private readonly IJSInProcessRuntime _js;
        private readonly ISerializer<string> _serializer;

        private readonly string _storage;

        protected StorageBase(
            IJSRuntime js,
            ISerializer<string> serializer,
            string storage
        )
        {
            _js = (IJSInProcessRuntime) js;
            _serializer = serializer;
            _storage = storage;
        }

        public IReadOnlyCollection<string> GetKeys()
        {
            var length = _js.Invoke<int>("eval", $"{_storage}.length");

            if (length == 0)
                return Array.Empty<string>();

            var keys = Enumerable.Range(0, length)
                .Select(i => _js.Invoke<string>($"{_storage}.key", i))
                .ToArray();

            return keys;
        }

        public bool HasKey(string key)
        {
            return _js.Invoke<bool>($"{_storage}.hasOwnProperty", key);
        }

        public T Get<T>(string key)
        {
            var raw = _js.Invoke<string>($"{_storage}.getItem", key);

            if (string.IsNullOrWhiteSpace(raw))
                throw new KeyNotFoundException($"Key {key} is not found in {_storage}");

            var value = _serializer.Deserialize<T>(raw);

            return value;
        }

        public bool Set<T>(string key, T value)
        {
            var raw = _serializer.Serialize(value);
            var hasKey = HasKey(key);
            _js.InvokeVoid($"{_storage}.setItem", key, raw);

            return !hasKey;
        }

        public bool Remove(string key)
        {
            var hasKey = HasKey(key);
            _js.InvokeVoid($"{_storage}.removeItem", key);

            return hasKey;
        }

        public void Clear()
        {
            _js.InvokeVoid($"{_storage}.clear");
        }
    }
}