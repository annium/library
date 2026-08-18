using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Annium.Blazor.State.Tests.Fakes;

/// <summary>
/// A fake <see cref="IJSInProcessRuntime"/> backed by in-memory dictionaries, one per JS storage object
/// (<c>localStorage</c> / <c>sessionStorage</c>), so <c>StorageBase</c> can be exercised without a real browser.
/// Understands exactly the identifier shapes <c>StorageBase</c> issues: the <c>eval</c> escape hatch used for
/// <c>{storage}.length</c>, and the direct <c>{storage}.key</c> / <c>hasOwnProperty</c> / <c>getItem</c> /
/// <c>setItem</c> / <c>removeItem</c> / <c>clear</c> calls. Any other identifier is unsupported and throws.
/// </summary>
internal sealed class FakeInProcessJsRuntime : IJSInProcessRuntime
{
    /// <summary>
    /// Message for the interface members <c>StorageBase</c> never calls.
    /// </summary>
    private const string NotExercised = "Not exercised by these tests";

    /// <summary>
    /// Per-storage-name key/value maps, created on first access.
    /// </summary>
    private readonly Dictionary<string, Dictionary<string, string>> _storages = new();

    /// <summary>
    /// Gets a read-only snapshot of the given storage's contents (e.g. <c>"localStorage"</c>), for asserting which
    /// JS storage object a call actually targeted.
    /// </summary>
    /// <param name="storage">The JS storage object name.</param>
    /// <returns>A read-only snapshot of that storage's contents.</returns>
    public IReadOnlyDictionary<string, string> this[string storage] => GetStorage(storage);

    /// <summary>
    /// Invokes an in-process JS function synchronously, interpreting the identifier/args pair as one of the
    /// storage operations <c>StorageBase</c> issues.
    /// </summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    /// <param name="identifier">The JS identifier (either <c>eval</c> or a direct <c>{storage}.{op}</c> path).</param>
    /// <param name="args">The call arguments.</param>
    /// <returns>The operation result, cast to <typeparamref name="TResult"/>.</returns>
    public TResult Invoke<TResult>(string identifier, params object?[]? args)
    {
        var (storage, op) = ParseTarget(identifier, args);
        var store = GetStorage(storage);

        object? result = op switch
        {
            "length" => store.Count,
            "key" => Key(store, (int)args![0]!),
            "hasOwnProperty" => store.ContainsKey((string)args![0]!),
            "getItem" => store.GetValueOrDefault((string)args![0]!),
            "setItem" => SetItem(store, (string)args![0]!, (string)args[1]!),
            "removeItem" => RemoveItem(store, (string)args![0]!),
            "clear" => ClearStore(store),
            _ => throw new NotSupportedException($"Unsupported identifier '{identifier}'"),
        };

        return (TResult)result!;
    }

    /// <summary>
    /// Not supported — <c>StorageBase</c> uses only the synchronous string-invoke API.
    /// </summary>
    /// <param name="identifier">The JS constructor identifier.</param>
    /// <param name="args">The constructor arguments.</param>
    /// <returns>Never returns; always throws <see cref="NotSupportedException"/>.</returns>
    public IJSInProcessObjectReference InvokeConstructor(string identifier, params object?[]? args) =>
        throw new NotSupportedException(NotExercised);

    /// <summary>
    /// Not supported — <c>StorageBase</c> uses only the synchronous string-invoke API.
    /// </summary>
    /// <typeparam name="TValue">The requested value type.</typeparam>
    /// <param name="identifier">The JS property identifier.</param>
    /// <returns>Never returns; always throws <see cref="NotSupportedException"/>.</returns>
    public TValue GetValue<TValue>(string identifier) => throw new NotSupportedException(NotExercised);

    /// <summary>
    /// Not supported — <c>StorageBase</c> uses only the synchronous string-invoke API.
    /// </summary>
    /// <typeparam name="TValue">The value type to set.</typeparam>
    /// <param name="identifier">The JS property identifier.</param>
    /// <param name="value">The value to set.</param>
    public void SetValue<TValue>(string identifier, TValue value) => throw new NotSupportedException(NotExercised);

    /// <summary>
    /// Not supported — <c>StorageBase</c> uses only the synchronous string-invoke API.
    /// </summary>
    /// <typeparam name="TValue">The expected result type.</typeparam>
    /// <param name="identifier">The JS identifier.</param>
    /// <param name="args">The call arguments.</param>
    /// <returns>Never returns; always throws <see cref="NotSupportedException"/>.</returns>
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
        throw new NotSupportedException($"{NotExercised} — StorageBase only uses the sync API");

    /// <summary>
    /// Not supported — <c>StorageBase</c> uses only the synchronous string-invoke API.
    /// </summary>
    /// <typeparam name="TValue">The expected result type.</typeparam>
    /// <param name="identifier">The JS identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <param name="args">The call arguments.</param>
    /// <returns>Never returns; always throws <see cref="NotSupportedException"/>.</returns>
    public ValueTask<TValue> InvokeAsync<TValue>(
        string identifier,
        CancellationToken cancellationToken,
        object?[]? args
    ) => throw new NotSupportedException($"{NotExercised} — StorageBase only uses the sync API");

    /// <summary>
    /// Not supported — <c>StorageBase</c> uses only the synchronous string-invoke API.
    /// </summary>
    /// <param name="identifier">The JS constructor identifier.</param>
    /// <param name="args">The constructor arguments.</param>
    /// <returns>Never returns; always throws <see cref="NotSupportedException"/>.</returns>
    public ValueTask<IJSObjectReference> InvokeConstructorAsync(string identifier, object?[]? args) =>
        throw new NotSupportedException(NotExercised);

    /// <summary>
    /// Not supported — <c>StorageBase</c> uses only the synchronous string-invoke API.
    /// </summary>
    /// <param name="identifier">The JS constructor identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <param name="args">The constructor arguments.</param>
    /// <returns>Never returns; always throws <see cref="NotSupportedException"/>.</returns>
    public ValueTask<IJSObjectReference> InvokeConstructorAsync(
        string identifier,
        CancellationToken cancellationToken,
        object?[]? args
    ) => throw new NotSupportedException(NotExercised);

    /// <summary>
    /// Not supported — <c>StorageBase</c> uses only the synchronous string-invoke API.
    /// </summary>
    /// <typeparam name="TValue">The requested value type.</typeparam>
    /// <param name="identifier">The JS property identifier.</param>
    /// <returns>Never returns; always throws <see cref="NotSupportedException"/>.</returns>
    public ValueTask<TValue> GetValueAsync<TValue>(string identifier) => throw new NotSupportedException(NotExercised);

    /// <summary>
    /// Not supported — <c>StorageBase</c> uses only the synchronous string-invoke API.
    /// </summary>
    /// <typeparam name="TValue">The requested value type.</typeparam>
    /// <param name="identifier">The JS property identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Never returns; always throws <see cref="NotSupportedException"/>.</returns>
    public ValueTask<TValue> GetValueAsync<TValue>(string identifier, CancellationToken cancellationToken) =>
        throw new NotSupportedException(NotExercised);

    /// <summary>
    /// Not supported — <c>StorageBase</c> uses only the synchronous string-invoke API.
    /// </summary>
    /// <typeparam name="TValue">The value type to set.</typeparam>
    /// <param name="identifier">The JS property identifier.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>Never returns; always throws <see cref="NotSupportedException"/>.</returns>
    public ValueTask SetValueAsync<TValue>(string identifier, TValue value) =>
        throw new NotSupportedException(NotExercised);

    /// <summary>
    /// Not supported — <c>StorageBase</c> uses only the synchronous string-invoke API.
    /// </summary>
    /// <typeparam name="TValue">The value type to set.</typeparam>
    /// <param name="identifier">The JS property identifier.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Never returns; always throws <see cref="NotSupportedException"/>.</returns>
    public ValueTask SetValueAsync<TValue>(string identifier, TValue value, CancellationToken cancellationToken) =>
        throw new NotSupportedException(NotExercised);

    /// <summary>
    /// Resolves the target storage name and operation name from an identifier/args pair: either directly from a
    /// <c>{storage}.{op}</c> identifier, or — for the <c>{storage}.length</c> getter, which <c>StorageBase</c>
    /// reads via <c>eval</c> — from the evaluated expression passed as the first argument.
    /// </summary>
    /// <param name="identifier">The JS identifier.</param>
    /// <param name="args">The call arguments.</param>
    /// <returns>The storage name and operation name.</returns>
    private static (string Storage, string Op) ParseTarget(string identifier, object?[]? args)
    {
        var target = identifier == "eval" ? (string)args![0]! : identifier;
        var separatorIndex = target.IndexOf('.');

        return (target[..separatorIndex], target[(separatorIndex + 1)..]);
    }

    /// <summary>
    /// Gets (creating if absent) the key/value map for the given storage name.
    /// </summary>
    /// <param name="storage">The JS storage object name.</param>
    /// <returns>The key/value map backing the given storage.</returns>
    private Dictionary<string, string> GetStorage(string storage)
    {
        if (!_storages.TryGetValue(storage, out var store))
            _storages[storage] = store = new Dictionary<string, string>();

        return store;
    }

    /// <summary>
    /// Returns the key at the given index, mirroring <c>Storage.key(index)</c>, or null if out of range.
    /// </summary>
    /// <param name="store">The storage map.</param>
    /// <param name="index">The zero-based key index.</param>
    /// <returns>The key at <paramref name="index"/>, or null if out of range.</returns>
    private static string? Key(Dictionary<string, string> store, int index) => store.Keys.ElementAtOrDefault(index);

    /// <summary>
    /// Sets a value, mirroring <c>Storage.setItem(key, value)</c>.
    /// </summary>
    /// <param name="store">The storage map.</param>
    /// <param name="key">The key to set.</param>
    /// <param name="value">The value to store.</param>
    /// <returns>Null — <c>setItem</c> is a void JS operation.</returns>
    private static object? SetItem(Dictionary<string, string> store, string key, string value)
    {
        store[key] = value;

        return null;
    }

    /// <summary>
    /// Removes a value, mirroring <c>Storage.removeItem(key)</c>.
    /// </summary>
    /// <param name="store">The storage map.</param>
    /// <param name="key">The key to remove.</param>
    /// <returns>Null — <c>removeItem</c> is a void JS operation.</returns>
    private static object? RemoveItem(Dictionary<string, string> store, string key)
    {
        store.Remove(key);

        return null;
    }

    /// <summary>
    /// Empties the storage, mirroring <c>Storage.clear()</c>.
    /// </summary>
    /// <param name="store">The storage map.</param>
    /// <returns>Null — <c>clear</c> is a void JS operation.</returns>
    private static object? ClearStore(Dictionary<string, string> store)
    {
        store.Clear();

        return null;
    }
}
